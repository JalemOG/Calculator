using System;

namespace Calculator.Client.Models
{
    public sealed class HistoryItem
    {
        public DateTime LocalTime { get; }
        public string Expression { get; }
        public bool IsOk { get; }
        public string DisplayResult { get; }

        public HistoryItem(DateTime localTime, string expression, bool isOk, string displayResult)
        {
            LocalTime = localTime;
            Expression = expression;
            IsOk = isOk;
            DisplayResult = displayResult;
        }

        public override string ToString()
        {
            string status = IsOk ? "=>" : "=> ERROR:";
            return $"{LocalTime:HH:mm:ss} | {Expression} {status} {DisplayResult}";
        }
    }
}
