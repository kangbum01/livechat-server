using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerExample
{
    public class ClientSession
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private ChatServer _server;


        // 동작과 로그인 상태 확인
        private bool _isRunning;
        private bool _isLoggedIn;
        public string SessionName
        {
            get;
            private set;
        }
        public int PosX {get; private set;}
        public int PosY {get; private set;}

        public bool IsConnected
        {
            get
            {
                return _client != null && _client.Connected;
            }
        }

        public ClientSession(TcpClient client, ChatServer server, string sessionName)
        {
            _client = client;
            _server = server;
            SessionName = sessionName;

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8);
            _writer.AutoFlush = true;

            _isRunning = true;
            _isLoggedIn = false;
            PosX = 0;
            PosY = 0;
        }

        public async Task ReceiveLoopAsync()
        {
            try
            {
                await SendAsync("[서버] 연결되었습니다. ");
                await SendAsync("[서버] 먼저 LOGIN|닉네임 형식으로 로그인하세요.");
            
                while (_isRunning)
                {
                    string packet = await _reader.ReadLineAsync();

                    if (packet == null)
                    {
                        break;
                    }

                    await HandlePacketAsync(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[오류]" + SessionName + "ReceiveLoopAsync 예외: " + ex.Message);
            }
            finally
            {
                Close();
                _server.RemoveSession(this);
                await _server.BroadcastAsync("[서버]" + SessionName + "님이 퇴장했습니다.", this);
            }
        }

        private async Task HandlePacketAsync(string packet)
        {
            Console.WriteLine("[DEBUG] raw packet = " + packet);
            string[] parts = packet.Split('|', 2);
            string command = parts[0].Trim().ToUpper();
            string body = parts.Length > 1 ? parts[1].Trim() : "";

            Console.WriteLine("[DEBUG] command = " + command);
            Console.WriteLine("[DEBUG] body = " + body);
            switch (command)
            {
                case "LOGIN":
                    await HandleLoginAsync(body);
                    break;
                case "CHAT":
                    await HandleChatAsync(body);
                    break;
                case "QUIT":
                    await HandleQuitAsync();
                    break;
                case "MOVE":
                    await HandleMoveAsync(body);
                    break;
                default:
                    await SendAsync("[서버] 알 수 없는 명령입니다. LOGIN|닉네임, CHAT|메시지, QUIT| 를 입력하세요.");
                    break;
            }
        }

        private async Task HandleMoveAsync(string move)
        {
            if (_isLoggedIn == false)
            {
                await SendAsync("[서버] 로그인 작업을 먼저 완료하세요. ");
            }
            switch (move.ToLower())
            {
                case "left":
                    PosX -= 1;
                    break;
                case "right":
                    PosX += 1;
                    break;
                case "up":
                    PosY += 1;
                    break;
                case "down":
                    PosY -= 1;
                    break;
                default:
                    await SendAsync("[서버] 잘못 된 방향 입니다. ex) left, right, up, down");
                    return;
            }
            Console.WriteLine("[이동] " + SessionName + " -> (" + PosX + " , " + PosY + ")");

            await SendAsync("[서버] 내 위치: (" + PosX + ", " + PosY + ")");
            await _server.BroadcastAsync("[이동] " + SessionName + " -> (" + PosX + ", " + PosY + ")", this);
        }

        private async Task HandleLoginAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await SendAsync("[서버] 닉네임이 비었습니다. 예: LOGIN|범석");
                return;
            }

            if (_isLoggedIn == false)
            {
                SessionName = name;
                _isLoggedIn = true;
                PosX = 0;
                PosY = 0;

                await SendAsync("[서버] 로그인 완료: "+ SessionName);
                await _server.BroadcastAsync("[서버] " + SessionName + " 님이 입장했습니다.", this);
                await SendAsync("[서버] 현재 위치 : (" + PosX + " , " + PosY + ")" );
            }
            else
            {
                string oldName = SessionName;
                SessionName = name;

                await SendAsync("[서버] 닉네임 변경 완료: " + SessionName);
                await _server.BroadcastAsync("[서버] " + oldName + "님이 " + SessionName +"(으)로 이름을 변경했습니다.", this);
            }
        }

        private async Task HandleChatAsync(string message)
        {
            if (_isLoggedIn == false)
            {
                await SendAsync("[서버] 먼저 로그인을 하세요.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                await SendAsync("[서버] 빈 메시지는 보낼 수 없습니다.");
                return;
            }

            Console.WriteLine("[" + SessionName + "]" + message);
            // 메시지가 잘 전달 되는지 확인하기 위한 코드
            //await SendAsync(" [나] " + message);
            await _server.BroadcastAsync("[" + SessionName + "]" + message, this);

        }

        private async Task HandleQuitAsync()
        {
            await SendAsync("[서버] 연결을 종료합니다. ");
            _isRunning = false;
        }
        
        public async Task SendAsync(string message)
        {
            try
            {
                await _writer.WriteLineAsync(message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("[오류]" + SessionName + " SendAsync 예외: " + ex.Message);
            }
        }

        public void Close()
        {
            try
            {
                if (_writer != null ) _writer.Close();
                if (_reader != null ) _reader.Close();
                if (_stream != null ) _stream.Close();
                if (_client != null ) _client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[오류] "+ SessionName + " Close 예외: " + ex.Message);
            }
        }
    }
}