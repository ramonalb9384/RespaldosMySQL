Imports System.ComponentModel
Imports System.Configuration.Install
Imports System.ServiceProcess

<RunInstaller(True)>
Public Class ProjectInstaller
    Inherits Installer

    Private serviceProcessInstaller As ServiceProcessInstaller
    Private serviceInstaller As ServiceInstaller

    Public Sub New()
        serviceProcessInstaller = New ServiceProcessInstaller()
        serviceInstaller = New ServiceInstaller()

        ' ServiceProcessInstaller configuration
        serviceProcessInstaller.Account = ServiceAccount.LocalSystem
        serviceProcessInstaller.Password = Nothing
        serviceProcessInstaller.Username = Nothing

        ' ServiceInstaller configuration
        serviceInstaller.ServiceName = "RespaldosMysqlService"
        serviceInstaller.DisplayName = "Admin Servicio de Respaldos MySQL"
        serviceInstaller.Description = "Ejecuta respaldos programados de bases de datos MySQL."
        serviceInstaller.StartType = ServiceStartMode.Automatic

        ' Add installers to collection
        Installers.Add(serviceProcessInstaller)
        Installers.Add(serviceInstaller)
    End Sub

End Class
