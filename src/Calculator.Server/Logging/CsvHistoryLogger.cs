using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Calculator.Server.Logging
{
    public sealed class CsvHistoryLogger
    {
        private readonly string _logsDirectory;
        private readonly object _fileLock = new object();

        public CsvHistoryLogger(string logsDirectory)
        {
            _logsDirectory = logsDirectory ?? throw new ArgumentNullException(nameof(logsDirectory));
            Directory.CreateDirectory(_logsDirectory);
        }

        public void Log(int clientId, DateTime timestampUtc, string expression, string resultOrError)
        {
            string filePath = Path.Combine(_logsDirectory, $"client_{clientId}.csv");
            bool fileExists = File.Exists(filePath);

            string line = string.Create(CultureInfo.InvariantCulture,
                $"{timestampUtc:O},{EscapeCsv(expression)},{EscapeCsv(resultOrError)}");

            lock (_fileLock)
            {
                using var writer = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8);

                if (!fileExists)
                    writer.WriteLine("timestamp,expression,result");

                writer.WriteLine(line);
            }
        }

        private static string EscapeCsv(string value)
        {
            value ??= "";
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
    }
}
