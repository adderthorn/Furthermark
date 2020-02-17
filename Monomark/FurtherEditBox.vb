Option Strict On

Imports Windows.System

Public Class FurtherEditBox
    Inherits RichEditBox

    Public Event OnFormatEventHandler As EventHandler(Of FormatEventArgs)
    Public Property SourceIsProgramatic As Boolean

    Public Sub New()
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
End Class

Public Class FormatEventArgs
    Inherits EventArgs

    Public Property FormatType As MdFormatType
End Class
