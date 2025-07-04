Imports System.IO
Imports System.Xml
Imports System.Diagnostics
Imports System.Configuration
Imports System.Security.Principal
Imports System.Configuration.Install
Imports System.ServiceProcess
Imports RespaldosMysqlLibrary

Public Class Form1

    Private backupManager As New BackupManager()
    Private servers As New List(Of Server)
    Private configFilePath As String = Path.Combine(Application.StartupPath, "servers.xml")

    Private WithEvents serviceStatusTimer As New System.Windows.Forms.Timer() ' Declaración del Timer
    Private logWatcher As FileSystemWatcher
    Private lastLogReadPosition As Long = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AppLogger.Log("Formulario principal cargado.", "UI")
        LoadServers()
        SetupDataGridView()
        DisplayServers()
        UpdateServiceStatus() ' Llamada inicial para actualizar el estado del servicio

        ' Configurar y iniciar el Timer para actualizar el estado del servicio
        serviceStatusTimer.Enabled = True
        serviceStatusTimer.Interval = 5000 ' Actualizar cada 5 segundos (5000 milisegundos)
        AddHandler serviceStatusTimer.Tick, AddressOf ServiceStatusTimer_Tick
        serviceStatusTimer.Start()

        ' Configurar el FileSystemWatcher para el log
        SetupLogWatcher()
        LoadInitialLog()
    End Sub

    Private Sub ServiceStatusTimer_Tick(sender As Object, e As EventArgs) Handles serviceStatusTimer.Tick
        UpdateServiceStatus()
    End Sub

    Public Sub LoadServers()
        AppLogger.Log("Cargando servidores desde BackupManager.", "UI")
        backupManager.LoadServers(configFilePath)
        servers = backupManager.GetServers()
        AppLogger.Log($"Se cargaron {servers.Count} servidores.", "UI")
    End Sub

    Private Sub SetupDataGridView()
        dgvServers.AutoGenerateColumns = False
        dgvServers.AllowUserToAddRows = False
        dgvServers.ReadOnly = True
        dgvServers.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvServers.MultiSelect = False

        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "Name", .HeaderText = "Nombre", .Width = 150})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "IP", .HeaderText = "IP/Host", .Width = 120})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "Port", .HeaderText = "Puerto", .Width = 60})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "User", .HeaderText = "Usuario", .Width = 100})
    End Sub

    Private Sub DisplayServers()
        dgvServers.DataSource = Nothing
        dgvServers.DataSource = servers
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        AppLogger.Log("Botón 'Añadir Servidor' presionado.", "UI")
        Using editorForm As New FormEditorServidor()
            If editorForm.ShowDialog() = DialogResult.OK Then
                servers.Add(editorForm.ServerData)
                backupManager.SetServers(servers)
                backupManager.SaveServers(configFilePath)
                DisplayServers()
                AppLogger.Log($"Servidor '{editorForm.ServerData.Name}' añadido.", "UI")
            Else
                AppLogger.Log("Operación de añadir servidor cancelada.", "UI")
            End If
        End Using
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        AppLogger.Log("Botón 'Editar Servidor' presionado.", "UI")
        If dgvServers.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor para editar.", "Ningún Servidor Seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            AppLogger.Log("Editar Servidor: No se seleccionó ningún servidor.", "ADVERTENCIA")
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, Server)
        Dim serverIndex = servers.IndexOf(selectedServer)

        Using editorForm As New FormEditorServidor(selectedServer)
            If editorForm.ShowDialog() = DialogResult.OK Then
                servers(serverIndex) = editorForm.ServerData
                backupManager.SetServers(servers)
                backupManager.SaveServers(configFilePath)
                DisplayServers()
                AppLogger.Log($"Servidor '{editorForm.ServerData.Name}' actualizado.", "UI")
            Else
                AppLogger.Log("Operación de editar servidor cancelada.", "UI")
            End If
        End Using
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        AppLogger.Log("Botón 'Eliminar Servidor' presionado.", "UI")
        If dgvServers.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor para eliminar.", "Ningún Servidor Seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            AppLogger.Log("Eliminar Servidor: No se seleccionó ningún servidor.", "ADVERTENCIA")
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, Server)

        If MessageBox.Show($"¿Está seguro de que desea eliminar el servidor '{selectedServer.Name}'?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            servers.Remove(selectedServer)
            backupManager.SetServers(servers)
            backupManager.SaveServers(configFilePath)
            DisplayServers()
            AppLogger.Log($"Servidor '{selectedServer.Name}' eliminado.", "UI")
        Else
            AppLogger.Log("Operación de eliminar servidor cancelada.", "UI")
        End If
    End Sub




    Private Sub btnBackupNow_Click(sender As Object, e As EventArgs) Handles btnBackupNow.Click
        AppLogger.Log("Botón 'Respaldar Ahora' presionado.", "UI")
        If dgvServers.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor para respaldar.", "Ningún Servidor Seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            AppLogger.Log("Respaldar Ahora: No se seleccionó ningún servidor.", "ADVERTENCIA")
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, Server)
        AppLogger.Log($"Respaldar Ahora: Servidor seleccionado '{selectedServer.Name}'.", "UI")

        Dim backupPath As String = backupManager.BackupDestinationPath
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Seleccione la carpeta donde guardar los respaldos"
            fbd.SelectedPath = backupPath
            If fbd.ShowDialog() = DialogResult.OK Then
                backupPath = fbd.SelectedPath
                AppLogger.Log($"Destino del respaldo seleccionado: {backupPath}", "UI")
            Else
                AppLogger.Log("Operación de respaldo cancelada por el usuario.", "UI")
                Return ' Usuario canceló
            End If
        End Using

        backupManager.PerformBackup(selectedServer, backupPath, False)
        MessageBox.Show("Proceso de respaldo finalizado.", "Proceso Finalizado", MessageBoxButtons.OK, MessageBoxIcon.Information)
        AppLogger.Log("Proceso de respaldo finalizado.", "UI")
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        AppLogger.Log("Botón 'Configuración' presionado.", "UI")
        Using settingsForm As New FormSettings()
            settingsForm.backupManager = backupManager
            If settingsForm.ShowDialog() = DialogResult.OK Then
                backupManager.SaveServers(configFilePath)
            End If
            AppLogger.Log("Formulario de configuración cerrado.", "UI")
        End Using
    End Sub

    Private Sub btnInstallService_Click(sender As Object, e As EventArgs) Handles btnInstallService.Click
        If Not IsAdministrator() Then
            MessageBox.Show("Esta operación requiere privilegios de administrador. Por favor, reinicie la aplicación como administrador.", "Permisos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If IsServiceInstalled() Then
            MessageBox.Show("El servicio ya está instalado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            ManagedInstallerClass.InstallHelper(New String() {Path.Combine(Application.StartupPath, "RespaldosMysqlService.exe")})
            AppLogger.Log("Servicio instalado con éxito.", "SERVICE_LIFECYCLE")
            MessageBox.Show("Servicio instalado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            UpdateServiceStatus()
        Catch ex As Exception
            MessageBox.Show($"Error al instalar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AppLogger.Log($"Error al instalar el servicio: {ex.ToString()}", "ERROR")
        End Try
    End Sub

    Private Sub btnUninstallService_Click(sender As Object, e As EventArgs) Handles btnUninstallService.Click
        If Not IsAdministrator() Then
            MessageBox.Show("Esta operación requiere privilegios de administrador. Por favor, reinicie la aplicación como administrador.", "Permisos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not IsServiceInstalled() Then
            MessageBox.Show("El servicio no está instalado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            ManagedInstallerClass.InstallHelper(New String() {"/u", Path.Combine(Application.StartupPath, "RespaldosMysqlService.exe")})
            AppLogger.Log("Servicio desinstalado con éxito.", "SERVICE_LIFECYCLE")
            MessageBox.Show("Servicio desinstalado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            UpdateServiceStatus()
        Catch ex As Exception
            MessageBox.Show($"Error al desinstalar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AppLogger.Log($"Error al desinstalar el servicio: {ex.ToString()}", "ERROR")
        End Try
    End Sub

    Private Sub UpdateServiceStatus()
        Try
            Dim serviceName As String = "RespaldosMysqlService"
            Dim sc As New ServiceController(serviceName)

            If IsServiceInstalled() Then
                lblServiceStatus.Text = $"Estado del Servicio: {sc.Status.ToString()}"
                Select Case sc.Status
                    Case ServiceControllerStatus.Running
                        lblServiceStatus.ForeColor = Color.Green
                        btnStartService.Enabled = False
                        btnStopService.Enabled = True
                        btnInstallService.Enabled = False
                        btnUninstallService.Enabled = True
                    Case ServiceControllerStatus.Stopped, ServiceControllerStatus.Paused
                        lblServiceStatus.ForeColor = Color.Red
                        btnStartService.Enabled = True
                        btnStopService.Enabled = False
                        btnInstallService.Enabled = False
                        btnUninstallService.Enabled = True
                    Case Else ' Pending states (StartPending, StopPending, etc.)
                        lblServiceStatus.ForeColor = Color.Orange
                        btnStartService.Enabled = False
                        btnStopService.Enabled = False
                        btnInstallService.Enabled = False
                        btnUninstallService.Enabled = True ' Still allow uninstall if pending
                End Select
            Else
                lblServiceStatus.Text = "Estado del Servicio: No Instalado"
                lblServiceStatus.ForeColor = Color.Gray
                btnStartService.Enabled = False
                btnStopService.Enabled = False
                btnInstallService.Enabled = True
                btnUninstallService.Enabled = False
            End If
        Catch ex As Exception
            lblServiceStatus.Text = "Estado del Servicio: Error"
            lblServiceStatus.ForeColor = Color.DarkRed
            AppLogger.Log($"Error al actualizar el estado del servicio: {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Function IsAdministrator() As Boolean
        Dim identity = WindowsIdentity.GetCurrent()
        Dim principal = New WindowsPrincipal(identity)
        Return principal.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

    Private Function IsServiceInstalled() As Boolean
        Return ServiceController.GetServices().Any(Function(s) s.ServiceName = "RespaldosMysqlService")
    End Function

    Private Sub btnStartService_Click(sender As Object, e As EventArgs) Handles btnStartService.Click
        If Not IsAdministrator() Then
            MessageBox.Show("Esta operación requiere privilegios de administrador. Por favor, reinicie la aplicación como administrador.", "Permisos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not IsServiceInstalled() Then
            MessageBox.Show("El servicio no está instalado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Dim sc As New ServiceController("RespaldosMysqlService")
            If sc.Status = ServiceControllerStatus.Stopped Then
                AppLogger.Log("Intentando iniciar el servicio.", "SERVICE_LIFECYCLE")
                sc.Start()
                sc.WaitForStatus(ServiceControllerStatus.Running, New TimeSpan(0, 0, 30)) ' Esperar hasta 30 segundos
                AppLogger.Log("Servicio iniciado con éxito.", "SERVICE_LIFECYCLE")
                MessageBox.Show("Servicio iniciado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show($"El servicio ya está en estado: {sc.Status}", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information)
                AppLogger.Log($"Intento de iniciar servicio, pero ya está en estado: {sc.Status}", "ADVERTENCIA")
            End If
        Catch ex As Exception
            MessageBox.Show($"Error al iniciar el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AppLogger.Log($"Error al iniciar el servicio: {ex.ToString()}", "ERROR")
        Finally
            UpdateServiceStatus()
        End Try
    End Sub

    Private Sub btnStopService_Click(sender As Object, e As EventArgs) Handles btnStopService.Click
        If Not IsAdministrator() Then
            MessageBox.Show("Esta operación requiere privilegios de administrador. Por favor, reinicie la aplicación como administrador.", "Permisos Requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not IsServiceInstalled() Then
            MessageBox.Show("El servicio no está instalado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Dim sc As New ServiceController("RespaldosMysqlService")
            If sc.Status = ServiceControllerStatus.Running Then
                AppLogger.Log("Intentando detener el servicio.", "SERVICE_LIFECYCLE")
                sc.Stop()
                sc.WaitForStatus(ServiceControllerStatus.Stopped, New TimeSpan(0, 0, 30)) ' Esperar hasta 30 segundos
                AppLogger.Log("Servicio detenido con éxito.", "SERVICE_LIFECYCLE")
                MessageBox.Show("Servicio detenido correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show($"El servicio ya está en estado: {sc.Status}", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information)
                AppLogger.Log($"Intento de detener servicio, pero ya está en estado: {sc.Status}", "ADVERTENCIA")
            End If
        Catch ex As Exception
            MessageBox.Show($"Error al detener el servicio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AppLogger.Log($"Error al detener el servicio: {ex.ToString()}", "ERROR")
        Finally
            UpdateServiceStatus()
        End Try
    End Sub

    Private Sub SetupLogWatcher()
        Try
            Dim logDirectory As String = Path.Combine(Application.StartupPath, "Logs")
            Dim logFileName As String = $"LOG_{DateTime.Now.ToString("yyyyMMdd")}.txt"
            Dim logFilePath As String = Path.Combine(logDirectory, logFileName)

            If Not Directory.Exists(logDirectory) Then
                Directory.CreateDirectory(logDirectory)
            End If

            logWatcher = New FileSystemWatcher()
            logWatcher.Path = logDirectory
            logWatcher.Filter = logFileName
            logWatcher.NotifyFilter = NotifyFilters.LastWrite Or NotifyFilters.Size
            logWatcher.EnableRaisingEvents = True

            AddHandler logWatcher.Changed, AddressOf LogFileChanged
            AddHandler logWatcher.Created, AddressOf LogFileChanged

        Catch ex As Exception
            MessageBox.Show($"Error al configurar el observador de logs: {ex.Message}", "Error de Log")
        End Try
    End Sub

    Private Sub LoadInitialLog()
        Dim logDirectory As String = Path.Combine(Application.StartupPath, "Logs")
        Dim logFileName As String = $"LOG_{DateTime.Now.ToString("yyyyMMdd")}.txt"
        Dim logFilePath As String = Path.Combine(logDirectory, logFileName)

        If File.Exists(logFilePath) Then
            Try
                Using fs As New FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using sr As New StreamReader(fs)
                        txtLogOutput.Text = sr.ReadToEnd()
                        lastLogReadPosition = fs.Length
                    End Using
                End Using
                txtLogOutput.SelectionStart = txtLogOutput.Text.Length
                txtLogOutput.ScrollToCaret()
            Catch ex As Exception
                MessageBox.Show($"Error al cargar el log inicial: {ex.Message}", "Error de Log")
            End Try
        End If
    End Sub

    Private Sub LogFileChanged(sender As Object, e As FileSystemEventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(AddressOf ReadAndAppendLog))
        Else
            ReadAndAppendLog()
        End If
    End Sub

    Private Sub ReadAndAppendLog()
        Dim logDirectory As String = Path.Combine(Application.StartupPath, "Logs")
        Dim logFileName As String = $"LOG_{DateTime.Now.ToString("yyyyMMdd")}.txt"
        Dim logFilePath As String = Path.Combine(logDirectory, logFileName)

        If File.Exists(logFilePath) Then
            Try
                Using fs As New FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    If fs.Length > lastLogReadPosition Then
                        fs.Seek(lastLogReadPosition, SeekOrigin.Begin)
                        Using sr As New StreamReader(fs)
                            Dim newContent As String = sr.ReadToEnd()
                            AppendLogText(newContent)
                            lastLogReadPosition = fs.Length
                        End Using
                    ElseIf fs.Length < lastLogReadPosition Then
                        ' File was truncated or recreated (e.g., new day's log)
                        txtLogOutput.Clear()
                        fs.Seek(0, SeekOrigin.Begin)
                        Using sr As New StreamReader(fs)
                            Dim newContent As String = sr.ReadToEnd()
                            AppendLogText(newContent)
                            lastLogReadPosition = fs.Length
                        End Using
                    End If
                End Using
            Catch ex As Exception
                ' Log this error, but don't show a MessageBox repeatedly
                Console.WriteLine($"Error al leer y añadir al log: {ex.Message}")
            End Try
        End If
    End Sub

    Private Sub AppendLogText(text As String)
        If txtLogOutput.InvokeRequired Then
            txtLogOutput.Invoke(New Action(Sub() AppendLogText(text)))
        Else
            txtLogOutput.AppendText(text)
            txtLogOutput.SelectionStart = txtLogOutput.Text.Length
            txtLogOutput.ScrollToCaret()
        End If
    End Sub

End Class