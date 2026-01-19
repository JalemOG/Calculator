using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Calculator.Client
{
    public partial class LogsForm : Form
    {
        private string? _logsFolderPath;

        public LogsForm()
        {
            InitializeComponent();
            lblFolder.Text = "No folder selected";
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the Server logs folder (e.g., Calculator.Server\\logs)"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _logsFolderPath = dialog.SelectedPath;
                lblFolder.Text = _logsFolderPath;

                RefreshFileList();
            }
        }

        private void RefreshFileList()
        {
            cmbFiles.Items.Clear();
            gridLogs.DataSource = null;

            if (string.IsNullOrWhiteSpace(_logsFolderPath) || !Directory.Exists(_logsFolderPath))
                return;

            var files = Directory.GetFiles(_logsFolderPath, "client_*.csv")
                                .OrderBy(f => f)
                                .ToArray();

            foreach (var file in files)
                cmbFiles.Items.Add(Path.GetFileName(file));

            if (cmbFiles.Items.Count > 0)
                cmbFiles.SelectedIndex = 0;
        }

        private void cmbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_logsFolderPath)) return;
            if (cmbFiles.SelectedItem == null) return;

            string fileName = cmbFiles.SelectedItem.ToString()!;
            string fullPath = Path.Combine(_logsFolderPath, fileName);

            if (!File.Exists(fullPath)) return;

            LoadCsvIntoGrid(fullPath);
        }

        private void LoadCsvIntoGrid(string csvPath)
        {
            // CSV: timestamp, "expression", "result"
            var table = new DataTable();
            table.Columns.Add("timestamp");
            table.Columns.Add("expression");
            table.Columns.Add("result");

            var lines = File.ReadAllLines(csvPath);

            // skip header
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (TryParse3Columns(line, out var ts, out var expr, out var res))
                {
                    table.Rows.Add(ts, expr, res);
                }
                else
                {
                    // Si una línea viene rara, la mostramos igual para no perder info
                    table.Rows.Add("", line, "");
                }
            }

            gridLogs.DataSource = table;
            gridLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Parser simple para 3 columnas con comillas (maneja "" dentro de comillas)
        private static bool TryParse3Columns(string line, out string col1, out string col2, out string col3)
        {
            col1 = col2 = col3 = "";

            int firstComma = line.IndexOf(',');
            if (firstComma < 0) return false;

            col1 = line.Substring(0, firstComma).Trim();
            string rest = line.Substring(firstComma + 1);

            // col2 y col3 vienen como "....","...."
            if (!TryReadQuotedCsvField(rest, out col2, out int consumed1))
                return false;

            rest = rest.Substring(consumed1);
            if (rest.StartsWith(",")) rest = rest.Substring(1);

            if (!TryReadQuotedCsvField(rest, out col3, out _))
                return false;

            return true;
        }

        private static bool TryReadQuotedCsvField(string text, out string value, out int consumed)
        {
            value = "";
            consumed = 0;

            text = text.TrimStart();
            if (!text.StartsWith("\"")) return false;

            int i = 1;
            var result = new System.Text.StringBuilder();

            while (i < text.Length)
            {
                if (text[i] == '"')
                {
                    // Escaped quote?
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        result.Append('"');
                        i += 2;
                        continue;
                    }

                    // End quote
                    i++;
                    value = result.ToString();
                    consumed = i;
                    return true;
                }

                result.Append(text[i]);
                i++;
            }

            return false;
        }
    }
}
