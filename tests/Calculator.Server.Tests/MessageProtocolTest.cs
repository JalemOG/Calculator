using System;
using Calculator.Server.Networking;
using Xunit;

namespace Calculator.Server.Tests
{
    public class MessageProtocolTests
    {
        [Fact]
        public void CreateOkResponse_ContainsOkTimestampAndResult()
        {
            var time = new DateTime(2026, 1, 18, 0, 0, 0, DateTimeKind.Utc);
            string response = MessageProtocol.CreateOkResponse(time, 37);

            Assert.StartsWith("OK|", response);
            Assert.Contains("|37", response);
            Assert.Contains(time.ToString("O"), response);
        }

        [Fact]
        public void CreateErrorResponse_SanitizesPipesAndNewlines()
        {
            var time = new DateTime(2026, 1, 18, 0, 0, 0, DateTimeKind.Utc);

            string response = MessageProtocol.CreateErrorResponse(time, "Bad|\nError\rMessage");

            Assert.StartsWith("ERR|", response);
            Assert.Contains(time.ToString("O"), response);

            // No debe contener saltos de línea ni pipes
            Assert.DoesNotContain("\n", response);
            Assert.DoesNotContain("\r", response);
            Assert.DoesNotContain("Bad|", response);

            // El pipe debe ser reemplazado por /
            Assert.Contains("Bad/", response);
        }

        [Fact]
        public void CreateErrorResponse_NullMessage_UsesDefault()
        {
            var time = DateTime.UtcNow;
            string response = MessageProtocol.CreateErrorResponse(time, null!);

            Assert.StartsWith("ERR|", response);
            Assert.Contains("Unknown error", response);
        }
    }
}
