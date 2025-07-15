Imports RespaldosMysqlLibrary
Imports System.Linq

Public Class ServerDisplayInfo
    Public Property ServerName As String
    Public Property ServerIP As String
    Public Property ServerPort As Integer
    Public Property ServerUser As String
    Public Property IsBackupEnabled As Boolean
    Public Property IsBackupUpdated As Boolean
    Public Property BackupStatusIcon As String
    Public Property UpdateStatusIcon As String

    Public Server As Server
    Public Sub New(server As Server, backupManager As BackupManager)
        Me.ServerName = server.Name
        Me.ServerIP = server.IP
        Me.ServerPort = server.Port
        Me.ServerUser = server.User
        Me.Server = server
        Me.IsBackupEnabled = server.Schedule.Enabled
        Me.BackupStatusIcon = If(Me.IsBackupEnabled, "✓", "✗")

        ' Calculate IsBackupUpdated
        If server.Schedule.Enabled Then
            Dim lastScheduledBackupTime As DateTime = backupManager.GetLastScheduledBackupTime(server.Schedule)
            Dim lastActualBackupTime As DateTime = backupManager.GetLastBackupTime(server)
            Me.IsBackupUpdated = (lastActualBackupTime >= lastScheduledBackupTime AndAlso lastScheduledBackupTime <> DateTime.MinValue)
        Else
            Me.IsBackupUpdated = False ' Not updated if backup is not enabled
        End If
        Me.UpdateStatusIcon = If(Me.IsBackupUpdated, "✓", "✗")
    End Sub
End Class