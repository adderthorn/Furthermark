' The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.ApplicationModel.DataTransfer

Public NotInheritable Class EditCssDialog
    Inherits ContentDialog
    Implements INotifyPropertyChanged

    Private _OriginalCss As String
    Private _Css As String

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Public Property Css As String
        Get
            Return _Css
        End Get
        Set(value As String)
            If _Css <> value Then
                _Css = value
                RaisePropertyChanged(NameOf(Css))
            End If
        End Set
    End Property
    Public ReadOnly Property IsEdited As Boolean
        Get
            Return Not _Css.Equals(_OriginalCss, StringComparison.InvariantCultureIgnoreCase)
        End Get
    End Property
    Public Property Settings As Settings
    Public ReadOnly Property Md5 As Byte()
        Get
            Return GetMd5(Css, IgnoreCase:=True)
        End Get
    End Property

    Public Sub New(ByVal CssString As String, ByVal Settings As Settings)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _OriginalCss = CssString
        Css = CssString
        Me.Settings = Settings
    End Sub

    Private Sub ContentDialog_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        '_IsEdited = Not (Css.Trim().Equals(_OriginalCss.Trim(), StringComparison.OrdinalIgnoreCase))
        args.Cancel = False
    End Sub

    Private Sub ContentDialog_CloseButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        args.Cancel = False
        _Css = _OriginalCss
    End Sub

    Private Sub RaisePropertyChanged(ByVal Name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub

    Private Async Sub ContentDialog_SecondaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        ' Cancel the default behavior
        args.Cancel = True
        Css = Await LoadDefaultCssAsync()
    End Sub

    Private Async Sub CssTextBox_Paste(sender As Object, e As TextControlPasteEventArgs)
        e.Handled = True
        Dim DataPkgView As DataPackageView = Clipboard.GetContent()
        If DataPkgView.Contains(StandardDataFormats.Text) Then
            CssTextBox.IsEnabled = False
            Css = Await DataPkgView.GetTextAsync()
            CssTextBox.IsEnabled = True
        End If
    End Sub
End Class
