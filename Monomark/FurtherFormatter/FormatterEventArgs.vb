Namespace FurtherFormatter
    Public Class FormatterEventArgs
        Inherits EventArgs

        Public Property ForceUpdate As Boolean

        Public Sub New()
            ForceUpdate = False
        End Sub

        Public Sub New(ByVal ForceUpdate As Boolean)
            Me.ForceUpdate = ForceUpdate
        End Sub

        Public Overrides Function ToString() As String
            Return ForceUpdate.ToString()
        End Function
    End Class
End Namespace