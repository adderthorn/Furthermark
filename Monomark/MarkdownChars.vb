Public Class MarkdownChars
    Public Property Front As String
    Public Property Back As String
    Public Property HasNewlines As Boolean
    Public ReadOnly Property Length As Integer
        Get
            Return Front.Length + Back.Length
        End Get
    End Property
    Public ReadOnly Property HalfLength As Integer
        Get
            Return Front.Length
        End Get
    End Property

    Public Sub New()
    End Sub

    Public Sub New(ByVal FrontAndBack As String)
        Front = FrontAndBack
        Back = FrontAndBack
    End Sub

    Public Sub New(ByVal Front As String, ByVal Back As String)
        Me.Front = Front
        Me.Back = Back
    End Sub
End Class
