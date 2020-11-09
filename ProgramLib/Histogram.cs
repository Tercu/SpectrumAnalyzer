using System.Collections.Generic;

namespace SpectrumAnalyser
{
    public class Histogram
    {
        public SortedDictionary<double, double> Data { get; private set; }
        public Histogram()
        {
            Data = new SortedDictionary<double, double>();
        }
        public void Add(double key, double value)
        {
            if (Data.ContainsKey(key))
            {
                Data[key] += value; ;
            }
            else
            {
                Data.Add(key, value);
            }
        }

    }
}
