using CSCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrogram
{
    public class Histogram
    {
        public double Min { get { return Data.Values.Min(); } }
        public double Max { get { return Data.Values.Max(); } }
        public Dictionary<double, double> Data { get; private set; }
        private readonly Logger logger = Logger.GetInstance();

        public Histogram()
        {
            Data = new Dictionary<double, double>();
        }
        public void Add(Complex[] complex, int sampleRate)
        {
            for (int i = 0; i < complex.Length / 2; i++)
            {
                double freq = i * sampleRate / complex.Length;
                double val = complex[i].Real * complex[i].Real + complex[i].Imaginary * complex[i].Imaginary;
                Data[freq] = val;
            }
        }

        public void ShiftToPositive(double oldMax = -10, double oldMin = -200, double newMax = 0, double newMin = 1023)
        {
            foreach (var key in Data.Keys)
            {
                double v = (Data[key] - oldMin) * (newMax - newMin) / (oldMax - oldMin) + newMin;
                if (v > newMin) { v = newMin; }
                if (v < newMax) { v = newMax; }
                Data[key] = v;
            }
        }

        public void CalculateDb()
        {
            foreach (var key in Data.Keys)
            {
                double log = -10;
                if (Data[key] != 0)
                {
                    log = Math.Log10(Data[key]);
                }
                Data[key] = 10 * log;
            }
        }
    }
}
