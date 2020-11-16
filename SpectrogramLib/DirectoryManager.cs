using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Spectrogram
{
    public class DirectoryManager
    {
        public string[] FilePath { get; set; }
        public string AudioFileExtension { get; set; }
        public string SpectrumFileExtension { get; set; }
        public List<string> AudioFileList { get; private set; }
        private Logger logger = Logger.GetInstance();

        private readonly IFileSystem fileSystem;

        public DirectoryManager(IFileSystem _fileSystem, string[] path, string audioExtension = ".flac", string spectrumExtension = ".png")
        {
            fileSystem = _fileSystem;
            FilePath = path;
            AudioFileExtension = audioExtension;
            SpectrumFileExtension = spectrumExtension;
        }

        public void CreateFileList()
        {
            List<string> audioFiles = GetFileList(AudioFileExtension);
            List<string> spectrumFiles = GetFileList(SpectrumFileExtension);

            RemoveFilesWithSpectrum(audioFiles, spectrumFiles);
        }

        private List<string> GetFileList(string extension)
        {
            List<string> files = new List<string>();
            foreach (var path in FilePath)
            {
                files.AddRange(fileSystem.Directory.GetFiles(path, $"*{extension}", SearchOption.AllDirectories));
            }
            return files;
        }

        private void RemoveFilesWithSpectrum(List<string> audioFiles, List<string> spectrumFiles)
        {
            for (int i = 0; i < spectrumFiles.Count; i++)
            {
                string path = @$"{ Path.GetDirectoryName(spectrumFiles[i]) }\{ Path.GetFileNameWithoutExtension(spectrumFiles[i]) }{ AudioFileExtension }";
                audioFiles.Remove(path);
            };
            AudioFileList = audioFiles;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Found {AudioFileList.Count} files in '{FilePath}'.");
        }
    }
}
