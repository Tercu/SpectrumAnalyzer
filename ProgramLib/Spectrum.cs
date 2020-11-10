using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpectrumAnalyser
{
    public class Spectrum
    {
        private readonly List<Task> tasks = new List<Task>();
        private readonly Logger logger = Logger.GetInstance();
        private readonly DirectoryManager dirManager;
        public string PathToDirectory { get; private set; }

        public void StartTasks()
        {
            tasks.ForEach(t => t.Start());
        }
        public void WaitForTasks()
        {
            tasks.ForEach(t => t.Wait());
        }
        public void CreateFileList()
        {
            dirManager.CreateFileList();
        }

        public Spectrum(string pathToDirectory)
        {
            PathToDirectory = pathToDirectory;
            dirManager = new DirectoryManager(pathToDirectory);
        }
        public void InitTasks()
        {
            foreach (var pathToFile in dirManager.AudioFileList)
            {
                tasks.Add(new Task(() => ProcessFile(pathToFile)));
            }
        }
        private void ProcessFile(string pathToFile)
        {
            DateTime s1 = DateTime.Now;

            AudioFile audio = new AudioFile(pathToFile);
            audio.ReadFile();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Done in: {s2 - s1}");
        }
    }
}
