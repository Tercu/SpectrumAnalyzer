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
        public ConcurrentQueue<HistogramProcessor> processorQueue { get; private set; }
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
            processorQueue = new ConcurrentQueue<HistogramProcessor>();
            Bitmap = bitmap;
            if (Bitmap == null)
            {
                long width = (AudioInfo.Length / AudioInfo.SampleRate) + 500;
                Bitmap = new BitmapGenerator(AudioInfo, (int)(width), AudioInfo.FftSampleSize / 2);
            }
        }

        public async Task ProcessFileAsync()
        {
            DateTime s1 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(AudioInfo.FilePath)}");


            var bitmapEditor = SaveToBitmapAsync();
            var distributor = DistributeDataInProcessors();
            var processor1 = HistogramProcessor();
            var processor2 = HistogramProcessor();

            await ReadFileToListAsync();
            distributor.Wait();
            processor1.Wait();
            processor2.Wait();
            bitmapEditor.Wait();

            Bitmap.SaveImage();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"File {Path.GetFileName(AudioInfo.FilePath)} done in: {s2 - s1}");
        }

        private async Task<Task> HistogramProcessor()
        {
            while (!processorQueue.IsEmpty || !FileFinished)
            {

                if (processorQueue.TryDequeue(out HistogramProcessor processor))
                {
                    processor.QueueToHistogram();
                    histogramQueue.Enqueue(processor.CalculatedHisotgramQueue);
                }
                else
                {
                    await Task.Delay(10);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<Task> DistributeDataInProcessors(int processorLimit = 20)
        {
            List<HistogramProcessor> histogramProcessor = new List<HistogramProcessor>();
            int row = -1;
            while (!SampleQueue.IsEmpty || !FileFinished)
            {
                while (histogramProcessor.Count < processorLimit)
                {
                    histogramProcessor.Add(new HistogramProcessor(AudioInfo, ++rangeStart * fftSamplesPerSecond, fftSamplesPerSecond, ++row));
                }
                if (SampleQueue.TryDequeue(out KeyValuePair<int, float[]> KVPair))
                {
                    DistributeSampleToHistogram(histogramProcessor, KVPair);
                }
                else
                {
                    await Task.Delay(10);
                }
                histogramProcessor.RemoveAll((x) => x.MarkedToRemove);
            }
            processorQueue.Enqueue(histogramProcessor[0]);
            histogramProcessor.Clear();
            return Task.CompletedTask;
        }

        private void DistributeSampleToHistogram(List<HistogramProcessor> histogramProcessor, KeyValuePair<int, float[]> KVPair)
        {
            for (int i = 0; i < histogramProcessor.Count; ++i)
            {
                if (histogramProcessor[i].IsFull && !histogramProcessor[i].MarkedToRemove)
                {
                    processorQueue.Enqueue(histogramProcessor[i]);
                    histogramProcessor[i].MarkToRemove();
                }
                else if (!histogramProcessor[i].MarkedToRemove && histogramProcessor[i].IsInRange(KVPair.Key))
                {
                    histogramProcessor[i].AddToQueue(KVPair);
                }
            }
        }

        private async Task<Task> SaveToBitmapAsync()
        {
            while (!SampleQueue.IsEmpty || !histogramQueue.IsEmpty || !FileFinished)
            {
                if (histogramQueue.TryDequeue(out KeyValuePair<int, Histogram> KVPair))
                {
                    Bitmap.EditRow(KVPair.Key, KVPair.Value);
                }
                else
                {
                    await Task.Delay(10);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<Task> ReadFileToListAsync(int maxQueueSize = 1000)
        {
            int currentSample = -1;
            long readedBits = -1;
            do
            {
                if (SampleQueue.Count < maxQueueSize)
                {
                    readedBits = AudioFile.ReadFile(out float[] result);
                    if (readedBits <= 0) break;
                    KeyValuePair<int, float[]> KVPair = new KeyValuePair<int, float[]>(++currentSample, result);
                    SampleQueue.Enqueue(KVPair);
                }
                else
                {
                    await Task.Delay(10);
                }
            }
            while (readedBits != 0);
            FileFinished = true;
            return Task.CompletedTask;
        }
    }
}
