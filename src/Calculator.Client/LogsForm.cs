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

            // 1) Autodetect al abrir
            _logsFolderPath = TryAutoDetectLogsFolder();

            if (!string.IsNullOrWhiteSpace(_logsFolderPath) && Directory.Exists(_logsFolderPath))
            {
                lblFolder.Text = _logsFolderPath;
                RefreshFileList();
            }
            else
            {
                lblFolder.Text = "Logs folder not found (select it).";
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the Server logs folder (e.g., src\\Calculator.Server\\logs)"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _logsFolderPath = dialog.SelectedPath;
                lblFolder.Text = _logsFolderPath;
                RefreshFileList();
            }
        }

        private static string? TryAutoDetectLogsFolder()
        {
            // Base: directorio de ejecución del cliente (bin\Debug\...)
            string baseDir = AppContext.BaseDirectory;

            // Subimos varios niveles buscando src\Calculator.Server\logs o Calculator.Server\logs
            var dir = new DirectoryInfo(baseDir);

            for (int i = 0; i < 10 && dir != null; i++)
            {
                // Opción 1: repo típico con src
                string candidate1 = Path.Combine(dir.FullName, "src", "Calculator.Server", "logs");
                if (Directory.Exists(candidate1)) return candidate1;

                // Opción 2: si estás ejecutando desde la raíz (sin src)
                string candidate2 = Path.Combine(dir.FullName, "Calculator.Server", "logs");
                if (Directory.Exists(candidate2)) return candidate2;

                // Opción 3: si logs está cerca
                string candidate3 = Path.Combine(dir.FullName, "logs");
                if (Directory.Exists(candidate3)) return candidate3;

                dir = dir.Parent;
            }

            return null;
        }

        private void RefreshFileList()
        {
            cmbFiles.Items.Clear();
            gridLogs.DataSource = null;

            if (string.IsNullOrWhiteSpace(_logsFolderPath) || !Directory.Exists(_logsFolderPath))
                return;

            var files = Directory.GetFiles(_logsFolderPath, "client_*.csv")
                                .OrderBy(f => f)
                                .Select(Path.GetFileName)
                                .ToArray();

            foreach (var file in files)
                cmbFiles.Items.Add(file);

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
            var table = new DataTable();
            table.Columns.Add("timestamp");
            table.Columns.Add("expression");
            table.Columns.Add("result");

            var lines = File.ReadAllLines(csvPath);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (TryParse3Columns(line, out var ts, out var expr, out var res))
                    table.Rows.Add(ts, expr, res);
                else
                    table.Rows.Add("", line, "");
            }

            gridLogs.DataSource = table;
            gridLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

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
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        result.Append('"');
                        i += 2;
                        continue;
                    }

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
