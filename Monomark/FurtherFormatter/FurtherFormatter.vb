Option Strict On

Imports System.Collections.Generic
Imports Windows.UI.Text
Imports Windows.UI
Imports Markdig
Imports Markdig.Syntax
Imports Markdig.Syntax.Inlines
Imports Markdig.Extensions.EmphasisExtras

Namespace FurtherFormatter
    Public Class Formatter

        Private ReadOnly LINK_COLOR As Color = Color.FromArgb(255, 0, 0, 255)
        Private ReadOnly CODE_COLOR As Color = Color.FromArgb(255, 0, 130, 0)
        Private ReadOnly HEAD_COLOR As Color = Color.FromArgb(255, 185, 0, 0)
        Private _CurrentDocument As String

        Public Property Document As ITextDocument
        Public Property CaretPosition As Integer

        Public Sub New(ByVal Document As ITextDocument)
            Me.Document = Document
        End Sub

        'Public Sub Format(Optional ByVal ForceUpdate As Boolean = False)
        '    If Document.Selection.StartPosition = CaretPosition And Not ForceUpdate Then
        '        Return
        '    End If

        '    CaretPosition = Document.Selection.StartPosition
        '    Dim Text As String
        '    Document.GetText(TextGetOptions.UseLf, Text)
        '    Dim MdDoc As MarkdownDocument = Markdown.Parse(Text, MARKDOWN_PIPELINE)
        '    Dim MdObjs = MdDoc.Descendants().Where(Function(f) f.Span.Start <= f.Span.End And f.Span.Length > 0)
        '    Document.GetRange(0, Text.Length).CharacterFormat = Document.GetDefaultCharacterFormat()
        '    For Each MdObj As MarkdownObject In MdObjs
        '        Dim TextRange As ITextRange = Document.GetRange(MdObj.Span.Start, MdObj.Span.End + 1)
        '        Dim CharFormat As ITextCharacterFormat = TextRange.CharacterFormat
        '        'If Not GetFormatType(MdObj, CharFormat).HasFlag(MdFormatType.None) Then
        '        '    TextRange.CharacterFormat = CharFormat
        '        'End If
        '        DoFormat(MdObj, CharFormat)
        '        'If FormatType.HasFlag(MdFormatType.Heading1) AndAlso
        '        '    TextRange.EndPosition >= (Text.Length + GetMarkdownCharacters(MdFormatType.Heading1).HalfLength) AndAlso
        '        '    Text.Substring(TextRange.EndPosition, GetMarkdownCharacters(MdFormatType.Heading1).HalfLength) = GetMarkdownCharacters(MdFormatType.Heading1).Back Then
        '        '    TextRange.EndPosition += GetMarkdownCharacters(MdFormatType.Heading1).HalfLength
        '        '    DoFormat(MdObj, TextRange.CharacterFormat)
        '        'End If
        '    Next
        'End Sub

        Public Sub Format(ByVal Optional ForceUpdate As Boolean = False)
            Dim Text As String : Document.GetText(TextGetOptions.UseLf, Text)
            If Not ForceUpdate AndAlso _CurrentDocument = Text Then Return
            _CurrentDocument = Text
            Dim MdDoc As MarkdownDocument = Markdown.Parse(Text, MARKDOWN_PIPELINE)
            Dim MdObjs As List(Of MarkdownObject) = MdDoc.Descendants().Where(Function(f) f.Span.Start <= f.Span.End And f.Span.Length > 0).ToList()
            Dim TempList As New List(Of MdFormatType)
            For Each MdObj In MdObjs
                Dim TextRange As ITextRange = Document.GetRange(MdObj.Span.Start, MdObj.Span.End + 1)
                Dim CharFormat As ITextCharacterFormat = TextRange.CharacterFormat
                TempList.Add(DoFormat(MdObj, CharFormat))
            Next
            If TempList.All(Function(f) f = MdFormatType.None) Then
                Document.GetRange(0, Text.Length).CharacterFormat = Document.GetDefaultCharacterFormat()
                Document.SetDefaultCharacterFormat(Document.GetDefaultCharacterFormat())
            End If
        End Sub

        Private Function DoFormat(MdObj As MarkdownObject, ByRef CharFormat As ITextCharacterFormat) As MdFormatType
            If TypeOf MdObj Is EmphasisInline Then
                Dim Emphasis = CType(MdObj, EmphasisInline)
                If Emphasis.DelimiterCount = 2 Then
                    If CharFormat.Bold <> FormatEffect.On Then CharFormat.Bold = FormatEffect.On
                    If CharFormat.Italic = FormatEffect.On Then CharFormat.Italic = FormatEffect.Off
                    Return MdFormatType.Bold
                ElseIf Not Emphasis.DelimiterCount = 2 Then
                    If CharFormat.Italic <> FormatEffect.On Then CharFormat.Italic = FormatEffect.On
                    If CharFormat.Bold = FormatEffect.On Then CharFormat.Bold = FormatEffect.Off
                    Return MdFormatType.Italic
                End If
            End If
            If TypeOf MdObj Is HeadingBlock Then
                If CharFormat.Bold <> FormatEffect.On Then CharFormat.Bold = FormatEffect.On
                If CharFormat.ForegroundColor <> HEAD_COLOR Then CharFormat.ForegroundColor = HEAD_COLOR
                Return MdFormatType.Heading1 Or MdFormatType.Heading2 Or MdFormatType.Heading3
            End If
            If TypeOf MdObj Is CodeInline Or TypeOf MdObj Is CodeBlock Then
                If CharFormat.BackgroundColor <> CODE_COLOR Then CharFormat.ForegroundColor = CODE_COLOR
                Return MdFormatType.Code
            End If
            If TypeOf MdObj Is QuoteBlock Then
                If CharFormat.ForegroundColor <> CODE_COLOR Then CharFormat.ForegroundColor = CODE_COLOR
                Return MdFormatType.Quote
            End If
            If TypeOf MdObj Is LinkInline Then
                If CharFormat.ForegroundColor <> LINK_COLOR Then CharFormat.ForegroundColor = LINK_COLOR
                Return MdFormatType.Link
            End If
            CharFormat = Document.GetDefaultCharacterFormat()
            Return MdFormatType.None
        End Function

    End Class
End Namespace