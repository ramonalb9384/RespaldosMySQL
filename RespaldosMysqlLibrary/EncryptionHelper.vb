Imports System.Security.Cryptography
Imports System.Text

Public Class EncryptionHelper

    ' Un "salt" opcional para añadir una capa extra de seguridad. 
    ' Puede ser cualquier valor, pero debe ser el mismo para cifrar y descifrar.
    Private Shared ReadOnly salt As Byte() = Encoding.UTF8.GetBytes("RespaldosSalt")

    Public Shared Function Encrypt(plainText As String) As String
        If String.IsNullOrEmpty(plainText) Then
            Return String.Empty
        End If

        Try
            ' Convertir el texto plano a bytes
            Dim data As Byte() = Encoding.UTF8.GetBytes(plainText)

            ' Cifrar los datos usando DPAPI a nivel de máquina
            Dim encryptedData As Byte() = ProtectedData.Protect(data, salt, DataProtectionScope.LocalMachine)

            ' Devolver los datos cifrados como una cadena Base64 para un almacenamiento seguro en XML
            Return Convert.ToBase64String(encryptedData)
        Catch ex As CryptographicException
            AppLogger.Log($"Error de criptografía al encriptar: {ex.Message}", "ERROR")
            ' En caso de error, devolver el texto original podría ser un riesgo de seguridad.
            ' Es mejor lanzar la excepción para que la capa superior decida cómo manejarla.
            Throw
        End Try
    End Function

    Public Shared Function Decrypt(encryptedText As String) As String
        If String.IsNullOrEmpty(encryptedText) Then
            Return String.Empty
        End If

        Try
            ' Convertir la cadena Base64 de nuevo a bytes
            Dim encryptedData As Byte() = Convert.FromBase64String(encryptedText)

            ' Descifrar los datos usando DPAPI
            Dim data As Byte() = ProtectedData.Unprotect(encryptedData, salt, DataProtectionScope.LocalMachine)

            ' Devolver los datos descifrados como una cadena de texto plano
            Return Encoding.UTF8.GetString(data)
        Catch ex As FormatException
            ' Esto ocurre si la cadena no es un Base64 válido, lo que significa que probablemente no estaba encriptada.
            AppLogger.Log($"Error de formato al intentar desencriptar. La contraseña podría no estar encriptada. {ex.Message}", "WARNING")
            Return encryptedText ' Devolver el texto original
        Catch ex As CryptographicException
            AppLogger.Log($"Error de criptografía al desencriptar: {ex.Message}. ¿Se está ejecutando en la misma máquina donde se encriptó?", "ERROR")
            ' Si falla el descifrado, es un problema serio.
            Throw
        End Try
    End Function

End Class