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
        private readonly ConcurrentQueue<KeyValuePair<int, float[]>> sampleQueue;
        private ConcurrentQueue<KeyValuePair<int, Histogram>> histogramQueue;
        private readonly int fftSamplesPerSecond;
        private int rangeStart = -1;

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
            sampleQueue = new ConcurrentQueue<KeyValuePair<int, float[]>>();
            histogramQueue = new ConcurrentQueue<KeyValuePair<int, Histogram>>();
            Bitmap = bitmap;
            if (Bitmap == null)
            {
                long width = (AudioInfo.Length / AudioInfo.SampleRate);
                Bitmap = new BitmapGenerator(AudioInfo, (int)(width), AudioInfo.FftSampleSize / 2);
            }
        }

        public void ProcessFile()
        {
            int row = -1;
            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(AudioInfo.FilePath)}");


            var fileReaderTask = ReadFileToListAsync();
            var bitmapEditor = SaveToBitmapAsync();


            List<HistogramProcessor> histogramProcessor = new List<HistogramProcessor>();
            histogramProcessor.Add(new HistogramProcessor(AudioInfo, ++rangeStart * fftSamplesPerSecond, fftSamplesPerSecond + 1, ++row));
            while (!sampleQueue.IsEmpty || !FileFinished)
            {
                if (sampleQueue.TryDequeue(out KeyValuePair<int, float[]> KVPair))
                {
                    for (int i = 0; i < histogramProcessor.Count; ++i)
                    {
                        if (histogramProcessor[i].IsFull())
                        {
                            FromHistogramToQueue(histogramProcessor, i);
                            histogramProcessor.RemoveAt(i);
                            histogramProcessor.Add(new HistogramProcessor(AudioInfo, ++rangeStart * fftSamplesPerSecond, fftSamplesPerSecond, ++row));
                        }
                        else
                        {
                            histogramProcessor[i].AddToQueue(KVPair);
                        }
                    }
                }
            }

            FromHistogramToQueue(histogramProcessor, 0);
            histogramProcessor.RemoveAt(0);

            fileReaderTask.Wait();
            bitmapEditor.Wait();

            Bitmap.SaveImage();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(AudioInfo.FilePath)} done in: {s2 - s1}");
        }

        private void FromHistogramToQueue(List<HistogramProcessor> histogramProcessor, int i)
        {
            histogramProcessor[i].QueueToHistogram();
            histogramQueue.Enqueue(histogramProcessor[i].CalculatedHisotgramQueue);
        }

        private async Task SaveToBitmapAsync()
        {
            while (!sampleQueue.IsEmpty || histogramQueue.Count > 0 || !FileFinished)
            {
                if (histogramQueue.TryDequeue(out KeyValuePair<int, Histogram> KVPair))
                {
                    Bitmap.EditRow(KVPair.Key, KVPair.Value);
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
                    KeyValuePair<int, float[]> KVPair = new KeyValuePair<int, float[]>(++currentSample, AudioFile.ReadFile());
                    sampleQueue.Enqueue(KVPair);
                }
                else
                {
                    await Task.Delay(50);
                }
            }
            FileFinished = true;
        }
    }
}
