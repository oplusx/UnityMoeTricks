using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Moe
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }
    interface ILog
    {
        bool Init();
        void Destroy();
        void Log(LogLevel level, string strLog);
    }

    class UnityLog : ILog
    {
        public bool Init()
        {
            return true;
        }
        public void Destroy()
        {

        }
        public void Log(LogLevel level, string log)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    Debug.Log(string.Format("<color=#888888>{0}</color>", log));
                    break;
                case LogLevel.Info:
                    Debug.Log(log);
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning(log);
                    break;
                case LogLevel.Error:
                    Debug.LogError(log);
                    break;
                default:
                    throw new Exception(log.ToString());
            }
        }
    }
    class FileLog : ILog
    {
        private StreamWriter m_Writer = null;
        public bool Init()
        {
            m_Writer = File.CreateText(MoeDefine.GetSaveDataPath() + "/game.log");
            if (m_Writer == null)
            {
                return false;
            }
            return true;
        }
        public void Destroy()
        {
            if (m_Writer != null)
            {
                m_Writer.Close();
            }
        }
        public void Log(LogLevel level, string log)
        {
            m_Writer.WriteLine(string.Format("[{0}]{1}:{2}",(level == LogLevel.Error ? "ERR" : "LOG"), DateTime.Now.ToString(), log));
            m_Writer.Flush();
        }
    }
    //public class UILog : ILog
    //{
    //    public bool Init()
    //    {
    //        return true;
    //    }
    //    public void Destroy()
    //    {
    //    }
    //    public void Log(LogLevel level, string log)
    //    {
    //        string str = string.Format("{0}:{1}", DateTime.Now.ToShortTimeString(), log);
    //        UI.UIDlg.UIuidebug.instance.AddLogString(str);
    //    }
    //}

    public class LogManager
    {
        #region Singleton
        private static readonly LogManager s_Instance = new LogManager();
        public static LogManager instance { get { return s_Instance; } }
        static LogManager()
        {
        }
        private LogManager()
        {
        }
        #endregion

        List<ILog> m_Logs = new List<ILog>();
        public bool Init()
        {
            if(Debug.isDebugBuild)
            {
                UnityLog defaultLog = new UnityLog();
                if (!defaultLog.Init())
                {
                    return false;
                }
                m_Logs.Add(defaultLog);
            }
            FileLog fileLog = new FileLog();
            if (!fileLog.Init())
            {
                return false;
            }
            m_Logs.Add(fileLog);
            //UILog uiLog = new UILog();
            //if (!uiLog.Init())
            //{
            //    return false;
            //}
            //m_Logs.Add(uiLog);
            return true;
        }

        public void Destroy()
        {
            for (int i = 0; i < m_Logs.Count; i++)
            {
                m_Logs[i].Destroy();
            }
            m_Logs.Clear();
        }
        public void Log(LogLevel level, object log)
        {
            Log(level, log.ToString());
        }
        public void Log(LogLevel level, string log)
        {
            for (int i = 0; i < m_Logs.Count; i++)
            {
                m_Logs[i].Log(level, log);
            }
        }

        public static void LogInfo(string strFormat, params object[] args)
        {
            instance.Log(LogLevel.Info, string.Format(strFormat, args));
        }
        public static void LogWarn(string strFormat, params object[] args)
        {
            instance.Log(LogLevel.Warn, string.Format(strFormat, args));
        }
        public static void LogError(string strFormat, params object[] args)
        {
            instance.Log(LogLevel.Error, string.Format(strFormat, args));
        }  
    }
}
