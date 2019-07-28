' The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

Public NotInheritable Class TabControl
    Inherits UserControl
    Implements INotifyPropertyChanged

    Private _FileName As String
    Private Const HELLIP As String = "..."

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Property FileName As String
        Get
            If _FileName.Length > 20 Then
                Return _FileName.Substring(0, 20) & HELLIP
            End If
            Return _FileName
        End Get
        Set(value As String)
            If _FileName <> value Then
                _FileName = value
                RaisePropertyChanged(NameOf(FileName))
            End If
        End Set
    End Property

    Protected Sub RaisePropertyChanged(ByVal Name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub

    Private Sub CloseButton_Tapped(sender As Object, e As TappedRoutedEventArgs)
        'TODO: Implement this.
    End Sub
End Class
