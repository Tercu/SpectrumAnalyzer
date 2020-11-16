﻿using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Spectrogram.Test
{
    public class DirectoryManagerTest
    {
        [Fact]
        public void ShouldReturnFileList()
        {
            string path = @"/dev/test";
            MockFileSystem filesSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"/dev/test/1.flac", new MockFileData(string.Empty)},
                {@"/dev/test/2.flac", new MockFileData(string.Empty)},
                {@"/dev/test/SubFolder/3.flac", new MockFileData(string.Empty)},
                {@"/dev/test/SubFolder/4.flac", new MockFileData(string.Empty)},
            });
            DirectoryManager directory = new DirectoryManager(filesSystem, path);
            directory.CreateFileList();
            var result = directory.AudioFileList.Count;
            Assert.Equal(4, result);
        }
    }
}
