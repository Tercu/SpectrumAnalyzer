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

            float[] expected = new float[] { 1, 0, 1, 0, 1, 0, 1, 0 };
            Mock<ISampleSource> SampleSourceMock = new Mock<ISampleSource>();

            SampleSourceMock.Setup(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<float[], int, int>((buffer, offset, count) => expected.CopyTo(buffer, 0))
                .Returns(expected.Length);

            AudioFile audioFile = new AudioFile(SampleSourceMock.Object, 3);
            float[] result = audioFile.ReadFile();

            SampleSourceMock.Verify(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
            Assert.Equal(expected, result);

        }
    }
}
