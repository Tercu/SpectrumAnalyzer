using CSCore.Utils;
using System.Collections.Generic;
using Xunit;
using System.Linq;

namespace Spectrogram.Test
{
    public class HistogramTest
    {
        readonly int precision = 7;
        readonly int samppleRate = 16;
        readonly Complex[] complex =  {
            new Complex( (float)1, (float)2 ),
            new Complex( (float)0.1, (float)0 ),
            new Complex( (float)1, (float)2 ),
            new Complex( (float)0.01, (float)0.01 ),
            new Complex( (float)1, (float)2 ),
            new Complex( (float)0.1, (float)0 ),
            new Complex( (float)1, (float)2 ),
            new Complex( (float)0.01, (float)0.01 ),
        };
        [Fact]
        public void ShouldCalculateFrequencyAndValue()
        {
            Dictionary<double, double> result = new Dictionary<double, double>
        {
            {0, 5},
            {2, 0.010000000707805157},
            {4, 5},
            {6, 0.00019999999494757503},
        };

            Histogram histogram = new Histogram();
            histogram.Add(complex, samppleRate);
            CheckValues(result, histogram);
        }

        [Fact]
        public void ShouldCalculateDbValues()
        {
            Dictionary<double, double> result = new Dictionary<double, double>
        {
            {0, 6.989700043360188},
            {2, -19.99999969260414},
            {4, 6.989700043360188},
            {6, -36.98970015307221},
        };

            Histogram histogram = new Histogram();
            histogram.Add(complex, samppleRate);
            histogram.CalculateDb();
            CheckValues(result, histogram);
        }

        [Fact]
        public void ShouldShiftToPositiveValues()
        {
            Dictionary<double, double> result = new Dictionary<double, double>
        {
            {0, 0},
            {2, 53.8421036080739},
            {4, 0},
            {6, 145.31822766627818},
        };

            Histogram histogram = new Histogram();
            histogram.Add(complex, samppleRate);
            histogram.CalculateDb();
            histogram.ShiftToPositive(-10, -200, 0, 1023);
            CheckValues(result, histogram);
        }
        private void CheckValues(Dictionary<double, double> result, Histogram actual)
        {
            var dictionaries = actual.Data.Values.Zip(result.Values, (l, r) => new { lhv = r, rhv = l });
            foreach (var item in dictionaries)
            {
                Assert.Equal(item.rhv, item.lhv, precision);
            }
        }
    }
}
