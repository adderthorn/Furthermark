Imports Windows.UI.Text
Imports Markdig
Imports Markdig.Syntax

Public Class FurtherDocument
    Implements ISaveable
    Implements IDisposable
    Implements INotifyPropertyChanged

    Private _IsDirty As Boolean = False

    Public Property Document As ITextDocument
    Public Property MarkdownDocument As MarkdownDocument
    Public Property MarkdownPipeline As MarkdownPipeline

    Public Property IsDirty As Boolean Implements ISaveable.IsDirty
        Get
            Return _IsDirty
        End Get
        Set(value As Boolean)
            If value <> _IsDirty Then
                _IsDirty = value
                RaisePropertyChanged(NameOf(IsDirty))
            End If
        End Set
    End Property

    Public Sub Clean() Implements ISaveable.Clean

    End Sub

    Public Async Function SaveAsync() As Task Implements ISaveable.SaveAsync
        Throw New NotImplementedException()
    End Function

#Region "Notify Property Changed"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub RaisePropertyChanged(ByVal Name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub
    Public Event Dirtied As EventHandler Implements ISaveable.Dirtied
    Protected Overridable Sub OnDirtied(ByVal e As EventArgs) Implements ISaveable.OnDirtied
        RaiseEvent Dirtied(Me, e)
    End Sub
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
