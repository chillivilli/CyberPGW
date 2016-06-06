using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cyberplat
{
    enum LogLevel
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Trace = 4
    }


    class Logger
    {


        private Action<string> m_logAction;
        private Action<string> m_detailLog;

        private LogLevel m_mainLogLevel;

        private LogLevel m_detailLogLevel;
        
        public Logger(Action<string> logAction, LogLevel mainLogLevel, Action<string> detailLogAction, LogLevel detailLogLevel)
        {
            m_logAction = logAction;
            m_detailLog = detailLogAction;
            m_detailLogLevel = detailLogLevel;
            m_mainLogLevel = mainLogLevel;
        }

        public void Trace(string format, params object[] args)
        {
            WriteMessage(LogLevel.Trace, format, args);
        }

        public void Info(string format, params object[] args)
        {
            WriteMessage(LogLevel.Info, format, args);
        }

        public void Warning(string format, params object[] args)
        {
            WriteMessage(LogLevel.Warning, format, args);
        }

        public void Error(string format, params object[] args)
        {
            WriteMessage(LogLevel.Error, format, args);
        }

        public void Error(string description, Exception ex)
        {
            bool writeMain = m_mainLogLevel >= LogLevel.Error;
            bool writeDetail = m_detailLogLevel >= LogLevel.Error;

            if(writeMain)
            {
                string message = GetMessage(LogLevel.Error, "{0}: {1}", description, ex.Message);
                m_logAction.Invoke(message);
            }

            if(writeDetail)
            {
                string message = GetMessage(LogLevel.Error, "{0}: {1}", description,  ex.ToString());
                m_detailLog.Invoke(message);
            }
        }

        public void Critical(string format, params object[] args)
        {
            WriteMessage(LogLevel.Critical, format, args);
        }

        public void Critical(string description, Exception ex)
        {
            m_logAction.Invoke(GetMessage(LogLevel.Critical, "{0}: {1}", description, ex.Message));
            m_detailLog.Invoke(GetMessage(LogLevel.Critical, "{0}: {1}", description, ex.ToString()));
        }
        
        private void WriteMessage(LogLevel level, string format, params object[] args)
        {
            bool skipMainLog = m_mainLogLevel < level;
            bool skipDetail = m_detailLogLevel < level;
            if (skipDetail && skipMainLog)
                return;

            string message = GetMessage(level, format, args);
            if (!skipMainLog)
                m_logAction.Invoke(message);
            if (!skipDetail)
                m_detailLog.Invoke(message);
        }
                
        private string GetMessage(LogLevel level, string format, params object[] args)
        {            
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[Thread: {0}]\t|{1}|\t", Thread.CurrentThread.ManagedThreadId, level);
            sb.AppendFormat(format, args);
            return sb.ToString();
        }


    }
}
