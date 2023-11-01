using static Advanced_Code.FileSystemVisitor;

namespace Advanced_Code
{
    class Program
    {
        public static void Main()
        {
            string downloadFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Downloads";
            FileSystemFilterDelegate textFileFilter = item => item is FileInfo fileInfo && fileInfo.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);


            FileSystemVisitor downloadFolder = new FileSystemVisitor(downloadFolderPath, textFileFilter);
            var downloadFolderEntities = downloadFolder.Inspect();

            foreach (var item in downloadFolderEntities)
            {
                Console.WriteLine(item.FullName);
            }
        }
    }
}