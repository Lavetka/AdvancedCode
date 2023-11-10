using System.Reflection;
using Advanced_Code;
using static Advanced_Code.FileSystemVisitor;

namespace UnitTests;

public class Tests
{
        [TestFixture]
    public class FileSystemVisitorTests
    {
        private string _testFolderPath;
        private string _testSubFolderPath;


        [OneTimeSetUp]
        public void Setup()
        {
            // Set up any common resources or data needed for your tests.
            _testFolderPath = Path.Combine(Path.GetTempPath(), "FileSystemVisitorTest");
            _testSubFolderPath = Path.Combine(_testFolderPath, "SubFolder");
            Directory.CreateDirectory(_testFolderPath);
            Directory.CreateDirectory(_testSubFolderPath);


            // Create some files and directories for testing
            for (int i = 1; i <= 5; i++)
            {
                File.Create(Path.Combine(_testFolderPath, $"file_{i}.txt")).Close();
                File.Create(Path.Combine(_testSubFolderPath, $"file_{i}.jpeg")).Close();
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            // Clean up resources after all tests are done.
            Directory.Delete(_testFolderPath, true);
        }

        [TestCase(".jpeg", 5)]
        [TestCase(".txt", 5)]
        [TestCase("", 0)]
        [TestCase(".png", 0)]
        [Test]
        public void FileSystemVisitor_StartSearchWithDifferentFiltersbyExtension_ValidDirectory(string filter, int expectedResults)
        {
            // Arrange
            FileFilterDelegate textFileFilter = item => item is FileInfo fileInfo && fileInfo.Extension.Equals(filter, StringComparison.OrdinalIgnoreCase);
            var folderPath = _testFolderPath;
            var visitor = new FileSystemVisitor(folderPath, textFileFilter);
            var eventRaised = false;
            var listOfFiles = new List<FileSystemInfo>();

            visitor.FilteredFilesFound += (sender, e) =>
            {
                listOfFiles = e;
                eventRaised = true;
            };

            // Act
            visitor.StartSearch();

            foreach (var item in listOfFiles)
            {
                Console.WriteLine(item);
            }

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.That(listOfFiles.Count() == expectedResults, $"The result listOf {listOfFiles.Count()} is not equal to expected {expectedResults}");
        }

        [TestCase(".jpeg", 5)]
        [TestCase("SubFolder", 10)]
        [TestCase("", 5)]
        [TestCase("lala", 5)]
        [Test]
        public void FileSystemVisitor_StartSearchWithDifferentFiltersbySubFolder_ValidDirectory(string filter, int expectedResults)
        {
            // Arrange
            SubFolderFilterDelegate subFolderFilter = item => item is DirectoryInfo directoryInfo && directoryInfo.Name.Equals(filter);
            var folderPath = _testFolderPath;
            var visitor = new FileSystemVisitor(folderPath, subFolderFilter);
            var eventRaised = false;
            var listOfFiles = new List<FileSystemInfo>();

            visitor.FilteredFilesFound += (sender, e) =>
            {
                listOfFiles = e;
                eventRaised = true;
            };

            // Act
            visitor.StartSearch();

            foreach (var item in listOfFiles)
            {
                Console.WriteLine(item);
            }

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.That(listOfFiles.Count() == expectedResults, $"The result listOf {listOfFiles.Count()} is not equal to expected {expectedResults}");
        }

        [Test]
        public void FileSystemVisitor_Inspect_ValidDirectory()
        {
            // Arrange
            var folderPath = _testFolderPath;
            var visitor = new FileSystemVisitor(folderPath);

            // Act
            var items = visitor.Inspect();

            // Assert
            CollectionAssert.IsNotEmpty(items);
            Assert.IsTrue(items.All(item => File.Exists(item.FullName) || Directory.Exists(item.FullName)));
        }

        [Test]
        public void FileSystemVisitor_Inspect_InvalidDirectory()
        {
            // Arrange
            var folderPath = "NonExistentDirectory";
            var visitor = new FileSystemVisitor(folderPath);

            // Act
            var items = visitor.Inspect();

            // Assert
            CollectionAssert.IsEmpty(items);
        }

        [Test]
        public void FileSystemVisitor_InspectObjectDirectoryWithoutFilter_ValidDirectory()
        {
            // Arrange
            var folderPath = _testFolderPath;
            var visitor = new FileSystemVisitor(folderPath);
            var eventRaised = false;
            var listOfFiles = new List<FileSystemInfo>();

            visitor.FilteredFilesFound += (sender, e) =>
            {
                listOfFiles = e;
                eventRaised = true;
            };

            // Act
            visitor.StartSearch();

            foreach (var item in listOfFiles)
            {
                Console.WriteLine(item);
            }

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.That(listOfFiles.Count() == 10, $"The result listOf {listOfFiles.Count()} is not equal to expected 10 ");
        }


        [Test]
        public void FileSystemVisitor_StartSearch_ValidDirectory()
        {
            // Arrange
            var folderPath = _testFolderPath;
            var visitor = new FileSystemVisitor(folderPath);
            var eventRaised = false;
            var listOfFiles = new List<FileSystemInfo>();

            visitor.FilteredFilesFound += (sender, e) =>
            {
                listOfFiles = e;
                eventRaised = true;
            };

            // Act
            visitor.StartSearch();

            foreach (var item in listOfFiles)
            {
                Console.WriteLine(item);
            }

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.That(listOfFiles.Count() == 10, $"The result listOf {listOfFiles.Count()} is not equal to expected 10 ");
        }

       



        [Test]
        public void FileSystemVisitor_StartSearch_InvalidDirectory()
        {
            // Arrange
            var folderPath = "NonExistentDirectory";
            var visitor = new FileSystemVisitor(folderPath);
            var eventRaised = false;

            visitor.FileFound += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act and Assert
            Assert.That(() =>
            {
                visitor.InspectObjectDirectory(new DirectoryInfo(folderPath));
            },
           Throws.Exception
            .TypeOf<Exception>()
            .With.Message.EqualTo("There is no such Directory"));
        }

    }
}

