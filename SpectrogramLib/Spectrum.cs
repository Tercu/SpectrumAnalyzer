using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spectrogram
{
    public class Spectrum
    {
        private readonly List<Task> tasks = new List<Task>();
        private readonly Logger logger = Logger.GetInstance();
        private readonly DirectoryManager dirManager;
        private List<AudioProcessor> audioProcessorList;
        public string[] PathToDirectory { get; private set; }

        public void RunSingleThread()
        {
            foreach (var audioProcessor in audioProcessorList)
            {
                audioProcessor.ProcessFile();
            }
        }
        public void RunMultiThread()
        {
            InitTasks();
            StartTasks();
            WaitForTasks();
        }

        public Spectrum(string[] pathToDirectory)
        {
            PathToDirectory = pathToDirectory;
            dirManager = new DirectoryManager(new System.IO.Abstractions.FileSystem(), pathToDirectory);
            dirManager.CreateFileList();
            audioProcessorList = new List<AudioProcessor>();

            foreach (var pathToFile in dirManager.AudioFileList)
            {
                audioProcessorList.Add(new AudioProcessor(new AudioFile(pathToFile)));
            }
        }
        private void InitTasks()
        {
            foreach (var audioProcessor in audioProcessorList)
            {
                tasks.Add(new Task(() => audioProcessor.ProcessFile(), TaskCreationOptions.LongRunning));
            }
        }
        private void StartTasks()
        {
            tasks.ForEach(t => t.Start());
        }
        private void WaitForTasks()
        {
            tasks.ForEach(t => t.Wait());
        }

    }
}
