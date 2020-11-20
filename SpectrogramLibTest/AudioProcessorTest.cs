using CSCore.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrogram.Test
{
    class AudioProcessorTest
    {
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
        public void ShouldProcessFile()
        {
            Mock<IAudioFile> audioFileMock = new Mock<IAudioFile>(); // Mock dla typu T
            var audioFile = audioFileMock.Object; // Obiekt typu T
            //audioFileMock.Setup(x => x.ReadFile()).Returns(complex);
            audioFileMock.Setup(x => x.SampleSource.WaveFormat.SampleRate).Returns(8);

            //AudioProcessor audioProcessor = new AudioProcessor(audioFile);
            //audioProcessor.ProcessFile();
        }
    }
}
