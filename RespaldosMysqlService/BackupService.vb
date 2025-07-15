Imports System.Threading
Imports System.Timers
Imports RespaldosMysqlLibrary
Imports System.IO

Public Class BackupService

    Private serviceTimer As System.Timers.Timer
    Private backupManager As New BackupManager()
    Private fileWatcher As IO.FileSystemWatcher
    Private configFilePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.xml")
    Private isChecking As Boolean = False

    Sub New()
        InitializeComponent()
        Me.ServiceName = "RespaldosMysqlService"
        Me.CanStop = True
        Me.CanPauseAndContinue = False
        Me.AutoLog = True
        AppLogger.LogBasePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        AppLogger.Log("Servicio instanciado.", "SERVICE_LIFECYCLE")
    End Sub

    Protected Overrides Sub OnStart(ByVal args() As String)
        AppLogger.Log("OnStart llamado.", "SERVICE_LIFECYCLE")
        Try
            serviceTimer = New System.Timers.Timer()
            serviceTimer.Interval = 60000
            AddHandler serviceTimer.Elapsed, AddressOf OnTimerTick
            serviceTimer.Start()
            AppLogger.Log("Temporizador del servicio iniciado.", "SERVICE_LIFECYCLE")

            Dim setupThread As New Thread(AddressOf InitialServiceSetup)
            setupThread.Start()
        Catch ex As Exception
            AppLogger.Log($"Error crítico en OnStart: {ex.Message}", "ERROR")
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        AppLogger.Log("OnStop llamado. Deteniendo el servicio.", "SERVICE_LIFECYCLE")
        If serviceTimer IsNot Nothing Then
            serviceTimer.Stop()
            serviceTimer.Dispose()
        End If
        If fileWatcher IsNot Nothing Then
            fileWatcher.Dispose()
        End If
        AppLogger.Log("Servicio detenido.", "SERVICE_LIFECYCLE")
    End Sub

    Private Sub InitialServiceSetup()
        AppLogger.Log("Iniciando configuración inicial del servicio...", "DEEP_TRACE")
        Try
            SetupFileWatcher()
            backupManager.LoadServers(configFilePath)
            backupManager.CheckForMissedBackups()
            AppLogger.Log("Configuración inicial completada.", "DEEP_TRACE")
        Catch ex As Exception
            AppLogger.Log($"Error en InitialServiceSetup: {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Sub OnTimerTick(sender As Object, e As ElapsedEventArgs)
        If isChecking Then
            AppLogger.Log("Saltando ciclo de OnTimerTick (comprobación anterior en curso).", "DEEP_TRACE")
            Return
        End If

        isChecking = True
        AppLogger.Log("OnTimerTick: Iniciando ciclo de comprobación.", "DEEP_TRACE")
        Try
            Dim servers = backupManager.GetServers()
            Dim now = DateTime.Now
            Dim currentDay = CInt(now.DayOfWeek)
            Dim configChanged As Boolean = False

            For Each server As Server In servers
                AppLogger.Log($"Procesando servidor: '{server.Name}'", "DEEP_TRACE")

                If server.Schedule.Enabled AndAlso server.Schedule.Days.Contains(currentDay) Then
                    Dim scheduleTime As TimeSpan
                    If TimeSpan.TryParse(server.Schedule.Time, scheduleTime) Then
                        AppLogger.Log($"Comprobando horario para '{server.Name}'. Hora actual: {now.TimeOfDay}, Hora programada: {scheduleTime}", "DEEP_TRACE")
                        If now.TimeOfDay >= scheduleTime AndAlso now.TimeOfDay < scheduleTime.Add(TimeSpan.FromMinutes(1)) Then
                            AppLogger.Log($"¡Coincidencia de horario para '{server.Name}'!", "INFO")
                            Dim backupThread As New Thread(Sub()
                                                               Try
                                                                   backupManager.PerformBackup(server, backupManager.BackupDestinationPath, True)
                                                               Catch exThread As Exception
                                                                   AppLogger.Log($"Error en hilo de respaldo para '{server.Name}': {exThread.Message}", "ERROR")
                                                               End Try
                                                           End Sub)
                            backupThread.Start()
                        End If
                    Else
                        AppLogger.Log($"Formato de hora inválido para '{server.Name}': '{server.Schedule.Time}'", "WARNING")
                    End If
                End If

                Dim eventosToRemove As New List(Of EventoRespaldo)
                For Each evento As EventoRespaldo In server.Eventos.ToList()
                    If evento.FechaHora <= now Then
                        AppLogger.Log($"Activando respaldo por evento para '{server.Name}': {evento.Descripcion}", "INFO")
                        Dim backupThread As New Thread(Sub()
                                                           Try
                                                               backupManager.PerformBackup(server, backupManager.BackupDestinationPath, True)
                                                           Catch exThread As Exception
                                                               AppLogger.Log($"Error en hilo de respaldo por evento para '{server.Name}': {exThread.Message}", "ERROR")
                                                           End Try
                                                       End Sub)
                        backupThread.Start()
                        eventosToRemove.Add(evento)
                        configChanged = True
                    End If
                Next

                If eventosToRemove.Any() Then
                    eventosToRemove.ForEach(Sub(ev) server.Eventos.Remove(ev))
                    AppLogger.Log($"Eventos de respaldo eliminados para '{server.Name}'", "DEEP_TRACE")
                End If
            Next

            If configChanged Then
                AppLogger.Log("Guardando configuración de servidores por cambios en eventos.", "DEEP_TRACE")
                backupManager.SaveServers(configFilePath)
            End If
        Catch ex As Exception
            AppLogger.Log($"Error crítico en OnTimerTick:  {ex.Message}", "ERROR")
        Finally
            isChecking = False
            AppLogger.Log("OnTimerTick Ciclo de comprobación finalizado.", "DEEP_TRACE")
        End Try
    End Sub

    Private Sub SetupFileWatcher()
        AppLogger.Log("Configurando FileSystemWatcher...", "DEEP_TRACE")
        Try
            Dim directory = Path.GetDirectoryName(configFilePath)
            Dim fileName = Path.GetFileName(configFilePath)

            fileWatcher = New FileSystemWatcher(directory, fileName) With {
                .NotifyFilter = NotifyFilters.LastWrite,
                .EnableRaisingEvents = True
            }
            AddHandler fileWatcher.Changed, AddressOf OnConfigFileChanged
            AppLogger.Log("FileSystemWatcher configurado.", "DEEP_TRACE")
        Catch ex As Exception
            AppLogger.Log($"Error en SetupFileWatcher:  {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Sub OnConfigFileChanged(source As Object, e As FileSystemEventArgs)
        AppLogger.Log($"Detectado cambio en '{e.Name}'. Esperando para recargar...", "CONFIG_CHANGE")
        Thread.Sleep(2000)
                    AppLogger.Log("Recargando configuración de servidores...", "CONFIG_CHANGE")
                    Try
                        backupManager.LoadServers(configFilePath)
                        AppLogger.Log("Configuración recargada exitosamente.", "CONFIG_CHANGE")
                    Catch ex As Exception
                        AppLogger.Log($"Error al recargar configuración: {ex.Message}", "ERROR")
                    End Try
    End Sub

End Class