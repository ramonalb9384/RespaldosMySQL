Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class EncryptionHelper

    Private Const AES_KEY_SIZE As Integer = 256
    Private Const AES_BLOCK_SIZE As Integer = 128
    Private Const PBKDF2_ITERATIONS As Integer = 10000 ' Recommended iterations for PBKDF2

    Public Shared Function Encrypt(plainText As String, password As String) As String
        Using aesAlg As New AesCryptoServiceProvider()
            aesAlg.KeySize = AES_KEY_SIZE
            aesAlg.BlockSize = AES_BLOCK_SIZE
            aesAlg.Mode = CipherMode.CBC
            aesAlg.Padding = PaddingMode.PKCS7

            ' Generate a random salt for key derivation
            Dim salt(15) As Byte
            Using rng As New RNGCryptoServiceProvider()
                rng.GetBytes(salt)
            End Using

            ' Derive key and IV from password and salt
            Using rfc2898 As New Rfc2898DeriveBytes(password, salt, PBKDF2_ITERATIONS)
                aesAlg.Key = rfc2898.GetBytes(aesAlg.KeySize / 8)
                aesAlg.IV = rfc2898.GetBytes(aesAlg.BlockSize / 8)
            End Using

            Using encryptor As ICryptoTransform = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV)
                Using msEncrypt As New MemoryStream()
                    Using csEncrypt As New CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)
                        Using swEncrypt As New StreamWriter(csEncrypt)
                            swEncrypt.Write(plainText)
                        End Using
                        Dim encryptedBytes As Byte() = msEncrypt.ToArray()

                        ' Prepend salt to the encrypted bytes for decryption
                        Dim result(salt.Length + encryptedBytes.Length - 1) As Byte
                        Buffer.BlockCopy(salt, 0, result, 0, salt.Length)
                        Buffer.BlockCopy(encryptedBytes, 0, result, salt.Length, encryptedBytes.Length)

                        Return Convert.ToBase64String(result)
                    End Using
                End Using
            End Using
        End Using
    End Function

    Public Shared Function Decrypt(cipherText As String, password As String) As String
        Dim fullCipherBytes As Byte() = Convert.FromBase64String(cipherText)

        ' Extract salt from the beginning of the cipher bytes
        Dim salt(15) As Byte
        Buffer.BlockCopy(fullCipherBytes, 0, salt, 0, salt.Length)

        Dim encryptedBytes(fullCipherBytes.Length - salt.Length - 1) As Byte
        Buffer.BlockCopy(fullCipherBytes, salt.Length, encryptedBytes, 0, encryptedBytes.Length)

        Using aesAlg As New AesCryptoServiceProvider()
            aesAlg.KeySize = AES_KEY_SIZE
            aesAlg.BlockSize = AES_BLOCK_SIZE
            aesAlg.Mode = CipherMode.CBC
            aesAlg.Padding = PaddingMode.PKCS7

            ' Derive key and IV from password and salt
            Using rfc2898 As New Rfc2898DeriveBytes(password, salt, PBKDF2_ITERATIONS)
                aesAlg.Key = rfc2898.GetBytes(aesAlg.KeySize / 8)
                aesAlg.IV = rfc2898.GetBytes(aesAlg.BlockSize / 8)
            End Using

            Using decryptor As ICryptoTransform = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV)
                Using msDecrypt As New MemoryStream(encryptedBytes)
                    Using csDecrypt As New CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)
                        Using srDecrypt As New StreamReader(csDecrypt)
                            Return srDecrypt.ReadToEnd()
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Function

End Class
