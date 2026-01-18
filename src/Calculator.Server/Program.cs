using System;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Server.Logging;
using Calculator.Server.Networking;

namespace Calculator.Server
{
    internal static class Program
    {
        public static async Task Main()
        {
            const int port = 5000;

            var logger = new CsvHistoryLogger("logs");
            var server = new TcpCalculatorServer(port, logger);

            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                server.Stop();
                Console.WriteLine("\n[SERVER] Stopping...");
            };

            Console.WriteLine("[SERVER] Press Ctrl+C to stop.");
            await server.StartAsync(cts.Token);
        }
    }
}
