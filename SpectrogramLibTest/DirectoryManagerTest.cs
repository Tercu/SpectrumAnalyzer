using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Spectrogram.Test
{
    public class DirectoryManagerTest
    {
        [Theory]
        [MemberData(nameof(GetFileList))]
        public void ShouldReturnFileListWithoutRepeats(MockFileSystem fileSystem,
            List<string> paths, List<string> extensions, int expected)
        {
            DirectoryManager directory = new DirectoryManager(fileSystem, paths, extensions);
            directory.CreateFileList();
            var result = directory.AudioFileList.Count;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ShoutldThrowExceptionWhenPathIsEmpty()
        {
            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    {@"/test/1.flac", new MockFileData(string.Empty)},
                    {@"/test/2.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/4.flac", new MockFileData(string.Empty)},
                });
            List<string> paths = new List<string> { @"", };

            Assert.Throws<System.IO.DirectoryNotFoundException>(() => new DirectoryManager(fileSystem, paths));
        }

        public static IEnumerable<object[]> GetFileList()
        {
            yield return new object[] {
                new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    {@"/test/1.flac", new MockFileData(string.Empty)},
                    {@"/test/2.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/4.flac", new MockFileData(string.Empty)},
                }),
                new List<string>
                    {
                    @"/test",
                    },
                new List<string>
                    {
                    @".flac",
                    },
                4 };

            yield return new object[] {
                new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    {@"/test/1.flac", new MockFileData(string.Empty)},
                    {@"/test/1.png", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/4.flac", new MockFileData(string.Empty)},
                }),
                new List<string>
                    {
                    @"/test",
                    },
                new List<string>
                    {
                    @".flac",
                    },
                2 };

            yield return new object[] {
                new MockFileSystem(new Dictionary<string, MockFileData>
                    {
                    {@"/test/1.flac", new MockFileData(string.Empty)},
                    {@"/test/1.png", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/4.flac", new MockFileData(string.Empty)},
                    {@"/test2/1.flac", new MockFileData(string.Empty)},
                    {@"/test2/1.png", new MockFileData(string.Empty)},
                    {@"/test2/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test2/SubFolder/4.flac", new MockFileData(string.Empty)},
                }),
                new List<string>
                    {
                    @"/test",
                    @"/test2",
                    },
                new List<string>
                    {
                    @".flac",
                    },
                4};

            yield return new object[] {
                new MockFileSystem(new Dictionary<string, MockFileData>
                    {
                    {@"/test/1.flac", new MockFileData(string.Empty)},
                    {@"/test/1.mp3", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test/SubFolder/4.flac", new MockFileData(string.Empty)},
                    {@"/test2/1.flac", new MockFileData(string.Empty)},
                    {@"/test2/1.png", new MockFileData(string.Empty)},
                    {@"/test2/SubFolder/3.flac", new MockFileData(string.Empty)},
                    {@"/test2/SubFolder/4.mp3", new MockFileData(string.Empty)},
                }),
                new List<string>
                    {
                    @"/test",
                    @"/test2",
                    },
                new List<string>
                    {
                    @".flac",
                    @".mp3",
                    },
                6};
        }

    }
}
