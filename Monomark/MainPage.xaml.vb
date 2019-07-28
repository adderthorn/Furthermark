Option Strict On

Imports System.Text.RegularExpressions
Imports Windows.System
Imports Markdig
Imports Windows.ApplicationModel.Resources
Imports Windows.ApplicationModel.DataTransfer
Imports WinRTXamlToolkit.Controls.Extensions
Imports Windows.UI.Text
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls
Imports Windows.Storage
Imports Windows.Storage.Pickers
Imports Windows.UI.Core
Imports Windows.UI.Popups
Imports Windows.UI.Notifications
Imports Windows.UI.Core.Preview
Imports System.Threading
Imports Windows.UI
Imports Furthermark.FurtherFormatter

#Region "Compiler Directives"
#Const LOAD_DEMO = False
#End Region

Public NotInheritable Class MainPage
    Inherits Page
    Implements INotifyPropertyChanged

#Region "Private Variables & Constants"
    'TODO: Allow choice between spaces or actual tab character
    Private Const TAB_CHAR As String = "    "
    Private Const HR_CHAR As String = vbLf & "---------------" & vbLf
    Private Const SCROLL_TO As String = "window.scrollTo({0},{1});"
    Private Const SELECT_ALL As String = "window.getSelection().selectAllChildren(document.getElementById('main'));"
    Private Const EVAL As String = "eval"
    Private Const GET_HEIGHT_JS As String = "getHeight"
    Private Const REFRESH_TIMEOUT As Double = 0.2D
    Private Const SAVE_TIMEOUT As Double = 2D
    Private Const FILE_SAVE_TIMEOUT As Double = 8D
    Private ReadOnly UpdaterTimer As DispatcherTimer
    Private ReadOnly SaveSettingsTimer As DispatcherTimer
    Private ReadOnly AutoSaveTimer As DispatcherTimer

    Private _Line As Integer = 1
    Private _Lines As Integer = 1
    Private _Length As Integer = 0
    Private _IsDirty As Boolean = False
    Private _CanUndo As Boolean = False
    Private _CanCopy As Boolean = False
    Private _CanPaste As Boolean = False
    Private _IsOvertype As Boolean = False
    Private OpenedFile As StorageFile
    Private FileHash As Byte() = GetMd5(String.Empty)
    Private ReadOnly FontList As ObservableCollection(Of String) =
        New ObservableCollection(Of String)(Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies().OrderBy(Function(f) f))
    Private ReadOnly FontSizes As ObservableCollection(Of Double) =
        New ObservableCollection(Of Double) From {8D, 9D, 10D, 11D, 12D, 13D, 14D, 16D, 18D, 20D, 22D, 24D, 26D, 28D, 32D, 36D, 48D, 72D}
    Private IsAppWindowActive As Boolean = True
    Private NeedToRaiseUpdateClipboard As Boolean
    Private Formatter As Formatter
    Private _IsPageLoading As Boolean = True
#End Region

#Region "Event Handlers"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Public Event UpdateDocument As EventHandler(Of FurtherFormatter.FormatterEventArgs)
#End Region

#Region "Public Properties"
    Public Property Settings As Settings
    Public Property LineCountText As String
    Public Property LineCountWidth As Double
    Public ReadOnly Property WordCount As String
        Get
            Dim EditorString As String, Count As Integer = 0
            Editor.Document.GetText(Windows.UI.Text.TextGetOptions.None, EditorString)
            If Not String.IsNullOrWhiteSpace(EditorString) Then
                Dim col As MatchCollection = Regex.Matches(EditorString, "[\S]+")
                Count = col.Count
            End If
            Return Count.ToString("N0")
        End Get
    End Property
    Public Property Col As Integer = 0
    Public ReadOnly Property Line As String
        Get
            Return _Line.ToString("N0")
        End Get
    End Property
    Public ReadOnly Property Lines As String
        Get
            Return _Lines.ToString("N0")
        End Get
    End Property
    Public ReadOnly Property Length As String
        Get
            Return _Length.ToString("N0")
        End Get
    End Property
    Public Property StatusText As String
    Public Property IsDirty As Boolean
        Get
            Return _IsDirty
        End Get
        Set(value As Boolean)
            If value <> _IsDirty Then
                _IsDirty = value
                SetTitleBar()
                RaisePropertyChanged(NameOf(IsDirty))
            End If
        End Set
    End Property
    Public ReadOnly Property PreviewWidth As Double
        Get
            If Settings Is Nothing Then
                Return 0
            End If
            Dim WindowWidth As Double = CType(Window.Current.Content, Frame).ActualWidth
            Dim ControlWidth As Double = WindowWidth * (Math.Abs(Settings.PreviewSize - 100D) / 100D)
            Return ControlWidth
        End Get
    End Property
    Public Property SettingsPreviewSliderPosition As Integer
        Get
            If Settings Is Nothing Then
                Return 50
            End If
            Return Settings.PreviewSize
        End Get
        Set(value As Integer)
            If value <> Settings.PreviewSize Or value = 50 Then
                Settings.PreviewSize = value
                RaisePropertyChanged(NameOf(SettingsPreviewSliderPosition))
                RaisePropertyChanged(NameOf(PreviewWidth))
            End If
        End Set
    End Property
    Public Property CanUndo As Boolean
        Get
            Return _CanUndo
        End Get
        Set(value As Boolean)
            If value <> _CanUndo Then
                _CanUndo = value
                RaisePropertyChanged(NameOf(CanUndo))
            End If
        End Set
    End Property
    Public Property CanCopy As Boolean
        Get
            Return _CanCopy
        End Get
        Set(value As Boolean)
            If value <> _CanCopy Then
                _CanCopy = value
                RaisePropertyChanged(NameOf(CanCopy))
            End If
        End Set
    End Property
    Public Property CanPaste As Boolean
        Get
            Return _CanPaste
        End Get
        Set(value As Boolean)
            If value <> _CanPaste Then
                _CanPaste = value
                RaisePropertyChanged(NameOf(CanPaste))
            End If
        End Set
    End Property
    Public ReadOnly Property InsOvr As String
        Get
            Dim Result As String = "INS"
            If _IsOvertype Then
                Result = "OVR"
            End If
            Return Result
        End Get
    End Property
    Public Property IsPageLoading As Boolean
        Get
            Return _IsPageLoading
        End Get
        Set(value As Boolean)
            If _IsPageLoading <> value Then
                _IsPageLoading = value
                RaisePropertyChanged(NameOf(IsPageLoading))
                RaisePropertyChanged(NameOf(CanChangeTheme))
            End If
        End Set
    End Property
    Public ReadOnly Property CanChangeTheme As Boolean
        Get
            Return Not IsPageLoading
        End Get
    End Property
