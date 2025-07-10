Imports RespaldosMysqlLibrary
Imports System.Globalization

Public Class FormEditorEvento

    Public Evento As EventoRespaldo

    Public Sub New(Optional eventoEditable As EventoRespaldo = Nothing)
        InitializeComponent()

        If eventoEditable IsNot Nothing Then
            Me.Evento = eventoEditable
            Me.Text = "Editar Evento de Respaldo"
            dtpFecha.Value = eventoEditable.FechaHora.Date
            txtHora.Text = eventoEditable.FechaHora.ToString("HH:mm")
            txtDescripcion.Text = eventoEditable.Descripcion
        Else
            Me.Evento = New EventoRespaldo()
            Me.Text = "Nuevo Evento de Respaldo"
            dtpFecha.Value = DateTime.Now
            txtHora.Text = DateTime.Now.ToString("HH:mm")
        End If

        AddHandler btnGuardar.Click, AddressOf BtnGuardar_Click
        AddHandler btnCancelar.Click, AddressOf BtnCancelar_Click
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs)
        Dim horaValida As DateTime
        If Not DateTime.TryParseExact(txtHora.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, horaValida) Then
            MessageBox.Show("El formato de la hora no es válido. Use HH:mm.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim fechaSeleccionada As DateTime = dtpFecha.Value.Date
        Dim horaSeleccionada As TimeSpan = horaValida.TimeOfDay

        Evento.FechaHora = fechaSeleccionada + horaSeleccionada
        Evento.Descripcion = txtDescripcion.Text

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs)
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
