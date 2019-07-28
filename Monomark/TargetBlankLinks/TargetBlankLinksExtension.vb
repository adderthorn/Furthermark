Option Strict On

Imports Markdig
Imports Markdig.Parsers.Inlines
Imports Markdig.Renderers
Imports Markdig.Renderers.Normalize
Imports Markdig.Renderers.Normalize.Inlines

Namespace TargetBlankLinks
    Public Class TargetBlankLinksExtension
        Implements IMarkdownExtension

        Public Sub Setup(pipeline As MarkdownPipelineBuilder) Implements IMarkdownExtension.Setup
            If Not pipeline.InlineParsers.Contains(Of TargetBlankLinkInlineParser)() Then
                pipeline.InlineParsers.InsertBefore(Of LinkInlineParser)(New TargetBlankLinkInlineParser())
            End If
        End Sub

        Public Sub Setup(pipeline As MarkdownPipeline, renderer As IMarkdownRenderer) Implements IMarkdownExtension.Setup

        End Sub

    End Class

    Public Module TargetBlankLinksExtensionFunctions
        <Extension()>
        Public Function UseTargetBlankLinks(ByVal aPipeline As MarkdownPipelineBuilder) As MarkdownPipelineBuilder
            If Not aPipeline.Extensions.Contains(Of TargetBlankLinksExtension)() Then
                aPipeline.Extensions.Add(New TargetBlankLinksExtension())
            End If
            Return aPipeline
        End Function
    End Module
End Namespace