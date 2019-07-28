Option Strict On

Imports Markdig
Imports Markdig.Syntax
Imports Markdig.Syntax.Inlines

Public Enum FormatOptions
    None
    Bold
    Italic
    Grey
    Blue
End Enum

Public Class CharacterMapping
    Public ReadOnly Property Format As FormatOptions
    Public ReadOnly Property StartPosition As Integer
    Public ReadOnly Property EndPosition As Integer

    Public Sub New(ByVal MarkdownObject As MarkdownObject)
        Select Case MarkdownObject.GetType()
            Case GetType(EmphasisInline)
                Dim TypedObject As EmphasisInline = CType(MarkdownObject, EmphasisInline)
                If TypedObject.DelimiterCount = 2 Then
                    Format = FormatOptions.Bold
                Else
                    Format = FormatOptions.Italic
                End If
            Case Else
                Format = FormatOptions.None
        End Select
        StartPosition = MarkdownObject.Span.Start
        EndPosition = MarkdownObject.Span.End + 1
    End Sub

    Public Shared Function GetAllCharacterFormats(ByVal Text As String, ByVal MarkdownPipeline As MarkdownPipeline) As List(Of CharacterMapping)
        Dim MarkdownDocument = Markdown.Parse(Text, MarkdownPipeline)
        Dim FormatList As New List(Of CharacterMapping)
        For Each MarkdownObject In MarkdownDocument.Descendants()
            Dim Mapping As New CharacterMapping(MarkdownObject)
            FormatList.Add(Mapping)
        Next
        Return FormatList
    End Function
End Class
