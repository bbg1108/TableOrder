using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir = Path.Combine(CommonPath.BaseDir, "Logs");

        public static void Log(Exception ex, string message = null)
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);

                string filePath = Path.Combine(_logDir, $"log-{DateTime.Now:yyyy-MM-dd}.txt");

                var sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine($"Time      : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Message   : {message}");
                sb.AppendLine($"Exception : {ex}");
                sb.AppendLine();

                lock (_lock) // 동시 접근 방지
                {
                    File.AppendAllText(filePath, sb.ToString());
                }
            }
            catch
            {
                // 로그 기록 실패는 무시 (로그 때문에 앱 죽으면 안됨)
            }
        }
    }
}
