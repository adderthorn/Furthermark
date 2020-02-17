Option Strict On
' The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.ApplicationModel
Imports Windows.Storage

Public NotInheritable Class AboutDialog
    Inherits ContentDialog
    Implements INotifyPropertyChanged

    Private _AboutText As String = String.Empty

    Public ReadOnly Property AppVersion As String
        Get
            Dim Version As PackageVersion = Package.Current.Id.Version
            Return String.Format("Version: {0}",
                                 String.Join(CChar("."), Version.Major, Version.Minor, Version.Build, Version.Revision))
        End Get
    End Property

    Public Property AboutText As String
        Get
            Return _AboutText
        End Get
        Set(value As String)
            If _AboutText <> value Then
                _AboutText = value
                RaisePropertyChanged(NameOf(AboutText))
            End If
        End Set
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        GetGplText()
    End Sub

    Private Sub ContentDialog_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        args.Cancel = False
    End Sub

    Private Async Sub GetGplText()
        Dim GplFile = Await StorageFile.GetFileFromApplicationUriAsync(New Uri("ms-appx:///WebAssets/GPLv3.txt"))
        Using Reader As New StreamReader((Await GplFile.OpenReadAsync()).AsStreamForRead())
            AboutText = Await Reader.ReadToEndAsync()
        End Using
    End Sub

    Private Sub RaisePropertyChanged(ByVal Name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub

    Private Async Sub HyperlinkButton_Click(sender As Object, e As RoutedEventArgs)
        Await Windows.System.Launcher.LaunchUriAsync(New Uri("http://www.furthermark.com/"))
    End Sub
End Class
