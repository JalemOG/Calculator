using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Client.Networking
{
    public sealed class TcpCalculatorClient : IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        public bool IsConnected => _tcpClient?.Connected == true;

        // Se dispara cuando detectamos desconexión real (socket muerto / server cerró)
        public event Action<string>? Disconnected;

        public async Task ConnectAsync(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host inválido.", nameof(host));

            if (_tcpClient != null)
                throw new InvalidOperationException("Ya existe una conexión activa.");

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port);

            _networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(_networkStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            _writer = new StreamWriter(_networkStream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
        }

        /// <summary>
        /// Heurística típica para detectar socket cerrado:
        /// si Poll(Read) == true y Available == 0 => el peer cerró.
        /// </summary>
        public bool IsAlive()
        {
            if (_tcpClient == null) return false;

            try
            {
                Socket socket = _tcpClient.Client;
                bool peerClosed = socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0;
                return !peerClosed;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> SendExpressionAsync(string expression)
        {
            if (!IsConnected || _writer == null || _reader == null)
                throw new InvalidOperationException("No estás conectado al servidor.");

            expression = (expression ?? "").Trim();
            if (expression.Length == 0)
                throw new ArgumentException("La expresión está vacía.", nameof(expression));

            try
            {
                await _writer.WriteLineAsync(expression);

                string? response = await _reader.ReadLineAsync();
                if (response == null)
                {
                    NotifyDisconnected("El servidor cerró la conexión.");
                    throw new IOException("El servidor cerró la conexión.");
                }

                return response;
            }
            catch (IOException ex)
            {
                NotifyDisconnected(ex.Message);
                throw;
            }
            catch (SocketException ex)
            {
                NotifyDisconnected(ex.Message);
                throw new IOException("Error de socket: " + ex.Message, ex);
            }
            catch (ObjectDisposedException ex)
            {
                NotifyDisconnected("Conexión finalizada.");
                throw new IOException("Conexión finalizada.", ex);
            }
        }

        private void NotifyDisconnected(string reason)
        {
            // Evita notificar varias veces si ya disposeamos
            if (_tcpClient == null) return;

            try { Disconnected?.Invoke(reason); } catch { }
            Dispose();
        }

        public void Dispose()
        {
            try { _writer?.Dispose(); } catch { }
            try { _reader?.Dispose(); } catch { }
            try { _networkStream?.Dispose(); } catch { }
            try { _tcpClient?.Close(); } catch { }

            _writer = null;
            _reader = null;
            _networkStream = null;
            _tcpClient = null;
        }
        public async Task<bool> PingAsync()
        {
            if (!IsConnected) return false;

            try
            {
                // Usamos "0" como ping porque siempre es una expresión válida
                string response = await SendExpressionAsync("0");
                return response.StartsWith("OK|", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

    }
}
