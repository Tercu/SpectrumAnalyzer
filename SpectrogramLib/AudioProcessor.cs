using CSCore.DSP;
using CSCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentQueue<KeyValuePair<int, Histogram>> calculatedHisotgramQueue;
        private readonly ConcurrentQueue<KeyValuePair<int, float[]>> sampleQueue;
        private readonly int fftSamplesPerSecond;

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
            fftSamplesPerSecond = AudioInfo.SampleRate / AudioInfo.FftSampleSize;
            complex = new Complex[AudioInfo.FftSampleSize];
            sampleQueue = new ConcurrentQueue<KeyValuePair<int, float[]>>();
            calculatedHisotgramQueue = new ConcurrentQueue<KeyValuePair<int, Histogram>>();
            Bitmap = bitmap;
            if (Bitmap == null)
            {
                long width = (AudioInfo.Length / AudioInfo.SampleRate) + 1000;
                Bitmap = new BitmapGenerator(AudioInfo, (int)(width), AudioInfo.FftSampleSize / 2);
            }
        }

        public void ProcessFile()
        {
            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(AudioInfo.FilePath)}");

            var fileReaderTask = ReadFileToListAsync();
            var histogramCalculator = QueueToHistogram();
            var bitmapEditor = SaveToBitmapAsync();

            fileReaderTask.Wait();
            histogramCalculator.Wait();
            bitmapEditor.Wait();
            Bitmap.SaveImage();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(AudioInfo.FilePath)} done in: {s2 - s1}");
        }

        private async Task SaveToBitmapAsync()
        {
            while (sampleQueue.Count > 0 || calculatedHisotgramQueue.Count > 0 || !FileFinished)
            {
                if (calculatedHisotgramQueue.TryDequeue(out KeyValuePair<int, Histogram> KVPair))
                {
                    Bitmap.EditRow(KVPair.Key, KVPair.Value);
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private void CalculateHistogram(Histogram histogram)
        {
            histogram.CalculateDb();
            histogram.ShiftToPositive();
        }

        private async Task QueueToHistogram()
        {
            int maxQueueSize = 100;
            int currentRow = 0;
            while (sampleQueue.Count > 0 || !FileFinished)
            {
                if (calculatedHisotgramQueue.Count <= maxQueueSize)
                {
                    Histogram histogram = new Histogram();
                    while (sampleQueue.TryPeek(out KeyValuePair<int, float[]> KVPair) && KVPair.Key == currentRow)
                    {
                        _ = sampleQueue.TryDequeue(out KVPair);
                        FftToComplex(AudioInfo.Exponent, KVPair.Value);
                        histogram.Add(complex, AudioInfo.SampleRate);
                    }
                    CalculateHistogram(histogram);
                    calculatedHisotgramQueue.Enqueue(new KeyValuePair<int, Histogram>(currentRow, histogram));
                    ++currentRow;
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private async Task ReadFileToListAsync(int maxQueueSize = 1000)
        {
            int currentSample = -1;
            while (AudioInfo.Length - AudioFile.SampleSource.Position > AudioInfo.FftSampleSize)
            {
                if (sampleQueue.Count < maxQueueSize)
                {
                    KeyValuePair<int, float[]> KVPair = new KeyValuePair<int, float[]>(++currentSample / fftSamplesPerSecond, AudioFile.ReadFile());
                    sampleQueue.Enqueue(KVPair);
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
