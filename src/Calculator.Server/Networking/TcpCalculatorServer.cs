using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Core;
using Calculator.Server.Logging;

namespace Calculator.Server.Networking
{
    public sealed class TcpCalculatorServer
    {
        private readonly int _port;
        private readonly CsvHistoryLogger _logger;

        private TcpListener? _listener;
        private int _clientCounter = 0;

        public TcpCalculatorServer(int port, CsvHistoryLogger logger)
        {
            _port = port;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            StartListening();
            Console.WriteLine($"[SERVER] Listening on port {_port}");

            try
            {
                await AcceptClientsLoopAsync(cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                // Normal cuando Stop() cierra el listener
            }
        }

        public void Stop()
        {
            _listener?.Stop();
            _listener = null;
        }

        private void StartListening()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
        }

        private async Task AcceptClientsLoopAsync(CancellationToken cancellationToken)
        {
            if (_listener == null)
                throw new InvalidOperationException("Listener not started.");

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                int clientId = Interlocked.Increment(ref _clientCounter);

                Console.WriteLine($"[SERVER] Client #{clientId} connected");

                _ = Task.Run(() => HandleClientAsync(clientId, tcpClient, cancellationToken), cancellationToken);
            }
        }

        private async Task HandleClientAsync(int clientId, TcpClient tcpClient, CancellationToken cancellationToken)
        {
            // OJO: usamos { } para que todas las declaraciones queden dentro del scope correcto
            using (tcpClient)
            {
                using (NetworkStream stream = tcpClient.GetStream())
                using (StreamReader reader = CreateReader(stream))
                using (StreamWriter writer = CreateWriter(stream))
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            string? expressionLine = await reader.ReadLineAsync();
                            if (expressionLine == null)
                                break; // cliente desconectó

                            string expression = expressionLine.Trim();
                            if (expression.Length == 0)
                                continue;

                            string response = ProcessExpression(clientId, expression);
                            await writer.WriteLineAsync(response);
                        }
                    }
                    catch (IOException)
                    {
                        // desconexión inesperada
                    }
                    finally
                    {
                        Console.WriteLine($"[SERVER] Client #{clientId} disconnected");
                    }
                }
            }
        }

        private string ProcessExpression(int clientId, string expression)
        {
            DateTime nowUtc = DateTime.UtcNow;

            try
            {
                int result = ExpressionEngine.EvaluateExpression(expression);
                _logger.Log(clientId, nowUtc, expression, result.ToString());
                return MessageProtocol.CreateOkResponse(nowUtc, result);
            }
            catch (Exception ex)
            {
                _logger.Log(clientId, nowUtc, expression, $"ERR: {ex.Message}");
                return MessageProtocol.CreateErrorResponse(nowUtc, ex.Message);
            }
        }

        private static StreamReader CreateReader(NetworkStream stream)
            => new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        private static StreamWriter CreateWriter(NetworkStream stream)
            => new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
    }
}
