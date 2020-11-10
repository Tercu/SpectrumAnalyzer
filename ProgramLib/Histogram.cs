using System;
using System.Collections.Generic;

namespace SpectrumAnalyser
{
    public class Histogram
    {
        public Dictionary<double, double> Data { get; private set; }
        public Dictionary<double, double> NormalizedData { get; private set; }
        public Dictionary<double, double> ShrinkedData { get; private set; }
        private Logger logger = Logger.GetInstance();

        private double maximum = 0;
        private double minimum = 0;
        private double shrinkedMaximum = 0;
        private double shrinkedMinimum = 0;

        public Histogram()
        {
            Data = new Dictionary<double, double>();
        }
        public void Add(double key, double value)
        {
            AddToDictionary(Data, key, value);
            SetMinMax(Data[key], ref minimum, ref maximum);
        }
        public void Normalize()
        {
            NormalizeDictionary(Data, minimum, maximum);
        }
        public void NormalizeShrinked()
        {
            NormalizeDictionary(ShrinkedData, shrinkedMinimum, shrinkedMaximum);
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
                AddToDictionary(ShrinkedData, key[i] / timesModified[i], value[i]);
                SetMinMax(value[i], ref shrinkedMinimum, ref shrinkedMaximum);
            }
        }
        private void AddToDictionary(Dictionary<double, double> dictionary, double key, double value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value; ;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
        private void SetMinMax(double value, ref double min, ref double max)
        {
            min = Math.Min(value, min);
            max = Math.Max(value, max);
        }
        private void NormalizeDictionary(Dictionary<double, double> dictionary, double min, double max)
        {
            NormalizedData = new Dictionary<double, double>();
            foreach (var key in dictionary.Keys)
            {
                NormalizedData[key] = (dictionary[key] - min) / (max - min);
            }
        }
    }
}