#End Region

#Region "Constructors"
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        UpdaterTimer = New DispatcherTimer() With {.Interval = TimeSpan.FromSeconds(REFRESH_TIMEOUT)}
        SaveSettingsTimer = New DispatcherTimer() With {.Interval = TimeSpan.FromSeconds(SAVE_TIMEOUT)}
        AutoSaveTimer = New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(FILE_SAVE_TIMEOUT)}
        Formatter = New Formatter(Editor.Document)

        AddHandler UpdaterTimer.Tick, AddressOf OnUpdateTick
        AddHandler SaveSettingsTimer.Tick, AddressOf OnSaveSettingsTick
        AddHandler AutoSaveTimer.Tick, AddressOf OnAutoSaveTick
        AddHandler Window.Current.CoreWindow.SizeChanged, AddressOf RaiseEventWindowResized
        AddHandler Clipboard.ContentChanged, AddressOf RaiseClipboardContentChanged
        AddHandler Window.Current.Activated, AddressOf RaiseWindowActivated
        AddHandler Editor.OnFormatEventHandler, AddressOf OnFormatHotkey
        AddHandler SystemNavigationManagerPreview.GetForCurrentView().CloseRequested, AddressOf OnCloseRequested
        LoadSettings()
    End Sub

    Private Async Sub OnCloseRequested(sender As Object, e As SystemNavigationCloseRequestedPreviewEventArgs)
        e.Handled = True
        If IsDirty Then
            Await YesNoCancel(Async Sub() 'Yes
                                  Await Save()
                                  IsAppWindowActive = False
                                  App.Current.Exit()
                              End Sub,
                              Sub() 'No
                                  IsAppWindowActive = False
                                  App.Current.Exit()
                              End Sub)
        Else
            IsAppWindowActive = False
            App.Current.Exit()
        End If
    End Sub

    Private Async Sub RaiseWindowActivated(sender As Object, e As WindowActivatedEventArgs)
        IsAppWindowActive = (e.WindowActivationState <> CoreWindowActivationState.Deactivated)
        If NeedToRaiseUpdateClipboard Then
            Await HandleClipboardChanged()
        End If
    End Sub

    Private Async Sub LoadSettings()
        Dim AnUiSettings = New UISettings()
        If Settings Is Nothing Then Settings = New Settings()
        Settings = Await Settings.LoadAsync()
        If Settings.IsDirty Then
            Await Settings.SaveAsync()
        End If
        AddHandler Settings.Dirtied, AddressOf OnSettingsDirtied
        AddHandler AnUiSettings.ColorValuesChanged, Sub(s, e)
                                                        If Settings.Theme = ElementTheme.Default Then
                                                            Page_ActualThemeChanged(Nothing, e)
                                                        End If
                                                    End Sub
        If Settings.AutoSave Then AutoSaveTimer.Start()
        RaisePropertyChanged(NameOf(Settings))
        TogglePreview(Settings.ShowPreview)
        Select Case Settings.Theme
            Case ElementTheme.Default
                If (AnUiSettings.GetColorValue(UIColorType.Background) = Colors.Black) Then
                    Editor.Style = CType(App.Current.Resources("FurtherEditBoxStyleDark"), Style)
                End If
            Case ElementTheme.Light
                'We are fine doing nothing
            Case ElementTheme.Dark
                Editor.Style = CType(App.Current.Resources("FurtherEditBoxStyleDark"), Style)
        End Select
        AddHandler Me.UpdateDocument, Sub(s, e)
                                          UpdatePreview(e.ForceUpdate)
                                          Formatter.Format(e.ForceUpdate)
                                      End Sub
    End Sub

    Private Async Sub RaiseClipboardContentChanged(sender As Object, e As Object)
        Await HandleClipboardChanged()
    End Sub

    Private Sub RaiseEventWindowResized(sender As Object, e As WindowSizeChangedEventArgs)
        RaisePropertyChanged(NameOf(PreviewWidth))
    End Sub

    Private Sub OnUpdateTick(sender As Object, e As Object)
        UpdaterTimer.Stop()
        RaiseEvent UpdateDocument(sender, New FurtherFormatter.FormatterEventArgs())
    End Sub

    Private Async Sub OnSaveSettingsTick(ByVal sender As Object, ByVal e As Object)
        SaveSettingsTimer.Stop()
        Await Settings.SaveAsync()
    End Sub

    Private Async Sub OnAutoSaveTick(ByVal sender As Object, ByVal e As Object)
        If Not OpenedFile Is Nothing _
            AndAlso IsDirty Then
            Await Save()
        End If
    End Sub
