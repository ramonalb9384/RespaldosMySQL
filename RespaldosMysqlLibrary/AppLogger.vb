Imports System.IO

''' <summary>
''' Proporciona funcionalidades centralizadas para el registro de eventos de la aplicación.
''' </summary>
''' <remarks>
''' Este módulo permite escribir mensajes de log en archivos de texto, organizados por fecha.
''' En caso de fallo al escribir en el archivo, intentará registrar el error en el Visor de Eventos de Windows.
''' </remarks>
Public Module AppLogger

    Public Property LogBasePath As String = AppDomain.CurrentDomain.BaseDirectory
    Public Property DeepLoggingEnabled As Boolean = False

    Public Sub Log(message As String, Optional logType As String = "GENERAL")
        Try
            Dim logDirectory As String = Path.Combine(LogBasePath, "Logs")
            If Not Directory.Exists(logDirectory) Then
                Directory.CreateDirectory(logDirectory)
            End If

            Dim logEntry As String = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [{logType}] {message}"

            ' Escribir siempre en el log normal, a menos que sea un log de tipo DEEP_TRACE
            If logType <> "DEEP_TRACE" Then
                Dim normalLogFileName As String = $"LOG_{DateTime.Now.ToString("yyyyMMdd")}.txt"
                Dim normalLogFilePath As String = Path.Combine(logDirectory, normalLogFileName)
                File.AppendAllText(normalLogFilePath, logEntry & Environment.NewLine)
            End If

            ' Si el log profundo está habilitado, escribir todo en el archivo de log profundo
            If DeepLoggingEnabled Then
                Dim deepLogFileName As String = $"LOG_DEEP_{DateTime.Now.ToString("yyyyMMdd")}.txt"
                Dim deepLogFilePath As String = Path.Combine(logDirectory, deepLogFileName)
                File.AppendAllText(deepLogFilePath, logEntry & Environment.NewLine)
            End If

        Catch ex As Exception
            Try
                Dim eventLog As New System.Diagnostics.EventLog()
                eventLog.Source = "RespaldosMysqlService"
                eventLog.WriteEntry($"ERROR al escribir en el log de archivo: {ex.Message}. Mensaje original: [{logType}] {message}", System.Diagnostics.EventLogEntryType.Error)
            Catch exEventLog As Exception
                Console.WriteLine($"ERROR crítico: Fallo al escribir en el log de archivo Y en el Visor de Eventos: {exEventLog.Message}. Mensaje original: [{logType}] {message}")
            End Try
        End Try
    End Sub

End Module
