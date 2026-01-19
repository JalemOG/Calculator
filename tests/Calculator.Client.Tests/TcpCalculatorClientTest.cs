using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Client.Networking;
using Xunit;

#nullable enable
namespace Calculator.Client.Tests
{
    public class TcpCalculatorClientTests
    {
        [Fact]
        public async Task Client_CanConnectSendAndReceive()
        {
            int port = GetFreePort();

            using var cts = new CancellationTokenSource();
            Task serverTask = RunFakeServerAsync(port, cts.Token);

            // darle chance al listener
            await Task.Delay(100);

            using var client = new TcpCalculatorClient();
            await client.ConnectAsync("127.0.0.1", port);

            string response = await client.SendExpressionAsync("1+2");

            Assert.StartsWith("OK|", response);
            Assert.EndsWith("|3", response);

            cts.Cancel();
            await Task.WhenAny(serverTask, Task.Delay(500));
        }

        [Fact]
        public async Task Client_RaisesDisconnected_WhenServerCloses()
        {
            int port = GetFreePort();

            using var cts = new CancellationTokenSource();
            Task serverTask = RunFakeServerCloseAfterFirstAsync(port, cts.Token);

            await Task.Delay(100);

            using var client = new TcpCalculatorClient();

            string? disconnectReason = null;
            client.Disconnected += reason => disconnectReason = reason;

            await client.ConnectAsync("127.0.0.1", port);

            // primera OK
            string ok = await client.SendExpressionAsync("1+2");
            Assert.StartsWith("OK|", ok);

            // segunda debería fallar porque el server cerró
            await Assert.ThrowsAnyAsync<IOException>(() => client.SendExpressionAsync("1+2"));

            Assert.NotNull(disconnectReason);

            cts.Cancel();
            await Task.WhenAny(serverTask, Task.Delay(500));
        }

        private static async Task RunFakeServerAsync(int port, CancellationToken token)
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            try
            {
                using TcpClient tcpClient = await listener.AcceptTcpClientAsync(token);
                using NetworkStream stream = tcpClient.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                while (!token.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null) break;

                    // Fake eval: si es "1+2" devolvemos 3, si es "0" devolvemos 0 (heartbeat)
                    string payload = line.Trim() switch
                    {
                        "1+2" => "3",
                        "0" => "0",
                        _ => "0"
                    };

                    await writer.WriteLineAsync($"OK|{DateTime.UtcNow:O}|{payload}");
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task RunFakeServerCloseAfterFirstAsync(int port, CancellationToken token)
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            try
            {
                using TcpClient tcpClient = await listener.AcceptTcpClientAsync(token);
                using NetworkStream stream = tcpClient.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // 1 request y cerramos
                string? line = await reader.ReadLineAsync();
                if (line != null)
                    await writer.WriteLineAsync($"OK|{DateTime.UtcNow:O}|3");
            }
            catch (OperationCanceledException) { }
            finally
            {
                listener.Stop();
            }
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
