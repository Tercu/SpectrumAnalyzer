using CSCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spectrogram
{
    public class AudioProcessor
    {
        private AudioFile audioFile;
        private readonly Logger logger = Logger.GetInstance();
        private BitmapGenerator bitmap;
        public AudioProcessor(AudioFile audioFile)
        {
            this.audioFile = audioFile;
        }
        public void ProcessFile()
        {

            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(audioFile.FilePath)}");

            CreateHistogramList();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(audioFile.FilePath)} done in: {s2 - s1}");
        }


        private void CreateHistogramList()
        {
            bitmap = new BitmapGenerator(audioFile.FilePath, (int)audioFile.SampleSource.Length / audioFile.SampleSource.WaveFormat.SampleRate, audioFile.FftSampleSize / 2);
            Histogram h = new Histogram();
            int sampleRate = audioFile.SampleSource.WaveFormat.SampleRate;
            int counter = 1;
            Complex[] complex;
            for (int i = 0; i <= audioFile.SampleSource.Length - audioFile.FftSampleSize; i = (int)audioFile.SampleSource.Position)
            {
                complex = audioFile.ReadFile();
                h.Add(complex, audioFile.SampleSource.WaveFormat.SampleRate);
                if (i / sampleRate == counter)
                {
                    h.CalculateDb();
                    h.ShiftToPositive();
                    bitmap.EditRow(counter - 1, h);
                    //histogramList.Add(h);
                    //h = new Histogram();
                    ++counter;
                }
            }
            bitmap.SaveImage();
        }
    }
}
