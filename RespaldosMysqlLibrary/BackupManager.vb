

Imports System.Xml
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Globalization

''' <summary>
''' Gestiona las operaciones de respaldo de bases de datos MySQL, incluyendo la conexión, ejecución de mysqldump, compresión y gestión de configuraciones.
''' </summary>
''' <remarks>
''' Esta clase es el núcleo de la lógica de respaldo, interactuando con la configuración del servidor y las herramientas externas como mysqldump y 7-Zip.
''' </remarks>
Public Class BackupManager

    Private _servers As New List(Of Server)

    ''' <summary>
    ''' Obtiene o establece la ruta completa al ejecutable de mysqldump.exe.
    ''' </summary>
    ''' <value>La ruta del ejecutable de mysqldump.</value>
    Public Property MySqlDumpPath As String
    ''' <summary>
    ''' Obtiene o establece la ruta de destino principal para los archivos de respaldo.
    ''' </summary>
    ''' <value>La ruta donde se guardarán los respaldos.</value>
    Public Property BackupDestinationPath As String
    ''' <summary>
    ''' Obtiene o establece la ruta completa al ejecutable de 7z.exe (7-Zip).
    ''' </summary>
    ''' <value>La ruta del ejecutable de 7-Zip.</value>
    Public Property SevenZipPath As String
    ''' <summary>
    ''' Obtiene o establece un valor que indica si las contraseñas de los servidores deben ser encriptadas.
    ''' </summary>
    ''' <value>True si las contraseñas deben encriptarse; de lo contrario, False.</value>
    Public Property EncryptPasswords As Boolean

    ''' <summary>
    ''' Valida una cadena para asegurarse de que no contiene caracteres que podrían ser peligrosos en un shell de comandos.
    ''' </summary>
    ''' <param name="input">La cadena a validar.</param>
    ''' <returns>True si la cadena es segura; de lo contrario, False.</returns>
    Public Function IsInputSafe(input As String) As Boolean
        ' Expresión regular para buscar caracteres peligrosos. 
        ' Incluye: & | ; $ > < ` \ !
        ' No incluimos comillas simples o dobles ya que las manejamos explícitamente.
        Dim unsafeCharsPattern As String = "[&|;$><`\\!]"
        If Regex.IsMatch(input, unsafeCharsPattern) Then
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Realiza una operación de respaldo completa para un servidor y sus bases de datos.
    ''' </summary>
    ''' <param name="server">El objeto <see cref="Server"/> que contiene los detalles de conexión y configuración.</param>
    ''' <param name="backupPath">La ruta base donde se guardarán los respaldos específicos de este servidor.</param>
    ''' <param name="isAutomatic">Indica si el respaldo se inició de forma automática (True) o manual (False).</param>
    ''' <returns>True si todas las operaciones de respaldo y compresión fueron exitosas; de lo contrario, False.</returns>
    ''' <remarks>
    ''' Este método se conecta a MySQL, obtiene la lista de bases de datos (excluyendo las configuradas),
    ''' ejecuta mysqldump para cada una, y luego comprime los archivos SQL resultantes usando 7-Zip.
    ''' </remarks>
    Public Function PerformBackup(server As Server, backupPath As String, isAutomatic As Boolean) As Boolean
        AppLogger.Log($"Iniciando respaldo para el servidor: {server.Name} (Automático: {isAutomatic})", "BACKUP")

        ' --- Comprobación de seguridad para evitar inyección de comandos ---
        If Not IsInputSafe(server.Name) OrElse Not IsInputSafe(server.IP) OrElse Not IsInputSafe(server.User) Then
            AppLogger.Log($"ALERTA DE SEGURIDAD: Se detectaron caracteres no válidos en la configuración del servidor '{server.Name}'. Abortando respaldo.", "ERROR")
            Return False
        End If

        Try
            Dim allDatabases As New List(Of String)
            Dim connBuilder As New MySqlConnectionStringBuilder()
            connBuilder.Server = server.IP
            connBuilder.Port = server.Port
            connBuilder.UserID = server.User

            Dim passwordToUse As String
            If server.IsPasswordEncrypted Then
                Try
                    passwordToUse = EncryptionHelper.Decrypt(server.Password)
                Catch ex As Exception
                    AppLogger.Log($"Error al desencriptar la contraseña para el servidor {server.Name}: {ex.Message}", "ERROR")
                    Return False ' No se puede proceder sin la contraseña
                End Try
            Else
                passwordToUse = server.Password
            End If
            connBuilder.Password = passwordToUse
            Dim connectionString As String = connBuilder.ConnectionString

            Try
                Using connection As New MySqlConnection(connectionString)
                    connection.Open()
                    AppLogger.Log($"Conexión exitosa a MySQL en {server.IP}:{server.Port}", "BACKUP")

                    Dim xcommand As New MySqlCommand("SHOW DATABASES;", connection)
                    Using reader As MySqlDataReader = xcommand.ExecuteReader()
                        While reader.Read()
                            Dim dbName As String = reader.GetString(0)
                            If Not dbName.Equals("Database", StringComparison.OrdinalIgnoreCase) Then
                                allDatabases.Add(dbName)
                            End If
                        End While
                    End Using
                    AppLogger.Log($"Se obtuvieron {allDatabases.Count} bases de datos con éxito.", "BACKUP")
                End Using
            Catch ex As MySqlException
                AppLogger.Log($"Error de MySQL al obtener las bases de datos del servidor {server.Name}: {ex.Message}", "ERROR")
                Console.WriteLine($"Error de MySQL al obtener bases de datos del servidor {server.Name}: {ex.Message}")
                Return False
            Catch ex As Exception
                AppLogger.Log($"Excepción general al obtener las bases de datos del servidor {server.Name}: {ex.Message}", "ERROR")
                Console.WriteLine($"Excepción general al obtener bases de datos del servidor {server.Name}: {ex.Message}")
                Return False
            End Try

            ' Filter out excluded databases
            Dim databasesToBackup As New List(Of String)
            For Each db As String In allDatabases
                If Not server.ExcludedDatabases.Contains(db, StringComparer.OrdinalIgnoreCase) Then
                    databasesToBackup.Add(db)
                End If
            Next
            AppLogger.Log($"Bases de datos a respaldar después de la exclusión: {String.Join(", ", databasesToBackup.ToArray())}", "BACKUP")

            ProgressReporter.WriteStatus("Iniciando respaldo...", 0)

            Dim allBackupsSuccessful As Boolean = True

            For i As Integer = 0 To databasesToBackup.Count - 1
                Dim dbName As String = databasesToBackup(i)

                ' --- Comprobación de seguridad para el nombre de la base de datos ---
                If Not IsInputSafe(dbName) Then
                    AppLogger.Log($"ALERTA DE SEGURIDAD: Se detectaron caracteres no válidos en el nombre de la base de datos '{dbName}' del servidor '{server.Name}'. Omitiendo esta base de datos.", "ERROR")
                    Continue For ' Saltar a la siguiente base de datos
                End If

                Dim progressPercentage As Integer = CInt((i + 1) / databasesToBackup.Count * 100)
                ProgressReporter.WriteStatus($"Respaldando {dbName}...", progressPercentage)
                Dim timestamp As String = DateTime.Now.ToString("yyMMdd_HHmm")
                Dim specificBackupFileName As String = $"{dbName}_{timestamp}.sql"
                Dim serverSpecificPath As String = Path.Combine(backupPath, $"{server.Name}_{server.IP}")
                Dim dbSpecificPath As String = Path.Combine(serverSpecificPath, dbName)
                Dim fullSpecificBackupPath As String = Path.Combine(dbSpecificPath, specificBackupFileName)

                ' Create directory if it doesn't exist
                If Not Directory.Exists(dbSpecificPath) Then
                    Directory.CreateDirectory(dbSpecificPath)
                    AppLogger.Log($"Directorio creado: {dbSpecificPath}", "BACKUP")
                End If

                Dim command As String = $"""{MySqlDumpPath}"" -h {server.IP} -P {server.Port} -u {server.User} --password={passwordToUse} {dbName} > ""{fullSpecificBackupPath}"""
                Dim commandForLog As String = $"""{MySqlDumpPath}"" -h {server.IP} -P {server.Port} -u {server.User} --password=***** {dbName} > ""{fullSpecificBackupPath}"""

                AppLogger.Log($"Ejecutando comando mysqldump para {dbName}: {commandForLog}", "BACKUP")
                Dim processInfo As New ProcessStartInfo("cmd.exe")
                processInfo.Arguments = $"/C ""{command}"""
                processInfo.RedirectStandardOutput = True
                processInfo.RedirectStandardError = True
                processInfo.UseShellExecute = False
                processInfo.CreateNoWindow = True

                Using process As Process = Process.Start(processInfo)
                    process.WaitForExit()

                    Dim output As String = process.StandardOutput.ReadToEnd()
                    Dim errorOutput As String = process.StandardError.ReadToEnd()

                    If process.ExitCode = 0 Then
                        AppLogger.Log($"Respaldo exitoso para {dbName}: {fullSpecificBackupPath}. Salida: {output}", "BACKUP")
                        Console.WriteLine($"Respaldo exitoso para {dbName}: {fullSpecificBackupPath}")
                    Else
                        AppLogger.Log($"Error al realizar el respaldo para {dbName} en {server.Name}: {errorOutput}", "ERROR")
                        Console.WriteLine($"Error al realizar el respaldo para {dbName} en {server.Name}: {errorOutput}")
                        allBackupsSuccessful = False
                    End If
                End Using
            Next

            If allBackupsSuccessful Then

                Dim zipFileName As String = $"{server.Name}_{server.IP}_{DateTime.Now.ToString("yyMMdd_HHmm")}.zip"
                Dim serverSpecificBackupRootPath As String = Path.Combine(backupPath, $"{server.Name}_{server.IP}")
                Dim fullZipFilePath As String = Path.Combine(serverSpecificBackupRootPath, zipFileName)

                If Directory.Exists(serverSpecificBackupRootPath) Then
                    Dim zipCommand As String = $"""{SevenZipPath}"" a -tzip ""{fullZipFilePath}"" ""{Path.Combine(serverSpecificBackupRootPath, "*")}"" -x!*.zip"
                    AppLogger.Log($"Ejecutando comando 7-Zip: {zipCommand}", "ZIP")

                    Dim zipProcessInfo As New ProcessStartInfo("cmd.exe")
                    zipProcessInfo.Arguments = $"/C ""{zipCommand}"""
                    zipProcessInfo.RedirectStandardOutput = True
                    zipProcessInfo.RedirectStandardError = True
                    zipProcessInfo.UseShellExecute = False
                    zipProcessInfo.CreateNoWindow = True

                    Using zipProcess As Process = Process.Start(zipProcessInfo)
                        zipProcess.WaitForExit()

                        Dim zipOutput As String = zipProcess.StandardOutput.ReadToEnd()
                        Dim zipErrorOutput As String = zipProcess.StandardError.ReadToEnd()

                        If zipProcess.ExitCode = 0 Then
                            AppLogger.Log($"Compresión exitosa: {fullZipFilePath}. Salida: {zipOutput}", "ZIP")
                            Console.WriteLine($"Compresión exitosa: {fullZipFilePath}")

                            ' Delete original .sql files
                            For Each dbName As String In databasesToBackup
                                ' --- Comprobación de seguridad (de nuevo por si acaso) ---
                                If Not IsInputSafe(dbName) Then Continue For

                                Dim timestamp As String = DateTime.Now.ToString("yyMMdd_HHmm")
                                Dim specificBackupFileName As String = $"{dbName}_{timestamp}.sql"
                                Dim dbSpecificPath As String = Path.Combine(serverSpecificBackupRootPath, dbName)
                                Dim fullSpecificBackupPath As String = Path.Combine(dbSpecificPath, specificBackupFileName)

                                If File.Exists(fullSpecificBackupPath) Then
                                    File.Delete(fullSpecificBackupPath)
                                    AppLogger.Log($"Archivo SQL original eliminado: {fullSpecificBackupPath}", "CLEANUP")
                                End If
                                If Directory.Exists(dbSpecificPath) Then
                                    Directory.Delete(dbSpecificPath, True)
                                    AppLogger.Log($"Directorio de base de datos eliminado (recursivo): {dbSpecificPath}", "CLEANUP")
                                End If
                            Next
                            If Directory.Exists(serverSpecificBackupRootPath) AndAlso Directory.GetFiles(serverSpecificBackupRootPath).Length = 0 AndAlso Directory.GetDirectories(serverSpecificBackupRootPath).Length = 0 Then
                                Directory.Delete(serverSpecificBackupRootPath)
                                AppLogger.Log($"Directorio de servidor vacío eliminado: {serverSpecificBackupRootPath}", "CLEANUP")
                            End If

                        Else
                            AppLogger.Log($"Error al comprimir respaldos para {server.Name}: {zipErrorOutput}", "ERROR")
                            Console.WriteLine($"Error al comprimir respaldos para {server.Name}: {zipErrorOutput}")
                            allBackupsSuccessful = False
                        End If
                    End Using
                Else
                    AppLogger.Log($"Directorio de respaldo del servidor no encontrado para comprimir: {serverSpecificBackupRootPath}", "ADVERTENCIA")
                End If
            End If

            Return allBackupsSuccessful

        Catch ex As Exception
            AppLogger.Log($"Excepción durante el respaldo para {server.Name}: {ex.Message}", "ERROR")
            Console.WriteLine($"Excepción durante el respaldo para {server.Name}: {ex.Message}")
            Return False
        Finally
            ProgressReporter.ClearStatus()
        End Try
    End Function

    ''' <summary>
    ''' Carga la configuración de los servidores desde un archivo XML especificado.
    ''' </summary>
    ''' <param name="filePath">La ruta completa del archivo XML (servers.xml) desde donde cargar la configuración.</param>
    ''' <remarks>
    ''' Si el archivo no existe, se crea uno por defecto con una configuración de ejemplo.
    ''' También carga la configuración global como las rutas de mysqldump y 7-Zip, y si las contraseñas deben encriptarse.
    ''' </remarks>
    Public Sub LoadServers(ByVal filePath As String)
        AppLogger.Log($"Intentando cargar la configuración de servidores desde {filePath}", "CONFIG")
        Console.WriteLine($"Cargando configuración de servidores desde {filePath}...")

        If Not File.Exists(filePath) Then
            AppLogger.Log("servers.xml no encontrado. Creando archivo por defecto.", "ADVERTENCIA")
            Console.WriteLine("servers.xml no encontrado. Creando archivo por defecto...")
            Dim defaultXml As String = "<?xml version=""1.0"" encoding=""utf-8"" ?>" & Environment.NewLine &
                                       "<Servers>" & Environment.NewLine &
                                       "  <GlobalSettings>" & Environment.NewLine &
                                       "    <MySqlDumpPath>C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe</MySqlDumpPath>" & Environment.NewLine &
                                       "    <BackupDestinationPath>C:\MySQL_Backups</BackupDestinationPath>" & Environment.NewLine &
                                       "    <SevenZipPath>C:\Program Files\7-Zip\7z.exe</SevenZipPath>" & Environment.NewLine &
                                       "    <EncryptPasswords>true</EncryptPasswords>" & Environment.NewLine &
                                       "  </GlobalSettings>" & Environment.NewLine &
                                       "  <Server>" & Environment.NewLine &
                                       "    <Name>Servidor de Ejemplo</Name>" & Environment.NewLine &
                                       "    <IP>127.0.0.1</IP>" & Environment.NewLine &
                                       "    <Port>3306</Port>" & Environment.NewLine &
                                       "    <User>root</User>" & Environment.NewLine &
                                       "    <Password>password_placeholder</Password>" & Environment.NewLine &
                                       "    <Databases>" & Environment.NewLine &
                                       "    </Databases>" & Environment.NewLine &
                                       "    <ExcludedDatabases>" & Environment.NewLine &
                                       "      <Database>information_schema</Database>" & Environment.NewLine &
                                       "      <Database>performance_schema</Database>" & Environment.NewLine &
                                       "      <Database>sys</Database>" & Environment.NewLine &
                                       "      <Database>mysql</Database>" & Environment.NewLine &
                                       "    </ExcludedDatabases>" & Environment.NewLine &
                                       "    <Schedule>" & Environment.NewLine &
                                       "      <Enabled>false</Enabled>" & Environment.NewLine &
                                       "      <Days>0,1,2,3,4,5,6</Days>" & Environment.NewLine &
                                       "      <Time>22:00</Time>" & Environment.NewLine &
                                       "    </Schedule>" & Environment.NewLine &
                                       "    <OmitBackupsInWindow>false</OmitBackupsInWindow>" & Environment.NewLine &
                                       "    <WindowStartTime>00:00:00</WindowStartTime>" & Environment.NewLine &
                                       "    <WindowEndTime>00:00:00</WindowEndTime>" & Environment.NewLine &
                                       "  </Server>" & Environment.NewLine &
                                       "</Servers>"
            File.WriteAllText(filePath, defaultXml)
            AppLogger.Log("servers.xml por defecto creado.", "INFO")
        End If

        Try
            Dim doc As New Xml.XmlDocument()
            doc.Load(filePath)

            ' Load global settings
            Dim globalSettingsNode As Xml.XmlNode = doc.SelectSingleNode("/Servers/GlobalSettings")
            If globalSettingsNode IsNot Nothing Then
                Me.MySqlDumpPath = globalSettingsNode.SelectSingleNode("MySqlDumpPath").InnerText
                Me.BackupDestinationPath = globalSettingsNode.SelectSingleNode("BackupDestinationPath").InnerText
                Me.SevenZipPath = globalSettingsNode.SelectSingleNode("SevenZipPath").InnerText
                Dim encryptNode As Xml.XmlNode = globalSettingsNode.SelectSingleNode("EncryptPasswords")
                If encryptNode IsNot Nothing Then
                    Me.EncryptPasswords = Boolean.Parse(encryptNode.InnerText)
                Else
                    Me.EncryptPasswords = False ' Valor por defecto si no se encuentra
                End If
            Else
                AppLogger.Log("No se encontró la sección GlobalSettings en servers.xml. Usando valores por defecto.", "ADVERTENCIA")
                ' Set default values if GlobalSettings section is missing
                Me.MySqlDumpPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
                Me.BackupDestinationPath = "C:\MySQL_Backups"
                Me.SevenZipPath = "C:\Program Files\7-Zip\7z.exe"
            End If

            _servers.Clear()
            For Each serverNode As Xml.XmlNode In doc.SelectNodes("/Servers/Server")
                Dim server As New Server()
                server.Name = serverNode.SelectSingleNode("Name").InnerText
                server.IP = serverNode.SelectSingleNode("IP").InnerText
                server.Port = Integer.Parse(serverNode.SelectSingleNode("Port").InnerText)
                server.User = serverNode.SelectSingleNode("User").InnerText
                server.Password = serverNode.SelectSingleNode("Password").InnerText

                For Each dbNode As Xml.XmlNode In serverNode.SelectNodes("Databases/Database")
                    server.Databases.Add(dbNode.InnerText)
                Next

                For Each excludedDbNode As Xml.XmlNode In serverNode.SelectNodes("ExcludedDatabases/Database")
                    server.ExcludedDatabases.Add(excludedDbNode.InnerText)
                Next

                Dim scheduleNode As Xml.XmlNode = serverNode.SelectSingleNode("Schedule")
                If scheduleNode IsNot Nothing Then
                    server.Schedule.Enabled = Boolean.Parse(scheduleNode.SelectSingleNode("Enabled").InnerText)
                    Dim daysList As New List(Of Integer)
                    For Each dayStr As String In scheduleNode.SelectSingleNode("Days").InnerText.Split(","c)
                        Dim dayNum As Integer
                        If Integer.TryParse(dayStr.Trim(), dayNum) Then
                            daysList.Add(dayNum)
                        End If
                    Next
                    server.Schedule.Days = daysList
                    server.Schedule.Time = scheduleNode.SelectSingleNode("Time").InnerText
                End If

                Dim omitNode As Xml.XmlNode = serverNode.SelectSingleNode("OmitBackupsInWindow")
                If omitNode IsNot Nothing Then
                    server.OmitirRespaldosVentana = Boolean.Parse(omitNode.InnerText)
                    server.InicioVentana = TimeSpan.Parse(serverNode.SelectSingleNode("WindowStartTime").InnerText)
                    server.FinVentana = TimeSpan.Parse(serverNode.SelectSingleNode("WindowEndTime").InnerText)
                End If

                Dim isEncryptedNode As Xml.XmlNode = serverNode.SelectSingleNode("IsPasswordEncrypted")
                If isEncryptedNode IsNot Nothing Then
                    server.IsPasswordEncrypted = Boolean.Parse(isEncryptedNode.InnerText)
                Else
                    server.IsPasswordEncrypted = False ' Valor por defecto si no se encuentra
                End If

                ' Cargar eventos de respaldo
                Dim eventosNode As Xml.XmlNode = serverNode.SelectSingleNode("Eventos")
                If eventosNode IsNot Nothing Then
                    For Each eventoNode As Xml.XmlNode In eventosNode.SelectNodes("Evento")
                        Try
                            Dim evento As New EventoRespaldo()
                            Dim fechaHoraString As String = eventoNode.SelectSingleNode("FechaHora").InnerText
                            Dim fechaHora As DateTime
                            If DateTime.TryParse(fechaHoraString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, fechaHora) Then
                                evento.FechaHora = fechaHora
                                evento.Descripcion = eventoNode.SelectSingleNode("Descripcion").InnerText
                                server.Eventos.Add(evento)
                            Else
                                AppLogger.Log($"Formato de FechaHora inválido ('{fechaHoraString}') para un evento en el servidor {server.Name}. Omitiendo evento.", "ADVERTENCIA")
                            End If
                        Catch ex As Exception
                            AppLogger.Log($"Error al procesar un evento para el servidor {server.Name}: {ex.Message}. Omitiendo evento.", "ERROR")
                        End Try
                    Next
                End If

                _servers.Add(server)
            Next

            AppLogger.Log($"Se cargaron {_servers.Count} servidores con éxito.", "CONFIG")
            Console.WriteLine($"Se cargaron {_servers.Count} servidores.")

        Catch ex As Exception
            AppLogger.Log($"Error al cargar servidores: {ex.Message}", "ERROR")
            Console.WriteLine($"Error al cargar servidores: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene la lista actual de servidores configurados.
    ''' </summary>
    ''' <returns>Una lista de objetos <see cref="Server"/>.</returns>
    Public Function GetServers() As List(Of Server)
        Return _servers
    End Function

    ''' <summary>
    ''' Establece la lista de servidores configurados.
    ''' </summary>
    ''' <param name="newServers">La nueva lista de objetos <see cref="Server"/> a establecer.</param>
    Public Sub SetServers(ByVal newServers As List(Of Server))
        _servers = newServers
    End Sub

    ''' <summary>
    ''' Guarda la configuración actual de los servidores en un archivo XML especificado.
    ''' </summary>
    ''' <param name="filePath">La ruta completa del archivo XML (servers.xml) donde guardar la configuración.</param>
    ''' <remarks>
    ''' Este método sobrescribe el archivo existente con la configuración actual de los servidores y los ajustes globales.
    ''' </remarks>
    Public Sub SaveServers(ByVal filePath As String)
        AppLogger.Log($"Guardando configuración de servidores en {filePath}. Número de servidores: {_servers.Count}", "CONFIG")

        Dim doc As New XmlDocument()

        If File.Exists(filePath) Then
            doc.Load(filePath)
        Else
            ' If file doesn't exist, create a new one with root element
            Dim declarationNode As XmlNode = doc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
            doc.AppendChild(declarationNode)
            doc.AppendChild(doc.CreateElement("Servers"))
        End If

        Dim serversNode As XmlNode = doc.SelectSingleNode("/Servers")
        If serversNode Is Nothing Then
            serversNode = doc.CreateElement("Servers")
            doc.AppendChild(serversNode)
        End If

        ' Find or create GlobalSettings node
        Dim globalSettingsNode As XmlNode = serversNode.SelectSingleNode("GlobalSettings")
        If globalSettingsNode Is Nothing Then
            globalSettingsNode = doc.CreateElement("GlobalSettings")
            serversNode.InsertBefore(globalSettingsNode, serversNode.FirstChild) ' Add at the beginning
        End If

        ' Update or create MySqlDumpPath
        Dim mySqlDumpPathNode As XmlNode = globalSettingsNode.SelectSingleNode("MySqlDumpPath")
        If mySqlDumpPathNode Is Nothing Then
            mySqlDumpPathNode = doc.CreateElement("MySqlDumpPath")
            globalSettingsNode.AppendChild(mySqlDumpPathNode)
        End If
        mySqlDumpPathNode.InnerText = Me.MySqlDumpPath

        ' Update or create BackupDestinationPath
        Dim backupDestinationPathNode As XmlNode = globalSettingsNode.SelectSingleNode("BackupDestinationPath")
        If backupDestinationPathNode Is Nothing Then
            backupDestinationPathNode = doc.CreateElement("BackupDestinationPath")
            globalSettingsNode.AppendChild(backupDestinationPathNode)
        End If
        backupDestinationPathNode.InnerText = Me.BackupDestinationPath

        ' Update or create SevenZipPath
        Dim sevenZipPathNode As XmlNode = globalSettingsNode.SelectSingleNode("SevenZipPath")
        If sevenZipPathNode Is Nothing Then
            sevenZipPathNode = doc.CreateElement("SevenZipPath")
            globalSettingsNode.AppendChild(sevenZipPathNode)
        End If
        sevenZipPathNode.InnerText = Me.SevenZipPath

        ' Update or create EncryptPasswords
        Dim encryptPasswordsNode As Xml.XmlNode = globalSettingsNode.SelectSingleNode("EncryptPasswords")
        If encryptPasswordsNode Is Nothing Then
            encryptPasswordsNode = doc.CreateElement("EncryptPasswords")
            globalSettingsNode.AppendChild(encryptPasswordsNode)
        End If
        encryptPasswordsNode.InnerText = Me.EncryptPasswords.ToString()

        ' Remove existing Server nodes before adding current ones
        For Each existingServerNode As Xml.XmlNode In serversNode.SelectNodes("Server")
            serversNode.RemoveChild(existingServerNode)
        Next

        For Each server As Server In _servers
            Dim serverNode As XmlNode = doc.CreateElement("Server")

            Dim nameNode = doc.CreateElement("Name")
            nameNode.InnerText = server.Name
            serverNode.AppendChild(nameNode)

            Dim ipNode = doc.CreateElement("IP")
            ipNode.InnerText = server.IP
            serverNode.AppendChild(ipNode)

            Dim portNode = doc.CreateElement("Port")
            portNode.InnerText = server.Port.ToString()
            serverNode.AppendChild(portNode)

            Dim userNode = doc.CreateElement("User")
            userNode.InnerText = server.User
            serverNode.AppendChild(userNode)

            Dim passwordNode = doc.CreateElement("Password")
            passwordNode.InnerText = server.Password
            serverNode.AppendChild(passwordNode)

            Dim dbsNode As XmlNode = doc.CreateElement("Databases")
            For Each dbName As String In server.Databases
                Dim dbNode = doc.CreateElement("Database")
                dbNode.InnerText = dbName
                dbsNode.AppendChild(dbNode)
            Next
            serverNode.AppendChild(dbsNode)

            Dim excludedDbsNode As XmlNode = doc.CreateElement("ExcludedDatabases")
            For Each excludedDbName As String In server.ExcludedDatabases
                Dim dbNode As XmlNode = doc.CreateElement("Database")
                dbNode.InnerText = excludedDbName
                excludedDbsNode.AppendChild(dbNode)
            Next
            serverNode.AppendChild(excludedDbsNode)

            Dim scheduleNode As Xml.XmlNode = doc.CreateElement("Schedule")
            Dim enabledNode = doc.CreateElement("Enabled")
            enabledNode.InnerText = server.Schedule.Enabled.ToString()
            scheduleNode.AppendChild(enabledNode)

            Dim daysNode = doc.CreateElement("Days")
            daysNode.InnerText = String.Join(",", server.Schedule.Days)
            scheduleNode.AppendChild(daysNode)

            Dim timeNode = doc.CreateElement("Time")
            timeNode.InnerText = server.Schedule.Time
            scheduleNode.AppendChild(timeNode)

            serverNode.AppendChild(scheduleNode)

            Dim omitNode = doc.CreateElement("OmitBackupsInWindow")
            omitNode.InnerText = server.OmitirRespaldosVentana.ToString()
            serverNode.AppendChild(omitNode)

            Dim inicioNode = doc.CreateElement("WindowStartTime")
            inicioNode.InnerText = server.InicioVentana.ToString()
            serverNode.AppendChild(inicioNode)

            Dim finNode = doc.CreateElement("WindowEndTime")
            finNode.InnerText = server.FinVentana.ToString()
            serverNode.AppendChild(finNode)

            Dim isEncryptedNode = doc.CreateElement("IsPasswordEncrypted")
            isEncryptedNode.InnerText = server.IsPasswordEncrypted.ToString()
            serverNode.AppendChild(isEncryptedNode)

            ' Guardar eventos de respaldo
            Dim eventosNode As Xml.XmlNode = doc.CreateElement("Eventos")
            For Each evento As EventoRespaldo In server.Eventos
                Dim eventoNode As Xml.XmlNode = doc.CreateElement("Evento")

                Dim fechaHoraNode = doc.CreateElement("FechaHora")
                fechaHoraNode.InnerText = evento.FechaHora.ToString("o") ' ISO 8601 format
                eventoNode.AppendChild(fechaHoraNode)

                Dim descripcionNode = doc.CreateElement("Descripcion")
                descripcionNode.InnerText = evento.Descripcion
                eventoNode.AppendChild(descripcionNode)

                eventosNode.AppendChild(eventoNode)
            Next
            serverNode.AppendChild(eventosNode)

            serversNode.AppendChild(serverNode)
        Next

        doc.Save(filePath)
        AppLogger.Log("Configuración de servidores guardada en servers.xml.", "UI")
    End Sub

    ''' <summary>
    ''' Añade un nuevo servidor o actualiza uno existente en la colección gestionada.
    ''' </summary>
    ''' <param name="server">El objeto <see cref="Server"/> a añadir o actualizar.</param>
    ''' <remarks>
    ''' Si ya existe un servidor con el mismo nombre, sus propiedades se actualizarán.
    ''' Para que los cambios persistan, se debe llamar al método <see cref="SaveServers"/>.
    ''' </remarks>
    Public Sub AddOrUpdateServer(server As Server)
        Dim existingServer = _servers.FirstOrDefault(Function(s) s.Name = server.Name)
        If existingServer IsNot Nothing Then
            ' Update existing server
            existingServer.IP = server.IP
            existingServer.Port = server.Port
            existingServer.User = server.User
            existingServer.Password = server.Password
            existingServer.IsPasswordEncrypted = server.IsPasswordEncrypted
            existingServer.Databases = server.Databases
            existingServer.ExcludedDatabases = server.ExcludedDatabases
            existingServer.Schedule = server.Schedule
            existingServer.OmitirRespaldosVentana = server.OmitirRespaldosVentana
            existingServer.InicioVentana = server.InicioVentana
            existingServer.FinVentana = server.FinVentana
            existingServer.Eventos = server.Eventos
        Else
            ' Add new server
            _servers.Add(server)
        End If
    End Sub

    ''' <summary>
    ''' Elimina un servidor de la colección gestionada por su nombre.
    ''' </summary>
    ''' <param name="serverName">El nombre del servidor a eliminar.</param>
    ''' <remarks>
    ''' Si se encuentra el servidor, se elimina.
    ''' Para que los cambios persistan, se debe llamar al método <see cref="SaveServers"/>.
    ''' </remarks>
    Public Sub RemoveServer(serverName As String)
        Dim serverToRemove = _servers.FirstOrDefault(Function(s) s.Name = serverName)
        If serverToRemove IsNot Nothing Then
            _servers.Remove(serverToRemove)
        End If
    End Sub


    ''' <summary>
    ''' Verifica si hay respaldos programados que se hayan omitido y, si es necesario, inicia nuevos respaldos.
    ''' </summary>
    ''' <remarks>
    ''' Este método carga la configuración de los servidores y compara la última hora de respaldo con la hora programada.
    ''' Considera la ventana de omisión de respaldos configurada para cada servidor.
    ''' </remarks>
    Public Sub CheckForMissedBackups()
        AppLogger.Log("Verificando respaldos omitidos...", "INFO")
        LoadServers("servers.xml") ' Asegúrate de que la ruta sea accesible

        For Each server As Server In _servers
            If Not server.Schedule.Enabled Then Continue For

            Dim lastScheduledBackupTime As DateTime = GetLastScheduledBackupTime(server.Schedule)
            If lastScheduledBackupTime = DateTime.MinValue Then Continue For

            Dim databasesToBackup As New List(Of String)
            If server.Databases.Any() Then
                databasesToBackup.AddRange(server.Databases)
            Else
                ' Aquí iría la lógica para obtener todas las bases de datos si no se especifica ninguna.
                ' Por ahora, se asume que las bases de datos a respaldar están explícitamente listadas.
            End If

            For Each dbName As String In databasesToBackup
                Dim lastBackupTime As DateTime = GetLastBackupTime(server, dbName)

                If lastBackupTime < lastScheduledBackupTime Then
                    AppLogger.Log($"Respaldo para {server.Name}\{dbName} está vencido. Último respaldo: {lastBackupTime}. Respaldo programado: {lastScheduledBackupTime}.", "INFO")

                    Dim omitir As Boolean = False
                    If server.OmitirRespaldosVentana Then
                        Dim ahora As TimeSpan = DateTime.Now.TimeOfDay
                        Dim inicio As TimeSpan = server.InicioVentana
                        Dim fin As TimeSpan = server.FinVentana

                        If inicio <= fin Then
                            ' Ventana en el mismo día (ej. 09:00 a 17:00)
                            If ahora >= inicio AndAlso ahora <= fin Then
                                omitir = True
                            End If
                        Else
                            ' Ventana que cruza la medianoche (ej. 22:00 a 06:00)
                            If ahora >= inicio OrElse ahora <= fin Then
                                omitir = True
                            End If
                        End If
                    End If

                    If omitir Then
                        AppLogger.Log($"Se omite el respaldo para {server.Name}\{dbName} porque la hora actual está dentro de la ventana de omisión ({server.InicioVentana} - {server.FinVentana}).", "INFO")
                    Else
                        AppLogger.Log($"Iniciando nuevo respaldo para {server.Name}\{dbName}.", "INFO")
                        ' Aquí se llamaría al método real de respaldo.
                        ' BackupDatabase(server, dbName)
                    End If
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Obtiene la fecha y hora del último respaldo exitoso para una base de datos específica de un servidor.
    ''' </summary>
    ''' <param name="server">El objeto <see cref="Server"/> para el cual se busca el respaldo.</param>
    ''' <param name="dbName">El nombre de la base de datos.</param>
    ''' <returns>Un objeto DateTime que representa la fecha y hora del último respaldo, o DateTime.MinValue si no se encuentra ninguno.</returns>
    Private Function GetLastBackupTime(server As Server, dbName As String) As DateTime
        Dim serverSpecificPath As String = Path.Combine(BackupDestinationPath, $"{server.Name}_{server.IP}")
        Dim dbSpecificPath As String = Path.Combine(serverSpecificPath, dbName)

        If Not Directory.Exists(dbSpecificPath) Then
            Return DateTime.MinValue
        End If

        Dim directoryInfo As New DirectoryInfo(dbSpecificPath)
        Dim lastBackupFile = directoryInfo.GetFiles("*.zip").OrderByDescending(Function(f) f.LastWriteTime).FirstOrDefault()

        If lastBackupFile IsNot Nothing Then
            Return lastBackupFile.LastWriteTime
        Else
            Return DateTime.MinValue
        End If
    End Function

    ''' <summary>
    ''' Calcula la fecha y hora del último respaldo que debería haberse programado según la configuración.
    ''' </summary>
    ''' <param name="schedule">El objeto <see cref="ScheduleInfo"/> que contiene la información de la programación.</param>
    ''' <returns>Un objeto DateTime que representa la última hora de respaldo programada, o DateTime.MinValue si la programación es inválida o no se encuentra un día programado en la última semana.</returns>
    Private Function GetLastScheduledBackupTime(schedule As ScheduleInfo) As DateTime
        Dim now As DateTime = DateTime.Now
        Dim scheduleTime As TimeSpan
        If Not TimeSpan.TryParse(schedule.Time, scheduleTime) Then
            Return DateTime.MinValue ' Formato de hora inválido
        End If

        ' Primero, comprobar si el respaldo estaba programado para hoy
        If schedule.Days.Contains(CInt(now.DayOfWeek)) AndAlso now.TimeOfDay >= scheduleTime Then
            Return now.Date + scheduleTime
        End If

        ' Si no, buscar en los 7 días anteriores
        For i As Integer = 1 To 7
            Dim prevDay As DateTime = now.AddDays(-i)
            If schedule.Days.Contains(CInt(prevDay.DayOfWeek)) Then
                Return prevDay.Date + scheduleTime
            End If
        Next

        Return DateTime.MinValue ' No se encontró un día programado en la última semana
    End Function
End Class


''' <summary>
''' Representa la información de configuración de un servidor MySQL.
''' </summary>
''' <remarks>
''' Contiene detalles como nombre, IP, puerto, usuario, contraseña, bases de datos a incluir/excluir,
''' y la configuración de programación y omisión de respaldos.
''' </remarks>
Public Class Server
    ''' <summary>
    ''' Obtiene o establece el nombre amigable del servidor.
    ''' </summary>
    Public Property Name As String
    ''' <summary>
    ''' Obtiene o establece la dirección IP o el nombre de host del servidor MySQL.
    ''' </summary>
    Public Property IP As String
    ''' <summary>
    ''' Obtiene o establece el número de puerto del servidor MySQL.
    ''' </summary>
    Public Property Port As Integer
    ''' <summary>
    ''' Obtiene o establece el nombre de usuario para la conexión al servidor MySQL.
    ''' </summary>
    Public Property User As String
    ''' <summary>
    ''' Obtiene o establece la contraseña para la conexión al servidor MySQL.
    ''' </summary>
    Public Property Password As String
    ''' <summary>
    ''' Obtiene la lista de bases de datos específicas a respaldar para este servidor.
    ''' Si está vacía, se respaldarán todas las bases de datos (excepto las excluidas).
    ''' </summary>
    Public Property Databases As New List(Of String)
    ''' <summary>
    ''' Obtiene la lista de bases de datos a excluir del respaldo para este servidor.
    ''' </summary>
    Public Property ExcludedDatabases As New List(Of String)
    ''' <summary>
    ''' Obtiene o establece la información de programación para los respaldos de este servidor.
    ''' </summary>
    Public Property Schedule As New ScheduleInfo
    ''' <summary>
    ''' Obtiene o establece un valor que indica si los respaldos deben omitirse dentro de una ventana de tiempo específica.
    ''' </summary>
    Public Property OmitirRespaldosVentana As Boolean
    ''' <summary>
    ''' Obtiene o establece la hora de inicio de la ventana de omisión de respaldos.
    ''' </summary>
    Public Property InicioVentana As TimeSpan
    ''' <summary>
    ''' Obtiene o establece la hora de fin de la ventana de omisión de respaldos.
    ''' </summary>
    Public Property FinVentana As TimeSpan
    ''' <summary>
    ''' Obtiene o establece un valor que indica si la contraseña de este servidor está encriptada.
    ''' </summary>
    Public Property IsPasswordEncrypted As Boolean
    ''' <summary>
    ''' Obtiene la lista de respaldos por evento programados para este servidor.
    ''' </summary>
    Public Property Eventos As New List(Of EventoRespaldo)

    ''' <summary>
    ''' Inicializa una nueva instancia de la clase <see cref="Server"/>.
    ''' </summary>
    Public Sub New()
        ' Constructor
    End Sub
End Class

''' <summary>
''' Representa la información de programación para un respaldo.
''' </summary>
''' <remarks>
''' Contiene si la programación está habilitada, los días de la semana y la hora del día para el respaldo.
''' </remarks>
Public Class ScheduleInfo
    ''' <summary>
    ''' Obtiene o establece un valor que indica si la programación está habilitada.
    ''' </summary>
    Public Property Enabled As Boolean
    ''' <summary>
    ''' Obtiene o establece una lista de enteros que representan los días de la semana para la programación (0=Domingo, 6=Sábado).
    ''' </summary>
    Public Property Days As New List(Of Integer)
    ''' <summary>
    ''' Obtiene o establece la hora del día para la programación en formato HH:mm.
    ''' </summary>
    Public Property Time As String

    ''' <summary>
    ''' Inicializa una nueva instancia de la clase <see cref="ScheduleInfo"/>.
    ''' </summary>
    Public Sub New()
        ' Constructor
    End Sub
End Class

''' <summary>
''' Representa un respaldo especial programado para una fecha y hora específicas.
''' </summary>
''' <remarks>
''' Utilizado para programar respaldos únicos en respuesta a eventos como ventanas de mantenimiento.
''' </remarks>
Public Class EventoRespaldo
    ''' <summary>
    ''' Obtiene o establece la fecha y hora programada para el respaldo.
    ''' </summary>
    Public Property FechaHora As DateTime
    ''' <summary>
    ''' Obtiene o establece una descripción opcional para el evento.
    ''' </summary>
    Public Property Descripcion As String

    ''' <summary>
    ''' Inicializa una nueva instancia de la clase <see cref="EventoRespaldo"/>.
    ''' </summary>
    Public Sub New()
        ' Constructor
    End Sub
End Class
