Public Interface ISaveable
    ''' <summary>
    ''' Property that represents if the object state has changed and should be saved
    ''' </summary>
    ''' <returns>True/False</returns>
    Property IsDirty As Boolean
    ''' <summary>
    ''' Cleans the file and makes IsDirty = False
    ''' </summary>
    Sub Clean()
    ''' <summary>
    ''' Saves the File asynchronously
    ''' </summary>
    Function SaveAsync() As Task
    ''' <summary>
    ''' Notifies that the savable file is now dirty
    ''' </summary>
    Event Dirtied As EventHandler
    Sub OnDirtied(ByVal e As EventArgs)
End Interface
