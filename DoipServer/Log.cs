using System.Text;

namespace DOIPUtils
{
    using System;
    using System.IO;

    internal static class LogHelper
    {
        private static bool _isLoggingEnabled = true;
        private static string LogFile = string.Empty;

        public static void EnableLogging(bool enable)
        {
            _isLoggingEnabled = enable;
        }

        public static void InitLogFile()
        {
            if (!_isLoggingEnabled) return;

            try
            {
                string LogDir = "log";
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);

                LogFile = Path.Combine(LogDir, $"log_{DateTime.Now:yyyyMMddHHmmss}.log");
            }
            catch (Exception ex)
            {
                _isLoggingEnabled = false;
                Console.WriteLine($"[Log] 日志目录创建失败，已禁用日志: {ex.Message}");
            }
        }

        public static void Write(string msg, params byte[][] data)
        {
            if (!_isLoggingEnabled) return;

            try
            {
                StringBuilder content = new StringBuilder($"[{DateTime.Now:HH:mm:ss:fff}] {msg}");
                if (data.Length > 0)
                {
                    foreach (var one in data)
                    {
                        content.Append(" data: " + string.Join(" ", one.Select(x => x.ToString("X2"))));
                    }
                }
                content.Append(Environment.NewLine);
                File.AppendAllText(LogFile, content.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log] 写入失败: {ex.Message}");
            }
        }

        public static void WritePlainText(string msg)
        {
            if (!_isLoggingEnabled) return;

            try
            {
                File.AppendAllText(LogFile, msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log] 写入失败: {ex.Message}");
            }
        }
    }
}
