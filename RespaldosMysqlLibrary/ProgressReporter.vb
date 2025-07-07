
Imports System.IO
Imports Newtonsoft.Json

Public Class ProgressReporter
    Private Shared ReadOnly StatusFilePath As String
    Private Shared ReadOnly StatusDirectory As String

    Shared Sub New()
        StatusDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RespaldosMysql")
        StatusFilePath = Path.Combine(StatusDirectory, "status.json")
        EnsureDirectoryExists()
    End Sub

    Private Shared Sub EnsureDirectoryExists()
        If Not Directory.Exists(StatusDirectory) Then
            Directory.CreateDirectory(StatusDirectory)
        End If
    End Sub

    Public Shared Sub WriteStatus(statusText As String, progressValue As Integer)
        Try
            Dim statusData As New ProgressStatus With {
                .Status = statusText,
                .Progress = progressValue,
                .Timestamp = DateTime.UtcNow
            }
            Dim jsonString As String = JsonConvert.SerializeObject(statusData, Formatting.Indented)
            File.WriteAllText(StatusFilePath, jsonString)
        Catch ex As Exception
            AppLogger.Log($"Error al escribir el estado del progreso: {ex.Message}", "ERROR")
        End Try
    End Sub

    Public Shared Sub ClearStatus()
        Try
            If File.Exists(StatusFilePath) Then
                File.Delete(StatusFilePath)
            End If
        Catch ex As Exception
            AppLogger.Log($"Error al limpiar el estado del progreso: {ex.Message}", "ERROR")
        End Try
    End Sub
End Class

Public Class ProgressStatus
    Public Property Status As String
    Public Property Progress As Integer
    Public Property Timestamp As DateTime
End Class
