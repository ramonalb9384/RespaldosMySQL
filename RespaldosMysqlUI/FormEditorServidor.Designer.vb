<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormEditorServidor
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormEditorServidor))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.txtIP = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtPort = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtUser = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.lblExclusionInfo = New System.Windows.Forms.Label()
        Me.btnGetDbs = New System.Windows.Forms.Button()
        Me.clbDatabases = New System.Windows.Forms.CheckedListBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.chkScheduleEnabled = New System.Windows.Forms.CheckBox()
        Me.dtpScheduleTime = New System.Windows.Forms.DateTimePicker()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.pnlScheduleDays = New System.Windows.Forms.Panel()
        Me.chkDay0 = New System.Windows.Forms.CheckBox()
        Me.chkDay6 = New System.Windows.Forms.CheckBox()
        Me.chkDay5 = New System.Windows.Forms.CheckBox()
        Me.chkDay4 = New System.Windows.Forms.CheckBox()
        Me.chkDay3 = New System.Windows.Forms.CheckBox()
        Me.chkDay2 = New System.Windows.Forms.CheckBox()
        Me.chkDay1 = New System.Windows.Forms.CheckBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.pnlScheduleDays.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(17, 16)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(134, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Nombre de Conexión"
        '
        'txtName
        '
        Me.txtName.Location = New System.Drawing.Point(164, 12)
        Me.txtName.Margin = New System.Windows.Forms.Padding(4)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(331, 22)
        Me.txtName.TabIndex = 1
        '
        'txtIP
        '
        Me.txtIP.Location = New System.Drawing.Point(164, 44)
        Me.txtIP.Margin = New System.Windows.Forms.Padding(4)
        Me.txtIP.Name = "txtIP"
        Me.txtIP.Size = New System.Drawing.Size(209, 22)
        Me.txtIP.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(17, 48)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(57, 16)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "IP / Host"
        '
        'txtPort
        '
        Me.txtPort.Location = New System.Drawing.Point(428, 44)
        Me.txtPort.Margin = New System.Windows.Forms.Padding(4)
        Me.txtPort.Name = "txtPort"
        Me.txtPort.Size = New System.Drawing.Size(67, 22)
        Me.txtPort.TabIndex = 5
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(383, 48)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(30, 16)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Pto."
        '
        'txtUser
        '
        Me.txtUser.Location = New System.Drawing.Point(164, 76)
        Me.txtUser.Margin = New System.Windows.Forms.Padding(4)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(331, 22)
        Me.txtUser.TabIndex = 7
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(17, 80)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(54, 16)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "Usuario"
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(164, 108)
        Me.txtPassword.Margin = New System.Windows.Forms.Padding(4)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(331, 22)
        Me.txtPassword.TabIndex = 9
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(17, 112)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(76, 16)
        Me.Label5.TabIndex = 8
        Me.Label5.Text = "Contraseña"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.lblExclusionInfo)
        Me.GroupBox1.Controls.Add(Me.btnGetDbs)
        Me.GroupBox1.Controls.Add(Me.clbDatabases)
        Me.GroupBox1.Location = New System.Drawing.Point(16, 137)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(484, 270)
        Me.GroupBox1.TabIndex = 10
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Bases de Datos a Excluir"
        '
        'lblExclusionInfo
        '
        Me.lblExclusionInfo.ForeColor = System.Drawing.Color.Red
        Me.lblExclusionInfo.Location = New System.Drawing.Point(9, 21)
        Me.lblExclusionInfo.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblExclusionInfo.Name = "lblExclusionInfo"
        Me.lblExclusionInfo.Size = New System.Drawing.Size(459, 113)
        Me.lblExclusionInfo.TabIndex = 2
        Me.lblExclusionInfo.Text = resources.GetString("lblExclusionInfo.Text")
        Me.lblExclusionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnGetDbs
        '
        Me.btnGetDbs.Location = New System.Drawing.Point(12, 138)
        Me.btnGetDbs.Margin = New System.Windows.Forms.Padding(4)
        Me.btnGetDbs.Name = "btnGetDbs"
        Me.btnGetDbs.Size = New System.Drawing.Size(459, 28)
        Me.btnGetDbs.TabIndex = 1
        Me.btnGetDbs.Text = "Probar Conexión y Obtener Bases de Datos"
        Me.btnGetDbs.UseVisualStyleBackColor = True
        '
        'clbDatabases
        '
        Me.clbDatabases.FormattingEnabled = True
        Me.clbDatabases.Location = New System.Drawing.Point(11, 174)
        Me.clbDatabases.Margin = New System.Windows.Forms.Padding(4)
        Me.clbDatabases.Name = "clbDatabases"
        Me.clbDatabases.Size = New System.Drawing.Size(457, 89)
        Me.clbDatabases.TabIndex = 0
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.chkScheduleEnabled)
        Me.GroupBox2.Controls.Add(Me.dtpScheduleTime)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.pnlScheduleDays)
        Me.GroupBox2.Location = New System.Drawing.Point(25, 414)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Size = New System.Drawing.Size(475, 191)
        Me.GroupBox2.TabIndex = 11
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Programación de Respaldo"
        '
        'chkScheduleEnabled
        '
        Me.chkScheduleEnabled.AutoSize = True
        Me.chkScheduleEnabled.Location = New System.Drawing.Point(13, 26)
        Me.chkScheduleEnabled.Margin = New System.Windows.Forms.Padding(4)
        Me.chkScheduleEnabled.Name = "chkScheduleEnabled"
        Me.chkScheduleEnabled.Size = New System.Drawing.Size(221, 20)
        Me.chkScheduleEnabled.TabIndex = 4
        Me.chkScheduleEnabled.Text = "Habilitar Respaldo Programado"
        Me.chkScheduleEnabled.UseVisualStyleBackColor = True
        '
        'dtpScheduleTime
        '
        Me.dtpScheduleTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpScheduleTime.Location = New System.Drawing.Point(60, 161)
        Me.dtpScheduleTime.Margin = New System.Windows.Forms.Padding(4)
        Me.dtpScheduleTime.Name = "dtpScheduleTime"
        Me.dtpScheduleTime.ShowUpDown = True
        Me.dtpScheduleTime.Size = New System.Drawing.Size(112, 22)
        Me.dtpScheduleTime.TabIndex = 3
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(9, 165)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(40, 16)
        Me.Label7.TabIndex = 2
        Me.Label7.Text = "Hora:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(9, 56)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(120, 16)
        Me.Label6.TabIndex = 1
        Me.Label6.Text = "Días de Respaldo:"
        '
        'pnlScheduleDays
        '
        Me.pnlScheduleDays.Controls.Add(Me.chkDay0)
        Me.pnlScheduleDays.Controls.Add(Me.chkDay6)
        Me.pnlScheduleDays.Controls.Add(Me.chkDay5)
        Me.pnlScheduleDays.Controls.Add(Me.chkDay4)
        Me.pnlScheduleDays.Controls.Add(Me.chkDay3)
        Me.pnlScheduleDays.Controls.Add(Me.chkDay2)
        Me.pnlScheduleDays.Controls.Add(Me.chkDay1)
        Me.pnlScheduleDays.Location = New System.Drawing.Point(12, 75)
        Me.pnlScheduleDays.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlScheduleDays.Name = "pnlScheduleDays"
        Me.pnlScheduleDays.Size = New System.Drawing.Size(455, 78)
        Me.pnlScheduleDays.TabIndex = 0
        '
        'chkDay0
        '
        Me.chkDay0.Location = New System.Drawing.Point(3, 7)
        Me.chkDay0.Name = "chkDay0"
        Me.chkDay0.Size = New System.Drawing.Size(65, 24)
        Me.chkDay0.TabIndex = 0
        Me.chkDay0.Text = "Dom"
        Me.chkDay0.UseVisualStyleBackColor = True
        Me.chkDay0.AutoSize = True
        '
        'chkDay6
        '
        Me.chkDay6.AutoSize = True
        Me.chkDay6.Location = New System.Drawing.Point(363, 9)
        Me.chkDay6.Margin = New System.Windows.Forms.Padding(4)
        Me.chkDay6.Name = "chkDay6"
        Me.chkDay6.Size = New System.Drawing.Size(54, 20)
        Me.chkDay6.TabIndex = 5
        Me.chkDay6.Text = "Sáb"
        Me.chkDay6.UseVisualStyleBackColor = True
        '
        'chkDay5
        '
        Me.chkDay5.AutoSize = True
        Me.chkDay5.Location = New System.Drawing.Point(305, 9)
        Me.chkDay5.Margin = New System.Windows.Forms.Padding(4)
        Me.chkDay5.Name = "chkDay5"
        Me.chkDay5.Size = New System.Drawing.Size(49, 20)
        Me.chkDay5.TabIndex = 4
        Me.chkDay5.Text = "Vie"
        Me.chkDay5.UseVisualStyleBackColor = True
        '
        'chkDay4
        '
        Me.chkDay4.AutoSize = True
        Me.chkDay4.Location = New System.Drawing.Point(246, 9)
        Me.chkDay4.Margin = New System.Windows.Forms.Padding(4)
        Me.chkDay4.Name = "chkDay4"
        Me.chkDay4.Size = New System.Drawing.Size(51, 20)
        Me.chkDay4.TabIndex = 3
        Me.chkDay4.Text = "Jue"
        Me.chkDay4.UseVisualStyleBackColor = True
        '
        'chkDay3
        '
        Me.chkDay3.AutoSize = True
        Me.chkDay3.Location = New System.Drawing.Point(187, 9)
        Me.chkDay3.Margin = New System.Windows.Forms.Padding(4)
        Me.chkDay3.Name = "chkDay3"
        Me.chkDay3.Size = New System.Drawing.Size(51, 20)
        Me.chkDay3.TabIndex = 2
        Me.chkDay3.Text = "Mié"
        Me.chkDay3.UseVisualStyleBackColor = True
        '
        'chkDay2
        '
        Me.chkDay2.AutoSize = True
        Me.chkDay2.Location = New System.Drawing.Point(127, 9)
        Me.chkDay2.Margin = New System.Windows.Forms.Padding(4)
        Me.chkDay2.Name = "chkDay2"
        Me.chkDay2.Size = New System.Drawing.Size(52, 20)
        Me.chkDay2.TabIndex = 1
        Me.chkDay2.Text = "Mar"
        Me.chkDay2.UseVisualStyleBackColor = True
        '
        'chkDay1
        '
        Me.chkDay1.AutoSize = True
        Me.chkDay1.Location = New System.Drawing.Point(70, 9)
        Me.chkDay1.Margin = New System.Windows.Forms.Padding(4)
        Me.chkDay1.Name = "chkDay1"
        Me.chkDay1.Size = New System.Drawing.Size(50, 20)
        Me.chkDay1.TabIndex = 0
        Me.chkDay1.Text = "Lun"
        Me.chkDay1.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(292, 613)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 28)
        Me.btnSave.TabIndex = 12
        Me.btnSave.Text = "Guardar"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Location = New System.Drawing.Point(400, 613)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(100, 28)
        Me.btnCancel.TabIndex = 13
        Me.btnCancel.Text = "Cancelar"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'FormEditorServidor
        '
        Me.AcceptButton = Me.btnSave
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancel
        Me.ClientSize = New System.Drawing.Size(512, 654)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtPort)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtIP)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormEditorServidor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Editor de Servidor"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.pnlScheduleDays.ResumeLayout(False)
        Me.pnlScheduleDays.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents txtName As TextBox
    Friend WithEvents txtIP As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtPort As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txtUser As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents btnGetDbs As Button
    Friend WithEvents clbDatabases As CheckedListBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents dtpScheduleTime As DateTimePicker
    Friend WithEvents Label7 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents pnlScheduleDays As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents chkScheduleEnabled As CheckBox
    Friend WithEvents chkDay0 As CheckBox
    Friend WithEvents chkDay6 As CheckBox
    Friend WithEvents chkDay5 As CheckBox
    Friend WithEvents chkDay4 As CheckBox
    Friend WithEvents chkDay3 As CheckBox
    Friend WithEvents chkDay2 As CheckBox
    Friend WithEvents chkDay1 As CheckBox
    Friend WithEvents lblExclusionInfo As Label
End Class
