using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk
{
    public static class TcpServer
    {
        static void Temp()
        {
            int port = 9000;
            TcpListener server = new TcpListener(IPAddress.Any, port);

            server.Start();
            Console.WriteLine($"서버 시작됨 (포트 {port})");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("클라이언트 연결됨");

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("수신 메시지: " + message);

                // 응답 전송
                string response = "서버에서 메시지 받음!";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);

                client.Close();
            }
        }
    }
}
