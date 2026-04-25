using System.Text;

namespace DOIPUtils
{
    using System;
    using System.IO;

    internal static class LogHelper
    {
        
        private static bool _isLoggingEnabled = true; // 控制日志开关 

        private static string LogFile = string.Empty;
        public static void EnableLogging(bool enable)
        {
            _isLoggingEnabled = enable;
        }   



        public static void InitLogFile()
        {
            if (_isLoggingEnabled)
            {
                string LogDir = "log";
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);

                LogFile = Path.Combine(LogDir, $"log_{DateTime.Now:yyyyMMddHHmmss}.log");
            }
        }

        public static void Write(string msg, params byte[][] data)
        {
            if (_isLoggingEnabled)
            {
                StringBuilder content = new StringBuilder($"[{DateTime.Now:HH:mm:ss:fff}] {msg}");
                if(data.Length > 0)
                {
                    foreach(var one in data)
                    {
                        content.Append(" data: " + string.Join(" ", one.Select(x => x.ToString("X2"))));
                    }
           
                }
                content.Append(Environment.NewLine);
                File.AppendAllText(LogFile, content.ToString());
                //Console.WriteLine(content); // 同时打印控制台
            }

        }

        public static void WritePlainText(string msg)
        {
            if (_isLoggingEnabled)
            {
                File.AppendAllText(LogFile, msg);
            }

        }

    }
}
