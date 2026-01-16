using System;

namespace Calculator.Server.Networking
{
    public static class MessageProtocol
    {
        // Construye respuesta de éxito
        public static string CreateOkResponse(DateTime timestampUtc, int result)
            => $"OK|{timestampUtc:O}|{result}";

        // Construye respuesta de error
        public static string CreateErrorResponse(DateTime timestampUtc, string errorMessage)
            => $"ERR|{timestampUtc:O}|{Sanitize(errorMessage)}";

        // Evita que el mensaje rompa el formato "A|B|C"
        private static string Sanitize(string text)
        {
            return (text ?? "Unknown error")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("|", "/");
        }
    }
}
