using CSCore.DSP;
using CSCore.Utils;
using System.Collections.Generic;

namespace Spectrogram
{
    public class HistogramProcessor
    {
        public int RangeStart { get; init; }
        public int RangeSize { get; init; }
        public int Row { get; set; }
        public KeyValuePair<int, Histogram> CalculatedHisotgramQueue { get; private set; }
        private List<KeyValuePair<int, float[]>> sampleQueue;
        private readonly AudioData audioInfo;
        private readonly Histogram histogram = new Histogram();

        public HistogramProcessor(AudioData audioInfo, int rangeStart, int rangeSize, int row)
        {
            RangeSize = rangeSize;
            RangeStart = rangeStart;
            Row = row;
            this.audioInfo = audioInfo;
            sampleQueue = new List<KeyValuePair<int, float[]>>();
        }

        public bool IsInRange(int index)
        {
            if (index >= RangeStart && index < RangeStart + RangeSize)
                return true;
            return false;
        }
        public bool IsFull()
        {
            if (sampleQueue.Count >= RangeSize)
                return true;
            return false;
        }

        public void AddToQueue(KeyValuePair<int, float[]> keyValuePair)
        {
            sampleQueue.Add(keyValuePair);

        }

        public void QueueToHistogram()
        {
            Complex[] complex = new Complex[audioInfo.FftSampleSize];
            foreach (var sample in sampleQueue)
            {
                FftToComplex(sample.Value, complex);
                histogram.Add(complex, audioInfo.SampleRate);
            }
            CalculateHistogram();
            CalculatedHisotgramQueue = new KeyValuePair<int, Histogram>(Row, histogram);
            sampleQueue.Clear();
            sampleQueue = null;
        }

        private void CalculateHistogram()
        {
            histogram.CalculateDb();
            histogram.ShiftToPositive();
        }
        private void FftToComplex(float[] samples, Complex[] complex)
        {

            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] *= (float)FastFourierTransformation.HammingWindow(i, samples.Length);
            }
            FillComplexArrayRealOnly(samples, complex);
            FastFourierTransformation.Fft(complex, audioInfo.Exponent, FftMode.Forward);
        }
        private void FillComplexArrayRealOnly(float[] samples, Complex[] complex)
        {
            for (int i = 0; i < samples.Length; ++i)
            {
                complex[i].Real = samples[i];
                complex[i].Imaginary = 0;
            }
        }
    }
}
