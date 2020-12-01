using Moq;
using Xunit;

namespace Spectrogram.Test
{
    public class AudioProcessorTest
    {
        delegate void ReadFileDelegate(out float[] outParam);
        readonly float[] samples;
        readonly Mock<IAudioFile> audioFileMock = new Mock<IAudioFile>();
        readonly Mock<IBitmapGenerator> bitmapMock = new Mock<IBitmapGenerator>();
        public AudioProcessorTest()
        {
            samples = new float[]
            {
                1,1,1,1,1,1,1,1
            };

            int times = 4;
            float[] output;
            audioFileMock.Setup(x => x.ReadFile(out output))
                .Callback(new ReadFileDelegate((out float[] output) => { output = (float[])samples.Clone(); }))
                .Returns(() => times--);

            audioFileMock.Setup(x => x.SampleSource.WaveFormat.SampleRate).Returns(8);

            int sampleLength = samples.Length * 4;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);

            int fftSampleSize = 8;
            audioFileMock.Setup(x => x.FftSampleSize).Returns(fftSampleSize);

            int position = 0;
            audioFileMock.Setup(x => x.SampleSource.Position)
                .Returns(() => position += 8);

            audioFileMock.Setup(x => x.GetAudioData())
                .Returns(() => new AudioData(audioFileMock.Object));

            bitmapMock.Setup((x) => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>())).Verifiable();
            bitmapMock.Setup((x) => x.SaveImage()).Verifiable();
        }

        [Fact]
        public void ShouldProcessShortFile()
        {
            int times = 1;
            float[] output;
            audioFileMock.Setup(x => x.ReadFile(out output))
                .Callback(new ReadFileDelegate((out float[] output) => { output = (float[])samples.Clone(); }))
                .Returns(() => times--);

            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            audioProcessor.ProcessFile();
            bitmapMock.Verify(x => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>()), Times.Once);
            bitmapMock.Verify(x => x.SaveImage(), Times.Once);
        }

        [Fact]
        public void ShouldProcessLongFile()
        {
            int multiplier = 30;
            int times = multiplier;
            float[] output;
            audioFileMock.Setup(x => x.ReadFile(out output))
                .Callback(new ReadFileDelegate((out float[] output) => { output = (float[])samples.Clone(); }))
                .Returns(() => times--);

            int sampleLength = samples.Length * multiplier;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);
            int position = 0;
            audioFileMock.Setup(x => x.SampleSource.Position)
                .Callback(() => position += 8)
                .Returns(() => position);

            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            audioProcessor.ProcessFile();
            int calls = (int)audioFileMock.Object.SampleSource.Length / audioFileMock.Object.FftSampleSize;
            int callTimes = calls / (audioFileMock.Object.SampleSource.WaveFormat.SampleRate / audioFileMock.Object.FftSampleSize);
            bitmapMock.Verify(x => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>()), Times.Exactly(callTimes));
            bitmapMock.Verify(x => x.SaveImage(), Times.Once);
        }

        [Fact]
        public void ShouldReadFile()
        {
            int times = 1;
            float[] output;
            audioFileMock.Setup(x => x.ReadFile(out output))
                .Callback(new ReadFileDelegate((out float[] output) => { output = (float[])samples.Clone(); }))
                .Returns(() => times--);

            int sampleLength = samples.Length * 1;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);
            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            var reader = audioProcessor.ReadFileToListAsync();
            reader.Wait();

            Assert.Single(audioProcessor.SampleQueue);
        }
    }
}
