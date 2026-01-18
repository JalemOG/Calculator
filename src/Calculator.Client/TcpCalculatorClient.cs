using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Client.Networking
{
    public sealed class TcpCalculatorClient : IDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        public bool IsConnected => _client?.Connected == true;

        public async Task ConnectAsync(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host inválido.", nameof(host));

            if (_client != null)
                throw new InvalidOperationException("Ya estás conectado.");

            _client = new TcpClient();
            await _client.ConnectAsync(host, port);

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            _writer = new StreamWriter(_stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
        }

        public async Task<string> SendExpressionAsync(string expression)
        {
            if (!IsConnected || _writer == null || _reader == null)
                throw new InvalidOperationException("No estás conectado al servidor.");

            expression = (expression ?? "").Trim();
            if (expression.Length == 0)
                throw new ArgumentException("La expresión está vacía.", nameof(expression));

            await _writer.WriteLineAsync(expression);

            string? response = await _reader.ReadLineAsync();
            if (response == null)
                throw new IOException("El servidor cerró la conexión.");

            return response;
        }

        public void Dispose()
        {
            try { _writer?.Dispose(); } catch { }
            try { _reader?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }

            _writer = null;
            _reader = null;
            _stream = null;
            _client = null;
        }
    }
}
