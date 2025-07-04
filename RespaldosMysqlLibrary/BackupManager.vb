
Imports System.Xml
Imports System.IO
Imports MySql.Data.MySqlClient

Public Class BackupManager

    Private _servers As New List(Of Server)

    Public Property MySqlDumpPath As String
    Public Property BackupDestinationPath As String
    Public Property SevenZipPath As String

    Public Function PerformBackup(server As Server, backupPath As String, isAutomatic As Boolean) As Boolean
        AppLogger.Log($"Iniciando respaldo para el servidor: {server.Name} (Automático: {isAutomatic})", "BACKUP")

        Try


            Dim allDatabases As New List(Of String)
            Dim connBuilder As New MySqlConnectionStringBuilder()
            connBuilder.Server = server.IP
            connBuilder.Port = server.Port
            connBuilder.UserID = server.User
            connBuilder.Password = server.Password
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



            Dim allBackupsSuccessful As Boolean = True

            For Each dbName As String In databasesToBackup
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

                Dim command As String = $"""{mySqlDumpPath}"" -h {server.IP} -P {server.Port} -u {server.User} --password={server.Password} {dbName} > ""{fullSpecificBackupPath}"""

                AppLogger.Log($"Ejecutando comando mysqldump para {dbName}: {command}", "BACKUP")
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

                Dim zipFileName As String = $"{server.Name}_{server.IP}_{DateTime.Now.ToString("yyMMdd")}.zip"
                Dim serverSpecificBackupRootPath As String = Path.Combine(backupPath, $"{server.Name}_{server.IP}")
                Dim fullZipFilePath As String = Path.Combine(serverSpecificBackupRootPath, zipFileName)

                If Directory.Exists(serverSpecificBackupRootPath) Then
                    Dim zipCommand As String = $"""{sevenZipPath}"" a -tzip ""{fullZipFilePath}"" ""{serverSpecificBackupRootPath}""\*"""
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
        End Try
    End Function

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

                _servers.Add(server)
            Next

            AppLogger.Log($"Se cargaron {_servers.Count} servidores con éxito.", "CONFIG")
            Console.WriteLine($"Se cargaron {_servers.Count} servidores.")

        Catch ex As Exception
            AppLogger.Log($"Error al cargar servidores: {ex.Message}", "ERROR")
            Console.WriteLine($"Error al cargar servidores: {ex.Message}")
        End Try
    End Sub

    Public Function GetServers() As List(Of Server)
        Return _servers
    End Function

    Public Sub SetServers(ByVal newServers As List(Of Server))
        _servers = newServers
    End Sub

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

        ' Remove existing Server nodes before adding current ones
        For Each existingServerNode As XmlNode In serversNode.SelectNodes("Server")
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
                Dim excludedDbNode = doc.CreateElement("Database")
                excludedDbNode.InnerText = excludedDbName
                excludedDbsNode.AppendChild(excludedDbNode)
            Next
            serverNode.AppendChild(excludedDbsNode)

            Dim scheduleNode As XmlNode = doc.CreateElement("Schedule")
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

            serversNode.AppendChild(serverNode)
        Next

        doc.Save(filePath)
        AppLogger.Log("Configuración de servidores guardada en servers.xml.", "UI")
    End Sub

End Class

Public Class Server
    Public Property Name As String
    Public Property IP As String
    Public Property Port As Integer
    Public Property User As String
    Public Property Password As String
    Public Property Databases As New List(Of String) ' This will now be used for specific databases to backup, or empty for all
    Public Property ExcludedDatabases As New List(Of String) ' New property for excluded databases
    Public Property Schedule As New ScheduleInfo

    Public Sub New()
        ' Constructor
    End Sub
End Class

Public Class ScheduleInfo
    Public Property Enabled As Boolean
    Public Property Days As New List(Of Integer)
    Public Property Time As String

    Public Sub New()
        ' Constructor
    End Sub
End Class


