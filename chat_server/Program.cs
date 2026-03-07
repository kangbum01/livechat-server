// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    private static List<ClientSession> sessions = new List<ClientSession>();
    private static int nextSessionId = 1;
    static async Task Main()
    {
        int port = 5000;

        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Listening on {port}... ");
        
        try
        {
            NetworkStream stream = client.GetStream();

            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            while (true)
            {
                string? message = await reader.ReadLineAsync();
                if (message == null)
                {
                    Console.WriteLine("[Server] Client disconnected. ");
                    break;
                }
                Console.WriteLine("[Server] Received: " + message);

                if (message == "/quit")
                {
                    await writer.WriteLineAsync("Goodbye!");
                    Console.WriteLine("[Server] Quit message received.");
                    break;
                }
                await writer.WriteLineAsync("Echo: " + message);
                Console.WriteLine("[Server] Echo sent.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("[Server] Read failed: "+ ex.Message);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("[Server] Socket failed: "+ ex.Message);
        }
        finally
        {
            client.Close();
            listener.Stop();
            Console.WriteLine("[Server] Done.");
        }
    }
}