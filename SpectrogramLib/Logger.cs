using System.Collections.Generic;

namespace Spectrogram
{
    public partial class Logger
    {
        private static Logger m_instance;
        public List<LogMessage> LogMessages { get; }

        private static readonly object m_createInstaceLock = new object();
        private static readonly object m_addMessageLock = new object();
        private static readonly object m_getMessageLock = new object();

        private Logger()
        {
            LogMessages = new List<LogMessage>();
        }
        public static Logger GetInstance()
        {
            if (m_instance == null)
            {
                lock (m_createInstaceLock)
                {
                    if (m_instance == null)
                    {
                        m_instance = new Logger();
                    }
                }
            }
            return m_instance;
        }
        public void AddLogMessage(LogMessage.LogLevel level, string message)
        {
            lock (m_addMessageLock)
            {
                LogMessages.Add(new LogMessage(level, message));
            }
        }
        public LogMessage GetLogMessage()
        {
            LogMessage output = new LogMessage();
            if (LogMessages.Count > 0)
            {
                lock (m_getMessageLock)
                {
                    output = LogMessages[0];
                    LogMessages.RemoveAt(0);

                }
            }
            return output;
        }

        public bool IsEmpty()
        {
            if (LogMessages.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}
