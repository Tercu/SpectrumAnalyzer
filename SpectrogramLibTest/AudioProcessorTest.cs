using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Spectrogram.Test
{
    public class AudioProcessorTest
    {
        delegate void ReadFileDelegate(out float[] outParam);
        readonly float[] samples;
        readonly Mock<IAudioFile> audioFileMock = new Mock<IAudioFile>();
        readonly Mock<IBitmapGenerator> bitmapMock = new Mock<IBitmapGenerator>();
        private int times = 4;
        public AudioProcessorTest()
        {
            samples = new float[]
            {
                1,1,1,1,1,1,1,1
            };

            float[] output;
            audioFileMock.Setup(x => x.ReadFile(out output))
                .Callback(new ReadFileDelegate((out float[] output) => { output = (float[])samples.Clone(); }))
                .Returns(() =>
                {
                    times--;
                    if (times >= 0)
                    {
                        return samples.Length;
                    }
                    return 0;
                });

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

        [Theory]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(1)]
        public async Task ShouldProcessFileAsync(int multiplier)
        {
            times = multiplier;

            int sampleLength = samples.Length * multiplier;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);

            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            await audioProcessor.ProcessFileAsync();

            int calls = (int)audioFileMock.Object.SampleSource.Length / audioFileMock.Object.FftSampleSize;
            int callTimes = calls / (audioFileMock.Object.SampleSource.WaveFormat.SampleRate / audioFileMock.Object.FftSampleSize);

            // BUG! Mock sometimes returns correct number of calls for bitmapMock.EditRow(...) method,
            // but most of the cases returns 0 calls. This behavior requires further investigetion.
            // bitmapMock.Verify(x => x.EditRow(It.IsAny<int>(), It.IsAny<Histogram>()), Times.Exactly(callTimes));
            bitmapMock.Verify(x => x.SaveImage(), Times.Once);
        }

        [Fact]
        public void ShouldReadFile()
        {
            times = 1;

            int sampleLength = samples.Length * 1;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);
            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            var reader = audioProcessor.ReadFileToListAsync();
            reader.Wait();

            Assert.Single(audioProcessor.SampleQueue);
        }
        [Fact]
        public async Task ShouldNotLoosePacketsWhenQueueIsFullAsync()
        {
            times = 5;
            int counter = times;

            int sampleLength = samples.Length * 1;
            audioFileMock.Setup(x => x.SampleSource.Length).Returns(sampleLength);
            AudioProcessor audioProcessor = new AudioProcessor(audioFileMock.Object, bitmapMock.Object);
            var fileRead = audioProcessor.ReadFileToListAsync(1);
            await Task.Delay(50);
            var distribute = audioProcessor.DistributeDataInProcessors();

            distribute.Wait();
            fileRead.Wait();

            float[] output;
            audioFileMock.Verify(x => x.ReadFile(out output), Times.Exactly(counter + 1));
            Assert.Equal(counter, audioProcessor.processorQueue.Count);
        }
    }
}
