using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Spectrogram
{
    public class DirectoryManager
    {
        public List<string> FilePath { get; set; }
        public List<string> AudioFileExtension { get; set; }
        public string SpectrumFileExtension { get; set; }
        public List<string> AudioFileList { get; private set; }
        private readonly Logger logger = Logger.GetInstance();

        private readonly IFileSystem fileSystem;

        public DirectoryManager(IFileSystem _fileSystem, List<string> path, List<string> audioExtension = null, string spectrumExtension = ".png")
        {
            fileSystem = _fileSystem;
            FilePath = path;
            int empty = 0;
            FilePath.ForEach((x) => empty += string.IsNullOrEmpty(x) ? 1 : 0);
            if (empty == FilePath.Count)
            {
                throw new DirectoryNotFoundException("Path cannot be empty!");
            }

            AudioFileExtension = audioExtension;
            if (AudioFileExtension == null)
            {
                AudioFileExtension = new List<string> { { ".flac" } };
            }
            SpectrumFileExtension = spectrumExtension;
        }

        public void CreateFileList()
        {
            List<string> audioFiles = new List<string>();
            AudioFileExtension.ForEach(x => audioFiles.AddRange(GetFileList(x)));
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
            char separator = Path.DirectorySeparatorChar;
            for (int i = 0; i < spectrumFiles.Count; i++)
            {
                foreach (var extension in AudioFileExtension)
                {
                    string path = @$"{ Path.GetDirectoryName(spectrumFiles[i]) }{separator}{ Path.GetFileNameWithoutExtension(spectrumFiles[i]) }{ extension }";
                    audioFiles.Remove(path);
                }
            }
            AudioFileList = audioFiles;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Found {AudioFileList.Count} files.");
        }
    }
}
