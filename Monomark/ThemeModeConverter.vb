Imports System.Runtime

Public Class ThemeModeConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim ParamString As String = CStr(parameter)
        If ParamString Is Nothing Then
            Return DependencyProperty.UnsetValue
        End If

        If [Enum].IsDefined(value.GetType(), value) = False Then
            Return DependencyProperty.UnsetValue
        End If

        Dim paramValue As Object = [Enum].Parse(value.GetType(), ParamString)
        Return paramValue.Equals(value)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Dim ParamString As String = CStr(parameter)
        If ParamString Is Nothing Then
            Return DependencyProperty.UnsetValue
        End If

        Return [Enum].Parse(targetType, ParamString)
    End Function
End Class
