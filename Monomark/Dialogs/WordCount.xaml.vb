' The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Public NotInheritable Class WordCount
    Inherits ContentDialog

    Private Sub ContentDialog_PrimaryButtonClick(sender As ContentDialog, args As ContentDialogButtonClickEventArgs)
        args.Cancel = False
    End Sub

    Private Sub CheckBox_Click(sender As Object, e As RoutedEventArgs)
        Throw New NotImplementedException("TODO: Add functionality to calculate this...")
    End Sub
End Class
