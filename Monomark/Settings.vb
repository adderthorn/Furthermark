Option Strict On

Imports System.Xml
Imports System.Runtime.Serialization
Imports Windows.Storage
Imports Windows.Storage.Streams

<DataContract()>
Public Class Settings
    Implements INotifyPropertyChanged
    Implements ISaveable

#Region "Private Variables"
    Private Const FILE_NAME As String = "config.xml"
    Public Const CURRENT_VERSION As Integer = 1
    Private Shared ReadOnly WriterSettings As New XmlWriterSettings() _
        With {.Indent = True, .IndentChars = "    ", .NewLineHandling = NewLineHandling.Replace,
        .NewLineChars = vbCrLf, .WriteEndDocumentOnClose = True, .Async = True, .NewLineOnAttributes = False}

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
#End Region

#Region "Public Properties"
    <DataMember()>
    Public Property WrapText As TextWrapping
        Get
            Return _WrapText
        End Get
        Set(value As TextWrapping)
            If value <> _WrapText Then
                _WrapText = value
                RaisePropertyChanged(NameOf(WrapText))
                RaisePropertyChanged(NameOf(WrapTextBool))
                MakeDirty()
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
    <DataMember()>
    Public Property AutoSave As Boolean
        Get
            Return _AutoSave
        End Get
        Set(value As Boolean)
            If value <> _AutoSave Then
                _AutoSave = value
                RaisePropertyChanged(NameOf(AutoSave))
                MakeDirty()
            End If
        End Set
    End Property
    <DataMember()>
    Public Property ShowPreview As Visibility
        Get
            Return _ShowPreview
        End Get
        Set(value As Visibility)
            If value <> _ShowPreview Then
                _ShowPreview = value
                RaisePropertyChanged(NameOf(ShowPreview))
                RaisePropertyChanged(NameOf(ShowPreviewBoolean))
                MakeDirty()
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
    <DataMember()>
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
                MakeDirty()
            End If
        End Set
    End Property
    Public Property IsDirty As Boolean Implements ISaveable.IsDirty
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
    <DataMember()>
    Public Property EditorFont As String
        Get
            Return _EditorFont
        End Get
        Set(value As String)
            If value <> _EditorFont Then
                _EditorFont = value
                RaisePropertyChanged(NameOf(EditorFont))
                MakeDirty()
            End If
        End Set
    End Property
    <DataMember()>
    Public Property FontSize As Double
        Get
            Return _FontSize
        End Get
        Set(value As Double)
            If value <> _FontSize Then
                _FontSize = value
                RaisePropertyChanged(NameOf(FontSize))
                MakeDirty()
            End If
        End Set
    End Property
    <DataMember()>
    Public Property SpellCheck As Boolean
        Get
            Return _SpellCheck
        End Get
        Set(value As Boolean)
            If value <> _SpellCheck Then
                _SpellCheck = value
                RaisePropertyChanged(NameOf(SpellCheck))
                MakeDirty()
            End If
        End Set
    End Property
    ' Added on 2018-10-13
    <DataMember()>
    Public Property Theme As ElementTheme
        Get
            Return _Theme
        End Get
        Set(value As ElementTheme)
            If value <> _Theme Then
                _Theme = value
                RaisePropertyChanged(NameOf(Theme))
                MakeDirty()
            End If
        End Set
    End Property

    'Added on 2019-07-04
    <DataMember()>
    Public Property UseCustomCss As Boolean
        Get
            Return _UseCustomCSS
        End Get
        Set(value As Boolean)
            If value <> _UseCustomCSS Then
                _UseCustomCSS = value
                RaisePropertyChanged(NameOf(UseCustomCss))
            End If
        End Set
    End Property

    <DataMember()>
    Public Property Version As Integer
        Get
            Return _Version
        End Get
        Set(value As Integer)
            If value <> _Version Then
                _Version = value
                RaisePropertyChanged(NameOf(Version))
            End If
        End Set
    End Property
#End Region

#Region "Event Handlers"
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Public Event Dirtied As EventHandler Implements ISaveable.Dirtied

    Protected Sub RaisePropertyChanged(<CallerMemberName> ByVal Name As String)
        'Console.WriteLine("Changed Property: " & Name)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
    End Sub
#End Region

#Region "Constructors"
    Public Sub New()
        WrapText = TextWrapping.Wrap
        AutoSave = False
        ShowPreview = Visibility.Visible
        PreviewSize = 50
        FontSize = 12D
        EditorFont = "Consolas"
        SpellCheck = True
        Theme = ElementTheme.Default
        Clean()
    End Sub
#End Region

#Region "Private Methods"
    Private Shared Async Function CreateStreamFromFileAsync(ByVal CollisionOption As CreationCollisionOption) As Task(Of IRandomAccessStream)
        Dim SubFile As StorageFile = Await ApplicationData.Current.RoamingFolder.CreateFileAsync(FILE_NAME, CollisionOption)
        Dim Stream = Await SubFile.OpenAsync(FileAccessMode.ReadWrite)
        Return Stream
    End Function

    Private Async Function UpgradeVersion() As Task
        While Version < CURRENT_VERSION
            Version += 1
            Select Case Version
                Case 1
                    Dim CurrentCss = Await LoadCssAsync(ElementTheme.Default)
                    _UseCustomCSS = Not (CurrentCss.Equals(Await LoadDefaultCssAsync(ElementTheme.Default), StringComparison.InvariantCultureIgnoreCase))
                Case Else
                    Exit While
            End Select
        End While
    End Function

    Protected Overridable Sub OnDirtied(ByVal e As EventArgs) Implements ISaveable.OnDirtied
        RaiseEvent Dirtied(Me, e)
    End Sub
#End Region

#Region "Public Methods"
    Public Async Function SaveAsync() As Task Implements ISaveable.SaveAsync
        Dim Folder As StorageFolder = ApplicationData.Current.RoamingFolder
        Dim ConfigFile As StorageFile = Await Folder.CreateFileAsync(FILE_NAME, CreationCollisionOption.OpenIfExists)
        Dim Serializer As New DataContractSerializer(GetType(Settings))
        Dim Stream As IRandomAccessStream = Await CreateStreamFromFileAsync(CreationCollisionOption.ReplaceExisting)
        Using Writer As XmlWriter = XmlWriter.Create(Stream.AsStream(), WriterSettings)
            Serializer.WriteObject(Writer, Me)
        End Using
        Await Stream.FlushAsync()
        Stream.Dispose()
        Clean()
    End Function

    Public Sub MakeDirty()
        If IsDirty = False Then
            IsDirty = True
            OnDirtied(EventArgs.Empty)
        End If
    End Sub

    Public Sub Clean() Implements ISaveable.Clean
        IsDirty = False
    End Sub
#End Region

#Region "Shared Methods"
    Public Shared Async Function LoadAsync() As Task(Of Settings)
        Dim NewSettings As Settings
        Dim Serializer As New DataContractSerializer(GetType(Settings))
        Using ConfigFileStream As IRandomAccessStream = Await CreateStreamFromFileAsync(CreationCollisionOption.OpenIfExists)
            Using Reader As XmlReader = XmlReader.Create(ConfigFileStream.AsStream())
                Try
                    NewSettings = CType(Serializer.ReadObject(Reader), Settings)
                    NewSettings.Clean()
                Catch ex As Exception
                    NewSettings = New Settings()
                    NewSettings.MakeDirty()
                End Try
            End Using
        End Using
        Await NewSettings.UpgradeVersion()
        Return NewSettings
    End Function
#End Region

End Class
