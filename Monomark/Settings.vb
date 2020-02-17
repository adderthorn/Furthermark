Option Strict On

Imports System.Xml
Imports System.Runtime.Serialization
Imports Windows.Storage
Imports Windows.Storage.Streams

<DataContract()>
Public Class Settings
    Implements INotifyPropertyChanged
    'Implements ISaveable

#Region "Private Variables"
    ' Private Const FILE_NAME As String = "config.xml"
    Public Const CURRENT_VERSION As Integer = 2
    Private Shared ReadOnly WriterSettings As New XmlWriterSettings() _
        With {.Indent = True, .IndentChars = "    ", .NewLineHandling = NewLineHandling.Replace,
        .NewLineChars = vbCrLf, .WriteEndDocumentOnClose = True, .Async = True, .NewLineOnAttributes = False}
    Private RoamingSettings As ApplicationDataContainer = Nothing

    Private _WrapText As TextWrapping
    Private _AutoSave As Boolean
    Private _ShowPreview As Visibility
    Private _IsDirty As Boolean = False
    Private _PreviewSize As Integer
    Private _EditorFont As String
    Private _FontSize As Double
    Private _SpellCheck As Boolean
    Private _Theme As ElementTheme
    Private _UseCustomCSS As Boolean
    Private _Version As Integer
    Private _TimestampFormatString As String
#End Region

#Region "Public Properties"

    Public Property WrapText As TextWrapping
        Get
            Return _WrapText
        End Get
        Set(value As TextWrapping)
            If value <> _WrapText Then
                _WrapText = value
                RaisePropertyChanged(NameOf(WrapText))
                RaisePropertyChanged(NameOf(WrapTextBool))
                RoamingSettings.Values(NameOf(WrapText)) = Convert.ChangeType(value, value.GetTypeCode())
            End If
        End Set
    End Property
    Public Property WrapTextBool As Boolean?
        Get
            If _WrapText = TextWrapping.Wrap Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(value As Boolean?)
            If value = True Then
                WrapText = TextWrapping.Wrap
            Else
                WrapText = TextWrapping.NoWrap
            End If
        End Set
    End Property

    Public Property AutoSave As Boolean
        Get
            Return _AutoSave
        End Get
        Set(value As Boolean)
            If value <> _AutoSave Then
                _AutoSave = value
                RaisePropertyChanged(NameOf(AutoSave))
                RoamingSettings.Values(NameOf(AutoSave)) = value
            End If
        End Set
    End Property

    Public Property ShowPreview As Visibility
        Get
            Return _ShowPreview
        End Get
        Set(value As Visibility)
            If value <> _ShowPreview Then
                _ShowPreview = value
                RaisePropertyChanged(NameOf(ShowPreview))
                RaisePropertyChanged(NameOf(ShowPreviewBoolean))
                RoamingSettings.Values(NameOf(ShowPreview)) = Convert.ChangeType(value, value.GetTypeCode)
            End If
        End Set
    End Property
    Public ReadOnly Property ShowPreviewBoolean As Boolean
        Get
            If _ShowPreview = Visibility.Collapsed Then
                Return False
            End If
            Return True
        End Get
    End Property

    Public Property PreviewSize As Integer
        Get
            Return _PreviewSize
        End Get
        Set(value As Integer)
            If value > 100 Or value < 0 Then
                Throw New IndexOutOfRangeException("Value must be between 0 and 100.")
            End If
            If value <> _PreviewSize Then
                _PreviewSize = value
                RaisePropertyChanged(NameOf(PreviewSize))
                RoamingSettings.Values(NameOf(PreviewSize)) = value
            End If
        End Set
    End Property

    Public Property IsDirty As Boolean 'Implements ISaveable.IsDirty
        Get
            Return _IsDirty
        End Get
        Private Set(value As Boolean)
            If value <> _IsDirty Then
                _IsDirty = value
                RaisePropertyChanged(NameOf(IsDirty))
            End If
        End Set
    End Property

    Public Property EditorFont As String
        Get
            Return _EditorFont
        End Get
        Set(value As String)
            If value <> _EditorFont Then
                _EditorFont = value
                RaisePropertyChanged(NameOf(EditorFont))
                RoamingSettings.Values(NameOf(EditorFont)) = value
            End If
        End Set
    End Property

    Public Property FontSize As Double
        Get
            Return _FontSize
        End Get
        Set(value As Double)
            If value <> _FontSize Then
                _FontSize = value
                RaisePropertyChanged(NameOf(FontSize))
                RoamingSettings.Values(NameOf(FontSize)) = value
            End If
        End Set
    End Property

    Public Property SpellCheck As Boolean
        Get
            Return _SpellCheck
        End Get
        Set(value As Boolean)
            If value <> _SpellCheck Then
                _SpellCheck = value
                RaisePropertyChanged(NameOf(SpellCheck))
                RoamingSettings.Values(NameOf(SpellCheck)) = value
            End If
        End Set
    End Property

    ' Added on 2018-10-13
    Public Property Theme As ElementTheme
        Get
            Return _Theme
        End Get
        Set(value As ElementTheme)
            If value <> _Theme Then
                _Theme = value
                RaisePropertyChanged(NameOf(Theme))
                RoamingSettings.Values(NameOf(Theme)) = Convert.ChangeType(value, value.GetTypeCode())
            End If
        End Set
    End Property

    'Added on 2019-07-04
    Public Property UseCustomCss As Boolean
        Get
            Return _UseCustomCSS
        End Get
        Set(value As Boolean)
            If value <> _UseCustomCSS Then
                _UseCustomCSS = value
                RaisePropertyChanged(NameOf(UseCustomCss))
                RoamingSettings.Values(NameOf(UseCustomCss)) = value
            End If
        End Set
    End Property

    Public Property Version As Integer
        Get
            Return _Version
        End Get
        Set(value As Integer)
            If value <> _Version Then
                _Version = value
                RaisePropertyChanged(NameOf(Version))
                RoamingSettings.Values(NameOf(Version)) = value
            End If
        End Set
    End Property

    Public Property TimestampFormatString As String
        Get
            Return _TimestampFormatString
        End Get
        Set(value As String)
            If value <> _TimestampFormatString Then
                _TimestampFormatString = value
                RaisePropertyChanged(NameOf(TimestampFormatString))
                RaisePropertyChanged(NameOf(TimestampFormat))
                RoamingSettings.Values(NameOf(TimestampFormatString)) = value
            End If
        End Set
    End Property

    Public Property TimestampFormat As TimestampFormat
        Get
            Return New TimestampFormat(TimestampFormatString)
        End Get
        Set(value As TimestampFormat)
            TimestampFormatString = value.FormatString
        End Set
    End Property
