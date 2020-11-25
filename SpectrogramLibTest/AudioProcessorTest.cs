using Moq;
using Xunit;

namespace Spectrogram.Test
{
    public class AudioProcessorTest
    {
        readonly float[] samples;
        readonly Mock<IAudioFile> audioFileMock = new Mock<IAudioFile>(); // Mock dla typu T
        readonly Mock<IBitmapGenerator> bitmapMock = new Mock<IBitmapGenerator>();
        readonly AudioData AudioInfo;
        public AudioProcessorTest()
        {
            samples = new float[]
            {
                1,1,1,1,1,1,1,1
            };
            audioFileMock.Setup(x => x.ReadFile()).Returns(samples);
            audioFileMock.Setup(x => x.SampleSource.WaveFormat.SampleRate).Returns(8);
            int sampleLength = samples.Length * 4;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);
            int fftSampleSize = 8;
            audioFileMock.Setup(x => x.FftSampleSize).Returns(fftSampleSize);
            int position = 8;
            audioFileMock.Setup(x => x.SampleSource.Position)
                .Callback(() => position += 8)
                .Returns(() => position);
            audioFileMock.Setup(x => x.GetAudioData())
                .Returns(() => new AudioData(audioFileMock.Object));

            bitmapMock.Setup((x) => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>())).Verifiable();
            bitmapMock.Setup((x) => x.SaveImage()).Verifiable();
        }

        [Fact]
        public void ShouldProcessShortFile()
        {
            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            audioProcessor.ProcessFile();
            bitmapMock.Verify(x => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>()), Times.Once);
            bitmapMock.Verify(x => x.SaveImage(), Times.Once);
        }

        [Fact]
        public void ShouldProcessLongFile()
        {
            int sampleLength = samples.Length * 30;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);

            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            audioProcessor.ProcessFile();
            int calls = (int)audioFileMock.Object.SampleSource.Length / audioFileMock.Object.FftSampleSize;
            int callTimes = calls / ((audioFileMock.Object.SampleSource.WaveFormat.SampleRate / audioFileMock.Object.FftSampleSize) + 1);
            bitmapMock.Verify(x => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>()), Times.Exactly(callTimes - 1));
            bitmapMock.Verify(x => x.SaveImage(), Times.Once);
        }
    }
}
