Imports System.Net
Imports System.Text

Public Class NotificationManager

    Public Shared Sub SendNtfyNotification(topicUrl As String, message As String, title As String, priority As Integer, tags As String())
        Try
            Dim fullUrl As String = topicUrl
            If Not fullUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) AndAlso Not fullUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                fullUrl = "https://" & fullUrl
            End If

            Using client As New WebClient()
                client.Headers(HttpRequestHeader.ContentType) = "text/plain; charset=utf-8"
                client.Headers.Add("Title", title)
                client.Headers.Add("Priority", priority.ToString())
                client.Headers.Add("Tags", String.Join(",", tags))

                Dim requestBody As String = message
                Dim responseBytes As Byte() = client.UploadData(fullUrl, "POST", Encoding.UTF8.GetBytes(requestBody))

                Dim responseString As String = Encoding.UTF8.GetString(responseBytes)
                AppLogger.Log($"Notificación ntfy enviada a '{fullUrl}'. Respuesta: {responseString}", "INFO")
            End Using
        Catch ex As Exception
            AppLogger.Log($"Error al enviar la notificación ntfy a '{topicUrl}': {ex.Message}", "ERROR")
        End Try
    End Sub

End Class
