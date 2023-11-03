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
        private List<DirectoryInfo> _excludedFolders;

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
            _excludedFolders = new List<DirectoryInfo>();
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
            var list = GetListOfFilesfromDirectory(new DirectoryInfo(_folderPath));
            if (list == null) Console.WriteLine("There is no such directory, no more");
            return list;
        }

        private IEnumerable<FileSystemInfo> GetListOfFilesfromDirectory(DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                foreach (var file in directory.GetFileSystemInfos())
                {
                    if (_fileFilter(file))
                    {
                        yield return file;
                    }
                }
                foreach (var subdirectory in directory.GetDirectories())
                {
                    if (_subFolderFilter(subdirectory))
                    {
                        foreach (var file in GetListOfFilesfromDirectory(subdirectory))
                        {
                            if (_fileFilter(file))
                            {
                                yield return file;
                            }
                        }
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        private void InspectObjectDirectory(DirectoryInfo directory)
        {
            try
            {
                if (directory.Exists)
                {
                    foreach (var file in directory.GetFileSystemInfos())
                    {
                        FileFound?.Invoke(this, file);
                        (_fileFilter(file) ? (Action)(() => FilteredFileFound?.Invoke(this, file))
                            : () => ExcludeFile(file)).Invoke();
                    }
                    foreach (var subdirectory in directory.GetDirectories())
                    {
                        DirectoryFound?.Invoke(this, subdirectory);
                        if (_subFolderFilter(subdirectory))
                        {
                            FilteredDirectoryFound?.Invoke(this, subdirectory);
                            foreach (var file in GetListOfFilesfromDirectory(subdirectory))
                            {
                                FileFound?.Invoke(this, file);
                                (_fileFilter(file) ? (Action)(() => FilteredFileFound?.Invoke(this, file))
                                    : () => ExcludeFile(file)).Invoke();
                            }
                        }
                        else ExcludeFolder(subdirectory);
                    }
                }
                else
                {
                    throw new Exception("There is no such Directory");
                }
            }
            catch (Exception e)
            {
                AbortSearch();
                throw;
            }
        }

        public void StartSearch()
        {
            _abortSearch = false;
            Start?.Invoke(this, EventArgs.Empty);
            try
            {
                InspectObjectDirectory(new DirectoryInfo(_folderPath));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Finish?.Invoke(this, new FileSystemVisitorsEventArgument(_abortSearch, IsExcluded()));
            }
        }

        public bool IsExcluded()
        {
            return _excludedFiles.Count > 0 || _excludedFolders.Count > 0;
        }

        public void ExcludeFile(FileSystemInfo item)
        {
            _excludedFiles.Add(item);
        }

        public void ExcludeFolder(DirectoryInfo item)
        {
            _excludedFolders.Add(item);
        }

        public void AbortSearch()
        {
            _abortSearch = true;
        }

        public delegate bool FileFilterDelegate(FileSystemInfo item);
        public delegate bool SubFolderFilterDelegate(DirectoryInfo item);
    }
}


