using System;
namespace Advanced_Code
{
    public class FileSystemVisitorsEventArgument : EventArgs
    {
        public bool AbortSearch { get; set; }
        public bool IsItemsExcluded { get; set; }

        public FileSystemVisitorsEventArgument(bool abortSearch, bool isExcluded)
        {
            AbortSearch = abortSearch;
            IsItemsExcluded = isExcluded;
        }

        public FileSystemVisitorsEventArgument(bool abortSearch)
            : this(abortSearch, false)
        {
        }
    }
}

