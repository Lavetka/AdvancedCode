using static Advanced_Code.FileSystemVisitor;

namespace Advanced_Code
{
    class Program
    {
        public static void Main()
        {
            string downloadFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Downloads";
            FileFilterDelegate textFileFilter = item => item is FileInfo fileInfo && fileInfo.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
            SubFolderFilterDelegate subFolderFilter = item => item is DirectoryInfo directoryInfo && directoryInfo.FullName.Contains("Hleb");

            FileSystemVisitor downloadFolderInspection = new FileSystemVisitor(downloadFolderPath, textFileFilter, subFolderFilter);
            var ListOfItemInDonloads = downloadFolderInspection.Inspect();

            foreach (var item in ListOfItemInDonloads)
            {
                Console.WriteLine("from inspect " + item.ToString());
            }

            downloadFolderInspection.Start += (sender, e) => Console.WriteLine($"Search started.");
            downloadFolderInspection.Finish += (sender, e) => Console.WriteLine
            ($"Search finished. Is search was aborted: {((FileSystemVisitorsEventArgument)e).AbortSearch}, and if some entities were restricted: {((FileSystemVisitorsEventArgument)e).IsItemsExcluded}");
            downloadFolderInspection.FileFound += (sender, e) => Console.WriteLine($"File found: {((FileSystemInfo)e).FullName}");
            downloadFolderInspection.DirectoryFound += (sender, e) => Console.WriteLine($"Directory found: {((FileSystemInfo)e).FullName}");
            downloadFolderInspection.FilteredFileFound += (sender, e) => Console.WriteLine($"Filtered file found: {((FileSystemInfo)e).FullName}");
            downloadFolderInspection.FilteredDirectoryFound += (sender, e) => Console.WriteLine($"Filtered directory found: {((FileSystemInfo)e).FullName}");

            downloadFolderInspection.StartSearch();
        }
    }
}