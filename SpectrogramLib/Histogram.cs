using CSCore.Utils;
using System;
using System.Collections.Generic;

namespace Spectrogram
{
    public class Histogram
    {
        public Dictionary<double, double> Data { get; private set; }
        private readonly Logger logger = Logger.GetInstance();

        public Histogram()
        {
            Data = new Dictionary<double, double>();
        }
        public void Add(Complex[] complex, int sampleRate, int fftSampleSize)
        {
            for (int i = 0; i < complex.Length / 2; i++)
            {
                double freq = i * sampleRate / fftSampleSize;
                double val = complex[i].Real * complex[i].Real + complex[i].Imaginary * complex[i].Imaginary;
                AddToDictionary(Data, freq, val);
            }
        }
        public void ShiftToPositive(double shift)
        {
            Dictionary<double, double> ToPositive = new Dictionary<double, double>();
            foreach (var key in Data.Keys)
            {
                double v = Data[key] - shift;
                ToPositive[key] = v;
            }
            Data = ToPositive;
        }
        public void Normalize(double max, double min)
        {
            Dictionary<double, double> NormalizedData = new Dictionary<double, double>();

            double factor = (max - min);
            foreach (var key in Data.Keys)
            {
                double v = (Data[key] - min) / factor;
                NormalizedData[key] = v;
            }
            Data = NormalizedData;
        }
        public void Shrink(int shrinkedSize)
        {
            double[] key = new double[shrinkedSize];
            double[] value = new double[shrinkedSize];
            int[] timesModified = new int[shrinkedSize];
            double modifier = shrinkedSize / Data.Count;
            int index = 0;
            int newIndex = (int)(index * modifier);
            foreach (var data in Data)
            {
                key[newIndex] += data.Key;
                value[newIndex] += data.Value;
                ++timesModified[newIndex];
                ++index;
            }
            for (int i = 0; i < key.Length; ++i)
            {
                //AddToDictionary(ShrinkedData, key[i] / timesModified[i], value[i]);
            }
        }
        private void AddToDictionary(Dictionary<double, double> dictionary, double key, double value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public void CalculateDb()
        {
            Dictionary<double, double> DbInfo = new Dictionary<double, double>();
            foreach (var key in Data.Keys)
            {
                double log = -18;
                if (Data[key] != 0)
                {
                    log = Math.Log10(Data[key]);
                }
                DbInfo[key] = 10 * log;
            }
            Data = DbInfo;
        }
    }
}
