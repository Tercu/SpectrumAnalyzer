using System;
using System.IO;

namespace SpectrumAnalyser
{
    public class AudioProcessor
    {
        private AudioFile audioFile;
        private readonly Logger logger = Logger.GetInstance();
        private Histogram histogram;
        private BitmapGenerator bitmap;
        public AudioProcessor(AudioFile audioFile)
        {
            this.audioFile = audioFile;
            this.histogram = new Histogram();
            bitmap = new BitmapGenerator(audioFile.FilePath, audioFile.BitmapWidth / 30 + 10, 1025);
        }
        public void ProcessFile()
        {

            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(audioFile.FilePath)}");
            do
            {
                audioFile.ReadFile(histogram, bitmap);
            }
            while (audioFile.SampleSource.Position < audioFile.SampleSource.Length - audioFile.FftSampleSize);
            bitmap.SaveImage();
            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(audioFile.FilePath)} done in: {s2 - s1}");
        }
    }
}
