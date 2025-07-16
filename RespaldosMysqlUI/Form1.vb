Imports System.IO
Imports System.Xml
Imports System.Diagnostics
Imports System.Configuration
Imports System.Security.Principal
Imports System.Configuration.Install
Imports System.ServiceProcess
Imports RespaldosMysqlLibrary
Imports Newtonsoft.Json
Imports System.Security.AccessControl

Public Class Form1

    Private backupManager As New BackupManager()
    Private servers As New List(Of Server)
    Private configFilePath As String = Path.Combine(Application.StartupPath, "servers.xml")

    Private WithEvents serviceStatusTimer As New System.Windows.Forms.Timer() ' Declaración del Timer
    Private WithEvents logUpdateTimer As New System.Windows.Forms.Timer()
    Private WithEvents progressUpdateTimer As New System.Windows.Forms.Timer()
    Private _logLinesReadCount As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AppLogger.LogBasePath = Application.StartupPath
        AppLogger.Log("Formulario principal cargado.", "UI")

        ' Attempt to set log folder permissions if running as administrator
        If IsAdministrator() Then
            Dim logDirectory As String = Path.Combine(AppLogger.LogBasePath, "Logs")
            SetLogFolderPermissions(logDirectory)
        Else
            AppLogger.Log("La aplicación no se está ejecutando como administrador. No se pueden establecer permisos de log automáticamente.", "WARNING")
        End If


        LoadServers()
        SetupDataGridView()
        DisplayServers()
        UpdateServiceStatus()


        serviceStatusTimer.Interval = 5000
        AddHandler serviceStatusTimer.Tick, AddressOf ServiceStatusTimer_Tick
        serviceStatusTimer.Start()

        logUpdateTimer.Interval = 2000
        AddHandler logUpdateTimer.Tick, AddressOf LogUpdateTimer_Tick
        logUpdateTimer.Start()
        LoadLog()

        progressUpdateTimer.Interval = 1000
        AddHandler progressUpdateTimer.Tick, AddressOf ProgressUpdateTimer_Tick
        progressUpdateTimer.Start()
    End Sub

    Private Sub ProgressUpdateTimer_Tick(sender As Object, e As EventArgs)
        UpdateProgress()
    End Sub

    Private Sub UpdateProgress()
        Dim statusData As ProgressStatus = ProgressReporter.ReadStatus()

        If statusData IsNot Nothing Then
            ProgressBar1.Value = statusData.Progress
            lblProgressStatus.Text = statusData.Status
            ProgressBar1.Visible = True
            lblProgressStatus.Visible = True

            If statusData.Progress >= 100 OrElse statusData.Status.ToLower().Contains("error") Then
                System.Threading.Thread.Sleep(5000)
                ProgressReporter.ClearStatus()
            End If

        Else
            ProgressBar1.Visible = False
            lblProgressStatus.Visible = False
        End If
    End Sub

    Private Sub LogUpdateTimer_Tick(sender As Object, e As EventArgs)
        LoadLog()
    End Sub

    Private Sub LoadLog()
        Try
            Dim logDirectory As String = Path.Combine(AppLogger.LogBasePath, "Logs")
            Dim logFileName As String = $"LOG_{DateTime.Now.ToString("yyyyMMdd")}.txt"
            Dim logFilePath As String = Path.Combine(logDirectory, logFileName)

            If File.Exists(logFilePath) Then
                Dim lines As String() = File.ReadAllLines(logFilePath)

                If lines.Length < _logLinesReadCount Then ' Log file has been reset (new day)
                    txtLogOutput.Clear()
                    _logLinesReadCount = 0
                End If

                If lines.Length > _logLinesReadCount Then
                    For i As Integer = _logLinesReadCount To lines.Length - 1
                        Dim line As String = lines(i)
                        Dim color As Color = Color.Black
                        If line.Contains("[ERROR]") Then
                            color = Color.Red
                        ElseIf line.Contains("[ADVERTENCIA]") Then
                            color = Color.Orange
                        ElseIf line.Contains("[BACKUP]") Then
                            color = Color.Blue
                        ElseIf line.Contains("[ZIP]") Then
                            color = Color.Green
                        ElseIf line.Contains("[CLEANUP]") Then
                            color = Color.Gray
                        End If
                        AppendText(line & Environment.NewLine, color)
                    Next
                    _logLinesReadCount = lines.Length
                    txtLogOutput.ScrollToCaret()
                End If
            Else
                txtLogOutput.Clear()
                _logLinesReadCount = 0
                txtLogOutput.Text = "No se ha generado ningún log para el día de hoy."
            End If
        Catch ex As Exception
            txtLogOutput.Text = $"Error al cargar el log: {ex.Message}"
        End Try
    End Sub

    Private Sub AppendText(text As String, color As Color)
        txtLogOutput.SelectionStart = txtLogOutput.TextLength
        txtLogOutput.SelectionLength = 0
        txtLogOutput.SelectionColor = color
        txtLogOutput.AppendText(text)
        txtLogOutput.SelectionColor = txtLogOutput.ForeColor
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

        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "ServerName", .HeaderText = "Nombre Servidor", .Width = 150})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "ServerIP", .HeaderText = "IP/Host", .Width = 120})
        'dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "ServerPort", .HeaderText = "Puerto", .Width = 60})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "ServerUser", .HeaderText = "Usuario", .Width = 100})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "BackupStatusIcon", .HeaderText = "Respaldo Habilitado", .Width = 80, .DefaultCellStyle = New DataGridViewCellStyle() With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
        dgvServers.Columns.Add(New DataGridViewTextBoxColumn() With {.DataPropertyName = "UpdateStatusIcon", .HeaderText = "Último Respaldo", .Width = 80, .DefaultCellStyle = New DataGridViewCellStyle() With {.Alignment = DataGridViewContentAlignment.MiddleCenter}})
    End Sub

    Private Sub DisplayServers()
        dgvServers.DataSource = Nothing
        Dim displayList As New List(Of ServerDisplayInfo)
        For Each s As Server In servers
            displayList.Add(New ServerDisplayInfo(s, backupManager))
        Next
        dgvServers.DataSource = displayList
        If displayList.Any() Then
            dgvServers.Rows(0).Selected = True
        End If
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        AppLogger.Log("Botón 'Añadir Servidor' presionado.", "UI")
        Using editorForm As New FormEditorServidor(backupManager)
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

        Dim selectedDisplayInfo = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo)
        Dim selectedServer = selectedDisplayInfo.Server
        Dim serverIndex = servers.IndexOf(selectedServer)

        Using editorForm As New FormEditorServidor(backupManager, selectedServer)
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

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo).Server

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




    Private Async Sub btnBackupNow_Click(sender As Object, e As EventArgs) Handles btnBackupNow.Click
        AppLogger.Log("Botón 'Respaldar Ahora' presionado.", "UI")
        If dgvServers.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor para respaldar.", "Ningún Servidor Seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            AppLogger.Log("Respaldar Ahora: No se seleccionó ningún servidor.", "ADVERTENCIA")
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo).Server
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

        btnBackupNow.Enabled = False
        Me.Cursor = Cursors.WaitCursor

        Await Task.Run(Sub()
                           backupManager.PerformBackup(selectedServer, backupPath, False)
                       End Sub)

        btnBackupNow.Enabled = True
        Me.Cursor = Cursors.Default

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



    Private Sub AppendLogText(text As String)
        If txtLogOutput.InvokeRequired Then
            txtLogOutput.Invoke(New Action(Sub() AppendLogText(text)))
        Else
            txtLogOutput.AppendText(text)
            txtLogOutput.SelectionStart = txtLogOutput.Text.Length
            txtLogOutput.ScrollToCaret()
        End If
    End Sub

    Private Sub SetLogFolderPermissions(logDirectory As String)
        Try
            ' 1. Asegurarse de que el directorio exista
            If Not Directory.Exists(logDirectory) Then
                Directory.CreateDirectory(logDirectory)
            End If

            ' 2. Obtener el control de acceso actual de la carpeta
            Dim directoryInfo As New DirectoryInfo(logDirectory)
            Dim directorySecurity As DirectorySecurity = directoryInfo.GetAccessControl()

            ' 3. Definir y añadir reglas de acceso para las cuentas de servicio comunes

            ' Regla para la cuenta LOCAL SERVICE
            Dim sidLocalService As New Security.Principal.SecurityIdentifier(Security.Principal.WellKnownSidType.LocalServiceSid, Nothing)
            Dim accountLocalService As New Security.Principal.NTAccount(sidLocalService.Translate(GetType(Security.Principal.NTAccount)).Value)
            Dim ruleLocalService As New FileSystemAccessRule(accountLocalService, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow)
            directorySecurity.AddAccessRule(ruleLocalService)

            ' Regla para la cuenta NETWORK SERVICE
            Dim sidNetworkService As New Security.Principal.SecurityIdentifier(Security.Principal.WellKnownSidType.NetworkServiceSid, Nothing)
            Dim accountNetworkService As New Security.Principal.NTAccount(sidNetworkService.Translate(GetType(Security.Principal.NTAccount)).Value)
            Dim ruleNetworkService As New FileSystemAccessRule(accountNetworkService, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow)
            directorySecurity.AddAccessRule(ruleNetworkService)

            ' Regla para la cuenta SYSTEM (Sistema local)
            Dim sidSystem As New Security.Principal.SecurityIdentifier(Security.Principal.WellKnownSidType.LocalSystemSid, Nothing)
            Dim accountSystem As New Security.Principal.NTAccount(sidSystem.Translate(GetType(Security.Principal.NTAccount)).Value)
            Dim ruleSystem As New FileSystemAccessRule(accountSystem, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow)
            directorySecurity.AddAccessRule(ruleSystem)

            ' 4. Aplicar los cambios de seguridad a la carpeta
            directoryInfo.SetAccessControl(directorySecurity)
            AppLogger.Log($"Permisos de escritura establecidos para la carpeta de logs: {logDirectory}", "UI")
        Catch ex As Exception
            AppLogger.Log($"Error al establecer permisos para la carpeta de logs {logDirectory}: {ex.Message}", "ERROR")
        End Try
    End Sub

    Private Sub btnExportConfig_Click(sender As Object, e As EventArgs) Handles btnExportConfig.Click
        AppLogger.Log("Botón 'Exportar Configuración' presionado.", "UI")
        Using sfd As New SaveFileDialog()
            sfd.Filter = "Archivos XML (*.xml)|*.xml"
            sfd.Title = "Guardar Configuración de Servidores"
            sfd.FileName = "servers.xml"
            If sfd.ShowDialog() = DialogResult.OK Then
                Try
                    backupManager.SaveServers(sfd.FileName)
                    MessageBox.Show("Configuración exportada con éxito.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    AppLogger.Log($"Configuración exportada a: {sfd.FileName}", "UI")
                Catch ex As Exception
                    MessageBox.Show($"Error al exportar la configuración: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    AppLogger.Log($"Error al exportar la configuración: {ex.ToString()}", "ERROR")
                End Try
            Else
                AppLogger.Log("Exportación de configuración cancelada.", "UI")
            End If
        End Using
    End Sub

    Private Sub btnImportConfig_Click(sender As Object, e As EventArgs) Handles btnImportConfig.Click
        AppLogger.Log("Botón 'Importar Configuración' presionado.", "UI")
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Archivos XML (*.xml)|*.xml"
            ofd.Title = "Seleccionar Archivo de Configuración de Servidores"
            If ofd.ShowDialog() = DialogResult.OK Then
                Try
                    backupManager.LoadServers(ofd.FileName)
                    servers = backupManager.GetServers()
                    DisplayServers()
                    MessageBox.Show("Configuración importada con éxito.", "Importar", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    AppLogger.Log($"Configuración importada desde: {ofd.FileName}", "UI")
                Catch ex As Exception
                    MessageBox.Show($"Error al importar la configuración: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    AppLogger.Log($"Error al importar la configuración: {ex.ToString()}", "ERROR")
                End Try
            Else
                AppLogger.Log("Importación de configuración cancelada.", "UI")
            End If
        End Using
    End Sub

#Region "Eventos de Respaldo"

    Private Sub dgvServers_SelectionChanged(sender As Object, e As EventArgs) Handles dgvServers.SelectionChanged
        If dgvServers.SelectedRows.Count > 0 Then
            Dim selectedDisplayInfo = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo)
            DisplayEventos(selectedDisplayInfo.Server)
        Else
            DisplayEventos(Nothing)
        End If
    End Sub

    Private Sub DisplayEventos(server As Server)
        dgvEventos.DataSource = Nothing ' Limpiar el DataSource
        grpEventos.Enabled = (server IsNot Nothing)

        If server IsNot Nothing Then
            ' Ordenar los eventos por fecha y hora
            Dim sortedEvents = server.Eventos.OrderBy(Function(ev) ev.FechaHora).ToList()
            dgvEventos.DataSource = sortedEvents
            dgvEventos.ClearSelection() ' Deseleccionar cualquier fila por defecto
        End If

    End Sub

    Private Sub btnAnadirEvento_Click(sender As Object, e As EventArgs) Handles btnAnadirEvento.Click
        If dgvServers.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor primero.", "Servidor no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo).Server

        Using form As New FormEditorEvento()
            If form.ShowDialog(Me) = DialogResult.OK Then
                selectedServer.Eventos.Add(form.Evento)
                backupManager.SaveServers(configFilePath)
                DisplayEventos(selectedServer)
                AppLogger.Log($"Evento añadido para el servidor '{selectedServer.Name}'", "UI")
            End If
        End Using
    End Sub

    Private Sub btnEditarEvento_Click(sender As Object, e As EventArgs) Handles btnEditarEvento.Click
        If dgvServers.SelectedRows.Count = 0 OrElse dgvEventos.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor y un evento para editar.", "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo).Server
        Dim selectedEvento = CType(dgvEventos.SelectedRows(0).DataBoundItem, EventoRespaldo)

        Using form As New FormEditorEvento(selectedEvento)
            If form.ShowDialog(Me) = DialogResult.OK Then
                ' El objeto se modifica por referencia, solo necesitamos guardar y refrescar
                backupManager.SaveServers(configFilePath)
                DisplayEventos(selectedServer)
                AppLogger.Log($"Evento actualizado para el servidor '{selectedServer.Name}'", "UI")
            End If
        End Using
    End Sub

    Private Sub btnEliminarEvento_Click(sender As Object, e As EventArgs) Handles btnEliminarEvento.Click
        If dgvServers.SelectedRows.Count = 0 OrElse dgvEventos.SelectedRows.Count = 0 Then
            MessageBox.Show("Por favor, seleccione un servidor y un evento para eliminar.", "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If MessageBox.Show("¿Está seguro de que desea eliminar el evento seleccionado?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then
            Return
        End If

        Dim selectedServer = CType(dgvServers.SelectedRows(0).DataBoundItem, ServerDisplayInfo).Server
        Dim selectedEvento = CType(dgvEventos.SelectedRows(0).DataBoundItem, EventoRespaldo)

        selectedServer.Eventos.Remove(selectedEvento)
        backupManager.SaveServers(configFilePath)
        DisplayEventos(selectedServer)
        AppLogger.Log($"Evento eliminado del servidor '{selectedServer.Name}'", "UI")
    End Sub




#End Region



End Class