using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kiosk
{
    public static class TcpClientService
    {
        private static string serverIP;
        private static int port;
        private static TcpClient _client;
        private static NetworkStream _stream;
        private static CancellationTokenSource _cts;

        public static bool IsConnected => _client?.Connected ?? false;

        /// 수신 이벤트
        public static event Action<byte[]> DataReceived;

        /// 연결 종료 이벤트
        public static event Action<string> Disconnected;

        /// 서버 연결
        public static async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            serverIP = ConfigurationManager.AppSettings["IP"];
            port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            _client = new TcpClient();
            _cts = new CancellationTokenSource();

            await _client.ConnectAsync(serverIP, port);
            _stream = _client.GetStream();

            _ = Task.Run(() => ReceiveLoop(_cts.Token));
        }

        /// 송신 (byte[])
        private static async Task<bool> SendAsync(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("TCP not connected");

            await _stream.WriteAsync(data, 0, data.Length);
            return true;
        }

        /// 송신
        public static Task<bool> SendAsync<T>(T data, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            string json = JsonConvert.SerializeObject(data);
            byte[] bytes = encoding.GetBytes(json);

            return SendAsync(bytes);
        }

        /// 수신 루프
        private static async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0)
                        break;

                    byte[] received = new byte[read];
                    Buffer.BlockCopy(buffer, 0, received, 0, read);

                    DataReceived?.Invoke(received);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            catch (Exception ex)
            {
                Disconnected?.Invoke(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        /// 연결 종료
        public static void Disconnect()
        {
            if (_cts?.IsCancellationRequested == false)
                _cts.Cancel();

            _stream?.Close();
            _client?.Close();

            _stream = null;
            _client = null;

            Disconnected?.Invoke("Disconnected");
        }
    }
}