Public Class ServerManager
    Private _servers As List(Of Server)
    Private Const FilePath As String = "servers.xml"

    Public Sub New()
        _servers = New List(Of Server)()
        LoadServers()
    End Sub

    Public Function GetServers() As List(Of Server)
        Return _servers
    End Function

    Public Sub AddOrUpdateServer(server As Server)
        Dim existingServer = _servers.FirstOrDefault(Function(s) s.Name = server.Name)
        If existingServer IsNot Nothing Then
            ' Update existing server
            existingServer.IP = server.IP
            existingServer.Port = server.Port
            existingServer.User = server.User
            existingServer.Password = server.Password
            existingServer.Databases = server.Databases
            existingServer.ExcludedDatabases = server.ExcludedDatabases
            existingServer.Schedule = server.Schedule
        Else
            ' Add new server
            _servers.Add(server)
        End If
        SaveServers()
    End Sub

    Public Sub RemoveServer(serverName As String)
        Dim serverToRemove = _servers.FirstOrDefault(Function(s) s.Name = serverName)
        If serverToRemove IsNot Nothing Then
            _servers.Remove(serverToRemove)
            SaveServers()
        End If
    End Sub

    Private Sub LoadServers()
        If Not File.Exists(FilePath) Then
            CreateDefaultServerFile()
            Return
        End If

        Try
            Dim doc As New XmlDocument()
            doc.Load(FilePath)

            _servers.Clear()
            For Each serverNode As XmlNode In doc.SelectNodes("/Servers/Server")
                Dim server As New Server()
                server.Name = serverNode.SelectSingleNode("Name").InnerText
                server.IP = serverNode.SelectSingleNode("IP").InnerText
                server.Port = Integer.Parse(serverNode.SelectSingleNode("Port").InnerText)
                server.User = serverNode.SelectSingleNode("User").InnerText
                server.Password = serverNode.SelectSingleNode("Password").InnerText

                ' Load databases to include
                Dim databasesNode = serverNode.SelectSingleNode("Databases")
                If databasesNode IsNot Nothing Then
                    For Each dbNode As XmlNode In databasesNode.SelectNodes("Database")
                        server.Databases.Add(dbNode.InnerText)
                    Next
                End If

                ' Load databases to exclude
                Dim excludedDatabasesNode = serverNode.SelectSingleNode("ExcludedDatabases")
                If excludedDatabasesNode IsNot Nothing Then
                    For Each dbNode As XmlNode In excludedDatabasesNode.SelectNodes("Database")
                        server.ExcludedDatabases.Add(dbNode.InnerText)
                    Next
                End If

                ' Load schedule
                Dim scheduleNode = serverNode.SelectSingleNode("Schedule")
                If scheduleNode IsNot Nothing Then
                    server.Schedule.Enabled = Boolean.Parse(scheduleNode.SelectSingleNode("Enabled").InnerText)
                    server.Schedule.Time = scheduleNode.SelectSingleNode("Time").InnerText
                    Dim daysList As New List(Of Integer)
                    For Each dayStr As String In scheduleNode.SelectSingleNode("Days").InnerText.Split(","c)
                        daysList.Add(Integer.Parse(dayStr.Trim()))
                    Next
                    server.Schedule.Days = daysList
                End If

                _servers.Add(server)
            Next
        Catch ex As Exception
            ' Log or handle exception
            Console.WriteLine($"Error loading servers: {ex.Message}")
        End Try
    End Sub

    Private Sub SaveServers()
        Dim doc As New XmlDocument()
        Dim root As XmlElement = doc.CreateElement("Servers")
        doc.AppendChild(root)

        For Each server As Server In _servers
            Dim serverNode As XmlElement = doc.CreateElement("Server")

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

            ' Save included databases
            Dim databasesNode = doc.CreateElement("Databases")
            For Each dbName As String In server.Databases
                Dim dbNode = doc.CreateElement("Database")
                dbNode.InnerText = dbName
                databasesNode.AppendChild(dbNode)
            Next
            serverNode.AppendChild(databasesNode)

            ' Save excluded databases
            Dim excludedDatabasesNode = doc.CreateElement("ExcludedDatabases")
            For Each dbName As String In server.ExcludedDatabases
                Dim dbNode = doc.CreateElement("Database")
                dbNode.InnerText = dbName
                excludedDatabasesNode.AppendChild(dbNode)
            Next
            serverNode.AppendChild(excludedDatabasesNode)

            ' Save schedule
            Dim scheduleNode = doc.CreateElement("Schedule")
            Dim enabledNode = doc.CreateElement("Enabled")
            enabledNode.InnerText = server.Schedule.Enabled.ToString()
            scheduleNode.AppendChild(enabledNode)
            Dim timeNode = doc.CreateElement("Time")
            timeNode.InnerText = server.Schedule.Time
            scheduleNode.AppendChild(timeNode)
            Dim daysNode = doc.CreateElement("Days")
            daysNode.InnerText = String.Join(",", server.Schedule.Days.Select(Function(d) d.ToString()).ToArray())
            scheduleNode.AppendChild(daysNode)
            serverNode.AppendChild(scheduleNode)

            root.AppendChild(serverNode)
        Next

        doc.Save(FilePath)
    End Sub

    Private Sub CreateDefaultServerFile()
        Dim doc As New XmlDocument()
        Dim root As XmlElement = doc.CreateElement("Servers")
        doc.AppendChild(root)
        doc.Save(FilePath)
    End Sub
End Class
