using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Spectrogram.Test
{
    public class DirectoryManagerTest
    {
        [Theory]
        [MemberData(nameof(GetFileList))]
        public void ShouldReturnFileListWithoutRepeats(MockFileSystem fileSystem, string[] paths, int expected)
        {
            DirectoryManager directory = new DirectoryManager(fileSystem, paths);
            directory.CreateFileList();
            var result = directory.AudioFileList.Count;
            Assert.Equal(expected, result);
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
                new string[]
                    {
                    @"/test",
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
                new string[]
                    {
                    @"/test",
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
                new string[]
                    {
                    @"/test",
                    @"/test2",
                    },
                4};

            //// Future test with multiple audio extensions
            //yield return new object[] {
            //    new MockFileSystem(new Dictionary<string, MockFileData>
            //        {
            //        {@"/test/1.flac", new MockFileData(string.Empty)},
            //        {@"/test/1.mp3", new MockFileData(string.Empty)},
            //        {@"/test/SubFolder/3.flac", new MockFileData(string.Empty)},
            //        {@"/test/SubFolder/4.flac", new MockFileData(string.Empty)},
            //        {@"/test2/1.flac", new MockFileData(string.Empty)},
            //        {@"/test2/1.png", new MockFileData(string.Empty)},
            //        {@"/test2/SubFolder/3.flac", new MockFileData(string.Empty)},
            //        {@"/test2/SubFolder/4.flac", new MockFileData(string.Empty)},
            //    }),
            //    new string[]
            //        {
            //        @"/test",
            //        @"/test2",
            //        },
            //    6};
        }

    }
}
