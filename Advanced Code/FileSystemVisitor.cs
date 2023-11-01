using System;
using System.IO;

namespace Advanced_Code
{
	public class FileSystemVisitor
	{
        private readonly string _folderPath;

        // we can use this:
        // private readonly Func<FileSystemInfo, bool> _filter;

        private readonly FileSystemFilterDelegate _filter;


        public FileSystemVisitor(string rootPath, FileSystemFilterDelegate filter)
        {
            _folderPath = rootPath;
            _filter = filter ?? (item => true); 
        }

        public FileSystemVisitor(string rootPath)
        : this(rootPath, null)
        {
        }

        public IEnumerable<FileSystemInfo> Inspect()
        {
            return InspectDirectory(new DirectoryInfo(_folderPath));
        }

        private IEnumerable<FileSystemInfo> InspectDirectory(DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                foreach (var file in directory.GetFileSystemInfos())
                {
                    if (_filter(file)) yield return file;
                }
                foreach (var subdirectory in directory.GetDirectories())
                {
                    foreach (var file in InspectDirectory(subdirectory))
                    {
                        if (_filter(file)) yield return file;
                    }
                }
            }
            else throw new Exception("There is no such Directory");
        }

        public delegate bool FileSystemFilterDelegate(FileSystemInfo item);
    }
}

