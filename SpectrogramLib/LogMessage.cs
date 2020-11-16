namespace Spectrogram
{
    public class LogMessage
    {
        public LogMessage(LogLevel level, string message)
        {
            Level = level;
            Message = message;
        }
        public LogMessage()
        {
        }

        public enum LogLevel
        {
            Info,
            Error
        }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return $"{Level}: {Message}";
        }
    }
}
