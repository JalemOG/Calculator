using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Calculator.Client.Models;
using Calculator.Client.Networking;

namespace Calculator.Client
{
    public partial class MainForm : Form
    {
        private TcpCalculatorClient? _client;

        public MainForm()
        {
            InitializeComponent();
            UpdateUiState(isConnected: false);
        }

        private void UpdateUiState(bool isConnected)
        {
            btnConnect.Enabled = !isConnected;
            btnSend.Enabled = isConnected;

            txtHost.Enabled = !isConnected;
            numPort.Enabled = !isConnected;

            lblStatus.Text = isConnected ? "Connected" : "Disconnected";
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;

                _client = new TcpCalculatorClient();
                await _client.ConnectAsync(txtHost.Text.Trim(), (int)numPort.Value);

                UpdateUiState(isConnected: true);
                AddHistory("SYSTEM", "Connected OK");
            }
            catch (Exception ex)
            {
                AddHistory("SYSTEM", "ERR: " + ex.Message);
                btnConnect.Enabled = true;
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            await SendExpressionFromUiAsync();
        }

        private async void txtExpression_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SendExpressionFromUiAsync();
            }
        }

        private async Task SendExpressionFromUiAsync()
        {
            if (_client == null || !_client.IsConnected)
            {
                AddHistory("SYSTEM", "Not connected");
                return;
            }

            string expression = txtExpression.Text.Trim();
            if (expression.Length == 0) return;

            try
            {
                btnSend.Enabled = false;

                string response = await _client.SendExpressionAsync(expression);
                AddHistory(expression, response);

                txtExpression.Clear();
                txtExpression.Focus();
            }
            catch (Exception ex)
            {
                AddHistory(expression, "ERR: " + ex.Message);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }

        private void AddHistory(string expression, string response)
        {
            var item = new HistoryItem(DateTime.Now, expression, response);
            lstHistory.Items.Insert(0, item);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _client?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
