Option Strict On

Imports System
Imports System.Text
Imports Markdig.Helpers
Imports Markdig.Parsers
Imports Markdig.Renderers.Html
Imports Markdig.Syntax.Inlines

Namespace TargetBlankLinks
    Public Class TargetBlankLinkInlineParser
        Inherits InlineParser

        Public Overrides Function Match(processor As InlineProcessor, ByRef slice As StringSlice) As Boolean
            'processor.
            'Dim Pc = slice.PeekCharExtra(-1)
            'If Not Pc.IsWhiteSpaceOrZero() And Pc <> CChar("(") Then
            '    Return False
            'End If

            'Dim CurrentChar As Char = slice.CurrentChar
            'While CurrentChar <> CChar(")") Or CurrentChar.IsWhiteSpaceOrZero()
            '    CurrentChar = slice.NextChar()
            'End While
            If TypeOf processor.Inline Is LinkInline Then
                Dim Link As LinkInline = CType(processor.Inline, LinkInline)
                Link.GetAttributes().AddProperty("target", "blank")
                processor.Inline = Link
                Return True
            End If
            Return False
        End Function

    End Class
End Namespace