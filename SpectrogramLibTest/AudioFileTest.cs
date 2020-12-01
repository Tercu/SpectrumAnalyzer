using CSCore;
using Moq;
using Xunit;

namespace Spectrogram.Test
{
    public class AudioFileTest
    {
        [Fact]
        public void ShouldReturnArrayWithPayload()
        {
            float[] payload = new float[] { 1, 0, 1, 0, 1, 0, 1, 0 };
            float[] expected = new float[] { 1, 0, 1, 0, 1, 0, 1, 0 };
            Mock<ISampleSource> SampleSourceMock = new Mock<ISampleSource>();

            SampleSourceMock.Setup(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<float[], int, int>((buffer, offset, count) => payload.CopyTo(buffer, 0))
                .Returns(payload.Length);

            AudioFile audioFile = new AudioFile(SampleSourceMock.Object, 3);
            _ = audioFile.ReadFile(out float[] result);

            SampleSourceMock.Verify(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
            Assert.Equal(expected, result);

        }
        [Fact]
        public void ShouldReturnArrayWithSmallerPayload()
        {

            float[] payload = new float[] { 1, 0, 1 };
            float[] expected = new float[] { 1, 0, 1, 0, 0, 0, 0, 0 };

            Mock<ISampleSource> SampleSourceMock = new Mock<ISampleSource>();

            SampleSourceMock.Setup(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<float[], int, int>((buffer, offset, count) => payload.CopyTo(buffer, 0))
                .Returns(payload.Length);

            AudioFile audioFile = new AudioFile(SampleSourceMock.Object, 3);

            _ = audioFile.ReadFile(out float[] result);

            SampleSourceMock.Verify(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
            Assert.Equal(expected, result);

        }
    }
}
