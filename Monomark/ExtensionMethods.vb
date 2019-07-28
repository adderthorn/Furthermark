Option Strict On

Imports System.Security.Cryptography
Imports System.Text
Imports Markdig
Imports Microsoft.Graphics.Canvas
Imports Microsoft.Graphics.Canvas.Text
Imports Furthermark.TargetBlankLinks
Imports Windows.Storage
Imports Windows.ApplicationModel.Resources

<Flags()>
Public Enum MdFormatType
    None = 0
    Italic = 1
    Bold = 2
    Quote = 4
    Code = 8
    Heading1 = 16
    Heading2 = 32
    Heading3 = 64
    Link = 128
    Strikethrough = 256
End Enum

Public Enum SearchMethods
    None
    [Next]
    Previous
    Count
End Enum

Public Module ExtensionMethods
    Private Const DEFAULT_INT As Integer = 1
    Private Const CSS_FILE As String = "MdStyle.css"
    Private Const DEFAULT_CSS As String = "GitHub.css"
    Private Const DARK_CSS As String = "GitHubDark.css"
    Public Const CSS_PLACEHOLDER As String = "***|***"

    Public ReadOnly MARKDOWN_PIPELINE As MarkdownPipeline = New MarkdownPipelineBuilder() _
        .UseAdvancedExtensions() _
        .UsePreciseSourceLocation() _
        .Build()

    <Extension()>
    Public Function GetLineCount(ByVal aRichEditBox As RichEditBox) As Integer
        Dim DocText As String
        aRichEditBox.Document.GetText(Windows.UI.Text.TextGetOptions.UseLf, DocText)
        If String.IsNullOrWhiteSpace(DocText) Then Return DEFAULT_INT
        Dim CrLfCount As Integer = DocText.Split(New String() {vbLf}, StringSplitOptions.None).Length
        If CrLfCount < DEFAULT_INT Then
            CrLfCount = DEFAULT_INT
        End If
        Return CrLfCount
    End Function

    <Extension()>
    Public Function GetCurrentLine(ByVal aString As String, ByVal EndPosition As Integer, ByVal NewLineChar As String) As String
        If String.IsNullOrEmpty(aString) OrElse Not aString.Contains(NewLineChar) Then
            Return Nothing
        End If
        Dim NewString As String
        NewString = aString.Substring(EndPosition)
        If String.IsNullOrEmpty(NewString) OrElse Not NewString.Contains(NewLineChar) Then
            Return Nothing
        End If
        NewString = NewString.Substring(0, NewString.Length - NewString.IndexOf(NewLineChar))
        Return NewString
    End Function

    Public Function GetStringWidthInPixels(ByVal Text As String, ByVal TextSize As Single,
                                           ByVal FontFamily As String) As Double
        Dim TextFormat As New CanvasTextFormat()
        With TextFormat
            .FontSize = TextSize
            .FontFamily = FontFamily
            '.WordWrapping = CanvasWordWrapping.WholeWord
        End With
        Dim Device = CanvasDevice.GetSharedDevice()
        Dim Layout As New CanvasTextLayout(Device, Text, TextFormat, 0.0F, 0.0F)
        Dim Width As Double = Layout.LayoutBounds.Width
        Return Width
    End Function

    Public Function GetMd5(ByVal Text As String, ByVal Optional IgnoreCase As Boolean = False) As Byte()
        Dim MD5 As MD5 = MD5.Create()
        If IgnoreCase Then Text = Text.ToUpper()
        Dim InputBytes As Byte() = Encoding.UTF8.GetBytes(Text)
        Return MD5.ComputeHash(InputBytes)
    End Function

    Public Function GetMarkdownCharacters(ByVal FormatType As MdFormatType) As MarkdownChars
        Dim SurroundString As MarkdownChars
        Select Case FormatType
            Case MdFormatType.Bold
                SurroundString = New MarkdownChars("**")
            Case MdFormatType.Italic
                SurroundString = New MarkdownChars("*")
            Case MdFormatType.Quote
                SurroundString = New MarkdownChars("> ", String.Empty) With {.HasNewlines = True}
            Case MdFormatType.Code
                SurroundString = New MarkdownChars("`")
            Case MdFormatType.Heading1
                SurroundString = New MarkdownChars("#") With {.HasNewlines = True}
            Case MdFormatType.Heading2
                SurroundString = New MarkdownChars("##") With {.HasNewlines = True}
            Case MdFormatType.Heading3
                SurroundString = New MarkdownChars("###") With {.HasNewlines = True}
            Case MdFormatType.Strikethrough
                SurroundString = New MarkdownChars("~~")
            Case Else
                SurroundString = New MarkdownChars(String.Empty)
        End Select
        Return SurroundString
    End Function

    Public Async Function LoadCssAsync(ByVal ThemeToLoad As ElementTheme) As Task(Of String)
        Dim CssStr As String
        Try
            Dim CssFile As StorageFile = Await ApplicationData.Current.RoamingFolder.CreateFileAsync(CSS_FILE, CreationCollisionOption.OpenIfExists)
            Dim Stream = Await CssFile.OpenAsync(FileAccessMode.ReadWrite)
            Using Reader As New StreamReader(Stream.AsStream())
                CssStr = Await Reader.ReadToEndAsync()
            End Using
            If String.IsNullOrWhiteSpace(CssStr) Then
                CssStr = Await LoadDefaultCssAsync(ThemeToLoad)
            End If
        Finally

        End Try
        If String.IsNullOrWhiteSpace(CssStr) Then
            CssStr = Await LoadDefaultCssAsync(ThemeToLoad)
        End If
        Return CssStr
    End Function

    Public Async Function LoadDefaultCssAsync(Optional ByVal Theme As ElementTheme = ElementTheme.Light) As Task(Of String)
        Dim CssUriString As String = "ms-appx:///WebAssets/{0}"
        Select Case Theme
            Case ElementTheme.Dark
                CssUriString = String.Format(CssUriString, DARK_CSS)
            Case Else 'ApplicationTheme.Light
                CssUriString = String.Format(CssUriString, DEFAULT_CSS)
        End Select
        Dim CssFile = Await StorageFile.GetFileFromApplicationUriAsync(New Uri(CssUriString))
        Dim Stream = Await CssFile.OpenReadAsync()
        Dim CssStr As String
        Using Reader As New StreamReader(Stream.AsStream())
            CssStr = Await Reader.ReadToEndAsync()
        End Using
        Return CssStr
    End Function

    Public Async Function SaveCssAsync(ByVal CssStr As String) As Task
        Dim CssFile As StorageFile = Await ApplicationData.Current.RoamingFolder.CreateFileAsync(CSS_FILE, CreationCollisionOption.ReplaceExisting)
        Dim Stream = Await CssFile.OpenStreamForWriteAsync()
        Using Writer As New StreamWriter(Stream)
            Await Writer.WriteAsync(CssStr)
        End Using
    End Function
End Module