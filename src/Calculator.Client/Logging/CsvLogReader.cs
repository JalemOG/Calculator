using System;
using System.Collections.Generic;
using System.IO;

namespace Calculator.Client.Logging
{
    public sealed record CsvLogEntry(string Timestamp, string Expression, string Result);

    public static class CsvLogReader
    {
        public static List<CsvLogEntry> ReadClientLog(string csvPath)
        {
            var result = new List<CsvLogEntry>();

            if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                return result;

            var lines = File.ReadAllLines(csvPath);

            // Esperamos encabezado: timestamp,expression,result
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (TryParse3Columns(line, out var ts, out var expr, out var res))
                    result.Add(new CsvLogEntry(ts, expr, res));
                else
                    result.Add(new CsvLogEntry("", line, ""));
            }

            return result;
        }

        // Parse de 3 columnas: timestamp,"expr","result"
        private static bool TryParse3Columns(string line, out string col1, out string col2, out string col3)
        {
            col1 = col2 = col3 = "";

            int firstComma = line.IndexOf(',');
            if (firstComma < 0) return false;

            col1 = line.Substring(0, firstComma).Trim();
            string rest = line.Substring(firstComma + 1);

            if (!TryReadQuotedCsvField(rest, out col2, out int consumed1))
                return false;

            rest = rest.Substring(consumed1);
            if (rest.StartsWith(",")) rest = rest.Substring(1);

            if (!TryReadQuotedCsvField(rest, out col3, out _))
                return false;

            return true;
        }

        // Lee un campo CSV entre comillas, soporta "" como escape de "
        private static bool TryReadQuotedCsvField(string text, out string value, out int consumed)
        {
            value = "";
            consumed = 0;

            text = text.TrimStart();
            if (!text.StartsWith("\"")) return false;

            int i = 1;
            var sb = new System.Text.StringBuilder();

            while (i < text.Length)
            {
                if (text[i] == '"')
                {
                    // "" -> comilla escapada
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        sb.Append('"');
                        i += 2;
                        continue;
                    }

                    // fin de campo
                    i++;
                    value = sb.ToString();
                    consumed = i;
                    return true;
                }

                sb.Append(text[i]);
                i++;
            }

            return false;
        }
    }
}
