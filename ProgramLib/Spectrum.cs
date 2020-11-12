using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SpectrumAnalyser
{
    public class Spectrum
    {
        private readonly List<Task> tasks = new List<Task>();
        private readonly Logger logger = Logger.GetInstance();
        private readonly DirectoryManager dirManager;
        private List<AudioFile> AudioList;
        private Histogram histogram;
        private BitmapGenerator bitmap;
        public string PathToDirectory { get; private set; }

        public void RunSinglethread()
        {
            foreach (var audio in AudioList)
            {
                ProcessFile(audio);
            }
        }

        public void StartTasks()
        {
            tasks.ForEach(t => t.Start());
        }
        public void WaitForTasks()
        {
            tasks.ForEach(t => t.Wait());
        }

        public Spectrum(string pathToDirectory)
        {
            PathToDirectory = pathToDirectory;
            dirManager = new DirectoryManager(pathToDirectory);
            dirManager.CreateFileList();
            AudioList = new List<AudioFile>();
            histogram = new Histogram();
            foreach (var pathToFile in dirManager.AudioFileList)
            {
                AudioList.Add(new AudioFile(pathToFile));
            }

        }
        public void ProcessFile(AudioFile audio)
        {

            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(audio.FilePath)}");
            InitBitmap(audio);
            do
            {
                audio.ReadFile(histogram, bitmap);
            }
            while (audio.SampleSource.Position < audio.SampleSource.Length - audio.FftSampleSize);
            bitmap.SaveImage();
            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(audio.FilePath)} done in: {s2 - s1}");

        }
        private void InitBitmap(AudioFile audio)
        {
            bitmap = new BitmapGenerator(audio.FilePath, audio.BitmapWidth / 30 + 10, 1025);
        }
        public void InitTasks()
        {
            foreach (var audio in AudioList)
            {
                tasks.Add(new Task(() => ProcessFile(audio)));
            }
        }
    }
}
