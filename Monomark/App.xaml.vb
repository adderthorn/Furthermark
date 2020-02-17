Option Strict On

Imports Windows.Storage
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
NotInheritable Class App
    Inherits Application

    'Public Shared Property Settings As Settings

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active
        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when opening a file type associated with the Application.
    ''' </summary>
    ''' <param name="e">Event arguments including file(s) to be opened.</param>
    Protected Overrides Sub OnFileActivated(e As FileActivatedEventArgs)
        For i As Integer = 0 To e.Files.Count - 1
            Dim RootFrame As Frame = CreateRootFrame()
            If RootFrame.Content Is Nothing Then
                If Not RootFrame.Navigate(GetType(MainPage)) Then
                    Throw New Exception("Failed to create initial page.")
                End If
            End If
            Dim Pg = TryCast(RootFrame.Content, MainPage)
            Pg.NavigateToPageWithDocument(CType(e.Files(i), IStorageFile))
            Window.Current.Activate()
        Next
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub

    Private Function CreateRootFrame() As Frame
        Dim RootFrame As Frame = TryCast(Window.Current.Content, Frame)
        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active
        If RootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            RootFrame = New Frame()
            With RootFrame
                .Language = Windows.Globalization.ApplicationLanguages.Languages(0)
                AddHandler .NavigationFailed, AddressOf OnNavigationFailed
            End With
            ' Place the frame in the current Window
            Window.Current.Content = RootFrame
        End If
        Return RootFrame
    End Function

End Class
