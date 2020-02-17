Option Strict On
Imports Windows.Storage

Public Class Program
    Private Shared Instances As IList(Of AppInstance)

    Shared Sub Main(ByVal args As String())
        Instances = AppInstance.GetInstances()
        Dim ActivatedArgs As IActivatedEventArgs = AppInstance.GetActivatedEventArgs()
        Dim FileArgs = TryCast(ActivatedArgs, FileActivatedEventArgs)
        If Not FileArgs Is Nothing Then
            Dim File As IStorageItem = FileArgs.Files.FirstOrDefault()
            If Not File Is Nothing Then
                Dim Instance = AppInstance.FindOrRegisterInstanceForKey(File.Name)
                If Instance.IsCurrentInstance Then
                    Global.Windows.UI.Xaml.Application.Start(Function(p) New App())
                Else
                    Instance.RedirectActivationTo()
                End If
            End If
        Else
            ' Test for recommended instance
            If Not AppInstance.RecommendedInstance Is Nothing Then
                AppInstance.RecommendedInstance.RedirectActivationTo()
            Else
                ' Start new instance
                Global.Windows.UI.Xaml.Application.Start(Function(p) New App())
            End If
        End If
    End Sub
End Class