Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports Windows.Graphics.Printing
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls
Imports Windows.UI.Xaml.Printing

Public Class PrintHelper
    Protected ApplicationContentMarginLeft As Double = 0.075D
    Protected ApplicationContentMarginTop As Double = 0.03D
    Protected PrintDocument As PrintDocument
    Protected PrintDocumentSource As IPrintDocumentSource
    Friend PrintPreviewPages As List(Of UIElement)
    Protected Event PreviewPagesCreated As EventHandler
    Protected FirstPage As FrameworkElement
    Protected Page As Page
    Protected ReadOnly Property PrintCanvas As Canvas
        Get
            Return CType(Page.FindName("PrintCanvas"), Canvas)
        End Get
    End Property
End Class
