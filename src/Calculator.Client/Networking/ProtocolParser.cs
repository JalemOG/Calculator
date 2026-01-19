using System;

namespace Calculator.Client.Networking
{
    public sealed class ProtocolMessage
    {
        public bool IsOk { get; }
        public DateTime? TimestampUtc { get; }
        public string Payload { get; }

        public ProtocolMessage(bool isOk, DateTime? timestampUtc, string payload)
        {
            IsOk = isOk;
            TimestampUtc = timestampUtc;
            Payload = payload;
        }
    }

    public static class ProtocolParser
    {
        public static ProtocolMessage Parse(string responseLine)
        {
            if (string.IsNullOrWhiteSpace(responseLine))
                return new ProtocolMessage(false, null, "Respuesta vacía del servidor.");

            string[] parts = responseLine.Split('|', 3);
            if (parts.Length < 3)
                return new ProtocolMessage(false, null, responseLine);

            bool isOk = parts[0] == "OK";

            DateTime? timestamp = null;
            if (DateTime.TryParse(
                    parts[1],
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                timestamp = parsed;
            }

            return new ProtocolMessage(isOk, timestamp, parts[2]);
        }
    }
}
