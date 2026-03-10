using System;
using System.Threading.Tasks;

namespace ChatServerExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ChatServer server = new ChatServer("127.0.0.1", 9000);
            await server.StartAsync();
        }
    }
}
