using CSCore.DSP;
using CSCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spectrogram
{
    public class AudioProcessor
    {
        public AudioFile AudioFile { get; private set; }
        private readonly Logger logger = Logger.GetInstance();
        private BitmapGenerator bitmap;
        private int sampleRate;

        public AudioProcessor(AudioFile audioFile)
        {
            this.AudioFile = audioFile;
            sampleRate = AudioFile.SampleSource.WaveFormat.SampleRate;
        }
        public void ProcessFile()
        {

            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(AudioFile.FilePath)}");

            CreateHistogramList();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(AudioFile.FilePath)} done in: {s2 - s1}");
        }


        private void CreateHistogramList()
        {
            bitmap = new BitmapGenerator(AudioFile.FilePath, (int)AudioFile.SampleSource.Length / AudioFile.SampleSource.WaveFormat.SampleRate, AudioFile.FftSampleSize / 2);
            Histogram h = new Histogram();
            int counter = 1;
            Complex[] complex = new Complex[AudioFile.FftSampleSize];
            float[] samples;
            while (AudioFile.SampleSource.Length - AudioFile.SampleSource.Position > AudioFile.FftSampleSize)
            {

                samples = AudioFile.ReadFile();
                PerformFft(AudioFile.Exponent, samples, complex);
                h.Add(complex, AudioFile.SampleSource.WaveFormat.SampleRate);
                _ = sampleRate / AudioFile.FftSampleSize;

                if (AudioFile.SampleSource.Position / sampleRate == counter)
                {
                    h.CalculateDb();
                    h.ShiftToPositive();
                    bitmap.EditRow(counter - 1, h);

                    ++counter;
                }
            }
            bitmap.SaveImage();
        }
        private void FillComplexArrayRealOnly(float[] samples, Complex[] complex)
        {
            for (int i = 0; i < samples.Length; ++i)
            {
                complex[i].Real = samples[i];
                complex[i].Imaginary = 0;
            }
        }
        private void PerformFft(int exponent, float[] samples, Complex[] complex)
        {

            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] *= (float)FastFourierTransformation.HammingWindow(i, samples.Length);
            }
            FillComplexArrayRealOnly(samples, complex);
            FastFourierTransformation.Fft(complex, exponent, FftMode.Forward);
        }
    }
}
