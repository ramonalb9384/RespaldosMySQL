Imports System.Configuration
Imports System.Windows.Forms
Imports RespaldosMysqlLibrary

Public Class FormSettings

    Public backupManager As BackupManager
    Private Sub FormSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        txtMySqlDumpPath.Text = backupManager.MySqlDumpPath
        txtBackupPath.Text = backupManager.BackupDestinationPath
        txtSevenZipPath.Text = backupManager.SevenZipPath
        chkEncryptPasswords.Checked = backupManager.EncryptPasswords
        chkDeepLogging.Checked = backupManager.DeepLoggingEnabled
    End Sub

    Private Sub btnBrowseMySqlDump_Click(sender As Object, e As EventArgs) Handles btnBrowseMySqlDump.Click
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            txtMySqlDumpPath.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub btnBrowseBackupPath_Click(sender As Object, e As EventArgs) Handles btnBrowseBackupPath.Click
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            txtBackupPath.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub btnBrowseSevenZip_Click(sender As Object, e As EventArgs) Handles btnBrowseSevenZip.Click
        OpenFileDialog1.FileName = "7z.exe"
        OpenFileDialog1.Filter = "Ejecutables|*.exe|Todos los archivos|*.*"
        OpenFileDialog1.Title = "Seleccionar 7z.exe"
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            txtSevenZipPath.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            backupManager.MySqlDumpPath = txtMySqlDumpPath.Text
            backupManager.BackupDestinationPath = txtBackupPath.Text
            backupManager.SevenZipPath = txtSevenZipPath.Text
            backupManager.EncryptPasswords = chkEncryptPasswords.Checked
            backupManager.DeepLoggingEnabled = chkDeepLogging.Checked



            MessageBox.Show("Configuración guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show($"Error al guardar la configuración: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
