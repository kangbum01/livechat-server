using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClientExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            TcpClient client = new TcpClient();

            await client.ConnectAsync("127.0.0.1", 9000);
            Console.WriteLine("[Client] Connect to server.");

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            //계속 동작하게 설정
            writer.AutoFlush = true;

            Task receiveTask = ReceiveLoopAsync(reader);

            Console.WriteLine("닉네임을 입력하세요: ");
            string nickname = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nickname))
            {
                nickname = "Player";
            }

            await writer.WriteLineAsync("LOGIN|" + nickname);

            Console.WriteLine("[Client] 채팅을 입력하세요.");
            Console.WriteLine("[Client] /name 새이름  -> 닉네임 변경");
            Console.WriteLine("[Client] /quit        -> 종료");

            try
            {
                while (true)
                {
                    Console.Write("You: ");
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    if (input.Trim().ToLower() == "/quit")
                    {
                        await writer.WriteLineAsync("QUIT|");
                        break;
                    }

                    if (input.StartsWith("/name"))
                    {
                        string newName = input.Substring(6).Trim();

                        if (string.IsNullOrWhiteSpace(newName))
                        {
                            Console.WriteLine("[Client] 새 닉네임을 입력하세요.");
                            continue;
                        }

                        await writer.WriteLineAsync("LOGIN|" + newName);
                        continue;
                    }
                    await writer.WriteLineAsync("CHAT|" + input);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Client 오류]" + ex.Message);
            }
            finally
            {
                try
                {
                    stream.Close();
                    client.Close();
                }
                catch
                {}
            }

            await receiveTask;
        }

        static async Task ReceiveLoopAsync(StreamReader reader)
        {
            try
            {
                while(true)
                {
                    string message = await reader.ReadLineAsync();

                    if (message == null)
                    {
                        Console.WriteLine("[Client] Server disconnected.");
                        break;
                    }

                    Console.WriteLine();
                    Console.WriteLine("[Client] Receive: " + message);
                    Console.Write("You: ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Client Receive 오류] " + ex.Message);
            }
        }
    }
    
}