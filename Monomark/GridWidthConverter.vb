Option Strict On

Public Class GridWidthConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim NewValue As Double = CDbl(value)
        Dim GridLength As New GridLength(NewValue)
        Return GridLength
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Dim NewValue As GridLength = CType(value, GridLength)
        Return NewValue.Value
    End Function
End Class