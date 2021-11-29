using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace shared
{
    public class Log
    {
        public enum Level
        {
            Info = 0,
            Warning,
            Error,
        }

        String logFilePath = "";

        public Log(String path)
        {
            Console.WriteLine(path);
            if (path.Substring(path.Length - 1, 1) == @"\")
            {
                path = path.Substring(0, path.Length - 2);
                Console.WriteLine(path);
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path))
            {
                DateTime today = DateTime.Today;
                String strToday = today.ToString("yyyy-MM-dd");
                this.logFilePath = path + @"\" + "city64_" + strToday + ".log";
                Console.WriteLine(this.logFilePath);
            }
        }

        public void WriteToLog(String message)
        {
            this.WriteToLog(Level.Info, message);
        }
        public void WriteToLog(Level level, String message)
        {
            using (StreamWriter sw = new StreamWriter(this.logFilePath, true, System.Text.Encoding.Default))
            {
                DateTime now = DateTime.Now;
                String servInfo = "[" + now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] [";
                switch (level)
                {
                    case Level.Info:
                        servInfo += "INFO";
                        break;
                    case Level.Warning:
                        servInfo += "WARN";
                        break;
                    case Level.Error:
                        servInfo += "ERROR";
                        break;
                }
                servInfo += "] ";
                sw.WriteLine(servInfo + message);
            }
        }
    }
}
