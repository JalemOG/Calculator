using System;
using System.IO;
using Calculator.Client.Logging;
using Xunit;

namespace Calculator.Client.Logging
{
    public class CsvLogReaderTests
    {
        [Fact]
        public void ReadClientLog_ParsesQuotedCsv()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "client_test_" + Guid.NewGuid() + ".csv");

            try
            {
                File.WriteAllLines(tempFile, new[]
                {
                    "timestamp,expression,result",
                    "2026-01-18T00:00:00.0000000Z,\"5 + \"\"7\"\"\",\"ERR: bad \"\"quote\"\"\""
                });

                var entries = CsvLogReader.ReadClientLog(tempFile);

                Assert.Single(entries);
                Assert.Equal("2026-01-18T00:00:00.0000000Z", entries[0].Timestamp);
                Assert.Equal("5 + \"7\"", entries[0].Expression);
                Assert.Equal("ERR: bad \"quote\"", entries[0].Result);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
