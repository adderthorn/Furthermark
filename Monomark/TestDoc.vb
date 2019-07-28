Option Strict On

Imports Windows.Storage.Streams
Imports Windows.UI.Text
Imports Markdig
Imports Markdig.Syntax
Imports Markdig.Syntax.Inlines

Public Class TestDoc
    Implements ITextDocument

    Private MarkdownDocument As MarkdownDocument

#Region "Implementation"
    Public Function CanCopy() As Boolean Implements ITextDocument.CanCopy
        Throw New NotImplementedException()
    End Function

    Public Function CanPaste() As Boolean Implements ITextDocument.CanPaste
        Throw New NotImplementedException()
    End Function

    Public Function CanRedo() As Boolean Implements ITextDocument.CanRedo
        Throw New NotImplementedException()
    End Function

    Public Function CanUndo() As Boolean Implements ITextDocument.CanUndo
        Throw New NotImplementedException()
    End Function

    Public Function ApplyDisplayUpdates() As Integer Implements ITextDocument.ApplyDisplayUpdates
        Throw New NotImplementedException()
    End Function

    Public Function BatchDisplayUpdates() As Integer Implements ITextDocument.BatchDisplayUpdates
        Throw New NotImplementedException()
    End Function

    Public Sub BeginUndoGroup() Implements ITextDocument.BeginUndoGroup
        Throw New NotImplementedException()
    End Sub

    Public Sub EndUndoGroup() Implements ITextDocument.EndUndoGroup
        Throw New NotImplementedException()
    End Sub

    Public Function GetDefaultCharacterFormat() As ITextCharacterFormat Implements ITextDocument.GetDefaultCharacterFormat
        Throw New NotImplementedException()
    End Function

    Public Function GetDefaultParagraphFormat() As ITextParagraphFormat Implements ITextDocument.GetDefaultParagraphFormat
        Throw New NotImplementedException()
    End Function

    Public Function GetRange(startPosition As Integer, endPosition As Integer) As ITextRange Implements ITextDocument.GetRange
        Throw New NotImplementedException()
    End Function

    Public Function GetRangeFromPoint(point As Point, options As PointOptions) As ITextRange Implements ITextDocument.GetRangeFromPoint
        Throw New NotImplementedException()
    End Function

    Public Sub GetText(options As TextGetOptions, ByRef value As String) Implements ITextDocument.GetText
        Throw New NotImplementedException()
    End Sub

    Public Sub LoadFromStream(options As TextSetOptions, value As IRandomAccessStream) Implements ITextDocument.LoadFromStream
        Throw New NotImplementedException()
    End Sub

    Public Sub Redo() Implements ITextDocument.Redo
        Throw New NotImplementedException()
    End Sub

    Public Sub SaveToStream(options As TextGetOptions, value As IRandomAccessStream) Implements ITextDocument.SaveToStream
        Throw New NotImplementedException()
    End Sub

    Public Sub SetDefaultCharacterFormat(value As ITextCharacterFormat) Implements ITextDocument.SetDefaultCharacterFormat
        Throw New NotImplementedException()
    End Sub

    Public Sub SetDefaultParagraphFormat(value As ITextParagraphFormat) Implements ITextDocument.SetDefaultParagraphFormat
        Throw New NotImplementedException()
    End Sub

    Public Sub SetText(options As TextSetOptions, value As String) Implements ITextDocument.SetText
        Throw New NotImplementedException()
    End Sub

    Public Sub Undo() Implements ITextDocument.Undo
        Throw New NotImplementedException()
    End Sub

    Public Property CaretType As CaretType Implements ITextDocument.CaretType
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As CaretType)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Property DefaultTabStop As Single Implements ITextDocument.DefaultTabStop
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As Single)
            Throw New NotImplementedException()
        End Set
    End Property

    Public ReadOnly Property Selection As ITextSelection Implements ITextDocument.Selection
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Property UndoLimit As UInteger Implements ITextDocument.UndoLimit
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As UInteger)
            Throw New NotImplementedException()
        End Set
    End Property
#End Region
End Class
