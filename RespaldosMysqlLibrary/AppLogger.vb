Imports System.IO

''' <summary>
''' Proporciona funcionalidades centralizadas para el registro de eventos de la aplicación.
''' </summary>
''' <remarks>
''' Este módulo permite escribir mensajes de log en archivos de texto, organizados por fecha.
''' En caso de fallo al escribir en el archivo, intentará registrar el error en el Visor de Eventos de Windows.
''' </remarks>
Public Module AppLogger

    ''' <summary>
    ''' Obtiene o establece la ruta base donde se creará la carpeta 'Logs'.
    ''' </summary>
    ''' <value>La ruta base para los archivos de log. Por defecto, es el directorio base del dominio de la aplicación.</value>
    Public Property LogBasePath As String = AppDomain.CurrentDomain.BaseDirectory ' Default to current domain base directory

    ''' <summary>
    ''' Escribe un mensaje en el archivo de log de la aplicación.
    ''' </summary>
    ''' <param name="message">El mensaje a registrar.</param>
    ''' <param name="logType">El tipo de log (por ejemplo, "GENERAL", "ERROR", "BACKUP"). Por defecto es "GENERAL".</param>
    ''' <remarks>
    ''' Los logs se organizan en una carpeta 'Logs' dentro de la ruta especificada por <see cref="LogBasePath"/>.
    ''' Cada día se crea un nuevo archivo de log con el formato LOG_YYYYMMDD.txt.
    ''' Si falla la escritura en el archivo, se intentará registrar el error en el Visor de Eventos de Windows.
    ''' </remarks>
    Public Sub Log(message As String, Optional logType As String = "GENERAL")
        Try

            Dim logDirectory As String = Path.Combine(LogBasePath, "Logs")
            Dim logFileName As String = $"LOG_{DateTime.Now.ToString("yyyyMMdd")}.txt"
            Dim logFilePath As String = Path.Combine(logDirectory, logFileName)

            ' Asegurarse de que el directorio exista
            If Not Directory.Exists(logDirectory) Then
                Directory.CreateDirectory(logDirectory)
            End If

            Dim logEntry As String = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [{logType}] {message}"

            ' Escribir en el archivo de log. Usar True para añadir al final.
            File.AppendAllText(logFilePath, logEntry & Environment.NewLine)

        Catch ex As Exception
            ' Si falla el log, al menos intentar escribir en el Visor de Eventos de Windows como fallback
            Try
                Dim eventLog As New System.Diagnostics.EventLog()
                eventLog.Source = "RespaldosMysqlService"
                eventLog.WriteEntry($"ERROR al escribir en el log de archivo: {ex.Message}. Mensaje original: [{logType}] {message}", System.Diagnostics.EventLogEntryType.Error)
            Catch exEventLog As Exception
                ' Si incluso el log de eventos falla, al menos intentar escribir en la consola
                Console.WriteLine($"ERROR crítico: Fallo al escribir en el log de archivo Y en el Visor de Eventos: {exEventLog.Message}. Mensaje original: [{logType}] {message}")
            End Try
        End Try
    End Sub

End Module
