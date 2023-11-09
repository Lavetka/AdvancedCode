public class UserPromptEventArgs : EventArgs
{
    public FileSystemInfo Item { get; }

    public bool ExcludeItem { get; set; }
    public bool AbortSearch { get; set; }

    public UserPromptEventArgs(FileSystemInfo item)
    {
        Item = item;
        ExcludeItem = false;
        AbortSearch = false;
    }
}