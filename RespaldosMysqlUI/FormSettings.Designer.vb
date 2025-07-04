<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormSettings
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtMySqlDumpPath = New System.Windows.Forms.TextBox()
        Me.btnBrowseMySqlDump = New System.Windows.Forms.Button()
        Me.btnBrowseBackupPath = New System.Windows.Forms.Button()
        Me.txtBackupPath = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtSevenZipPath = New TextBox
        btnBrowseSevenZip = New Button
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 15)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(106, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Ruta mysqldump.exe:"
        '
        'txtMySqlDumpPath
        '
        Me.txtMySqlDumpPath.Location = New System.Drawing.Point(124, 12)
        Me.txtMySqlDumpPath.Name = "txtMySqlDumpPath"
        Me.txtMySqlDumpPath.Size = New System.Drawing.Size(280, 20)
        Me.txtMySqlDumpPath.TabIndex = 1
        '
        'btnBrowseMySqlDump
        '
        Me.btnBrowseMySqlDump.Location = New System.Drawing.Point(410, 10)
        Me.btnBrowseMySqlDump.Name = "btnBrowseMySqlDump"
        Me.btnBrowseMySqlDump.Size = New System.Drawing.Size(75, 23)
        Me.btnBrowseMySqlDump.TabIndex = 2
        Me.btnBrowseMySqlDump.Text = "Examinar..."
        Me.btnBrowseMySqlDump.UseVisualStyleBackColor = True
        '
        'btnBrowseBackupPath
        '
        Me.btnBrowseBackupPath.Location = New System.Drawing.Point(410, 49)
        Me.btnBrowseBackupPath.Name = "btnBrowseBackupPath"
        Me.btnBrowseBackupPath.Size = New System.Drawing.Size(75, 23)
        Me.btnBrowseBackupPath.TabIndex = 5
        Me.btnBrowseBackupPath.Text = "Examinar..."
        Me.btnBrowseBackupPath.UseVisualStyleBackColor = True
        '
        'txtBackupPath
        '
        Me.txtBackupPath.Location = New System.Drawing.Point(124, 51)
        Me.txtBackupPath.Name = "txtBackupPath"
        Me.txtBackupPath.Size = New System.Drawing.Size(280, 20)
        Me.txtBackupPath.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 54)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(106, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Ruta de Respaldos:"
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(329, 133)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 9
        Me.btnSave.Text = "Guardar"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Location = New System.Drawing.Point(410, 133)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(75, 23)
        Me.btnCancel.TabIndex = 10
        Me.btnCancel.Text = "Cancelar"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "mysqldump.exe"
        Me.OpenFileDialog1.Filter = "Ejecutables|*.exe|Todos los archivos|*.*"
        Me.OpenFileDialog1.Title = "Seleccionar mysqldump.exe"
        '
        'FolderBrowserDialog1
        '
        Me.FolderBrowserDialog1.Description = "Seleccionar carpeta de destino para los respaldos"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 93)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(80, 13)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Ruta 7z.exe:"
        '
        'txtSevenZipPath
        '
        Me.txtSevenZipPath.Location = New System.Drawing.Point(124, 90)
        Me.txtSevenZipPath.Name = "txtSevenZipPath"
        Me.txtSevenZipPath.Size = New System.Drawing.Size(280, 20)
        Me.txtSevenZipPath.TabIndex = 7
        '
        'btnBrowseSevenZip
        '
        Me.btnBrowseSevenZip.Location = New System.Drawing.Point(410, 88)
        Me.btnBrowseSevenZip.Name = "btnBrowseSevenZip"
        Me.btnBrowseSevenZip.Size = New System.Drawing.Size(75, 23)
        Me.btnBrowseSevenZip.TabIndex = 8
        Me.btnBrowseSevenZip.Text = "Examinar..."
        Me.btnBrowseSevenZip.UseVisualStyleBackColor = True
        '
        'FormSettings
        '
        Me.AcceptButton = Me.btnSave
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancel
        Me.ClientSize = New System.Drawing.Size(497, 170)
        Me.Controls.Add(Me.btnBrowseSevenZip)
        Me.Controls.Add(Me.txtSevenZipPath)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnBrowseBackupPath)
        Me.Controls.Add(Me.txtBackupPath)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btnBrowseMySqlDump)
        Me.Controls.Add(Me.txtMySqlDumpPath)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Configuración de la Aplicación"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents txtMySqlDumpPath As TextBox
    Friend WithEvents btnBrowseMySqlDump As Button
    Friend WithEvents btnBrowseBackupPath As Button
    Friend WithEvents txtBackupPath As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents btnSave As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents Label3 As Label
    Friend WithEvents txtSevenZipPath As TextBox
    Friend WithEvents btnBrowseSevenZip As Button

End Class