using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServerExample
{
    public class ChatServer
    {
        private TcpListener _listener;
        // 서버에 접속한 모든 클라이언트를 저장하는 리스트
        private List<ClientSession> _sessions;
        private object _lockObject;
        private int _sessionId;

        public ChatServer(string ip, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _sessions = new List<ClientSession>();
            _lockObject = new object();
            _sessionId = 0;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("[서버] 서버가 시작되었습니다.");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();

                _sessionId++;
                string sessionName = "Guest" + _sessionId;
                Console.WriteLine("[서버] 클라이언트 접속: " + sessionName);

                ClientSession session = new ClientSession(client, this, sessionName);

                AddSession(session);

                _ = session.ReceiveLoopAsync();
            }
        }

        public void AddSession(ClientSession session)
        {
            lock (_lockObject)
            {
                _sessions.Add(session);
                Console.WriteLine("[서버] 현재 접속자 수: " + _sessions.Count);
            }
        }

        public void RemoveSession(ClientSession session)
        {
            lock (_lockObject)
            {
                if (_sessions.Contains(session))
                {
                    _sessions.Remove(session);
                    Console.WriteLine("[서버] 세션 제거: "+ session.SessionName);
                    Console.WriteLine("[서버] 현재 접속자 수: " + _sessions.Count);
                }
            }
        }

        public async Task BroadcastAsync(string message, ClientSession sender)
        {
            List<ClientSession> copiedSessions;

            lock (_lockObject)
            {
                copiedSessions = new List<ClientSession>(_sessions);
            }

            foreach (ClientSession session in copiedSessions)
            {
                if (session != sender)
                {
                    await session.SendAsync(message);
                }
            }
        }
    }
}