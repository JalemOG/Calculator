using System;
using Calculator.Client.Networking;
using Xunit;

namespace Calculator.Client.Tests
{
    public class ProtocolParserTests
    {
        [Fact]
        public void Parse_OkMessage_ReturnsOkWithPayload()
        {
            string raw = "OK|2026-01-18T00:00:00.0000000Z|37";

            var msg = ProtocolParser.Parse(raw);

            Assert.True(msg.IsOk);
            Assert.Equal("37", msg.Payload);
            Assert.NotNull(msg.TimestampUtc);
        }

        [Fact]
        public void Parse_ErrMessage_ReturnsNotOkWithPayload()
        {
            string raw = "ERR|2026-01-18T00:00:00.0000000Z|Division by zero";

            var msg = ProtocolParser.Parse(raw);

            Assert.False(msg.IsOk);
            Assert.Equal("Division by zero", msg.Payload);
            Assert.NotNull(msg.TimestampUtc);
        }

        [Fact]
        public void Parse_InvalidFormat_ReturnsNotOkAndRawPayload()
        {
            string raw = "something weird";

            var msg = ProtocolParser.Parse(raw);

            Assert.False(msg.IsOk);
            Assert.Equal(raw, msg.Payload);
        }

        [Fact]
        public void Parse_Empty_ReturnsNotOkWithFriendlyMessage()
        {
            var msg = ProtocolParser.Parse("   ");

            Assert.False(msg.IsOk);
            Assert.Contains("vacía", msg.Payload, StringComparison.OrdinalIgnoreCase);
        }
    }
}
