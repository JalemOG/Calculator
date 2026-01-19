using System;
using Calculator.Client.Models;
using Xunit;

namespace Calculator.Client.Tests
{
    public class HistoryItemTests
    {
        [Fact]
        public void ToString_Ok_ShowsArrow()
        {
            var item = new HistoryItem(
                localTime: new DateTime(2026, 1, 18, 12, 30, 5),
                expression: "1+2",
                isOk: true,
                displayResult: "3");

            string text = item.ToString();

            Assert.Contains("1+2", text);
            Assert.Contains("=> 3", text);
            Assert.Contains("12:30:05", text);
        }

        [Fact]
        public void ToString_Error_ShowsErrorLabel()
        {
            var item = new HistoryItem(
                localTime: new DateTime(2026, 1, 18, 12, 30, 5),
                expression: "5/0",
                isOk: false,
                displayResult: "División por cero");

            string text = item.ToString();

            Assert.Contains("5/0", text);
            Assert.Contains("=> ERROR:", text);
            Assert.Contains("División por cero", text);
        }
    }
}
