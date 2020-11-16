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
        private List<Histogram> histogramList;
        private BitmapGenerator bitmap;
        public AudioProcessor(AudioFile audioFile)
        {
            this.audioFile = audioFile;
            this.histogramList = new List<Histogram>();
        }
        public void ProcessFile()
        {

            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(audioFile.FilePath)}");

            CreateHistogramList();
            CalculateDbData();
            DbToPositiveNumbers();
            NormalizeHistogram();
            GenerateBitmap();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(audioFile.FilePath)} done in: {s2 - s1}");
        }

        private void GenerateBitmap()
        {
            bitmap = new BitmapGenerator(audioFile.FilePath, histogramList.Count, histogramList[0].Data.Count);
            for (int i = 0; i < histogramList.Count; ++i)
            {
                bitmap.EditRow(i, histogramList[i]);
            }
            bitmap.SaveImage();
        }

        private void CreateHistogramList()
        {
            Histogram h = new Histogram();
            int counter = 1;
            for (int i = 0; i <= audioFile.SampleSource.Length - audioFile.FftSampleSize; i = (int)audioFile.SampleSource.Position)
            {
                int sampleRate = audioFile.SampleSource.WaveFormat.SampleRate;
                Complex[] complex = audioFile.ReadFile();
                h.Add(complex, audioFile.SampleSource.WaveFormat.SampleRate, audioFile.FftSampleSize);
                if (i / sampleRate == counter)
                {
                    histogramList.Add(h);
                    h = new Histogram();
                    ++counter;
                }
            }
        }
        private void DbToPositiveNumbers()
        {
            double min = Double.MaxValue;
            foreach (var h in histogramList)
            {
                min = Math.Min(min, h.Data.Values.Min());
            }
            foreach (var h in histogramList)
            {
                h.ShiftToPositive(min);
            }
        }
        private void NormalizeHistogram()
        {
            double min = Double.MaxValue;
            double max = Double.MinValue;
            foreach (var h in histogramList)
            {
                max = Math.Max(max, h.Data.Values.Max());
                min = Math.Min(min, h.Data.Values.Min());
                h.Normalize(max, min);
            }
        }
        private void CalculateDbData()
        {
            foreach (var h in histogramList)
            {
                h.CalculateDb();
            }
        }
    }
}
