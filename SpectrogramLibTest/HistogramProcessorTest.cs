using System;
using Xunit;

namespace Spectrogram.Test
{
    public class HistogramProcessorTest
    {
        [Theory]
        [InlineData(6)]
        [InlineData(0)]
        [InlineData(9)]
        public void ShouldBeInRange(int x)
        {
            HistogramProcessor histogramProcessor = new HistogramProcessor(new AudioData(), 0, 10, 0);
            Assert.True(histogramProcessor.IsInRange(x));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10)]
        [InlineData(11)]
        public void ShouldNotBeInRange(int x)
        {
            HistogramProcessor histogramProcessor = new HistogramProcessor(new AudioData(), 0, 10, 0);
            Assert.False(histogramProcessor.IsInRange(x));
        }

        [Theory]
        [InlineData(1, 0, 3)]
        [InlineData(2, 0, 3)]
        [InlineData(2, 5, 3)]
        public void ShouldNotBeFull(int times, int rangeStart, int rangeSize)
        {
            AudioData info = new AudioData(new TimeSpan(), 0, 0, 0, 0, 0, "bla");
            HistogramProcessor histogramProcessor = new HistogramProcessor(info, rangeStart, rangeSize, 0);
            float[] val = { 5f, 5f };
            for (int i = 0; i < times; ++i)
            {
                histogramProcessor.AddToQueue(new System.Collections.Generic.KeyValuePair<int, float[]>(5, val));
            }
            Assert.False(histogramProcessor.IsFull());
        }

        [Theory]
        [InlineData(3, 0, 3)]
        [InlineData(4, 0, 3)]
        [InlineData(5, 0, 3)]
        [InlineData(5, 10, 3)]
        public void ShouldBeFull(int times, int rangeStart, int rangeSize)
        {
            AudioData info = new AudioData(new TimeSpan(), 2, 20, 1, 1, 2, "bla");
            HistogramProcessor histogramProcessor = new HistogramProcessor(info, rangeStart, rangeSize, 0);
            float[] val = { 5f, 5f };
            for (int i = 0; i < times; ++i)
            {
                histogramProcessor.AddToQueue(new System.Collections.Generic.KeyValuePair<int, float[]>(5, val));
            }
            Assert.True(histogramProcessor.IsFull());
        }
    }

}
