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
        Me.txtLogOutput = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        CType(Me.dgvServers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
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
        Me.dgvServers.Size = New System.Drawing.Size(880, 203)
        Me.dgvServers.TabIndex = 0
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(16, 358)
        Me.btnAdd.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(140, 37)
        Me.btnAdd.TabIndex = 1
        Me.btnAdd.Text = "Añadir"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'btnEdit
        '
        Me.btnEdit.Location = New System.Drawing.Point(164, 358)
        Me.btnEdit.Margin = New System.Windows.Forms.Padding(4)
        Me.btnEdit.Name = "btnEdit"
        Me.btnEdit.Size = New System.Drawing.Size(140, 37)
        Me.btnEdit.TabIndex = 2
        Me.btnEdit.Text = "Editar"
        Me.btnEdit.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        Me.btnDelete.Location = New System.Drawing.Point(312, 358)
        Me.btnDelete.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(140, 37)
        Me.btnDelete.TabIndex = 3
        Me.btnDelete.Text = "Eliminar"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'btnBackupNow
        '
        Me.btnBackupNow.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBackupNow.Location = New System.Drawing.Point(616, 358)
        Me.btnBackupNow.Margin = New System.Windows.Forms.Padding(4)
        Me.btnBackupNow.Name = "btnBackupNow"
        Me.btnBackupNow.Size = New System.Drawing.Size(280, 37)
        Me.btnBackupNow.TabIndex = 4
        Me.btnBackupNow.Text = "Respaldar Seleccionado Ahora"
        Me.btnBackupNow.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 124)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(216, 20)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Servidores Configurados"
        '
        'btnSettings
        '
        Me.btnSettings.Location = New System.Drawing.Point(467, 358)
        Me.btnSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSettings.Name = "btnSettings"
        Me.btnSettings.Size = New System.Drawing.Size(140, 37)
        Me.btnSettings.TabIndex = 4
        Me.btnSettings.Text = "Configuración"
        Me.btnSettings.UseVisualStyleBackColor = True
        '
        'btnInstallService
        '
        Me.btnInstallService.Location = New System.Drawing.Point(293, 54)
        Me.btnInstallService.Name = "btnInstallService"
        Me.btnInstallService.Size = New System.Drawing.Size(120, 24)
        Me.btnInstallService.TabIndex = 5
        Me.btnInstallService.Text = "Instalar Servicio"
        Me.btnInstallService.UseVisualStyleBackColor = True
        '
        'btnUninstallService
        '
        Me.btnUninstallService.Location = New System.Drawing.Point(419, 54)
        Me.btnUninstallService.Name = "btnUninstallService"
        Me.btnUninstallService.Size = New System.Drawing.Size(120, 24)
        Me.btnUninstallService.TabIndex = 6
        Me.btnUninstallService.Text = "Desinstalar Servicio"
        Me.btnUninstallService.UseVisualStyleBackColor = True
        '
        'btnStartService
        '
        Me.btnStartService.Location = New System.Drawing.Point(16, 54)
        Me.btnStartService.Name = "btnStartService"
        Me.btnStartService.Size = New System.Drawing.Size(122, 24)
        Me.btnStartService.TabIndex = 8
        Me.btnStartService.Text = "Inicia el Servicio"
        Me.btnStartService.UseVisualStyleBackColor = True
        '
        'btnStopService
        '
        Me.btnStopService.Location = New System.Drawing.Point(144, 54)
        Me.btnStopService.Name = "btnStopService"
        Me.btnStopService.Size = New System.Drawing.Size(143, 24)
        Me.btnStopService.TabIndex = 8
        Me.btnStopService.Text = "Detener el Servicio"
        Me.btnStopService.UseVisualStyleBackColor = True
        '
        'lblServiceStatus
        '
        Me.lblServiceStatus.AutoSize = True
        Me.lblServiceStatus.Location = New System.Drawing.Point(13, 23)
        Me.lblServiceStatus.Name = "lblServiceStatus"
        Me.lblServiceStatus.Size = New System.Drawing.Size(109, 17)
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
        Me.GroupBox1.Location = New System.Drawing.Point(16, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(592, 84)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Servicio de windows"
        '
        'txtLogOutput
        '
        Me.txtLogOutput.Location = New System.Drawing.Point(12, 453)
        Me.txtLogOutput.Multiline = True
        Me.txtLogOutput.Name = "txtLogOutput"
        Me.txtLogOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtLogOutput.Size = New System.Drawing.Size(884, 179)
        Me.txtLogOutput.TabIndex = 10
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(13, 420)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(145, 20)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "Log del Servicio"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(912, 644)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtLogOutput)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSettings)
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
    Friend WithEvents txtLogOutput As TextBox
    Friend WithEvents Label3 As Label
End Class