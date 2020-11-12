using CSCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpectrumAnalyser
{
    public class Histogram
    {
        public Dictionary<double, double> Data { get; private set; }
        public Dictionary<double, double> NormalizedData { get; private set; }
        public Dictionary<double, double> ShrinkedData { get; private set; }
        private Logger logger = Logger.GetInstance();

        public Histogram()
        {
            Data = new Dictionary<double, double>();
            NormalizedData = new Dictionary<double, double>();
        }
        public void Add(double key, Complex complex)
        {
            //double val = 10 * Math.Log(Math.Pow(complex.Real, 2) + Math.Pow(complex.Imaginary, 2));
            double val = 10 * Math.Log(complex.Real * complex.Real + complex.Real * complex.Imaginary);
            AddToDictionary(Data, key, complex);
        }
        public void Normalize()
        {
            NormalizeDictionary(Data, NormalizedData);
        }
        public void NormalizeShrinked()
        {
            NormalizeDictionary(ShrinkedData, NormalizedData);
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

        private void NormalizeDictionary(Dictionary<double, double> source, Dictionary<double, double> destination)
        {
            //destination = new Dictionary<double, double>();
            double max = source.Values.Max();
            double min = source.Values.Min();
            if (min != max)
            {
                foreach (var key in source.Keys)
                {
                    double v = (source[key] - min) / (max - min);
                    destination[key] = v;

                }
            }
        }
    }
}
