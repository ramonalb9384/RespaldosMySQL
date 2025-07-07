Public Class FormPassword
    Private _password As String

    Public ReadOnly Property Password As String
        Get
            Return _password
        End Get
    End Property

    Public Sub New(message As String)
        InitializeComponent()
        lblMessage.Text = message
        Me.Text = "Contraseña"
        txtPassword.UseSystemPasswordChar = True
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If String.IsNullOrWhiteSpace(txtPassword.Text) Then
            MessageBox.Show("La contraseña no puede estar vacía.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        _password = txtPassword.Text
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        _password = String.Empty
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
End Class