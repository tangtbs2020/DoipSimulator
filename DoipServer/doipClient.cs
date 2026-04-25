using System.Net;
using System.Net.Sockets;

namespace DOIPUtils
{
    class DoipClient
    {
        // 配置
        static string broadcastIp = "169.254.255.255"; // 广播地址
        static int udpPort = 13400;                     // DoIP UDP 端口
        //static int tcpPort = 13400;                     // DoIP TCP 端口
        static string? foundVehicleIp = null;            // 发现的车辆 IP

        //static void Main(string[] args)
        //{
        //    Console.WriteLine("=== 完整 DoIP 流程开始 ===");

        //    try
        //    {
        //        // 1. UDP 广播发现车辆
        //        Console.WriteLine("\n[步骤 1] 正在通过 UDP 广播寻找车辆...");
        //        if (DiscoverVehicle())
        //        {
        //            Console.WriteLine($"发现车辆 IP: {foundVehicleIp}");

        //            // 2. 建立 TCP 连接
        //            Console.WriteLine($"\n[步骤 2] 正在连接 TCP {foundVehicleIp}:{tcpPort}...");
        //            using (TcpClient client = new TcpClient())
        //            {
        //                client.ReceiveTimeout = 5000; // 5秒超时
        //                client.Connect(foundVehicleIp, tcpPort);
        //                Console.WriteLine("TCP 连接成功。");

        //                using (NetworkStream stream = client.GetStream())
        //                {
        //                    // 3. 路由激活
        //                    Console.WriteLine("\n[步骤 3] 发送路由激活请求...");
        //                    if (SendRoutingActivation(stream))
        //                    {
        //                        // 4. 发送 UDS 请求 (读取 VIN)
        //                        Console.WriteLine("\n[步骤 4] 发送 UDS 读取 VIN 请求 (0x22 0xF1 90)...");
        //                        SendUdsMessage(stream, 0x0E00, 0x1001, new byte[] { 0x22, 0xF1, 0x90 });

        //                        // 5. 接收响应
        //                        Console.WriteLine("\n[步骤 5] 等待响应...");
        //                        ReceiveUdsResponse(stream);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("未找到车辆，请检查网络或车辆电源。");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"发生错误: {ex.Message}");
        //    }

        //    Console.WriteLine("\n流程结束。按任意键退出...");
        //    Console.ReadKey();
        //}

        // --- 1. UDP 广播发现逻辑 ---
        static bool DiscoverVehicle()
        {
            using (UdpClient udpClient = new UdpClient())
            {
                // 允许接收广播包
                udpClient.EnableBroadcast = true;
                // 绑定到本地任意端口（系统会自动分配），或者指定本地端口
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                // 设置广播目标
                udpClient.Connect(broadcastIp, udpPort);

                // 构造 车辆识别请求 (Payload Type 0x0004)
                // ISO 13400-2: Header(8 bytes) + Payload(0 bytes for simple request)
                byte[] header = new byte[] {
                    0x02, 0xFD,       // Protocol Version & Inverse
                    0x00, 0x04,       // Payload Type: Vehicle Identification Request
                    0x00, 0x00, 0x00, 0x00 // Payload Length: 0
                };

                Console.WriteLine("发送广播包...");
                udpClient.Send(header, header.Length);

                // 等待响应 (阻塞 3 秒)
                udpClient.Client.ReceiveTimeout = 3000;
                IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    byte[] receivedBytes = udpClient.Receive(ref remoteEndpoint);

                    // 简单解析响应头
                    if (receivedBytes.Length >= 8)
                    {
                        ushort pType = (ushort)((receivedBytes[2] << 8) | receivedBytes[3]);
                        // 0x0005 = Vehicle Announcement Message / Identification Response
                        if (pType == 0x0005 || pType == 0x0006)
                        {
                            foundVehicleIp = remoteEndpoint.Address.ToString();
                            return true;
                        }
                    }
                }
                catch (SocketException)
                {
                    // 超时
                    Console.WriteLine("等待响应超时...");
                }
            }
            return false;
        }

        // --- 2. 路由激活 ---
        static bool SendRoutingActivation(NetworkStream stream)
        {
            byte[] header = new byte[16];
            header[0] = 0x02; header[1] = 0xFD;
            header[2] = 0x00; header[3] = 0x05; // Routing Activation Request
            header[7] = 0x07; // Length
            header[8] = 0x0E; header[9] = 0x00; // Source Address (Tester)
            header[10] = 0x00; // Activation Type (Default)

            stream.Write(header, 0, header.Length);

            // 读取响应
            byte[]? response = ReadExact(stream, 16);
            if (response != null)
            {
                ushort pType = (ushort)((response[2] << 8) | response[3]);
                if (pType == 0x0200) // Routing Activation Response (Success)
                {
                    Console.WriteLine("路由激活成功。");
                    return true;
                }
            }
            return false;
        }

        // --- 3. 发送 UDS 消息 ---
        static void SendUdsMessage(NetworkStream stream, ushort srcAddr, ushort tgtAddr, byte[] udsData)
        {
            int payloadLen = 4 + udsData.Length;
            byte[] header = new byte[8];
            header[0] = 0x02; header[1] = 0xFD;
            header[2] = 0x80; header[3] = 0x01; // Diagnostic Message
            // Length (Big Endian)
            header[4] = (byte)(payloadLen >> 24);
            header[5] = (byte)(payloadLen >> 16);
            header[6] = (byte)(payloadLen >> 8);
            header[7] = (byte)(payloadLen);

            // 组合包
            byte[] packet = new byte[8 + payloadLen];
            Array.Copy(header, packet, 8);

            // Address Info
            packet[8] = (byte)(tgtAddr >> 8); packet[9] = (byte)(tgtAddr);
            packet[10] = (byte)(srcAddr >> 8); packet[11] = (byte)(srcAddr);

            // UDS Data
            Array.Copy(udsData, 0, packet, 12, udsData.Length);

            stream.Write(packet, 0, packet.Length);
        }

        // --- 4. 接收 UDS 响应 ---
        static void ReceiveUdsResponse(NetworkStream stream)
        {
            // 简单读取 8 字节头
            byte[]? header = ReadExact(stream, 8);
            if (header != null)
            {
                // 读取长度
                int len = (header[4] << 24) | (header[5] << 16) | (header[6] << 8) | header[7];
                byte[]? payload = ReadExact(stream, len);

                if (payload != null)
                {
                    // 打印 UDS 数据 (跳过 4 字节地址信息)
                    byte[] udsBytes = new byte[len - 4];
                    Array.Copy(payload, 4, udsBytes, 0, len - 4);
                    Console.WriteLine($"收到 UDS 数据: {BitConverter.ToString(udsBytes)}");
                }
            }
        }

        // 辅助：精确读取指定字节数
        static byte[]? ReadExact(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);
                if (read == 0) return null; // 断开
                offset += read;
            }
            return buffer;
        }
    }
}