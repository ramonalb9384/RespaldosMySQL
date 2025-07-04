Imports System.IO

Public Module AppLogger

    Public Sub Log(message As String, Optional logType As String = "GENERAL")
        Try
            Dim logDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
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
            ' Si falla el log, al menos intentar escribir en la consola
            Console.WriteLine($"ERROR al escribir en el log: {ex.Message}. Mensaje original: [{logType}] {message}")
        End Try
    End Sub

End Module
