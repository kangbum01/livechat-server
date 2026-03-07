using System;
using System.IO;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    static async Task Main()
    {
        string host = "127.0.0.1";
        int port = 5000;

        TcpClient client = new TcpClient();
        await client.ConnectAsync(host, port);

        Console.WriteLine("[Client] Connect to server.");
        try
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            while(true)
            {
                Console.Write("You: ");
                string? input = Console.ReadLine();

                if(string.IsNullOrEmpty(input))
                {
                    continue;
                }

                await writer.WriteLineAsync(input);
                Console.WriteLine("[Client] Message sent.");

                string? reply = await reader.ReadLineAsync();
                Console.WriteLine("[Client] Receive: "+ reply);

                if (input == "/quit")
                {
                    Console.WriteLine("[Client] Quit.");
                    break;
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("[Client] Write failed: "+ ex.Message);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("[Client] Socket failed: " + ex.Message);
        }
        finally
        {
            client.Close();
            Console.WriteLine("[Client] Done.");
        }
    }
}