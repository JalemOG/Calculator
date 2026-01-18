using System;

namespace Calculator.Client.Models
{
    public sealed class HistoryItem
    {
        public DateTime LocalTime { get; }
        public string Expression { get; }
        public string Response { get; }

        public HistoryItem(DateTime localTime, string expression, string response)
        {
            LocalTime = localTime;
            Expression = expression;
            Response = response;
        }

        public override string ToString()
        {
            // Esto controla cómo se ve en la ListBox
            return $"{LocalTime:HH:mm:ss} | {Expression}  =>  {Response}";
        }
    }
}
