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

        // Timer para detectar desconexión aunque el usuario no envíe nada
        private readonly System.Windows.Forms.Timer _connectionTimer = new System.Windows.Forms.Timer();

        public MainForm()
        {
            InitializeComponent();

            UpdateUiState(isConnected: false);

            _connectionTimer.Interval = 3000; // 1 segundo
            _connectionTimer.Tick += ConnectionTimer_Tick;
            _connectionTimer.Start();
        }

        // -------------------------
        // Estado de UI
        // -------------------------
        private void UpdateUiState(bool isConnected)
        {
            btnConnect.Enabled = !isConnected;

            btnSend.Enabled = isConnected;
            txtExpression.Enabled = isConnected;

            txtHost.Enabled = !isConnected;
            numPort.Enabled = !isConnected;

            lblStatus.Text = isConnected ? "Connected" : "Disconnected";
        }

        // -------------------------
        // Conectar
        // -------------------------
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;

                _client = new TcpCalculatorClient();
                _client.Disconnected += OnClientDisconnected;

                await _client.ConnectAsync(txtHost.Text.Trim(), (int)numPort.Value);

                UpdateUiState(isConnected: true);
                AddSystemHistory(ok: true, "Connected");
                txtExpression.Focus();
            }
            catch (Exception ex)
            {
                AddSystemHistory(ok: false, "Connect failed: " + ex.Message);
                btnConnect.Enabled = true;
                UpdateUiState(isConnected: false);
            }
        }

        private void OnClientDisconnected(string reason)
        {
            // Puede venir de otro hilo (por lectura async), así que lo subimos al hilo UI
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnClientDisconnected(reason)));
                return;
            }

            UpdateUiState(isConnected: false);
            AddSystemHistory(ok: false, "Disconnected: " + reason);

            // liberar referencia
            _client?.Dispose();
            _client = null;
        }

        // -------------------------
        // Timer: detecta desconexión sin enviar
        // -------------------------
        private async void ConnectionTimer_Tick(object? sender, EventArgs e)
        {
            if (_client == null) return;
            if (!_client.IsConnected) return;

            // Para evitar reentradas si el tick cae mientras el anterior aún corre
            _connectionTimer.Stop();

            try
            {
                bool ok = await _client.PingAsync();
                if (!ok)
                {
                    // Si Ping falla, el propio cliente ya va a disparar Disconnected (por SendExpressionAsync)
                    // Pero por seguridad:
                    if (_client == null || !_client.IsConnected)
                        UpdateUiState(isConnected: false);
                }
            }
            finally
            {
                // Si el form sigue abierto, reanudamos
                if (!IsDisposed)
                    _connectionTimer.Start();
            }
        }


        // -------------------------
        // Enviar (botón y Enter)
        // -------------------------
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
                AddSystemHistory(ok: false, "Not connected");
                UpdateUiState(isConnected: false);
                return;
            }

            string expression = txtExpression.Text.Trim();
            if (expression.Length == 0) return;

            try
            {
                btnSend.Enabled = false;

                string rawResponse = await _client.SendExpressionAsync(expression);

                // Parseamos OK|timestamp|payload / ERR|timestamp|payload
                var parsed = ProtocolParser.Parse(rawResponse);

                if (parsed.IsOk)
                {
                    AddHistory(expression, ok: true, displayResult: parsed.Payload);
                }
                else
                {
                    AddHistory(expression, ok: false, displayResult: parsed.Payload);
                }

                txtExpression.Clear();
                txtExpression.Focus();
            }
            catch (Exception ex)
            {
                // Si el server cayó, igual queda reflejado
                AddHistory(expression, ok: false, displayResult: ex.Message);
                UpdateUiState(isConnected: false);
            }
            finally
            {
                btnSend.Enabled = (_client != null && _client.IsConnected);
            }
        }

        // -------------------------
        // Historial bonito
        // -------------------------
        private void AddHistory(string expression, bool ok, string displayResult)
        {
            var item = new HistoryItem(DateTime.Now, expression, ok, displayResult);
            lstHistory.Items.Insert(0, item);
        }

        private void AddSystemHistory(bool ok, string message)
        {
            AddHistory("SYSTEM", ok, message);
        }

        // -------------------------
        // Cierre
        // -------------------------
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _connectionTimer.Stop();

            _client?.Dispose();
            _client = null;

            base.OnFormClosed(e);
        }
    }
}
