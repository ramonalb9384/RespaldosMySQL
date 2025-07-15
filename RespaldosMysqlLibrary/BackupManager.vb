Imports System.Xml
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports System.Threading

Public Class BackupManager

    Private _servers As New List(Of Server)

    Public Property MySqlDumpPath As String
    Public Property BackupDestinationPath As String
    Public Property SevenZipPath As String
    Public Property EncryptPasswords As Boolean
    Public Property DeepLoggingEnabled As Boolean

    Public Function IsInputSafe(input As String) As Boolean
        Dim unsafeCharsPattern As String = "[&|;$><`\!]"
        If Regex.IsMatch(input, unsafeCharsPattern) Then
            Return False
        End If
        Return True
    End Function

    Public Function PerformBackup(server As Server, backupPath As String, isAutomatic As Boolean) As Boolean
        AppLogger.Log($"Iniciando PerformBackup para '{server.Name}' (Automático: {isAutomatic})", "DEEP_TRACE")

        If Server.NtfyEnabled AndAlso Not String.IsNullOrWhiteSpace(Server.NtfyTopic) Then
            NotificationManager.SendNtfyNotification(Server.NtfyTopic, $"Iniciando respaldo para '{Server.Name}' a las {DateTime.Now:HH:mm:ss}.", "Inicio de Respaldo", 3, New String() {"hourglass"})
        End If

        AppLogger.Log($"Iniciando respaldo para el servidor: {Server.Name} (Automático: {isAutomatic})", "BACKUP")

        If Not IsInputSafe(Server.Name) OrElse Not IsInputSafe(Server.IP) OrElse Not IsInputSafe(Server.User) Then
            AppLogger.Log($"ALERTA DE SEGURIDAD: Caracteres no válidos en la configuración del servidor '{Server.Name}'. Abortando.", "ERROR")
            Return False
        End If

        Try
            Dim allDatabases As New List(Of String)
            Dim connBuilder As New MySqlConnectionStringBuilder() With {
                .Server = Server.IP,
                .Port = Server.Port,
                .UserID = Server.User
            }

            Dim passwordToUse As String
            If Server.IsPasswordEncrypted Then
                Try
                    passwordToUse = EncryptionHelper.Decrypt(Server.Password)
                Catch ex As Exception
                    AppLogger.Log($"Error al desencriptar la contraseña para '{Server.Name}': {ex.Message}", "ERROR")
                    SendBackupNotification(Server, False)
                    Return False
                End Try
            Else
                passwordToUse = Server.Password
            End If
            connBuilder.Password = passwordToUse

            AppLogger.Log($"Intentando conectar a MySQL en {Server.IP}:{Server.Port}...", "DEEP_TRACE")
            Try
                Using connection As New MySqlConnection(connBuilder.ConnectionString)
                    connection.Open()
                    AppLogger.Log("Conexión exitosa. Obteniendo lista de bases de datos...", "DEEP_TRACE")
                    Dim xcommand As New MySqlCommand("SHOW DATABASES;", connection)
                    Using reader As MySqlDataReader = xcommand.ExecuteReader()
                        While reader.Read()
                            Dim dbName As String = reader.GetString(0)
                            If Not dbName.Equals("Database", StringComparison.OrdinalIgnoreCase) Then
                                allDatabases.Add(dbName)
                            End If
                        End While
                    End Using
                    AppLogger.Log($"Se encontraron {allDatabases.Count} bases de datos: {String.Join(", ", allDatabases)}", "DEEP_TRACE")
                End Using
            Catch ex As Exception
                AppLogger.Log($"Error de conexión o consulta a MySQL para '{Server.Name}': {ex.Message}", "ERROR")
                SendBackupNotification(Server, False)
                Return False
            End Try

            Dim excludedDbs = Server.ExcludedDatabases.Select(Function(s) s.ToLowerInvariant()).ToHashSet()
            Dim databasesToBackup = allDatabases.Where(Function(db) Not excludedDbs.Contains(db.ToLowerInvariant())).ToList()
            AppLogger.Log($"Bases de datos a respaldar después de exclusión: {String.Join(", ", databasesToBackup)}", "DEEP_TRACE")

            Dim allBackupsSuccessful As Boolean = True
            For i As Integer = 0 To databasesToBackup.Count - 1
                Dim dbName As String = databasesToBackup(i)
                If Not IsInputSafe(dbName) Then
                    AppLogger.Log($"ALERTA DE SEGURIDAD: Caracteres no válidos en el nombre de la BD '{dbName}'. Omitiendo.", "ERROR")
                    Continue For
                End If

                Dim timestamp As String = DateTime.Now.ToString("yyMMdd_HHmm")
                Dim specificBackupFileName As String = $"{dbName}_{timestamp}.sql"
                Dim serverSpecificPath As String = Path.Combine(backupPath, $"{Server.Name}_{Server.IP}")
                Dim dbSpecificPath As String = Path.Combine(serverSpecificPath, dbName)
                Dim fullSpecificBackupPath As String = Path.Combine(dbSpecificPath, specificBackupFileName)

                If Not Directory.Exists(dbSpecificPath) Then
                    Directory.CreateDirectory(dbSpecificPath)
                    AppLogger.Log($"Directorio creado: {dbSpecificPath}", "DEEP_TRACE")
                End If

                Dim command As String = $"""{MySqlDumpPath}"" -h {Server.IP} -P {Server.Port} -u {Server.User} --password={passwordToUse} {Server.Parameters} {dbName} > ""{fullSpecificBackupPath}"""
                Dim commandForLog As String = $"""{MySqlDumpPath}"" -h {Server.IP} -P {Server.Port} -u {Server.User} --password=***** {Server.Parameters} {dbName} > ""{fullSpecificBackupPath}"""
                AppLogger.Log($"Ejecutando mysqldump para '{dbName}': {commandForLog}", "DEEP_TRACE")

                Dim processInfo As New ProcessStartInfo("cmd.exe") With {
                    .Arguments = $"/C ""{command}""",
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                Using process As Process = Process.Start(processInfo)
                    process.WaitForExit()
                    Dim errorOutput As String = process.StandardError.ReadToEnd()
                    If process.ExitCode = 0 Then
                        AppLogger.Log($"Respaldo de '{dbName}' exitoso.", "DEEP_TRACE")
                    Else
                        AppLogger.Log($"Error en mysqldump para '{dbName}' en '{Server.Name}': {errorOutput}", "ERROR")
                        allBackupsSuccessful = False
                    End If
                End Using
            Next

            If allBackupsSuccessful Then
                Dim zipFileName As String = $"{Server.Name}_{Server.IP}_{DateTime.Now.ToString("yyMMdd_HHmm")}.zip"
                Dim serverSpecificBackupRootPath As String = Path.Combine(backupPath, $"{Server.Name}_{Server.IP}")
                Dim fullZipFilePath As String = Path.Combine(serverSpecificBackupRootPath, zipFileName)

                If Directory.Exists(serverSpecificBackupRootPath) Then
                    Dim zipCommand As String = $"""{SevenZipPath}"" a -tzip ""{fullZipFilePath}"" ""{Path.Combine(serverSpecificBackupRootPath, "*")}"" -x!*.zip"
                    AppLogger.Log($"Ejecutando 7-Zip: {zipCommand}", "DEEP_TRACE")

                    Dim zipProcessInfo As New ProcessStartInfo("cmd.exe") With {
                        .Arguments = $"/C ""{zipCommand}""",
                        .RedirectStandardOutput = True,
                        .RedirectStandardError = True,
                        .UseShellExecute = False,
                        .CreateNoWindow = True
                    }

                    Using zipProcess As Process = Process.Start(zipProcessInfo)
                        zipProcess.WaitForExit()
                        Dim zipErrorOutput As String = zipProcess.StandardError.ReadToEnd()
                        If zipProcess.ExitCode = 0 Then
                            AppLogger.Log($"Compresión exitosa: {fullZipFilePath}", "DEEP_TRACE")
                            AppLogger.Log("Iniciando limpieza de archivos .sql y carpetas.", "DEEP_TRACE")
                            For Each dbName As String In databasesToBackup
                                If Not IsInputSafe(dbName) Then Continue For
                                Dim dbSpecificPath As String = Path.Combine(serverSpecificBackupRootPath, dbName)
                                If Directory.Exists(dbSpecificPath) Then
                                    Directory.Delete(dbSpecificPath, True)
                                    AppLogger.Log($"Directorio de BD eliminado: {dbSpecificPath}", "DEEP_TRACE")
                                End If
                            Next
                        Else
                            AppLogger.Log($"Error al comprimir para '{Server.Name}': {zipErrorOutput}", "ERROR")
                            allBackupsSuccessful = False
                        End If
                    End Using
                End If
            End If

            SendBackupNotification(Server, allBackupsSuccessful)
            AppLogger.Log($"PerformBackup para '{Server.Name}' finalizado. Éxito: {allBackupsSuccessful}", "DEEP_TRACE")
            Return allBackupsSuccessful

        Catch ex As Exception
            AppLogger.Log($"Excepción mayor en PerformBackup para '{Server.Name}': {ex.Message}", "ERROR")
            SendBackupNotification(Server, False)
            Return False
        End Try
    End Function

    Private Sub SendBackupNotification(server As Server, success As Boolean)
        If server.NtfyEnabled AndAlso Not String.IsNullOrWhiteSpace(server.NtfyTopic) Then
            Dim title = If(success, $"Respaldo Exitoso: {server.Name}", $"Error de Respaldo: {server.Name}")
            Dim message = If(success, $"El respaldo del servidor '{server.Name}' ({server.IP}) se completó correctamente.", $"Error en el respaldo del servidor '{server.Name}' ({server.IP}). Revise los logs.")
            Dim priority = If(success, 4, 5)
            Dim tags = If(success, New String() {"white_check_mark"}, New String() {"x"})
            NotificationManager.SendNtfyNotification(server.NtfyTopic, message, title, priority, tags)
        End If
    End Sub

    Public Sub LoadServers(ByVal filePath As String)
        AppLogger.Log($"Iniciando LoadServers desde {filePath}", "DEEP_TRACE")

        If Not File.Exists(filePath) Then
            AppLogger.Log("servers.xml no encontrado. Creando archivo por defecto.", "ADVERTENCIA")
            CreateDefaultServersFile(filePath)
        End If

        Try
            Dim doc As New Xml.XmlDocument()
            doc.Load(filePath)

            Dim globalSettingsNode As Xml.XmlNode = doc.SelectSingleNode("/Servers/GlobalSettings")
            If globalSettingsNode IsNot Nothing Then
                Me.MySqlDumpPath = If(globalSettingsNode.SelectSingleNode("MySqlDumpPath") IsNot Nothing, globalSettingsNode.SelectSingleNode("MySqlDumpPath").InnerText, "")
                Me.BackupDestinationPath = If(globalSettingsNode.SelectSingleNode("BackupDestinationPath") IsNot Nothing, globalSettingsNode.SelectSingleNode("BackupDestinationPath").InnerText, "")
                Me.SevenZipPath = If(globalSettingsNode.SelectSingleNode("SevenZipPath") IsNot Nothing, globalSettingsNode.SelectSingleNode("SevenZipPath").InnerText, "")
                Me.EncryptPasswords = Boolean.Parse(If(globalSettingsNode.SelectSingleNode("EncryptPasswords") IsNot Nothing, globalSettingsNode.SelectSingleNode("EncryptPasswords").InnerText, "False"))
                Me.DeepLoggingEnabled = Boolean.Parse(If(globalSettingsNode.SelectSingleNode("DeepLoggingEnabled") IsNot Nothing, globalSettingsNode.SelectSingleNode("DeepLoggingEnabled").InnerText, "False"))
                AppLogger.DeepLoggingEnabled = Me.DeepLoggingEnabled
                AppLogger.Log($"Ajustes globales cargados. DeepLogging: {Me.DeepLoggingEnabled}", "DEEP_TRACE")
            Else
                AppLogger.Log("No se encontró GlobalSettings. Usando valores por defecto.", "ADVERTENCIA")
            End If

            _servers.Clear()
            For Each serverNode As Xml.XmlNode In doc.SelectNodes("/Servers/Server")
                Dim server As New Server() With {
                    .Name = If(serverNode.SelectSingleNode("Name") IsNot Nothing, serverNode.SelectSingleNode("Name").InnerText, ""),
                    .IP = If(serverNode.SelectSingleNode("IP") IsNot Nothing, serverNode.SelectSingleNode("IP").InnerText, ""),
                    .Port = Integer.Parse(If(serverNode.SelectSingleNode("Port") IsNot Nothing, serverNode.SelectSingleNode("Port").InnerText, "3306")),
                    .User = If(serverNode.SelectSingleNode("User") IsNot Nothing, serverNode.SelectSingleNode("User").InnerText, ""),
                    .Password = If(serverNode.SelectSingleNode("Password") IsNot Nothing, serverNode.SelectSingleNode("Password").InnerText, ""),
                    .Parameters = If(serverNode.SelectSingleNode("Parameters") IsNot Nothing, serverNode.SelectSingleNode("Parameters").InnerText, ""),
                    .IsPasswordEncrypted = Boolean.Parse(If(serverNode.SelectSingleNode("IsPasswordEncrypted") IsNot Nothing, serverNode.SelectSingleNode("IsPasswordEncrypted").InnerText, "False")),
                    .NtfyEnabled = Boolean.Parse(If(serverNode.SelectSingleNode("NtfyEnabled") IsNot Nothing, serverNode.SelectSingleNode("NtfyEnabled").InnerText, "False")),
                    .NtfyTopic = If(serverNode.SelectSingleNode("NtfyTopic") IsNot Nothing, serverNode.SelectSingleNode("NtfyTopic").InnerText, ""),
                    .OmitirRespaldosVentana = Boolean.Parse(If(serverNode.SelectSingleNode("OmitBackupsInWindow") IsNot Nothing, serverNode.SelectSingleNode("OmitBackupsInWindow").InnerText, "False")),
                    .InicioVentana = TimeSpan.Parse(If(serverNode.SelectSingleNode("WindowStartTime") IsNot Nothing, serverNode.SelectSingleNode("WindowStartTime").InnerText, "00:00:00")),
                    .FinVentana = TimeSpan.Parse(If(serverNode.SelectSingleNode("WindowEndTime") IsNot Nothing, serverNode.SelectSingleNode("WindowEndTime").InnerText, "00:00:00"))
                }

                For Each dbNode As Xml.XmlNode In serverNode.SelectNodes("Databases/Database")
                    server.Databases.Add(dbNode.InnerText)
                Next
                For Each excludedDbNode As Xml.XmlNode In serverNode.SelectNodes("ExcludedDatabases/Database")
                    server.ExcludedDatabases.Add(excludedDbNode.InnerText)
                Next

                Dim scheduleNode As Xml.XmlNode = serverNode.SelectSingleNode("Schedule")
                If scheduleNode IsNot Nothing Then
                    server.Schedule.Enabled = Boolean.Parse(If(scheduleNode.SelectSingleNode("Enabled") IsNot Nothing, scheduleNode.SelectSingleNode("Enabled").InnerText, "False"))
                    server.Schedule.Days = scheduleNode.SelectSingleNode("Days").InnerText.Split(","c).Select(Function(d) Integer.Parse(d.Trim())).ToList()
                    server.Schedule.Time = If(scheduleNode.SelectSingleNode("Time") IsNot Nothing, scheduleNode.SelectSingleNode("Time").InnerText, "00:00")
                End If

                Dim eventosNode As Xml.XmlNode = serverNode.SelectSingleNode("Eventos")
                If eventosNode IsNot Nothing Then
                    For Each eventoNode As Xml.XmlNode In eventosNode.SelectNodes("Evento")
                        Dim fechaHora As DateTime
                        Dim fechaHoraStr = If(eventoNode.SelectSingleNode("FechaHora") IsNot Nothing, eventoNode.SelectSingleNode("FechaHora").InnerText, "")
                        If DateTime.TryParse(fechaHoraStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, fechaHora) Then
                            server.Eventos.Add(New EventoRespaldo() With {
                                .FechaHora = fechaHora,
                                .Descripcion = If(eventoNode.SelectSingleNode("Descripcion") IsNot Nothing, eventoNode.SelectSingleNode("Descripcion").InnerText, "")
                            })
                        End If
                    Next
                End If
                _servers.Add(server)
            Next
            AppLogger.Log($"LoadServers finalizado. Se cargaron {_servers.Count} servidores.", "DEEP_TRACE")
        Catch ex As Exception
            AppLogger.Log($"Error mayor en LoadServers: {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Sub CreateDefaultServersFile(filePath As String)
        Dim defaultXml = <Servers>
                             <GlobalSettings>
                                 <MySqlDumpPath>C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe</MySqlDumpPath>
                                 <BackupDestinationPath>C:\MySQL_Backups</BackupDestinationPath>
                                 <SevenZipPath>C:\Program Files\7-Zip\7z.exe</SevenZipPath>
                                 <EncryptPasswords>true</EncryptPasswords>
                                 <DeepLoggingEnabled>false</DeepLoggingEnabled>
                             </GlobalSettings>
                             <Server>
                                 <Name>Servidor de Ejemplo</Name>
                                 <IP>127.0.0.1</IP>
                                 <Port>3306</Port>
                                 <User>root</User>
                                 <Password>password_placeholder</Password>
                                 <Parameters></Parameters>
                                 <Databases></Databases>
                                 <ExcludedDatabases>
                                     <Database>information_schema</Database>
                                     <Database>performance_schema</Database>
                                     <Database>sys</Database>
                                     <Database>mysql</Database>
                                 </ExcludedDatabases>
                                 <Schedule>
                                     <Enabled>false</Enabled>
                                     <Days>0,1,2,3,4,5,6</Days>
                                     <Time>22:00</Time>
                                 </Schedule>
                                 <OmitBackupsInWindow>false</OmitBackupsInWindow>
                                 <WindowStartTime>00:00:00</WindowStartTime>
                                 <WindowEndTime>00:00:00</WindowEndTime>
                                 <IsPasswordEncrypted>false</IsPasswordEncrypted>
                                 <NtfyEnabled>false</NtfyEnabled>
                                 <NtfyTopic></NtfyTopic>
                                 <Eventos></Eventos>
                             </Server>
                         </Servers>
        File.WriteAllText(filePath, defaultXml.ToString())
        AppLogger.Log("Archivo servers.xml por defecto creado.", "INFO")
    End Sub

    Public Sub SaveServers(ByVal filePath As String)
        AppLogger.Log($"Iniciando SaveServers en {filePath}", "DEEP_TRACE")
        Try
            Dim doc As New XmlDocument()
            Dim rootNode As XmlNode
            If File.Exists(filePath) Then
                doc.Load(filePath)
                rootNode = doc.SelectSingleNode("/Servers")
                If rootNode Is Nothing Then
                    rootNode = doc.CreateElement("Servers")
                    doc.AppendChild(rootNode)
                End If
            Else
                Dim declarationNode As XmlNode = doc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
                doc.AppendChild(declarationNode)
                rootNode = doc.CreateElement("Servers")
                doc.AppendChild(rootNode)
            End If

            Dim globalSettingsNode As XmlNode = rootNode.SelectSingleNode("GlobalSettings")
            If globalSettingsNode Is Nothing Then
                globalSettingsNode = doc.CreateElement("GlobalSettings")
                rootNode.PrependChild(globalSettingsNode)
            End If

            UpdateOrCreateElement(doc, globalSettingsNode, "MySqlDumpPath", Me.MySqlDumpPath)
            UpdateOrCreateElement(doc, globalSettingsNode, "BackupDestinationPath", Me.BackupDestinationPath)
            UpdateOrCreateElement(doc, globalSettingsNode, "SevenZipPath", Me.SevenZipPath)
            UpdateOrCreateElement(doc, globalSettingsNode, "EncryptPasswords", Me.EncryptPasswords.ToString().ToLower())
            UpdateOrCreateElement(doc, globalSettingsNode, "DeepLoggingEnabled", Me.DeepLoggingEnabled.ToString().ToLower())

            rootNode.SelectNodes("Server").Cast(Of XmlNode)().ToList().ForEach(Sub(n) n.ParentNode.RemoveChild(n))

            For Each server As Server In _servers
                Dim serverNode = doc.CreateElement("Server")
                UpdateOrCreateElement(doc, serverNode, "Name", server.Name)
                UpdateOrCreateElement(doc, serverNode, "IP", server.IP)
                UpdateOrCreateElement(doc, serverNode, "Port", server.Port.ToString())
                UpdateOrCreateElement(doc, serverNode, "User", server.User)
                UpdateOrCreateElement(doc, serverNode, "Password", server.Password)
                UpdateOrCreateElement(doc, serverNode, "Parameters", server.Parameters)
                UpdateOrCreateElement(doc, serverNode, "IsPasswordEncrypted", server.IsPasswordEncrypted.ToString().ToLower())
                UpdateOrCreateElement(doc, serverNode, "NtfyEnabled", server.NtfyEnabled.ToString().ToLower())
                UpdateOrCreateElement(doc, serverNode, "NtfyTopic", server.NtfyTopic)
                UpdateOrCreateElement(doc, serverNode, "OmitBackupsInWindow", server.OmitirRespaldosVentana.ToString().ToLower())
                UpdateOrCreateElement(doc, serverNode, "WindowStartTime", server.InicioVentana.ToString())
                UpdateOrCreateElement(doc, serverNode, "WindowEndTime", server.FinVentana.ToString())

                Dim dbsNode = doc.CreateElement("Databases")
                server.Databases.ForEach(Sub(db) UpdateOrCreateElement(doc, dbsNode, "Database", db))
                serverNode.AppendChild(dbsNode)

                Dim excludedDbsNode = doc.CreateElement("ExcludedDatabases")
                server.ExcludedDatabases.ForEach(Sub(db) UpdateOrCreateElement(doc, excludedDbsNode, "Database", db))
                serverNode.AppendChild(excludedDbsNode)

                Dim scheduleNode = doc.CreateElement("Schedule")
                UpdateOrCreateElement(doc, scheduleNode, "Enabled", server.Schedule.Enabled.ToString().ToLower())
                UpdateOrCreateElement(doc, scheduleNode, "Days", String.Join(",", server.Schedule.Days))
                UpdateOrCreateElement(doc, scheduleNode, "Time", server.Schedule.Time)
                serverNode.AppendChild(scheduleNode)

                Dim eventosNode = doc.CreateElement("Eventos")
                server.Eventos.ForEach(Sub(ev)
                                           Dim eventoNode = doc.CreateElement("Evento")
                                           UpdateOrCreateElement(doc, eventoNode, "FechaHora", ev.FechaHora.ToString("o"))
                                           UpdateOrCreateElement(doc, eventoNode, "Descripcion", ev.Descripcion)
                                           eventosNode.AppendChild(eventoNode)
                                       End Sub)
                serverNode.AppendChild(eventosNode)

                rootNode.AppendChild(serverNode)
            Next

            doc.Save(filePath)
            AppLogger.Log($"SaveServers finalizado.", "DEEP_TRACE")
        Catch ex As Exception
            AppLogger.Log($"Error mayor en SaveServers: {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Sub UpdateOrCreateElement(doc As XmlDocument, parent As XmlNode, name As String, value As String)
        Dim node = parent.SelectSingleNode(name)
        If node Is Nothing Then
            node = doc.CreateElement(name)
            parent.AppendChild(node)
        End If
        node.InnerText = value
    End Sub

    Public Sub AddOrUpdateServer(server As Server)
        Dim existingServer = _servers.FirstOrDefault(Function(s) s.Name = server.Name)
        If existingServer IsNot Nothing Then
            AppLogger.Log($"Actualizando servidor existente: '{server.Name}'", "DEEP_TRACE")
            existingServer.IP = server.IP
            existingServer.Port = server.Port
            existingServer.User = server.User
            existingServer.Password = server.Password
            existingServer.Parameters = server.Parameters
            existingServer.IsPasswordEncrypted = server.IsPasswordEncrypted
            existingServer.Databases = server.Databases
            existingServer.ExcludedDatabases = server.ExcludedDatabases
            existingServer.Schedule = server.Schedule
            existingServer.OmitirRespaldosVentana = server.OmitirRespaldosVentana
            existingServer.InicioVentana = server.InicioVentana
            existingServer.FinVentana = server.FinVentana
            existingServer.Eventos = server.Eventos
            existingServer.NtfyEnabled = server.NtfyEnabled
            existingServer.NtfyTopic = server.NtfyTopic
        Else
            AppLogger.Log($"Añadiendo nuevo servidor: '{server.Name}'", "DEEP_TRACE")
            _servers.Add(server)
        End If
    End Sub

    Public Sub RemoveServer(serverName As String)
        Dim serverToRemove = _servers.FirstOrDefault(Function(s) s.Name = serverName)
        If serverToRemove IsNot Nothing Then
            AppLogger.Log($"Eliminando servidor: '{serverName}'", "DEEP_TRACE")
            _servers.Remove(serverToRemove)
        End If
    End Sub

    Public Sub CheckForMissedBackups()
        AppLogger.Log("Iniciando CheckForMissedBackups", "DEEP_TRACE")

        For Each server As Server In _servers
            If Not server.Schedule.Enabled Then
                AppLogger.Log($"Omitiendo comprobación de respaldos omitidos para '{server.Name}' (deshabilitado).", "DEEP_TRACE")
                Continue For
            End If

            Dim lastScheduledBackupTime As DateTime = GetLastScheduledBackupTime(server.Schedule)
            If lastScheduledBackupTime = DateTime.MinValue Then
                AppLogger.Log($"No se pudo determinar la última hora programada para '{server.Name}'.", "DEEP_TRACE")
                Continue For
            End If

            Dim lastActualBackupTime As DateTime = GetLastBackupTime(server)
            AppLogger.Log($"Comprobando '{server.Name}'. Último respaldo: {lastActualBackupTime}. Último programado: {lastScheduledBackupTime}", "DEEP_TRACE")

            If lastActualBackupTime < lastScheduledBackupTime Then
                AppLogger.Log($"Respaldo para '{server.Name}' está vencido.", "INFO")

                Dim omitir As Boolean = False
                If server.OmitirRespaldosVentana Then
                    Dim ahora = DateTime.Now.TimeOfDay
                    Dim inicio = server.InicioVentana
                    Dim fin = server.FinVentana
                    If (inicio <= fin AndAlso ahora >= inicio AndAlso ahora <= fin) OrElse (inicio > fin AndAlso (ahora >= inicio OrElse ahora <= fin)) Then
                        omitir = True
                    End If
                End If

                If omitir Then
                    AppLogger.Log($"Se omite el respaldo vencido para '{server.Name}' por ventana de exclusión.", "INFO")
                Else
                    AppLogger.Log($"Iniciando respaldo omitido para '{server.Name}'.", "INFO")
                    Dim backupThread As New Thread(Sub()
                                                       Try
                                                           PerformBackup(server, Me.BackupDestinationPath, True)
                                                       Catch ex As Exception
                                                           AppLogger.Log($"Error en hilo de respaldo omitido para '{server.Name}': {ex.Message}", "ERROR")
                                                       End Try
                                                   End Sub)
                    backupThread.Start()
                End If
            Else
                AppLogger.Log($"El respaldo para '{server.Name}' está actualizado.", "DEEP_TRACE")
            End If
        Next
        AppLogger.Log("Finalizado CheckForMissedBackups", "DEEP_TRACE")
    End Sub

    Public Function GetLastBackupTime(server As Server) As DateTime
        Dim serverSpecificPath As String = Path.Combine(BackupDestinationPath, $"{server.Name}_{server.IP}")
        If Not Directory.Exists(serverSpecificPath) Then Return DateTime.MinValue

        Dim directoryInfo As New DirectoryInfo(serverSpecificPath)
        Dim lastBackupFile = directoryInfo.GetFiles("*.zip").OrderByDescending(Function(f) f.LastWriteTime).FirstOrDefault()

        Return If(lastBackupFile IsNot Nothing, lastBackupFile.LastWriteTime, DateTime.MinValue)
    End Function

    Public Function GetLastScheduledBackupTime(schedule As ScheduleInfo) As DateTime
        Dim now = DateTime.Now
        Dim scheduleTime As TimeSpan
        If Not TimeSpan.TryParse(schedule.Time, scheduleTime) Then Return DateTime.MinValue

        For i As Integer = 0 To 7
            Dim checkDay = now.AddDays(-i)
            If schedule.Days.Contains(CInt(checkDay.DayOfWeek)) Then
                Dim potentialTime = checkDay.Date + scheduleTime
                If potentialTime <= now Then
                    Return potentialTime
                End If
            End If
        Next

        Return DateTime.MinValue
    End Function

    Public Function GetServers() As List(Of Server)
        Return _servers
    End Function

    Public Sub SetServers(ByVal newServers As List(Of Server))
        _servers = newServers
    End Sub
End Class

Public Class Server
    Public Property Name As String = ""
    Public Property IP As String = ""
    Public Property Port As Integer = 3306
    Public Property User As String = ""
    Public Property Password As String = ""
    Public Property Parameters As String = ""
    Public Property Databases As New List(Of String)
    Public Property ExcludedDatabases As New List(Of String)
    Public Property Schedule As New ScheduleInfo()
    Public Property OmitirRespaldosVentana As Boolean
    Public Property InicioVentana As TimeSpan
    Public Property FinVentana As TimeSpan
    Public Property IsPasswordEncrypted As Boolean
    Public Property Eventos As New List(Of EventoRespaldo)
    Public Property NtfyEnabled As Boolean
    Public Property NtfyTopic As String = ""
End Class

Public Class ScheduleInfo
    Public Property Enabled As Boolean
    Public Property Days As New List(Of Integer)
    Public Property Time As String = "00:00"
End Class

Public Class EventoRespaldo
    Public Property FechaHora As DateTime
    Public Property Descripcion As String = ""
End Class