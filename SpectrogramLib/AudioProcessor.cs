using CSCore.DSP;
using CSCore.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Spectrogram
{
    public class AudioProcessor
    {
        public IBitmapGenerator Bitmap { get; private set; }
        public AudioData AudioInfo { get; init; }
        private readonly Logger logger = Logger.GetInstance();
        private IAudioFile AudioFile { get; init; }
        private readonly Histogram histogram = new Histogram();
        private readonly ConcurrentQueue<float[]> sampleQueue;

        private readonly Complex[] complex;
        private Boolean FileFinished = false;

        ~AudioProcessor()
        {
            AudioFile.Dispose();
            Bitmap.Dispose();
        }
        public AudioProcessor(IAudioFile audioFile, IBitmapGenerator bitmap = null)
        {
            AudioFile = audioFile;
            AudioInfo = AudioFile.GetAudioData();
            sampleQueue = new ConcurrentQueue<float[]>();
            complex = new Complex[AudioInfo.FftSampleSize];
            Bitmap = bitmap;
            if (Bitmap == null)
            {
                long width = (AudioInfo.Length / AudioInfo.SampleRate) + 1;
                Bitmap = new BitmapGenerator(AudioInfo, (int)(width), AudioInfo.FftSampleSize / 2);
            }
        }

        public void ProcessFile()
        {
            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(AudioInfo.FilePath)}");

            int currentRow = 0;
            _ = ReadFileToQueueAsync();
            while (!sampleQueue.IsEmpty || !FileFinished)
            {
                QueueToHistogram();
                CalculateHistogram();

                Bitmap.EditRow(currentRow, histogram);
                ++currentRow;
            }
            Bitmap.SaveImage();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(AudioInfo.FilePath)} done in: {s2 - s1}");
        }

        private void CalculateHistogram()
        {
            histogram.CalculateDb();
            histogram.ShiftToPositive();
        }

        private void QueueToHistogram()
        {
            for (int i = 0; i <= AudioInfo.SampleRate / AudioInfo.FftSampleSize; ++i)
            {
                if (sampleQueue.TryDequeue(out float[] sample))
                {
                    FftToComplex(AudioInfo.Exponent, sample);
                    histogram.Add(complex, AudioInfo.SampleRate);
                }
                else if (!FileFinished) { --i; }
                else { break; }
            }
        }

        private async Task ReadFileToQueueAsync(int maxQueueSize = 100)
        {
            while (AudioInfo.Length - AudioFile.SampleSource.Position > AudioInfo.FftSampleSize)
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
        private void FillComplexArrayRealOnly(float[] samples)
        {
            for (int i = 0; i < samples.Length; ++i)
            {
                complex[i].Real = samples[i];
                complex[i].Imaginary = 0;
            }
        }
        private void FftToComplex(int exponent, float[] samples)
        {

            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] *= (float)FastFourierTransformation.HammingWindow(i, samples.Length);
            }
            FillComplexArrayRealOnly(samples);
            FastFourierTransformation.Fft(complex, exponent, FftMode.Forward);
        }
    }
}
