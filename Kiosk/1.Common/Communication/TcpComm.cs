using Kiosk.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    public class TcpComm
    {
        private static TcpComm _instance;
        private static readonly object _lock = new object();

        private string _serverIP;
        private int _port;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private ConcurrentQueue<byte[]> _recvQueue;

        /// 약속된 패킷헤더 길이
        private const int packetHeaderLength = 8;   // 패킷 길이 4 + 메시지 타입 4 = 8

        public bool IsConnected => _client?.Connected ?? false;
        /// 연결 종료 이벤트
        public event Action<string> Disconnected;

        public static TcpComm Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TcpComm();
                    }
                    return _instance;
                }
            }
        }

        private TcpComm()
        {
            _recvQueue = new ConcurrentQueue<byte[]>();
        }

        /// 서버 연결
        public async Task ConnectAsync(string ip, int port)
        {
            if (IsConnected)
                return;

            _serverIP = ip;
            _port = port;
            _client = new TcpClient();
            _cts = new CancellationTokenSource();

            await _client.ConnectAsync(_serverIP, _port);
            _stream = _client.GetStream();

            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            _ = Task.Run(() => ParsePacketAsync(_cts.Token));
        }

        /// 수신 루프
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            List<byte> _buffer = new List<byte>();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] temp = new byte[8192];
                    int bytesRead = await _stream.ReadAsync(temp, 0, temp.Length, token);
                    if (bytesRead == 0)
                        break;

                    // 새로 받은 데이터를 누적
                    _buffer.AddRange(temp.Take(bytesRead));

                    while (true)
                    {
                        // 헤더가 안왔거나 버퍼에 남은 데이터가 없을시
                        if (_buffer.Count < packetHeaderLength)
                            break;

                        int packetLen = BitConverter.ToInt32(_buffer.ToArray(), 0);

                        // 헤더에 있는 패킷 길이 필드에 잘못된 값이 들어올시
                        if (packetLen <= 0 || packetLen > 65536)
                        {
                            Console.WriteLine("잘못된 패킷 길이");
                            _buffer.Clear();
                            break;
                        }

                        // 패킷 전체가 아직 안 왔으면 대기
                        if (_buffer.Count < packetLen)
                            break;

                        // 🔹 패킷 하나 완성
                        byte[] packet = _buffer.GetRange(0, packetLen).ToArray();
                        _recvQueue.Enqueue(packet);

                        // 사용한 데이터 제거 (헤더 + 바디)
                        _buffer.RemoveRange(0, packetLen);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        private async Task ParsePacketAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_recvQueue.TryDequeue(out var data))
                {
                    byte[] header = new byte[packetHeaderLength];
                    Buffer.BlockCopy(data, 0, header, 0, packetHeaderLength);

                    byte[] body = new byte[data.Length - packetHeaderLength];
                    Buffer.BlockCopy(data, packetHeaderLength, body, 0, data.Length - packetHeaderLength);

                    AnalyzePacket(header, body);
                }

                await Task.Delay(500);
            }
        }

        private void AnalyzePacket(byte[] header, byte[] body)
        {
            int packetLen = BitConverter.ToInt32(header, 0);
            var msgID = (MsgIDEnum)BitConverter.ToInt32(header, sizeof(int));

            if (msgID == MsgIDEnum.MenuList)
            {
                if (DataManager.instance.GetAllProducts().Count == 0)
                {
                    var menu = Deserialize<MenuList>(body);
                    DataManager.instance.CopyAllProducts(menu.Products);
                }
            }
        }

        /// 연결 종료
        public void Disconnect()
        {
            if (_cts?.IsCancellationRequested == false)
                _cts.Cancel();

            _stream?.Close();
            _client?.Close();

            _stream = null;
            _client = null;

            Disconnected?.Invoke("서버 연결이 끊겼습니다.");
        }


        /// 송신 (byte[])
        private async Task<bool> SendAsync(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("TCP not connected");

            await _stream.WriteAsync(data, 0, data.Length);
            return true;
        }

        /// 송신
        public Task<bool> SendAsync<T>(T data)
        {
            MsgIDEnum msgID = GetMsgID(data);
            byte[] payload = Serialize(data);
            byte[] packet = MakePacket(msgID, payload);

            return SendAsync(packet);
        }

        private byte[] MakeHeader(MsgIDEnum msgID, int payloadLen)
        {
            byte[] header = new byte[packetHeaderLength];
            int packetLen = packetHeaderLength + payloadLen;

            // 패킷 길이는 헤더 시작에 위치
            Buffer.BlockCopy(BitConverter.GetBytes(packetLen), 0, header, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes((int)msgID), 0, header, sizeof(int), sizeof(int));
            return header;
        }

        private byte[] MakePacket(MsgIDEnum msgID, byte[] payload)
        {
            byte[] header = MakeHeader(msgID, payload.Length);
            byte[] packet = new byte[header.Length + payload.Length];

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(payload, 0, packet, header.Length, payload.Length);
            return packet;
        }

        /// 객체 → 바이트 배열
        private byte[] Serialize<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        /// 바이트 배열 → 객체
        private T Deserialize<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private MsgIDEnum GetMsgID<T>(T obj)
        {
            var attr = obj.GetType().GetCustomAttributes(false);
            return (attr[0] as MessageTypeAttribute).MsgID;
        }
    }
}
