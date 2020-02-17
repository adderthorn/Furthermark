Option Strict On

Imports System.Text
Imports System.Collections.Generic
Imports System.Security.Cryptography
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
        Private _CurrentDocumentMd5 As Byte()
        Private _Document As ITextDocument

        Public Property Document As ITextDocument
            Get
                Return _Document
            End Get
            Private Set(value As ITextDocument)
                _Document = value
            End Set
        End Property
        Public Property CaretPosition As Integer

        Public Sub New(ByVal Document As ITextDocument)
            Me.Document = Document
            _CurrentDocumentMd5 = Nothing
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

        ''' <summary>
        ''' Formats the editor window. Will not update if no change in the document has been detected.
        ''' </summary>
        ''' <param name="ForceUpdate">Force the update.</param>
        Public Sub Format(ByVal Optional ForceUpdate As Boolean = False)
            Document.BeginUndoGroup()
            Dim Text As String : Document.GetText(TextGetOptions.UseLf, Text)
            Dim TextMd5 As Byte() = GetMd5(Text)
            If Not ForceUpdate AndAlso _CurrentDocumentMd5.SequenceEqual(TextMd5) Then Return
            _CurrentDocumentMd5 = TextMd5
            Dim MdDoc As MarkdownDocument = Markdown.Parse(Text, MARKDOWN_PIPELINE)
            Dim MdObjs As List(Of MarkdownObject) = MdDoc.Descendants().Where(Function(f) f.Span.Start <= f.Span.End And f.Span.Length > 0).ToList()
            Dim TempList As New Dictionary(Of MarkdownObject, MdFormatType)
            Dim Ranges As New List(Of ITextRange)
            For Each MdObj In MdObjs
                Dim TextRange As ITextRange = Document.GetRange(MdObj.Span.Start, MdObj.Span.End + 1)
                If Not Ranges.Any(Function(t) TextRange.InRange(t)) Then
                    Dim ObjFormat = DoFormat(MdObj, TextRange)
                    If ObjFormat <> MdFormatType.None Then
                        Ranges.Add(TextRange)
                    End If
                    TempList.Add(MdObj, ObjFormat)
                End If
            Next
            If TempList.Count = 0 Or TempList.All(Function(f) f.Value = MdFormatType.None) Then
                Document.GetRange(0, Text.Length).CharacterFormat = Document.GetDefaultCharacterFormat()
                Document.SetDefaultCharacterFormat(Document.GetDefaultCharacterFormat())
            Else
                Document.Selection.CharacterFormat = Document.GetDefaultCharacterFormat()
            End If
            Document.EndUndoGroup()
        End Sub

        Private Function DoFormat(ByVal MdObj As MarkdownObject, ByRef TextRange As ITextRange) As MdFormatType
            If TypeOf MdObj Is EmphasisInline Then
                Dim Emphasis = CType(MdObj, EmphasisInline)
                If Emphasis.DelimiterCount = 2 Then
                    If TextRange.CharacterFormat.Bold <> FormatEffect.On Then TextRange.CharacterFormat.Bold = FormatEffect.On
                    If TextRange.CharacterFormat.Italic = FormatEffect.On Then TextRange.CharacterFormat.Italic = FormatEffect.Off
                    Return MdFormatType.Bold
                Else
                    If TextRange.CharacterFormat.Italic <> FormatEffect.On Then TextRange.CharacterFormat.Italic = FormatEffect.On
                    If TextRange.CharacterFormat.Bold = FormatEffect.On Then TextRange.CharacterFormat.Bold = FormatEffect.Off
                    Return MdFormatType.Italic
                End If
            End If
            If TypeOf MdObj Is HeadingBlock Then
                TextRange.EndOf(TextRangeUnit.Paragraph, extend:=True)
                If TextRange.CharacterFormat.Bold <> FormatEffect.On Then TextRange.CharacterFormat.Bold = FormatEffect.On
                If TextRange.CharacterFormat.ForegroundColor <> HEAD_COLOR Then TextRange.CharacterFormat.ForegroundColor = HEAD_COLOR
                Return MdFormatType.Heading1 Or MdFormatType.Heading2 Or MdFormatType.Heading3
            End If
            If TypeOf MdObj Is CodeInline Or TypeOf MdObj Is CodeBlock Then
                If TextRange.CharacterFormat.BackgroundColor <> CODE_COLOR Then TextRange.CharacterFormat.ForegroundColor = CODE_COLOR
                Return MdFormatType.Code
            End If
            If TypeOf MdObj Is QuoteBlock Then
                If TextRange.CharacterFormat.ForegroundColor <> CODE_COLOR Then TextRange.CharacterFormat.ForegroundColor = CODE_COLOR
                Return MdFormatType.Quote
            End If
            If TypeOf MdObj Is LinkInline Then
                If TextRange.CharacterFormat.ForegroundColor <> LINK_COLOR Then TextRange.CharacterFormat.ForegroundColor = LINK_COLOR
                Return MdFormatType.Link
            End If
            If TypeOf MdObj Is ContainerInline Or TypeOf MdObj Is LiteralInline Then
                TextRange.CharacterFormat = Document.GetDefaultCharacterFormat()
                Return MdFormatType.Plain
            End If
            Return MdFormatType.None
        End Function
    End Class
End Namespace