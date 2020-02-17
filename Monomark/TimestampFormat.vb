Option Strict On
Imports Furthermark

Public Class TimestampFormat
    Implements INotifyPropertyChanged

    Private _FormatString As String

    Public Shared ReadOnly Now As DateTime = DateTime.Now

    Public Property FormatString As String
        Get
            Return _FormatString
        End Get
        Set(value As String)
            If _FormatString <> value Then
                _FormatString = value
                RaiseNotifyPropertyChanged(NameOf(FormatString))
                RaiseNotifyPropertyChanged(NameOf(FormattedDateTime))
            End If
        End Set
    End Property
    Public ReadOnly Property FormattedDateTime As String
        Get
            Return Now.ToString(FormatString)
        End Get
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Sub New(ByVal FormatString As String)
        Me.FormatString = FormatString
    End Sub

    Public Overrides Function ToString() As String
        Return FormattedDateTime
    End Function

    Private Sub RaiseNotifyPropertyChanged(ByVal Name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub
End Class