#End Region

#Region "Event Handlers"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub RaisePropertyChanged(<CallerMemberName> ByVal Name As String)
        'Console.WriteLine("Changed Property: " & Name)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub
#End Region

#Region "Constructors"
    Public Sub New()
        RoamingSettings = ApplicationData.Current.RoamingSettings
        Dim VersionTest As [Object] = RoamingSettings.Values(NameOf(Version))
        If Not VersionTest Is Nothing AndAlso TypeOf VersionTest Is Integer Then
            LoadSettings()
        Else
            WrapText = TextWrapping.Wrap
            AutoSave = False
            ShowPreview = Visibility.Visible
            PreviewSize = 50
            FontSize = 12D
            EditorFont = "Consolas"
            SpellCheck = True
            Theme = ElementTheme.Default
            Version = CURRENT_VERSION
            TimestampFormatString = "f"
            Save()
        End If
    End Sub
#End Region

#Region "Private Methods"
    Private Sub LoadSettings()
        AutoSave = CBool(RoamingSettings.Values(NameOf(AutoSave)))
        EditorFont = CStr(RoamingSettings.Values(NameOf(EditorFont)))
        FontSize = CDbl(RoamingSettings.Values(NameOf(FontSize)))
        PreviewSize = CInt(RoamingSettings.Values(NameOf(PreviewSize)))
        ShowPreview = CType(RoamingSettings.Values(NameOf(ShowPreview)), Visibility)
        SpellCheck = CBool(RoamingSettings.Values(NameOf(SpellCheck)))
        Theme = CType(RoamingSettings.Values(NameOf(Theme)), ElementTheme)
        UseCustomCss = CBool(RoamingSettings.Values(NameOf(UseCustomCss)))
        WrapText = CType(RoamingSettings.Values(NameOf(WrapText)), TextWrapping)
        TimestampFormatString = CStr(RoamingSettings.Values(NameOf(TimestampFormatString)))
        UpdateVersion()
    End Sub

    Private Sub UpdateVersion()
        While Version < CURRENT_VERSION
            If Version = 1 Then
                TimestampFormatString = "f"
            End If
            Version += 1
        End While
    End Sub
#End Region

#Region "Public Methods"
    Public Sub Save()
        RoamingSettings.Values(NameOf(AutoSave)) = AutoSave
        RoamingSettings.Values(NameOf(EditorFont)) = EditorFont
        RoamingSettings.Values(NameOf(FontSize)) = FontSize
        RoamingSettings.Values(NameOf(PreviewSize)) = PreviewSize
        RoamingSettings.Values(NameOf(SpellCheck)) = SpellCheck
        RoamingSettings.Values(NameOf(Theme)) = Convert.ChangeType(Theme, Theme.GetTypeCode())
        RoamingSettings.Values(NameOf(UseCustomCss)) = UseCustomCss
        RoamingSettings.Values(NameOf(Version)) = Version
        RoamingSettings.Values(NameOf(WrapText)) = Convert.ChangeType(WrapText, WrapText.GetTypeCode())
        RoamingSettings.Values(NameOf(ShowPreview)) = Convert.ChangeType(ShowPreview, ShowPreview.GetTypeCode())
        RoamingSettings.Values(NameOf(TimestampFormatString)) = TimestampFormatString
    End Sub
#End Region

End Class
