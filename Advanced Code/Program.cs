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

          /*  foreach (var item in ListOfItemInDonloads)
            {
                Console.WriteLine("from inspect " + item.ToString());
            }
          */

            downloadFolderInspection.Start += (sender, e) => Console.WriteLine($"Search started.");
            downloadFolderInspection.Finish += (sender, e) => Console.WriteLine($"Search finished.");
            downloadFolderInspection.FileFound += (sender, e) => Console.WriteLine($"File found: {((FileSystemInfo)e).FullName}");
            downloadFolderInspection.DirectoryFound += (sender, e) => Console.WriteLine($"Directory found: {((FileSystemInfo)e).FullName}");
            downloadFolderInspection.FilteredFilesFound += (sender, e) =>
            {
                if (e.Count == 0)
                {
                    Console.WriteLine("Nothing was found");
                }
                Console.WriteLine("Filtered files found:");
                foreach (var file in e)
                {
                    Console.WriteLine(file.FullName);
                }
            };

            downloadFolderInspection.Abort += (sender, e) => Console.WriteLine($"Searh was Aborted");

            downloadFolderInspection.UserPrompt += (sender, e) =>
            {
                Console.WriteLine($"Item found: {e.Item.FullName}");
                Console.Write("Exclude this item? (Y/N), or Abort the Search? (A): ");
                var input = Console.ReadLine();

                if (input.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    e.ExcludeItem = true;
                }
                else if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    e.AbortSearch = true;
                }
            };

            downloadFolderInspection.StartSearch();
            Console.ReadLine();
        }
    }
}