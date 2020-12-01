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
        public ConcurrentQueue<KeyValuePair<int, float[]>> SampleQueue { get; private set; }
        private readonly ConcurrentQueue<KeyValuePair<int, Histogram>> histogramQueue;
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
            SampleQueue = new ConcurrentQueue<KeyValuePair<int, float[]>>();
            histogramQueue = new ConcurrentQueue<KeyValuePair<int, Histogram>>();
            Bitmap = bitmap;
            if (Bitmap == null)
            {
                long width = (AudioInfo.Length / AudioInfo.SampleRate) + 1000;
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

            int processorLimit = 3;
            List<HistogramProcessor> histogramProcessor = new List<HistogramProcessor>();

            while (!SampleQueue.IsEmpty || !FileFinished)
            {
                while (histogramProcessor.Count < processorLimit)
                {
                    //histogramProcessor.Add(new HistogramProcessor(AudioInfo, ++rangeStart * fftSamplesPerSecond, fftSamplesPerSecond + 1, ++row));
                    histogramProcessor.Add(new HistogramProcessor(AudioInfo, ++rangeStart * fftSamplesPerSecond, fftSamplesPerSecond, ++row));
                }
                if (SampleQueue.TryDequeue(out KeyValuePair<int, float[]> KVPair))
                {
                    for (int i = 0; i < histogramProcessor.Count; ++i)
                    {
                        if (histogramProcessor[i].IsFull && !histogramProcessor[i].MarkedToRemove)
                        {
                            FromHistogramToQueue(histogramProcessor, i);
                        }
                        else if (!histogramProcessor[i].MarkedToRemove && histogramProcessor[i].IsInRange(KVPair.Key))
                        {
                            histogramProcessor[i].AddToQueue(KVPair);
                        }
                    }
                }
                histogramProcessor.RemoveAll((x) => x.MarkedToRemove);
            }

            FromHistogramToQueue(histogramProcessor, 0);
            histogramProcessor.Clear();

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
            histogramProcessor[i].MarkToRemove();
        }

        private async Task SaveToBitmapAsync()
        {
            while (!SampleQueue.IsEmpty || histogramQueue.Count > 0 || !FileFinished)
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

        public async Task ReadFileToListAsync(int maxQueueSize = 1000)
        {
            int currentSample = -1;
            while (AudioFile.ReadFile(out float[] result) != 0)
            {
                if (SampleQueue.Count < maxQueueSize)
                {
                    KeyValuePair<int, float[]> KVPair = new KeyValuePair<int, float[]>(++currentSample, result);
                    SampleQueue.Enqueue(KVPair);
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