#End Region

#Region "Private Functions"
    Private Async Function HandleClipboardChanged() As Task
        If IsAppWindowActive Then
            'As it turns out, running this as-is will cause an accessed denied exception
            'this is because you cannot access the clipboard from the background;
            'However this error only shows itself when you are *not* debugging because the
            'default debugging behavior suppresses this permission requirement...ugh.
            Await Windows.ApplicationModel.Core.CoreApplication _
                .MainView.CoreWindow.Dispatcher _
                .RunAsync(CoreDispatcherPriority.Normal,
                          Sub()
                              Dim DataPkgView As DataPackageView = Clipboard.GetContent()
                              If DataPkgView.Contains(StandardDataFormats.Text) _
                                 And Editor.Document.CanPaste() Then
                                  CanPaste = True
                              Else
                                  CanPaste = False
                              End If
                          End Sub)
        Else
            'We cannot get clipboard contents from the backgound (unless we are debugging)
            'so we do this flag thing instead
            NeedToRaiseUpdateClipboard = True
        End If
    End Function

    Private Sub TextBox_KeyDown(sender As Object, e As KeyRoutedEventArgs)
        If Not TypeOf sender Is FurtherEditBox Then Return
        Dim ThisTextBox = CType(sender, FurtherEditBox)
        Select Case e.Key
            Case VirtualKey.Tab
                Dim StartPos = ThisTextBox.Document.Selection.StartPosition
                Dim NewLength As Integer
                If CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) Then
                    If StartPos >= TAB_CHAR.Length Then
                        NewLength = TAB_CHAR.Length * -1
                    End If
                Else
                    NewLength = TAB_CHAR.Length
                End If
                'ThisTextBox.Document.SetText(TextSetOptions.None, NewText)
                'ThisTextBox.Document.Selection.StartPosition = StartPos + NewLength
                'RaiseEvent UpdateDocument(Me, New FurtherFormatter.FormatterEventArgs(ForceUpdate:=True))
                'e.Handled = True
                ThisTextBox.Document.Selection.SetText(TextSetOptions.None, TAB_CHAR)
                ThisTextBox.Document.Selection.StartPosition = StartPos + TAB_CHAR.Length
                e.Handled = True
            Case VirtualKey.Insert
                _IsOvertype = Not ThisTextBox.Document.Selection.Options.HasFlag(SelectionOptions.Overtype)
                RaisePropertyChanged(NameOf(InsOvr))
        End Select
    End Sub

    Protected Sub OnSettingsDirtied(ByVal sender As Object, ByVal e As EventArgs)
        SaveSettingsTimer.Stop()
        SaveSettingsTimer.Start()
        If Settings.AutoSave Then
            AutoSaveTimer.Start()
        Else
            AutoSaveTimer.Stop()
        End If
    End Sub

    Private Async Sub OnNew(sender As Object, e As RoutedEventArgs)
        If IsDirty Then
            Await YesNoCancel(Async Sub(Yes)
                                  Await Save()
                                  NewDoc()
                              End Sub _
                              , Sub(No)
                                    NewDoc()
                                End Sub)
        Else
            NewDoc()
        End If
    End Sub

    Private Async Function YesNoCancel(ByVal Yes As UICommandInvokedHandler, ByVal No As UICommandInvokedHandler) As Task
        Dim MsgDlg = New MessageDialog("This document has been changed. Would you like to save it first?")
        With MsgDlg
            .Commands.Add(New UICommand("&Save", Yes))
            .Commands.Add(New UICommand("Do&n't Save", No))
            .Commands.Add(New UICommand("Cancel"))
            .DefaultCommandIndex = 0
            .CancelCommandIndex = 2
        End With
        Await MsgDlg.ShowAsync()
    End Function

    Private Sub NewDoc()
        OpenedFile = Nothing
        FileHash = Nothing
        Editor.Document.SetText(TextSetOptions.None, String.Empty)
        Formatter.Format(ForceUpdate:=True)
        IsDirty = False
    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        SetTitleBar()
        Await NavigateWithCssAsync()
    End Sub

    Private Sub Editor_TextChanged(sender As Object, e As RoutedEventArgs)
        UpdaterTimer.Stop()
        UpdaterTimer.Start()
    End Sub

    Private Async Sub UpdatePreview(ByVal Optional ForceUpdate As Boolean = False)
        If IsPageLoading Then Return
        Dim Text As String
        Editor.Document.GetText(TextGetOptions.UseLf, Text)
        REM I need to do something here to make it stay where we are when typing but move if the line count jumps significantly?
        REM I'm not sure what I want to do here yet...this is still a work-in-progress
        If (Settings.ShowPreviewBoolean AndAlso PreviewWidth > 0) Or ForceUpdate Then
            Dim UpdateTask = PreviewWebView.InvokeScriptAsync("setText", New String() {Markdown.ToHtml(Text, MARKDOWN_PIPELINE)}).AsTask()
            'Dim LineText As String = Text.GetCurrentLine(ThisTextBox.Document.Selection.EndPosition, vbLf)
            Dim ScrollVO As Double = GetObjectScrollViewer(Editor).VerticalOffset
            'Give me a range here to work with
            Dim ScrollMax As Double = GetObjectScrollViewer(Editor).ScrollableHeight
            Dim ScrollPct = ScrollVO / ScrollMax
            Dim MaxHeight As Integer = Integer.Parse(Await PreviewWebView.InvokeScriptAsync(GET_HEIGHT_JS, Nothing))
            Dim NewScrollHeight As Double = MaxHeight * ScrollPct
            Await PreviewWebView.InvokeScriptAsync(EVAL, New String() {String.Format(SCROLL_TO, 0, NewScrollHeight)})
            Await UpdateTask
        End If
        RaisePropertyChanged(NameOf(WordCount))
        ' TODO: Fix IsDity testing
        Dim CurrentHash As Byte() = GetMd5(Text)
        If FileHash Is Nothing Then FileHash = CurrentHash
        If CurrentHash.SequenceEqual(FileHash) Then
            IsDirty = False
        Else
            IsDirty = True
        End If
    End Sub

    Private Sub Editor_Loaded(sender As Object, e As RoutedEventArgs)
        AddHandler GetObjectScrollViewer(Editor).ViewChanged, AddressOf Editor_ViewChanged
    End Sub

    Private Function GetObjectScrollViewer(ByVal DepObject As DependencyObject) As ScrollViewer
        'This function requires the WinRTXamlToolkit
        Return DepObject.GetFirstDescendantOfType(Of ScrollViewer)()
    End Function

    'This does some fancy scrolling nonsense get the vertical offset of the webview to match the editor
    Private Async Sub Editor_ViewChanged(sender As Object, e As ScrollViewerViewChangedEventArgs)
        If Not TypeOf sender Is ScrollViewer Then Return

        Dim EditorScrollViewer = CType(sender, ScrollViewer)
        Dim EditorVO As Double = EditorScrollViewer.VerticalOffset
        Dim EditorMaxHeight As Double = EditorScrollViewer.ScrollableHeight
        Dim EditorScrollPct = EditorVO / EditorMaxHeight
        Dim WebViewMaxHeight = Double.Parse(Await PreviewWebView.InvokeScriptAsync(GET_HEIGHT_JS, Nothing))
        Dim WebViewNewHight = WebViewMaxHeight * EditorScrollPct
        Dim ScrollPositionString As String = String.Format(SCROLL_TO, 0, WebViewNewHight)
        Dim JSJob = PreviewWebView.InvokeScriptAsync(EVAL, New String() {ScrollPositionString})
        'LineBox.GetFirstDescendantOfType(Of ScrollViewer).ChangeView(horizontalOffset:=Nothing, verticalOffset:=EditorVO, zoomFactor:=Nothing)
        Await JSJob
    End Sub

    Protected Sub RaisePropertyChanged(ByVal Name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub

    Private Sub PreviewToggleButton_Click(sender As Object, e As RoutedEventArgs)
        TogglePreview(Nothing)
    End Sub

    Private Sub Editor_SelectionChanged(sender As Object, e As RoutedEventArgs)
        If Not TypeOf sender Is FurtherEditBox Then Return
        'UpdaterTimer.Stop()
        'UpdaterTimer.Start()
        Dim ThisEditor = CType(sender, FurtherEditBox)
        Dim DocText As String
        _Lines = ThisEditor.GetLineCount()
        ThisEditor.Document.GetText(TextGetOptions.UseLf, DocText)
        _Length = DocText.Length

        RaisePropertyChanged(NameOf(Lines))
        RaisePropertyChanged(NameOf(Length))

        Dim Col As Integer, LastNewLine As Integer, Text As String
        ThisEditor.Document.GetText(TextGetOptions.UseLf, Text)
        Col = ThisEditor.Document.Selection.EndPosition
        While Col > Text.Length And Col > 0
            Col -= 1
        End While
        Text = Text.Substring(0, Col)
        LastNewLine = Text.LastIndexOf(vbLf)
        Me.Col = Col - LastNewLine
        RaisePropertyChanged(NameOf(Col))

        _Line = Text.Count(Function(a) a = CChar(vbLf)) + 1
        RaisePropertyChanged(NameOf(Line))

        CanCopy = ThisEditor.Document.CanCopy()
        CanUndo = ThisEditor.Document.CanUndo()
    End Sub

    Private Async Sub OnOpen(sender As Object, e As RoutedEventArgs)
        If IsDirty Then
            Await YesNoCancel(Async Sub(Yes)
                                  Await Save()
                                  Await OpenFile()
                              End Sub _
                           , Async Sub(No)
                                 Await OpenFile()
                             End Sub)
        Else
            Await OpenFile()
        End If
    End Sub

    Private Async Function OpenFile() As Task
        Dim Picker As New FileOpenPicker()
        With Picker
            .FileTypeFilter.Add(".md")
            .FileTypeFilter.Add(".txt")
            .SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            .ViewMode = PickerViewMode.List
        End With
        Dim File As StorageFile = Await Picker.PickSingleFileAsync()
        Await OpenFile(File)
    End Function

    Private Async Function OpenFile(ByVal File As StorageFile) As Task
        If File Is Nothing Then Return
        Dim Text As String = Await FileIO.ReadTextAsync(File)
        Editor.Document.SetText(TextSetOptions.None, Text)
        RaiseEvent UpdateDocument(Me, New FurtherFormatter.FormatterEventArgs(ForceUpdate:=True))
        FileHash = Nothing
        If File.Attributes.HasFlag(FileAttributes.ReadOnly) Then
            IsDirty = True
        Else
            IsDirty = False
            OpenedFile = File
        End If
        SetTitleBar()
    End Function

    Private Sub SetTitleBar()
        Dim AppView As ApplicationView = ApplicationView.GetForCurrentView()
        Dim Title As String = "Untitled"
        If Not OpenedFile Is Nothing Then
            Title = OpenedFile.Name
        End If
        If _IsDirty Then
            Title = "*" & Title
        End If
        AppView.Title = Title
    End Sub

    Private Sub OnInsert(sender As Object, e As RoutedEventArgs)
        If Not TypeOf sender Is AppBarButton Then Return
        Dim SenderButton As AppBarButton = CType(sender, AppBarButton)

        Dim Pos As Integer = Editor.Document.Selection.StartPosition
        Dim Stamp As String

        Select Case CStr(SenderButton.Tag)
            Case "Timestamp"
                Stamp = DateTime.Now.ToLongDateString() & " " & DateTime.Now.ToShortTimeString()
            Case "HR"
                Stamp = HR_CHAR
            Case Else
                Stamp = String.Empty
        End Select

        Dim Text As String
        Editor.Document.GetText(TextGetOptions.UseLf, Text)
        Text = Text.Insert(Pos, Stamp)
        Editor.Document.SetText(TextSetOptions.ApplyRtfDocumentDefaults, Text)
        Editor.Document.Selection.StartPosition = Pos + Stamp.Length
    End Sub

    Private Sub OnFormatHotkey(ByVal sender As Object, ByVal e As FormatEventArgs)
        FormatText(e.FormatType)
    End Sub

    Private Sub OnFormat(sender As Object, e As RoutedEventArgs)
        If TypeOf sender Is AppBarButton Then
            Dim SenderButton As AppBarButton = CType(sender, AppBarButton)
            Dim FormatType As MdFormatType = MdFormatType.None
            [Enum].TryParse(SenderButton.Tag.ToString(), FormatType)
            FormatText(FormatType)
        ElseIf TypeOf sender Is KeyboardAccelerator Then
            Dim SenderAction As KeyboardAccelerator = CType(sender, KeyboardAccelerator)
            Dim FormatType As MdFormatType
            Select Case SenderAction.Key
                Case VirtualKey.B
                    FormatType = MdFormatType.Bold
                Case VirtualKey.I
                    FormatType = MdFormatType.Italic
                Case Else
                    FormatType = MdFormatType.None
            End Select
            FormatText(FormatType)
        End If
    End Sub

    Private Sub FormatText(ByVal FormatType As MdFormatType)
        Console.WriteLine("Formatting: {0}", FormatType.ToString())
        Dim SurroundString As MarkdownChars = GetMarkdownCharacters(FormatType)
        Dim Text As String
        Editor.Document.GetText(TextGetOptions.UseLf, Text)
        Editor.Focus(FocusState.Programmatic)
        If Editor.Document.Selection.Length <> 0 Then
            Dim StartPos As Integer = Math.Min(Editor.Document.Selection.StartPosition, Editor.Document.Selection.EndPosition)
            Dim EndPos As Integer = Math.Max(Editor.Document.Selection.StartPosition, Editor.Document.Selection.EndPosition) _
                + SurroundString.Front.Length
            While EndPos > Text.Length
                EndPos -= 1
            End While
            Text = Text.Insert(StartPos, SurroundString.Front).Insert(EndPos, SurroundString.Back)
            Editor.Document.SetText(TextSetOptions.None, Text)
            Editor.Document.Selection.SetRange(EndPos, EndPos)
        Else
            Dim Pos As Integer = Editor.Document.Selection.StartPosition
            Text = Text.Insert(Pos, SurroundString.Front & SurroundString.Back)
            Editor.Document.SetText(TextSetOptions.None, Text)
            Editor.Document.Selection.SetRange(Pos + SurroundString.Front.Length, Pos + SurroundString.Front.Length)
        End If
    End Sub

    Private Async Sub GetSaveAsFileAsync()
        Dim Picker As New FileSavePicker(), TextList As New List(Of String)
        With TextList
            .Add(".txt")
            .Add(".md")
        End With
        With Picker
            .FileTypeChoices.Add("Text", TextList)
            .SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        End With
        OpenedFile = Await Picker.PickSaveFileAsync()
    End Sub

    Private Async Sub OnSave(sender As Object, e As RoutedEventArgs)
        Await Save()
        Dim Message As String = String.Format("Saved - {0} - {1} {2}.", OpenedFile.DisplayName, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString())
        ShowStatusNotification(Message)
    End Sub

    ''' <summary>
    ''' Saves the Opened File
    ''' </summary>
    Private Async Function Save() As Task
        If OpenedFile Is Nothing Then
            Dim Picker As New FileSavePicker(), TextList As New List(Of String)
            With TextList
                .Add(".txt")
                .Add(".md")
            End With
            With Picker
                .FileTypeChoices.Add("Text", TextList)
                .SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            End With
            OpenedFile = Await Picker.PickSaveFileAsync()
        End If
        Try
            IsDirty = False
            Dim Text As String
            Editor.Document.GetText(TextGetOptions.UseCrlf, Text)
            Await FileIO.WriteTextAsync(OpenedFile, Text)
        Catch Ex As Exception
            Debug.WriteLine(Ex.Message)
        End Try
    End Function

    Private Sub OnResetPreviewWidth(sender As Object, e As RoutedEventArgs)
        SettingsPreviewSliderPosition = 50
    End Sub

    Private Sub Slider_Loaded(sender As Object, e As RoutedEventArgs)
        RaisePropertyChanged(NameOf(PreviewWidth))
    End Sub

    Private Sub OnClipboard(sender As Object, e As RoutedEventArgs)
        If Not TypeOf sender Is AppBarButton Then Return
        Dim Btn As AppBarButton = CType(sender, AppBarButton)
        Try
            Select Case CStr(Btn.Tag)
                Case "C"
                    Editor.Document.Selection.Copy()
                Case "X"
                    Editor.Document.Selection.Cut()
                Case "V"
                    'Per function notes, zero represents the best format
                    '13 represemts CF_UNICODETEXT for plain text
                    Editor.Document.Selection.Paste(format:=13)
                Case "Z"
                    Editor.Document.Undo()
            End Select
        Catch ex As Exception
            Dim Msg As String = String.Format("Error: {0}", ex.Message)
            ShowStatusNotification(Msg)
        End Try
    End Sub

    Private Async Sub OnPreviewSelectAll(sender As Object, e As RoutedEventArgs)
        Await PreviewWebView.InvokeScriptAsync(EVAL, New String() {SELECT_ALL})
    End Sub

    Private Sub OnCopyHTML(sender As Object, e As RoutedEventArgs)
        Dim DataPkg As New DataPackage, DocText As String
        Editor.Document.GetText(TextGetOptions.UseCrlf, DocText)
        DataPkg.RequestedOperation = DataPackageOperation.Copy
        DataPkg.SetText(Markdown.ToHtml(DocText, MARKDOWN_PIPELINE))
        Try
            Clipboard.SetContent(DataPkg)
            ShowStatusNotification("Copied HTML!")
        Catch ex As Exception
            Dim Msg As String = String.Format("Error: {0}", ex.Message)
            ShowStatusNotification(Msg)
        End Try
    End Sub

    Private Async Sub ShowStatusNotification(ByVal Text As String)
        StatusText = Text
        RaisePropertyChanged(NameOf(StatusText))
        RaiseStatusText.Begin()
        Await Task.Delay(4000)
        ExitStatusText.Begin()
    End Sub

    Private Sub ShowToastNoficiation(ByVal Text As String)
        Dim Notifier As ToastNotifier = ToastNotificationManager.CreateToastNotifier()
        Dim ToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01)
        Dim ToastNodeList = ToastXml.GetElementsByTagName("text")
        ToastNodeList.Item(0).AppendChild(ToastXml.CreateTextNode(Text))
        Dim ToastNode = ToastXml.SelectSingleNode("/toast")

        Dim Toast As New ToastNotification(ToastXml)
        Toast.ExpirationTime = DateTime.Now.AddSeconds(2)
        Notifier.Show(Toast)
    End Sub

    Private Sub OnPreviewToggleHotkey(sender As KeyboardAccelerator, args As KeyboardAcceleratorInvokedEventArgs)
        TogglePreview(SetVis:=Nothing)
    End Sub

    'This needs some more help
    Private Sub FindText_Click(sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot Button Then Return
        Dim ClickedButton = CType(sender, Button)
        Select Case ClickedButton.Tag.ToString()
            Case "Next"
                FindText(SearchMethods.Next)
            Case "Prev"
                FindText(SearchMethods.Previous)
            Case Else
                Return
        End Select
    End Sub

    Private Sub FindTextBox_PreviewKeyDown(sender As Object, e As KeyRoutedEventArgs)
        If e.Key = VirtualKey.Enter Then
            If CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) Then
                FindText(SearchMethods.Previous)
            Else
                FindText(SearchMethods.Next)
            End If
            e.Handled = True
        End If
    End Sub

    Private Sub FindText(ByVal SearchMethod As SearchMethods)
        If String.IsNullOrWhiteSpace(FindTextBox.Text) Then
            Return
        End If
        Dim FindText As String, EditorText As String, StartPos As Integer, EndPos As Integer
        FindText = FindTextBox.Text
        Editor.Document.GetText(TextGetOptions.UseLf, EditorText)
        Select Case SearchMethod
            Case SearchMethods.Next
                StartPos = Editor.Document.Selection.EndPosition
                EndPos = EditorText.Length
            Case SearchMethods.Previous
                StartPos = Editor.Document.Selection.StartPosition
                EndPos = 0
            Case Else
                Return
        End Select
        Dim FindOption As FindOptions = FindOptions.None
        Dim Range = Editor.Document.GetRange(StartPos, EndPos)
        If MatchCaseCheckbox.IsChecked Then
            FindOption = FindOptions.Case
        End If
        Dim FoundText As Integer = Range.FindText(FindText, EndPos - StartPos, FindOption)
        If FoundText > 0 Then
            Editor.Document.Selection.SetRange(Range.StartPosition, Range.EndPosition)
            Range.ScrollIntoView(PointOptions.None)
            Editor.Focus(FocusState.Programmatic)
            'Editor.Document.Selection.EndPosition = MatchPos + FindText.Length + StartPos
        Else
            Dim NotificationText As String = String.Format("Cannot find ""{0}""", FindText)
            Editor.Document.Selection.SetRange(StartPos, StartPos)
            ShowToastNoficiation(NotificationText)
            ShowStatusNotification(NotificationText)
        End If
    End Sub

    Private Sub ResizerRect_PointerEntered(sender As Object, e As PointerRoutedEventArgs)
        MainGrid.ManipulationMode = ManipulationModes.TranslateX
        Window.Current.CoreWindow.PointerCursor = New CoreCursor(CoreCursorType.SizeWestEast, id:=1)
    End Sub

    Private Sub ResizerRect_PointerExited(sender As Object, e As PointerRoutedEventArgs)
        MainGrid.ManipulationMode = ManipulationModes.None
        Window.Current.CoreWindow.PointerCursor = New CoreCursor(CoreCursorType.Arrow, id:=1)
    End Sub

    Private Sub Grid_ManipulationDelta(sender As Object, e As ManipulationDeltaRoutedEventArgs)
        Dim NewWidth As Double = PreviewWebView.Width - e.Delta.Translation.X
        Dim WindowWidth As Double = (CType(Window.Current.Content, Frame).ActualWidth)
        If NewWidth < 0 Or NewWidth > (WindowWidth + 2) Then Return
        PreviewWebView.Width = NewWidth
        Settings.PreviewSize = 100 - CInt(NewWidth / WindowWidth * 100)
        RaisePropertyChanged(NameOf(SettingsPreviewSliderPosition))
    End Sub

    Private Sub ResizerRect_DoubleTapped(sender As Object, e As DoubleTappedRoutedEventArgs)
        OnResetPreviewWidth(sender, Nothing)
    End Sub

    Private Sub Editor_Paste(sender As Object, e As TextControlPasteEventArgs)
        CType(sender, FurtherEditBox).Document.Selection.Paste(format:=13)
        e.Handled = True
    End Sub

    Private Sub TogglePreview(ByVal SetVis As Visibility?)
        If SetVis Is Nothing Then
            SetVis = Visibility.Collapsed
            If PreviewToggleButton.IsChecked = True Then
                SetVis = Visibility.Visible
            End If
        End If
        If Settings.ShowPreview <> SetVis.Value Then Settings.MakeDirty()
        Settings.ShowPreview = SetVis.Value
        ResizerRect.Visibility = SetVis.Value
        PreviewToggleButton.IsChecked = Settings.ShowPreviewBoolean
    End Sub

    Private Async Sub EditCssButton_Click(sender As Object, e As RoutedEventArgs)
        Dim dlg = New EditCssDialog(Await LoadCssAsync(Me.ActualTheme), Settings)
        Await dlg.ShowAsync()
        Settings.UseCustomCss = Not dlg.Md5.SequenceEqual(GetMd5(Await LoadDefaultCssAsync(), IgnoreCase:=True))
        If dlg.IsEdited Then
            Await SaveCssAsync(dlg.Css)
            Await NavigateWithCssAsync()
            RaiseEvent UpdateDocument(sender, New FurtherFormatter.FormatterEventArgs(ForceUpdate:=True))
        End If
    End Sub

    Private Async Sub PreviewWebView_NavigationStarting(sender As WebView, args As WebViewNavigationStartingEventArgs)
        If IsPageLoading Then Return
        IsPageLoading = True
        If Not args.Uri Is Nothing Then
            args.Cancel = True
            Await Launcher.LaunchUriAsync(args.Uri)
            IsPageLoading = False
        End If
    End Sub

    Private Sub Editor_DragOver(sender As Object, e As DragEventArgs)
        e.AcceptedOperation = DataPackageOperation.Copy
    End Sub

    Private Async Sub Editor_Drop(sender As Object, e As DragEventArgs)
        If (e.DataView.Contains(StandardDataFormats.StorageItems)) Then
            Dim Items = Await e.DataView.GetStorageItemsAsync()
            If Items.Count > 0 Then
                Dim OneFile As StorageFile = CType(Items.FirstOrDefault(), StorageFile)
                If IsDirty Then
                    Await YesNoCancel(Async Sub(Yes)
                                          Await Save()
                                          Await OpenFile(OneFile)
                                      End Sub,
                                      Async Sub(No)
                                          Await OpenFile(OneFile)
                                      End Sub)
                Else
                    Await OpenFile(OneFile)
                End If
            End If
        End If
    End Sub

    Private Async Function NavigateWithCssAsync() As Task
        Dim CssStringTask As Task(Of String)
        If Settings.UseCustomCss Then
            CssStringTask = LoadCssAsync(Me.ActualTheme)
        Else
            CssStringTask = LoadDefaultCssAsync(Me.ActualTheme)
        End If
        Dim HtmlFile = Await StorageFile.GetFileFromApplicationUriAsync(New Uri("ms-appx:///WebAssets/Default.html"))
        Dim HtmlString = String.Empty
        Using Reader As New StreamReader((Await HtmlFile.OpenReadAsync()).AsStreamForRead())
            HtmlString = Await Reader.ReadToEndAsync()
        End Using
        HtmlString = HtmlString.Replace(ExtensionMethods.CSS_PLACEHOLDER, Await CssStringTask)
        PreviewWebView.NavigateToString(HtmlString)
    End Function

    Private Sub PreviewWebView_NavigationCompleted(sender As WebView, args As WebViewNavigationCompletedEventArgs)
        IsPageLoading = False
        RaiseEvent UpdateDocument(sender, New FurtherFormatter.FormatterEventArgs(ForceUpdate:=True))
    End Sub

    Private Async Sub Page_ActualThemeChanged(sender As FrameworkElement, args As Object)
        Select Case Me.ActualTheme
            Case ElementTheme.Default
                'This shouldn't be reached
            Case ElementTheme.Light
                Editor.Style = Nothing
            Case ElementTheme.Dark
                Editor.Style = CType(App.Current.Resources("FurtherEditBoxStyleDark"), Style)
        End Select
        Await NavigateWithCssAsync()
    End Sub

    Private Sub PreviewWebView_NavigationFailed(sender As Object, e As WebViewNavigationFailedEventArgs)
        IsPageLoading = False
        Debug.WriteLine("Error: {0}", e.WebErrorStatus)
    End Sub

    Private Sub StatusBarItem_PointerEntered(sender As Object, e As PointerRoutedEventArgs)
        If Not TypeOf sender Is StackPanel Then Return
        Dim Sp = CType(sender, StackPanel)
        'Sp.Background = New Brush().
        Dim b As SolidColorBrush = CType(App.Current.Resources("SystemControlForegroundAccentBrush"), SolidColorBrush)
        Dim c As Color = Color.FromArgb(b.Color.A, Darken(b.Color.R, 80), Darken(b.Color.G, 80), Darken(b.Color.B, 80))
        Sp.Background = New SolidColorBrush(c)
        Window.Current.CoreWindow.PointerCursor = New CoreCursor(CoreCursorType.Hand, id:=1)
    End Sub

    Private Sub StatusBarItem_PointerExited(ByVal sender As Object, ByVal e As PointerRoutedEventArgs)
        If Not TypeOf sender Is StackPanel Then Return
        Dim Sp = CType(sender, StackPanel)
        Sp.Background = CType(App.Current.Resources("SystemControlForegroundAccentBrush"), SolidColorBrush)
        Window.Current.CoreWindow.PointerCursor = New CoreCursor(CoreCursorType.Arrow, id:=1)
    End Sub

    Private Function Darken(ByVal Value As Byte, ByVal By As Integer) As Byte
        Dim ByD As Double = By / 100
        If ByD > 1 Or ByD < 0 Then ByD = 0
        Dim IntValue = CInt(Value * ByD)
        IntValue = Math.Max(Math.Min(IntValue, 255), 0)
        Return CByte(IntValue)
    End Function

    Private Sub StatusBarItem_Tapped(sender As Object, e As PointerRoutedEventArgs)
        If Not TypeOf sender Is StackPanel Then Return
        Dim Sp = CType(sender, StackPanel)
        Dim b As SolidColorBrush = CType(App.Current.Resources("SystemControlForegroundAccentBrush"), SolidColorBrush)
        Dim c As Color = Color.FromArgb(b.Color.A, Darken(b.Color.R, 50), Darken(b.Color.G, 50), Darken(b.Color.B, 80))
        Sp.Background = New SolidColorBrush(c)
        Editor.Document.Selection.Options = Editor.Document.Selection.Options Xor SelectionOptions.Overtype
    End Sub

    Private Sub StatusBarItem_UnTapped(sender As Object, e As PointerRoutedEventArgs)
        If Not TypeOf sender Is StackPanel Then Return
        Dim Sp = CType(sender, StackPanel)
        Dim b As SolidColorBrush = CType(App.Current.Resources("SystemControlForegroundAccentBrush"), SolidColorBrush)
        Sp.Background = b
        _IsOvertype = Not _IsOvertype
        RaisePropertyChanged(NameOf(InsOvr))
    End Sub
#End Region

End Class