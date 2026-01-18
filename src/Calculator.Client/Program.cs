using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Client
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            string host = args.Length >= 1 ? args[0] : "127.0.0.1";
            int port = args.Length >= 2 && int.TryParse(args[1], out int p) ? p : 5000;

            Console.WriteLine($"[CLIENT] Connecting to {host}:{port} ...");

            using var client = new TcpClient();
            await client.ConnectAsync(host, port);

            Console.WriteLine("[CLIENT] Connected.");
            Console.WriteLine("[CLIENT] Type an expression and press Enter (type 'exit' to quit).");

            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (input == null) break;

                input = input.Trim();
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                if (input.Length == 0) continue;

                await writer.WriteLineAsync(input);

                string? response = await reader.ReadLineAsync();
                if (response == null)
                {
                    Console.WriteLine("[CLIENT] Server disconnected.");
                    break;
                }

                Console.WriteLine($"< {response}");
            }

            Console.WriteLine("[CLIENT] Closing...");
        }
    }
}
