using CSCore.DSP;
using CSCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spectrogram
{
    public class AudioProcessor
    {
        public IAudioFile AudioFile { get; private set; }
        public IBitmapGenerator Bitmap { get; private set; }
        private readonly Logger logger = Logger.GetInstance();
        private readonly int sampleRate;
        private Histogram h = new Histogram();
        private ConcurrentQueue<float[]> sampleQueue;

        private Complex[] complex;
        private Boolean FileFinished = false;

        ~AudioProcessor()
        {
            AudioFile.Dispose();
            Bitmap.Dispose();
        }
        public AudioProcessor(IAudioFile audioFile, IBitmapGenerator bitmap = null)
        {
            AudioFile = audioFile;
            sampleRate = AudioFile.SampleSource.WaveFormat.SampleRate;
            sampleQueue = new ConcurrentQueue<float[]>();
            complex = new Complex[AudioFile.FftSampleSize];
            Bitmap = bitmap;
            if (Bitmap == null)
            {
                long width = (AudioFile.SampleSource.Length / sampleRate) + 1;
                Bitmap = new BitmapGenerator(AudioFile.FilePath, (int)(width), AudioFile.FftSampleSize / 2);
            }
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
            int currentRow = 0;
            ReadFileToQueue();
            while (sampleQueue.IsEmpty == false || FileFinished == false)
            {
                QueueToHistogram();
                CalculateHistogram();

                Bitmap.EditRow(currentRow, h);
                ++currentRow;
            }
            Debug.WriteLine($"Bitmap width: {AudioFile.SampleSource.Length / sampleRate}, row: { currentRow }");
            Bitmap.SaveImage();
        }

        private void CalculateHistogram()
        {
            h.CalculateDb();
            h.ShiftToPositive();
        }

        private void QueueToHistogram()
        {
            for (int i = 0; i <= sampleRate / AudioFile.FftSampleSize; ++i)
            {
                if (sampleQueue.TryDequeue(out float[] sample))
                {
                    FftToComplex(AudioFile.Exponent, sample, complex);
                    h.Add(complex, AudioFile.SampleSource.WaveFormat.SampleRate);
                }
                else if (FileFinished == false) { --i; }
                else { break; }
            }
        }

        private async void ReadFileToQueue(int maxQueueSize = 100)
        {
            while (AudioFile.SampleSource.Length - AudioFile.SampleSource.Position > AudioFile.FftSampleSize)
            {
                if (sampleQueue.Count < maxQueueSize)
                {
                    sampleQueue.Enqueue(AudioFile.ReadFile());
                }
                else
                {
                    await Task.Delay(50);
                }
            }
            FileFinished = true;
        }
        private void FillComplexArrayRealOnly(float[] samples, Complex[] complex)
        {
            for (int i = 0; i < samples.Length; ++i)
            {
                complex[i].Real = samples[i];
                complex[i].Imaginary = 0;
            }
        }
        private void FftToComplex(int exponent, float[] samples, Complex[] complex)
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
