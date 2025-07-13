<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.dgvServers = New System.Windows.Forms.DataGridView()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.btnEdit = New System.Windows.Forms.Button()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnBackupNow = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnSettings = New System.Windows.Forms.Button()
        Me.btnInstallService = New System.Windows.Forms.Button()
        Me.btnUninstallService = New System.Windows.Forms.Button()
        Me.btnStartService = New System.Windows.Forms.Button()
        Me.btnStopService = New System.Windows.Forms.Button()
        Me.lblServiceStatus = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtLogOutput = New System.Windows.Forms.RichTextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lblProgressStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.GroupBoxConfig = New System.Windows.Forms.GroupBox()
        Me.btnExportConfig = New System.Windows.Forms.Button()
        Me.btnImportConfig = New System.Windows.Forms.Button()
        Me.btnAnadirEvento = New System.Windows.Forms.Button()
        Me.btnEditarEvento = New System.Windows.Forms.Button()
        Me.btnEliminarEvento = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.grpEventos = New System.Windows.Forms.Panel()
        Me.dgvEventos = New System.Windows.Forms.DataGridView()
        CType(Me.dgvServers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.GroupBoxConfig.SuspendLayout()
        Me.grpEventos.SuspendLayout()
        CType(Me.dgvEventos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvServers
        '
        Me.dgvServers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvServers.Location = New System.Drawing.Point(16, 148)
        Me.dgvServers.Margin = New System.Windows.Forms.Padding(4)
        Me.dgvServers.Name = "dgvServers"
        Me.dgvServers.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
        Me.dgvServers.Size = New System.Drawing.Size(616, 203)
        Me.dgvServers.TabIndex = 0
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(16, 358)
        Me.btnAdd.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(100, 37)
        Me.btnAdd.TabIndex = 1
        Me.btnAdd.Text = "Añadir"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'btnEdit
        '
        Me.btnEdit.Location = New System.Drawing.Point(124, 358)
        Me.btnEdit.Margin = New System.Windows.Forms.Padding(4)
        Me.btnEdit.Name = "btnEdit"
        Me.btnEdit.Size = New System.Drawing.Size(100, 37)
        Me.btnEdit.TabIndex = 2
        Me.btnEdit.Text = "Editar"
        Me.btnEdit.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        Me.btnDelete.Location = New System.Drawing.Point(232, 358)
        Me.btnDelete.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(100, 37)
        Me.btnDelete.TabIndex = 3
        Me.btnDelete.Text = "Eliminar"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'btnBackupNow
        '
        Me.btnBackupNow.Location = New System.Drawing.Point(340, 359)
        Me.btnBackupNow.Margin = New System.Windows.Forms.Padding(4)
        Me.btnBackupNow.Name = "btnBackupNow"
        Me.btnBackupNow.Size = New System.Drawing.Size(292, 37)
        Me.btnBackupNow.TabIndex = 4
        Me.btnBackupNow.Text = "Respaldar Ahora el Servidor Seleccionado"
        Me.btnBackupNow.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 124)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(156, 16)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Servidores Configurados"
        '
        'btnSettings
        '
        Me.btnSettings.Location = New System.Drawing.Point(177, 47)
        Me.btnSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSettings.Name = "btnSettings"
        Me.btnSettings.Size = New System.Drawing.Size(121, 37)
        Me.btnSettings.TabIndex = 4
        Me.btnSettings.Text = "Configuración"
        Me.btnSettings.UseVisualStyleBackColor = True
        '
        'btnInstallService
        '
        Me.btnInstallService.Location = New System.Drawing.Point(293, 54)
        Me.btnInstallService.Name = "btnInstallService"
        Me.btnInstallService.Size = New System.Drawing.Size(120, 30)
        Me.btnInstallService.TabIndex = 5
        Me.btnInstallService.Text = "Instalar Servicio"
        Me.btnInstallService.UseVisualStyleBackColor = True
        '
        'btnUninstallService
        '
        Me.btnUninstallService.Location = New System.Drawing.Point(419, 54)
        Me.btnUninstallService.Name = "btnUninstallService"
        Me.btnUninstallService.Size = New System.Drawing.Size(120, 30)
        Me.btnUninstallService.TabIndex = 6
        Me.btnUninstallService.Text = "Desinstalar Servicio"
        Me.btnUninstallService.UseVisualStyleBackColor = True
        '
        'btnStartService
        '
        Me.btnStartService.Location = New System.Drawing.Point(16, 54)
        Me.btnStartService.Name = "btnStartService"
        Me.btnStartService.Size = New System.Drawing.Size(122, 30)
        Me.btnStartService.TabIndex = 8
        Me.btnStartService.Text = "Inicia el Servicio"
        Me.btnStartService.UseVisualStyleBackColor = True
        '
        'btnStopService
        '
        Me.btnStopService.Location = New System.Drawing.Point(144, 54)
        Me.btnStopService.Name = "btnStopService"
        Me.btnStopService.Size = New System.Drawing.Size(143, 30)
        Me.btnStopService.TabIndex = 8
        Me.btnStopService.Text = "Detener el Servicio"
        Me.btnStopService.UseVisualStyleBackColor = True
        '
        'lblServiceStatus
        '
        Me.lblServiceStatus.AutoSize = True
        Me.lblServiceStatus.Location = New System.Drawing.Point(13, 23)
        Me.lblServiceStatus.Name = "lblServiceStatus"
        Me.lblServiceStatus.Size = New System.Drawing.Size(104, 16)
        Me.lblServiceStatus.TabIndex = 7
        Me.lblServiceStatus.Text = "lblServiceStatus"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.btnStartService)
        Me.GroupBox1.Controls.Add(Me.lblServiceStatus)
        Me.GroupBox1.Controls.Add(Me.btnStopService)
        Me.GroupBox1.Controls.Add(Me.btnUninstallService)
        Me.GroupBox1.Controls.Add(Me.btnInstallService)
        Me.GroupBox1.Location = New System.Drawing.Point(17, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(592, 109)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Servicio de windows"
        '
        'txtLogOutput
        '
        Me.txtLogOutput.Location = New System.Drawing.Point(12, 453)
        Me.txtLogOutput.Name = "txtLogOutput"
        Me.txtLogOutput.Size = New System.Drawing.Size(1162, 179)
        Me.txtLogOutput.TabIndex = 11
        Me.txtLogOutput.Text = ""
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(13, 420)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(104, 16)
        Me.Label3.TabIndex = 12
        Me.Label3.Text = "Log del Servicio"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblProgressStatus, Me.ProgressBar1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 655)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1185, 26)
        Me.StatusStrip1.TabIndex = 13
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'lblProgressStatus
        '
        Me.lblProgressStatus.Name = "lblProgressStatus"
        Me.lblProgressStatus.Size = New System.Drawing.Size(153, 20)
        Me.lblProgressStatus.Text = "ToolStripStatusLabel1"
        Me.lblProgressStatus.Visible = False
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(200, 18)
        Me.ProgressBar1.Visible = False
        '
        'GroupBoxConfig
        '
        Me.GroupBoxConfig.Controls.Add(Me.btnExportConfig)
        Me.GroupBoxConfig.Controls.Add(Me.btnImportConfig)
        Me.GroupBoxConfig.Controls.Add(Me.btnSettings)
        Me.GroupBoxConfig.Location = New System.Drawing.Point(865, 12)
        Me.GroupBoxConfig.Name = "GroupBoxConfig"
        Me.GroupBoxConfig.Size = New System.Drawing.Size(309, 109)
        Me.GroupBoxConfig.TabIndex = 10
        Me.GroupBoxConfig.TabStop = False
        Me.GroupBoxConfig.Text = "Configuración"
        '
        'btnExportConfig
        '
        Me.btnExportConfig.Location = New System.Drawing.Point(14, 47)
        Me.btnExportConfig.Name = "btnExportConfig"
        Me.btnExportConfig.Size = New System.Drawing.Size(75, 37)
        Me.btnExportConfig.TabIndex = 0
        Me.btnExportConfig.Text = "Exportar"
        Me.btnExportConfig.UseVisualStyleBackColor = True
        '
        'btnImportConfig
        '
        Me.btnImportConfig.Location = New System.Drawing.Point(95, 47)
        Me.btnImportConfig.Name = "btnImportConfig"
        Me.btnImportConfig.Size = New System.Drawing.Size(75, 37)
        Me.btnImportConfig.TabIndex = 1
        Me.btnImportConfig.Text = "Importar"
        Me.btnImportConfig.UseVisualStyleBackColor = True
        '
        'btnAnadirEvento
        '
        Me.btnAnadirEvento.Location = New System.Drawing.Point(12, 236)
        Me.btnAnadirEvento.Name = "btnAnadirEvento"
        Me.btnAnadirEvento.Size = New System.Drawing.Size(128, 36)
        Me.btnAnadirEvento.TabIndex = 15
        Me.btnAnadirEvento.Text = "Añadir Evento"
        Me.btnAnadirEvento.UseVisualStyleBackColor = True
        '
        'btnEditarEvento
        '
        Me.btnEditarEvento.Location = New System.Drawing.Point(146, 235)
        Me.btnEditarEvento.Name = "btnEditarEvento"
        Me.btnEditarEvento.Size = New System.Drawing.Size(128, 36)
        Me.btnEditarEvento.TabIndex = 16
        Me.btnEditarEvento.Text = "Editar Evento"
        Me.btnEditarEvento.UseVisualStyleBackColor = True
        '
        'btnEliminarEvento
        '
        Me.btnEliminarEvento.Location = New System.Drawing.Point(280, 237)
        Me.btnEliminarEvento.Name = "btnEliminarEvento"
        Me.btnEliminarEvento.Size = New System.Drawing.Size(128, 34)
        Me.btnEliminarEvento.TabIndex = 17
        Me.btnEliminarEvento.Text = "Eliminar Evento"
        Me.btnEliminarEvento.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(9, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(328, 16)
        Me.Label4.TabIndex = 18
        Me.Label4.Text = "Respaldos por Evento para el Servidor Seleccionado"
        '
        'grpEventos
        '
        Me.grpEventos.Controls.Add(Me.dgvEventos)
        Me.grpEventos.Controls.Add(Me.Label4)
        Me.grpEventos.Controls.Add(Me.btnAnadirEvento)
        Me.grpEventos.Controls.Add(Me.btnEliminarEvento)
        Me.grpEventos.Controls.Add(Me.btnEditarEvento)
        Me.grpEventos.Location = New System.Drawing.Point(639, 124)
        Me.grpEventos.Name = "grpEventos"
        Me.grpEventos.Size = New System.Drawing.Size(546, 291)
        Me.grpEventos.TabIndex = 19
        '
        'dgvEventos
        '
        Me.dgvEventos.AllowUserToAddRows = False
        Me.dgvEventos.AllowUserToDeleteRows = False
        Me.dgvEventos.AllowUserToOrderColumns = True
        Me.dgvEventos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvEventos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvEventos.Location = New System.Drawing.Point(12, 24)
        Me.dgvEventos.MultiSelect = False
        Me.dgvEventos.Name = "dgvEventos"
        Me.dgvEventos.ReadOnly = True
        Me.dgvEventos.RowHeadersVisible = False
        Me.dgvEventos.RowHeadersWidth = 51
        Me.dgvEventos.RowTemplate.Height = 24
        Me.dgvEventos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvEventos.Size = New System.Drawing.Size(522, 203)
        Me.dgvEventos.TabIndex = 19
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1185, 681)
        Me.Controls.Add(Me.grpEventos)
        Me.Controls.Add(Me.GroupBoxConfig)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtLogOutput)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnBackupNow)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnEdit)
        Me.Controls.Add(Me.btnAdd)
        Me.Controls.Add(Me.dgvServers)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Gestor de Respaldos MySQL"
        CType(Me.dgvServers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.GroupBoxConfig.ResumeLayout(False)
        Me.grpEventos.ResumeLayout(False)
        Me.grpEventos.PerformLayout()
        CType(Me.dgvEventos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents dgvServers As DataGridView
    Friend WithEvents btnAdd As Button
    Friend WithEvents btnEdit As Button
    Friend WithEvents btnDelete As Button
    Friend WithEvents btnBackupNow As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents btnSettings As Button
    Friend WithEvents btnInstallService As Button
    Friend WithEvents btnUninstallService As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents btnStartService As Button
    Friend WithEvents btnStopService As Button
    Friend WithEvents lblServiceStatus As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtLogOutput As RichTextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ProgressBar1 As ToolStripProgressBar
    Friend WithEvents lblProgressStatus As ToolStripStatusLabel
    Friend WithEvents GroupBoxConfig As GroupBox
    Friend WithEvents btnExportConfig As Button
    Friend WithEvents btnImportConfig As Button
    Friend WithEvents btnAnadirEvento As Button
    Friend WithEvents btnEditarEvento As Button
    Friend WithEvents btnEliminarEvento As Button
    Friend WithEvents Label4 As Label
    Friend WithEvents grpEventos As Panel
    Friend WithEvents dgvEventos As DataGridView
End Class