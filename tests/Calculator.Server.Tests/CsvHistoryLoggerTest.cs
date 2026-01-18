using System;
using System.IO;
using Calculator.Server.Logging;
using Xunit;

namespace Calculator.Server.Tests
{
    public class CsvHistoryLoggerTests
    {
        [Fact]
        public void Log_CreatesFileAndWritesHeaderAndRow()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "Calculator_ServerTests_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                var logger = new CsvHistoryLogger(tempDir);
                int clientId = 1;
                var time = new DateTime(2026, 1, 18, 0, 0, 0, DateTimeKind.Utc);

                logger.Log(clientId, time, "(5*7)+(12/6)", "37");

                string filePath = Path.Combine(tempDir, "client_1.csv");
                Assert.True(File.Exists(filePath));

                string[] lines = File.ReadAllLines(filePath);

                Assert.True(lines.Length >= 2);
                Assert.Equal("timestamp,expression,result", lines[0]);

                // Debe contener timestamp y el resultado
                Assert.Contains(time.ToString("O"), lines[1]);
                Assert.Contains("37", lines[1]);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void Log_EscapesQuotesInCsv()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "Calculator_ServerTests_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                var logger = new CsvHistoryLogger(tempDir);
                int clientId = 2;
                var time = DateTime.UtcNow;

                string expr = "5 + \"7\"";
                logger.Log(clientId, time, expr, "ERR: bad \"quote\"");

                string filePath = Path.Combine(tempDir, "client_2.csv");
                string[] lines = File.ReadAllLines(filePath);

                // La línea debe tener comillas escapadas como ""
                Assert.Contains("\"5 + \"\"7\"\"\"", lines[1]);
                Assert.Contains("\"ERR: bad \"\"quote\"\"\"", lines[1]);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
