using System;
using System.IO;
using System.Collections.Generic;

namespace Advanced_Code
{
    public class FileSystemVisitor
    {
        private readonly string _folderPath;
        private bool _abortSearch;
        private List<FileSystemInfo> _excludedFiles;
        private List<DirectoryInfo> _excludedFolder;

        // we can use this:
        // private readonly Func<FileSystemInfo, bool> _filter;
        private readonly FileFilterDelegate _fileFilter;
        private readonly SubFolderFilterDelegate _subFolderFilter;

        public event EventHandler Start;
        public event EventHandler Finish;
        public event EventHandler<FileSystemInfo> FileFound;
        public event EventHandler<FileSystemInfo> DirectoryFound;
        public event EventHandler<FileSystemInfo> FilteredFileFound;
        public event EventHandler<FileSystemInfo> FilteredDirectoryFound;


        public FileSystemVisitor(string rootPath, FileFilterDelegate filter, SubFolderFilterDelegate subFolderFilter)
        {
            _folderPath = rootPath;
            _fileFilter = filter ?? (item => true);
            _subFolderFilter = subFolderFilter ?? (item => true);
            _excludedFiles = new List<FileSystemInfo>();
            _excludedFolder = new List<DirectoryInfo>();
        }

        public FileSystemVisitor(string rootPath, FileFilterDelegate filter)
       : this(rootPath, filter, null)
        {
        }

        public FileSystemVisitor(string rootPath, SubFolderFilterDelegate subFolderFilter)
       : this(rootPath, null, subFolderFilter)
        {
        }


        public FileSystemVisitor(string rootPath)
        : this(rootPath, null, null)
        {
        }

        public IEnumerable<FileSystemInfo> Inspect()
        {
            return InspectDirectory(new DirectoryInfo(_folderPath));
        }

        private IEnumerable<FileSystemInfo> InspectDirectory(DirectoryInfo directory)
        {
            Console.WriteLine("here problem");
            if (directory.Exists)
            {
                foreach (var file in directory.GetFileSystemInfos())
                {
                    FileFound?.Invoke(this, file);
                    if (_fileFilter(file))
                    {
                        FilteredFileFound?.Invoke(this, file);
                        yield return file;
                    }
                    else ExcludeFile(file);
                }
                foreach (var subdirectory in directory.GetDirectories())
                {
                    DirectoryFound?.Invoke(this, subdirectory);
                    if (_subFolderFilter(subdirectory))
                    {
                        FilteredDirectoryFound?.Invoke(this, subdirectory);
                        foreach (var file in InspectDirectory(subdirectory))
                        {
                            FileFound?.Invoke(this, file);
                            if (_fileFilter(file))
                            {
                                FilteredFileFound?.Invoke(this, file);
                                yield return file;
                            }
                            else ExcludeFile(file);
                        }
                    }
                    else ExcludeFolder(subdirectory);
                }
            }
            else
            {
                AbortSearch();
                throw new Exception("There is no such Directory");
            }
        }

        public void StartSearch()
        {
            _abortSearch = false;
            Start?.Invoke(this, EventArgs.Empty);
            try
            {
                Inspect();
            }
            finally
            {
                Finish?.Invoke(this, new FileSystemVisitorsEventArgument(_abortSearch, IsExcluded()));
            }
        }

        public bool IsExcluded()
        {
            return _excludedFiles.Count > 0 || _excludedFolder.Count > 0;
        }

        public void ExcludeFile(FileSystemInfo item)
        {
            _excludedFiles.Add(item);
        }

        public void ExcludeFolder(DirectoryInfo item)
        {
            _excludedFolder.Add(item);
        }

        public void AbortSearch()
        {
            _abortSearch = true;
        }

        public delegate bool FileFilterDelegate(FileSystemInfo item);
        public delegate bool SubFolderFilterDelegate(DirectoryInfo item);
    }
}


