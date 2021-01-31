Option Strict On

Imports Windows.System

Public Class FurtherEditBox
    Inherits RichEditBox

    Public Event OnFormatEventHandler As EventHandler(Of FormatEventArgs)
    Public Event OnClipboardEventHandler As EventHandler(Of ClipboardEventArgs)
    Public Property SourceIsProgramatic As Boolean

    Public Sub New()
        Me.SelectionFlyout = GetSelectionFlyout()
    End Sub

    Protected Overrides Sub OnProcessKeyboardAccelerators(args As ProcessKeyboardAcceleratorEventArgs)
        MyBase.OnProcessKeyboardAccelerators(args)

        If (args.Modifiers.HasFlag(VirtualKeyModifiers.Control)) AndAlso
            (args.Key = VirtualKey.B Or args.Key = VirtualKey.I Or args.Key = VirtualKey.U) Then
            Dim FormatType As MdFormatType
            Select Case args.Key
                Case VirtualKey.B
                    FormatType = MdFormatType.Bold
                Case VirtualKey.I
                    FormatType = MdFormatType.Italic
                Case Else
                    FormatType = MdFormatType.None
            End Select
            RaiseEvent OnFormatEventHandler(Me, New FormatEventArgs() With {.FormatType = FormatType})
            args.Handled = True
        End If
    End Sub

    Private Function GetSelectionFlyout() As CommandBarFlyout
        Dim Flyout As CommandBarFlyout = New CommandBarFlyout()

        Dim BoldButton As New AppBarButton()
        With BoldButton
            .Icon = New SymbolIcon(Symbol.Bold)
            .Label = "Bold"
            .Tag = MdFormatType.Bold
            AddHandler .Click, AddressOf FlyoutButton_Click
        End With

        Dim ItalicButton As New AppBarButton()
        With ItalicButton
            .Icon = New SymbolIcon(Symbol.Italic)
            .Label = "Italic"
            .Tag = MdFormatType.Italic
        End With

        'Dim UnderlineButton As New AppBarButton()
        'With UnderlineButton
        '    .Icon = New SymbolIcon(Symbol.Underline)
        '    .Label = "Underline"
        '    .Tag = MdFormatType.u
        'End With

        Dim CutButton As New AppBarButton()
        With CutButton
            .Command = New StandardUICommand(StandardUICommandKind.Cut)
            .Tag = "X"
            AddHandler .Click, AddressOf FlyoutClipboardButton_Click
        End With

        Dim CopyButton As New AppBarButton()
        With CopyButton
            .Command = New StandardUICommand(StandardUICommandKind.Copy)
            .Tag = "C"
            AddHandler .Click, AddressOf FlyoutClipboardButton_Click
        End With

        Dim PasteButton As New AppBarButton()
        With PasteButton
            .Command = New StandardUICommand(StandardUICommandKind.Paste)
            .Tag = "V"
            AddHandler .Click, AddressOf FlyoutClipboardButton_Click
        End With

        With Flyout.PrimaryCommands
            .Add(BoldButton)
            .Add(ItalicButton)
        End With

        With Flyout.SecondaryCommands
            .Add(CutButton)
        End With

        Return Flyout
    End Function

    Private Sub FlyoutButton_Click(sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot AppBarButton Then Return
        Dim Button = CType(sender, AppBarButton)
        Dim FormatType = CType(Button.Tag, MdFormatType)
        RaiseEvent OnFormatEventHandler(Me, New FormatEventArgs() With {.FormatType = FormatType})
    End Sub

    Private Sub FlyoutClipboardButton_Click(ByVal sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot AppBarButton Then Return
        Dim Button = CType(sender, AppBarButton)
        Dim ClipboardTag As String = CStr(Button.Tag)
        RaiseEvent OnClipboardEventHandler(Me, New ClipboardEventArgs() With {.ClipboardTag = ClipboardTag})
    End Sub
End Class

Public Class FormatEventArgs
    Inherits EventArgs

    Public Property FormatType As MdFormatType
End Class

Public Class ClipboardEventArgs
    Inherits EventArgs

    Public Property ClipboardTag As String
End Class