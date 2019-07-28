Option Strict On

Public Class TestConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        'Debugger.Break()
        Dim Family As New FontFamily(value.ToString())
        Return Family
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        'Debugger.Break()
        Return value.ToString()
    End Function
End Class