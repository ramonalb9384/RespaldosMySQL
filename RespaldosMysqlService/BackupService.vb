Imports System.Threading
Imports System.Timers
Imports RespaldosMysqlLibrary
Imports System.IO
Public Class BackupService


    Private serviceTimer As System.Timers.Timer
    Private backupManager As New BackupManager()
    Private fileWatcher As IO.FileSystemWatcher
    Private configFilePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.xml")
    Private isChecking As Boolean = False ' Flag to prevent overlapping checks


    Sub New()

        ' Esta llamada es exigida por el diseñador.
        InitializeComponent()

        ' Agregue cualquier inicialización después de la llamada a InitializeComponent().
        Me.ServiceName = "RespaldosMysqlService"
        Me.CanStop = True
        Me.CanPauseAndContinue = False
        Me.AutoLog = True
        AppLogger.LogBasePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        AppLogger.Log("Iniciando servicio de respaldo MySQL", "SERVICE_LIFECYCLE")
    End Sub
    Protected Overrides Sub OnStart(ByVal args() As String)
        Try
            AppLogger.Log("Iniciando el servicio de respaldo MySQL.", "SERVICE_LIFECYCLE")

            ' Configure and start the main timer first
            serviceTimer = New System.Timers.Timer()
            serviceTimer.Interval = 60000 ' Check every 60 seconds
            AddHandler serviceTimer.Elapsed, AddressOf OnTimerTick
            serviceTimer.Start()

            ' Perform initial setup in a separate thread to not block OnStart
            Dim setupThread As New Thread(AddressOf InitialServiceSetup)
            setupThread.Start()

            AppLogger.Log("Servicio de respaldo MySQL iniciado con éxito. El temporizador está activo.", "SERVICE_LIFECYCLE")
        Catch ex As Exception
            AppLogger.Log($"Error crítico al iniciar el servicio: {ex.Message}", "ERROR")
            ' Re-throw the exception to indicate service startup failure
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        AppLogger.Log("Deteniendo el servicio de respaldo MySQL.", "SERVICE_LIFECYCLE")
        serviceTimer.Stop()
        serviceTimer.Dispose()
        If fileWatcher IsNot Nothing Then
            fileWatcher.Dispose()
        End If
        AppLogger.Log("Servicio de respaldo MySQL detenido.", "SERVICE_LIFECYCLE")
    End Sub
    Private Sub InitialServiceSetup()
        Try
            AppLogger.Log("Configuración inicial del servicio: Iniciando la configuración del observador de archivos.", "SERVICE_SETUP")
            ' Configure and start the file watcher to reload config on change
            SetupFileWatcher()
            AppLogger.Log("Configuración inicial del servicio: Observador de archivos configurado. Cargando servidores.", "SERVICE_SETUP")
            ' Load initial configuration
            backupManager.LoadServers(configFilePath)
            AppLogger.Log("Configuración inicial del servicio: Configuración de servidores cargada con éxito.", "SERVICE_SETUP")
            backupManager.CheckForMissedBackups()
        Catch ex As Exception
            AppLogger.Log($"Configuración inicial del servicio: Error durante la configuración inicial del servicio: {ex.Message}", "ERROR")
        End Try
    End Sub
    Private Sub OnTimerTick(sender As Object, e As ElapsedEventArgs)
        If isChecking Then
            AppLogger.Log("Saltando ciclo de temporizador porque la comprobación anterior sigue en curso.", "WARNING")
            Return
        End If

        Try
            isChecking = True

            Dim servers As List(Of Server) = backupManager.GetServers()
            Dim now As DateTime = DateTime.Now
            Dim currentDay As Integer = CInt(now.DayOfWeek)
            Dim currentTime As String = now.ToString("HH:mm")
            Dim configChanged As Boolean = False ' Flag para indicar si la configuración ha cambiado

            For Each server As Server In servers
                ' --- Procesar respaldos programados regulares ---
                If server.Schedule.Enabled AndAlso server.Schedule.Days.Contains(currentDay) AndAlso server.Schedule.Time = currentTime Then
                    AppLogger.Log($"Respaldo programado activado para el servidor: {server.Name}.", "BACKUP_JOB")

                    Dim backupPath As String = backupManager.BackupDestinationPath
                    Dim backupThread As New Thread(Sub()
                                                       Try
                                                           Dim success As Boolean = backupManager.PerformBackup(server, backupPath, True)
                                                           If success Then
                                                               AppLogger.Log($"Respaldo para el servidor {server.Name} completado con éxito.", "BACKUP_JOB")
                                                           Else
                                                               AppLogger.Log($"Respaldo para el servidor {server.Name} falló. Revise los logs de error para más detalles.", "ERROR")
                                                           End If
                                                       Catch exThread As Exception
                                                           AppLogger.Log($"Excepción no controlada durante el respaldo para {server.Name}: {exThread.Message}", "ERROR")
                                                       End Try
                                                   End Sub)
                    backupThread.Start()
                End If

                ' --- Procesar respaldos por evento ---
                Dim eventosToRemove As New List(Of EventoRespaldo)
                For Each evento As EventoRespaldo In server.Eventos.ToList() ' Usar ToList para permitir la modificación de la colección original
                    If evento.FechaHora <= now Then
                        AppLogger.Log($"Respaldo por evento activado para el servidor: {server.Name} (Evento: {evento.Descripcion}, Fecha: {evento.FechaHora}).", "EVENT_BACKUP_JOB")
                        Dim backupPath As String = backupManager.BackupDestinationPath
                        Dim backupThread As New Thread(Sub()
                                                           Try
                                                               Dim success As Boolean = backupManager.PerformBackup(server, backupPath, True)
                                                               If success Then
                                                                   AppLogger.Log($"Respaldo por evento para el servidor {server.Name} completado con éxito.", "EVENT_BACKUP_JOB")
                                                               Else
                                                                   AppLogger.Log($"Respaldo por evento para el servidor {server.Name} falló. Revise los logs de error para más detalles.", "ERROR")
                                                               End If
                                                           Catch exThread As Exception
                                                               AppLogger.Log($"Excepción no controlada durante el respaldo por evento para {server.Name}: {exThread.Message}", "ERROR")
                                                           End Try
                                                       End Sub)
                        backupThread.Start()
                        eventosToRemove.Add(evento)
                        configChanged = True ' Se ha procesado un evento, la configuración ha cambiado
                    End If
                Next

                ' Eliminar eventos procesados
                For Each evento As EventoRespaldo In eventosToRemove
                    server.Eventos.Remove(evento)
                    AppLogger.Log($"Evento de respaldo eliminado después de su procesamiento: {evento.Descripcion} para {server.Name}.", "EVENT_BACKUP_JOB")
                Next
            Next

            ' Guardar los cambios en la configuración SOLO si se procesó algún evento
            If configChanged Then
                backupManager.SaveServers(configFilePath)
            End If
        Catch ex As Exception
            AppLogger.Log($"Error crítico en el ciclo del temporizador del servicio: {ex.Message}", "ERROR")
        Finally
            isChecking = False
        End Try
    End Sub

    Private Sub SetupFileWatcher()
        Try
            Dim directory = Path.GetDirectoryName(configFilePath)
            Dim fileName = Path.GetFileName(configFilePath)

            fileWatcher = New FileSystemWatcher(directory, fileName)
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite
            AddHandler fileWatcher.Changed, AddressOf OnConfigFileChanged
            fileWatcher.EnableRaisingEvents = True
            AppLogger.Log($"Observador de archivos configurado para: {configFilePath}", "SERVICE_LIFECYCLE")
        Catch ex As Exception
            AppLogger.Log($"Error al configurar el observador de archivos: {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Sub OnConfigFileChanged(source As Object, e As FileSystemEventArgs)
        ' The config file has changed. Reload the servers.
        ' Add a small delay to ensure the file is fully written and released.
        Thread.Sleep(2000) ' Wait 2 seconds
        AppLogger.Log("Se detectó un cambio en el archivo de configuración. Recargando servidores.", "CONFIG_CHANGE")
        Try
            backupManager.LoadServers(configFilePath)
            AppLogger.Log("La configuración de los servidores se ha recargado con éxito.", "CONFIG_CHANGE")
        Catch ex As Exception
            AppLogger.Log($"Error al recargar la configuración del servidor: {ex.Message}", "ERROR")
        End Try
    End Sub


End Class
