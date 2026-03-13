using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClientExample
{
    internal class Program
    {
        private static bool _isClosing = false;
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

            Console.WriteLine("닉네임을 입력하세요: ");
            string nickname = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nickname))
            {
                nickname = "Player";
            }

            await writer.WriteLineAsync("LOGIN|" + nickname);

            Task receiveTask = ReceiveLoopAsync(reader);

            Console.WriteLine("[Client] 채팅을 입력하세요.");
            Console.WriteLine("[Client] /name 새이름  -> 닉네임 변경");
            Console.WriteLine("[Client] /quit        -> 종료");
            Console.WriteLine("[Cleint] /move 방향    -> 캐릭터 위치 이동");

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
                        _isClosing = true;
                        await writer.WriteLineAsync("QUIT|");
                        break;
                    }

                    if (input.StartsWith("/name "))
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

                    if (input.Trim().ToLower() == "/move")
                    {
                        Console.WriteLine("[Client] 이동 방향을 입력하세요. 예: /move left");
                        continue;
                    }
                    if (input.StartsWith("/move "))
                    {
                        string direction = input.Substring(6).Trim().ToLower();

                        if (string.IsNullOrWhiteSpace(direction))
                        {
                            Console.WriteLine("[Client] 이동 방향을 입력하세요.");
                            continue;
                        }
                        await writer.WriteLineAsync("MOVE|" + direction);
                        continue;
                    }
                    await writer.WriteLineAsync("CHAT|" + input);
                }
            }
            catch (Exception ex)
            {
                if( _isClosing == false)
                {
                    Console.WriteLine("[Client 오류]" + ex.Message);
                }
            }
            finally
            {
                try
                {
                    _isClosing = true;
                    reader.Close();
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
                if(_isClosing == false)
                {
                    Console.WriteLine("[Client Receive 오류] " + ex.Message);
                }
                
            }
        }
    }
    
}