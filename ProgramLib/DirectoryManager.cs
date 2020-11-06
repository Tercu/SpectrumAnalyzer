using System.Collections.Generic;
using System.IO;

namespace SpectrumAnalyser
{
    public class DirectoryManager
    {
        public string FilePath { get; set; }
        public string AudioFileExtension { get; set; }
        public string SpectrumFileExtension { get; set; }
        public List<string> AudioFileList { get; private set; }
        public DirectoryManager(string path, string audioExtension = ".flac", string spectrumExtension = ".png")
        {
            FilePath = path;
            AudioFileExtension = audioExtension;
            SpectrumFileExtension = spectrumExtension;
        }

        public void CreateFileList()
        {
            List<string> audioFiles = new List<string>(Directory.GetFiles(FilePath, $"*{AudioFileExtension}", SearchOption.AllDirectories));
            List<string> spectrumFiles = new List<string>(Directory.GetFiles(FilePath, $"*{SpectrumFileExtension}", SearchOption.AllDirectories));

            RemoveFilesWithSpectrum(audioFiles, spectrumFiles);
        }

        private void RemoveFilesWithSpectrum(List<string> audioFiles, List<string> spectrumFiles)
        {
            for (int i = 0; i < spectrumFiles.Count; i++)
            {
                string path = @$"{ Path.GetDirectoryName(spectrumFiles[i]) }\{ Path.GetFileNameWithoutExtension(spectrumFiles[i]) }{ AudioFileExtension }";
                audioFiles.Remove(path);
            };
            AudioFileList = audioFiles;
        }
    }
}
