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
        private List<FileSystemInfo> _foundFiles = new List<FileSystemInfo>(); 

        public event EventHandler Start;
        public event EventHandler Finish;
        public event EventHandler Abort;
        public event EventHandler<FileSystemInfo> FileFound;
        public event EventHandler<FileSystemInfo> DirectoryFound;
        public event EventHandler<List<FileSystemInfo>> FilteredFilesFound;
        public event EventHandler<FileSystemInfo> FilteredDirectoryFound;
        public event EventHandler<UserPromptEventArgs> UserPrompt;


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

        public void InspectObjectDirectory(DirectoryInfo directory)
        {
            bool isAborted = false;

            try
            {
                if (directory.Exists)
                {
                    foreach (var file in directory.GetFileSystemInfos())
                    {
                        if (_fileFilter(file))
                        {
                            var userPromptArgs = new UserPromptEventArgs(file);
                            UserPrompt?.Invoke(this, userPromptArgs);

                            if (userPromptArgs.AbortSearch)
                            {
                                AbortSearch();
                                Abort?.Invoke(this, EventArgs.Empty);
                                isAborted = true;
                                break;
                            }

                            if (userPromptArgs.ExcludeItem)
                            {
                                ExcludeFile(file);

                            }
                            else
                            {
                                if ((file != null) && (file is FileInfo fileInfo)) _foundFiles.Add(file);
                            }
                        }
                    }
                    if (!isAborted)
                    {
                        foreach (var subdirectory in directory.GetDirectories())
                        {
                            if (_subFolderFilter(subdirectory))
                            {
                                foreach (var file in GetListOfFilesfromDirectory(subdirectory))
                                {
                                    if (_fileFilter(file))
                                    {
                                        var userPromptArgs = new UserPromptEventArgs(file);
                                        UserPrompt?.Invoke(this, userPromptArgs);
                                        if (userPromptArgs.AbortSearch)
                                        {
                                            AbortSearch();
                                            Abort?.Invoke(this, EventArgs.Empty);
                                            break;
                                        }
                                        if (userPromptArgs.ExcludeItem)
                                        {
                                            ExcludeFile(file);

                                        }
                                        else
                                        {
                                            if ((file != null) && (file is FileInfo fileInfo)) _foundFiles.Add(file);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    FilteredFilesFound?.Invoke(this, _foundFiles);
                }
                else
                {
                    throw new Exception("There is no such Directory");
                }
            }
            catch
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Finish?.Invoke(this, EventArgs.Empty);
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


