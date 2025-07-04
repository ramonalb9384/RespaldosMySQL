
Imports MySql.Data.MySqlClient
Imports RespaldosMysqlLibrary

Public Class FormEditorServidor

    Public Property ServerData As Server
    Private isEditMode As Boolean



    Public Sub New(Optional serverToEdit As Server = Nothing)
        InitializeComponent()
        AppLogger.Log("FormEditorServidor abierto.", "UI")

        If serverToEdit IsNot Nothing Then
            isEditMode = True
            ServerData = serverToEdit
            Me.Text = "Editar Servidor"
            AppLogger.Log($"FormEditorServidor en modo edición para el servidor: {ServerData.Name}", "UI")
        Else
            isEditMode = False
            ServerData = New Server With {
                .Name = "",
                .IP = "",
                .Port = 3306, ' Puerto por defecto
                .User = "",
                .Password = "",
                .Databases = New List(Of String), ' This will now be empty as we're not selecting specific DBs to backup
                .ExcludedDatabases = New List(Of String), ' Initialize new list
                .Schedule = New ScheduleInfo With {.Enabled = False, .Days = New List(Of Integer), .Time = "22:00"}
            }
            Me.Text = "Añadir Servidor"
            AppLogger.Log("FormEditorServidor en modo añadir.", "UI")
        End If
    End Sub

    Private Sub FormEditorServidor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Cargar datos en los controles
        txtName.Text = ServerData.Name
        txtIP.Text = ServerData.IP
        txtPort.Text = ServerData.Port.ToString()
        txtUser.Text = ServerData.User
        txtPassword.Text = ServerData.Password

        ' Cargar configuración de la programación
        chkScheduleEnabled.Checked = ServerData.Schedule.Enabled
        dtpScheduleTime.Value = DateTime.Parse(ServerData.Schedule.Time)

        For Each chk As CheckBox In pnlScheduleDays.Controls.OfType(Of CheckBox)()
            Dim dayNumber As Integer
            If chk.Name.StartsWith("chkDay") AndAlso Integer.TryParse(chk.Name.Replace("chkDay", ""), dayNumber) Then
                If ServerData.Schedule.Days.Contains(dayNumber) Then
                    chk.Checked = True
                End If
            End If
        Next

        UpdateScheduleControls()

        ' Populate clbDatabases with all databases from the server and check excluded ones
        If isEditMode Then
            ' We need to get all databases from the server first, then mark the excluded ones.
            ' This will be handled by btnGetDbs_Click, so we can call it here.
            btnGetDbs_Click(sender, e)
        End If
    End Sub

    Private Sub btnGetDbs_Click(sender As Object, e As EventArgs) Handles btnGetDbs.Click
        AppLogger.Log("Attempting to get databases from server.", "UI")
        Dim connBuilder As New MySqlConnectionStringBuilder()
        connBuilder.Server = txtIP.Text
        connBuilder.Port = Integer.Parse(txtPort.Text)
        connBuilder.UserID = txtUser.Text
        connBuilder.Password = txtPassword.Text
        Dim tempConn As New MySqlConnection(connBuilder.ConnectionString)

        Try
            Cursor = Cursors.WaitCursor
            tempConn.Open()
            MessageBox.Show("Conexión exitosa!", "Prueba de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information)
            AppLogger.Log("Database connection successful.", "UI")

            Dim cmd As New MySqlCommand("SHOW DATABASES;", tempConn)
            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            clbDatabases.Items.Clear()
            Dim allDatabases As New List(Of String)
            While reader.Read()
                Dim dbName As String = reader.GetString(0)
                allDatabases.Add(dbName)
            End While

            ' Add all databases to the CheckedListBox and check the ones that are in ExcludedDatabases
            For Each dbName As String In allDatabases
                Dim isExcluded As Boolean = ServerData.ExcludedDatabases.Contains(dbName, StringComparer.OrdinalIgnoreCase)
                clbDatabases.Items.Add(dbName, isExcluded)
            Next
            AppLogger.Log($"Found {allDatabases.Count} databases. Populated exclusion list.", "UI")

        Catch ex As Exception
            MessageBox.Show($"Error al conectar o al obtener las bases de datos:{vbCrLf}{ex.Message}", "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AppLogger.Log($"Error connecting or getting databases: {ex.Message}", "ERROR")
        Finally
            If tempConn.State = ConnectionState.Open Then
                tempConn.Close()
            End If
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        AppLogger.Log("Save button clicked in FormEditorServidor.", "UI")
        ' Validaciones básicas
        If String.IsNullOrWhiteSpace(txtName.Text) OrElse String.IsNullOrWhiteSpace(txtIP.Text) OrElse String.IsNullOrWhiteSpace(txtPort.Text) OrElse String.IsNullOrWhiteSpace(txtUser.Text) Then
            MessageBox.Show("Por favor, complete todos los campos de conexión.", "Campos Incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AppLogger.Log("Save operation failed: Incomplete fields.", "WARNING")
            Return
        End If

        ' Recopilar datos
        ServerData.Name = txtName.Text
        ServerData.IP = txtIP.Text
        ServerData.Port = Integer.Parse(txtPort.Text)
        ServerData.User = txtUser.Text
        ServerData.Password = txtPassword.Text

        ' Clear existing Databases and populate ExcludedDatabases
        ServerData.Databases.Clear() ' No longer used for selection
        ServerData.ExcludedDatabases.Clear()
        For Each item As Object In clbDatabases.CheckedItems
            ServerData.ExcludedDatabases.Add(item.ToString())
        Next
        AppLogger.Log($"Excluded databases updated for server '{ServerData.Name}'. Count: {ServerData.ExcludedDatabases.Count}", "UI")

        ServerData.Schedule.Enabled = chkScheduleEnabled.Checked
        ServerData.Schedule.Time = dtpScheduleTime.Value.ToString("HH:mm")

        Dim selectedDays As New List(Of Integer)
        For Each chk As CheckBox In pnlScheduleDays.Controls.OfType(Of CheckBox)().Where(Function(c) c.Checked)
            Dim dayNumber As Integer
            If chk.Name.StartsWith("chkDay") AndAlso Integer.TryParse(chk.Name.Replace("chkDay", ""), dayNumber) Then
                selectedDays.Add(dayNumber)
            End If
        Next
        ServerData.Schedule.Days = selectedDays
        AppLogger.Log($"Schedule settings updated for server '{ServerData.Name}'. Enabled: {ServerData.Schedule.Enabled}, Days: {String.Join(",", ServerData.Schedule.Days)}, Time: {ServerData.Schedule.Time}", "UI")

        Me.DialogResult = DialogResult.OK
        Me.Close()
        AppLogger.Log($"FormEditorServidor closed. Server '{ServerData.Name}' data saved.", "UI")
    End Sub

    Private Sub chkScheduleEnabled_CheckedChanged(sender As Object, e As EventArgs) Handles chkScheduleEnabled.CheckedChanged
        UpdateScheduleControls()
    End Sub

    Private Sub UpdateScheduleControls()
        pnlScheduleDays.Enabled = chkScheduleEnabled.Checked
        dtpScheduleTime.Enabled = chkScheduleEnabled.Checked
    End Sub

End Class
