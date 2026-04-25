using Microsoft.VisualBasic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;


namespace DOIPUtils
{
    internal sealed class DoIPServer
    {
        // 模拟器配置
        private static List<DataGroup>? EthernetData = null;

        private static DOIP.Information DoipInfo = new DOIP.Information();

        // 模拟的 VIN 码
        //private static byte[] VIN = Encoding.ASCII.GetBytes("LBV8A9406GMF25307");


        public static void SetEthernetData(List<DataGroup> dataGroups)
        {
            EthernetData = dataGroups;
        }



        public static void StartServer(DOIP.Information info)
        {
            DoipInfo = info;    
            LogHelper.InitLogFile();


            LogHelper.Write("DoIP ECU 模拟器启动");
            LogHelper.Write($"本 机 IP: {DoipInfo.IP}");
            LogHelper.Write($"TCP 端口: {DoipInfo.TCPPort}");
            LogHelper.Write($"UDP 端口: {DoipInfo.UDPPort}");
            LogHelper.WritePlainText($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");

            // 启动 UDP 广播监听线程 (处理车辆发现)
            Thread udpThread = new Thread(StartUdpListener);
            udpThread.IsBackground = true;
            udpThread.Start();

            //// 启动 TCP 监听 (处理诊断通信)
            //StartTcpListener();


            //Task.Run(delegate ()
            //{
            //    //只处理一个 UDP 请求，收到后就进入 TCP 监听
            //    StartUdpListener();

            //}).Wait();



            StartTcpListener();
        }

        // --- UDP 部分：响应车辆发现 ---
        static void StartUdpListener()
        {
            UdpClient udpClient = new UdpClient(DoipInfo.UDPPort);
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("[UDP] 开始监听广播...");

            while (true)
            {
                try
                {
                    byte[] receivedBytes = udpClient.Receive(ref remoteEndpoint);

                    // 解析 DoIP 头
                    if (receivedBytes.Length >= 6)
                    {
                        ushort payloadType = (ushort)((receivedBytes[4] << 8) | receivedBytes[5]);

                        // 0x0004: Vehicle Identification Request
                        if (payloadType == 0x0011)
                        {
                            Console.WriteLine($"[UDP] 发现请求来自: {remoteEndpoint.Address}");
                            SendVehicleAnnouncement(udpClient, remoteEndpoint);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UDP] 错误: {ex.Message}");
                }
            }
        }

        // 发送车辆公告消息 (Vehicle Announcement Message)
        static void SendVehicleAnnouncement(UdpClient client, IPEndPoint endpoint)
        {
            // 构造响应包 (Payload Type 0x0005)
            // 这是一个简化的响应，实际标准可能包含更多逻辑地址信息
            //byte[] response = new byte[] { 0x00, 0x00, 0x00, 0x32, 0x00, 0x11, 0x44, 0x49, 0x41, 0x47, 0x41, 0x44, 0x52, 0x31, 0x30, 0x42, 0x4D, 0x57, 0x4D, 0x41, 0x43, 0x30, 0x30, 0x31, 0x41, 0x33, 0x37, 0x32, 0x37, 0x44, 0x46, 0x32, 0x32, 0x42, 0x4D, 0x57, 0x56, 0x49, 0x4E, 0x4C, 0x42, 0x56, 0x38, 0x41, 0x39, 0x34, 0x30, 0x36, 0x47, 0x4D, 0x46, 0x32, 0x35, 0x33, 0x30, 0x37 }; // 8 Header + 22 Payload


            
            var dataDiag = Encoding.ASCII.GetBytes("DIAGADR10BMWMAC");
            var dataMac = Encoding.ASCII.GetBytes(DoipInfo.MAC);
            var dataVin = Encoding.ASCII.GetBytes("BMWVIN" + DoipInfo.VIN);
            int flag = 0x0011;
            int len = dataDiag.Length + dataMac.Length + dataVin.Length;


            byte[] response = new byte[len + 6];
            response[0] = (byte)(len >> 24);
            response[1] = (byte)(len >> 16);
            response[2] = (byte)(len >> 8);
            response[3] = (byte)(len);
            response[4] = (byte)(flag >> 8);
            response[5] = (byte)(flag);
            Array.Copy(dataDiag, 0, response, 6, dataDiag.Length);
            Array.Copy(dataMac, 0, response, 6 + dataDiag.Length, dataMac.Length);
            Array.Copy(dataVin, 0, response, 6 + dataDiag.Length + dataMac.Length, dataVin.Length);

            // Header
            //response[0] = 0x02; // Version
            //response[1] = 0xFD; // Inverse
            //response[2] = 0x00; // Payload Type High
            //response[3] = 0x05; // Payload Type Low (0x0005)

            // Payload Length (22 bytes)
            //response[4] = 0x00; response[5] = 0x00; response[6] = 0x00; response[7] = 0x06; // 修正长度

            // Payload: Logical Address of ECU (0x1001 - Engine)
            //response[8] = 0x10; response[9] = 0x01;

            // Payload: VIN (这里简单填充，实际需符合标准结构)
            // 为了演示，我们只填充部分字段，实际长度需严格计算
            // 此处仅做演示，发送一个简短的响应
            //byte[] simpleResponse = new byte[16];
            //Array.Copy(response, 0, simpleResponse, 0, 8); // Copy Header
            //simpleResponse[4] = 0x00; simpleResponse[5] = 0x00; simpleResponse[6] = 0x00; simpleResponse[7] = 0x02; // Length 2
            //simpleResponse[8] = 0x10; simpleResponse[9] = 0x01; // ECU Address

            client.Send(response, response.Length, endpoint);
            //Console.WriteLine("[UDP] 发送车辆公告响应。");
        }

        //// --- TCP 部分：处理诊断会话 ---
        //static void StartTcpListener()
        //{
        //    TcpListener server = new TcpListener(IPAddress.Parse(IPAddr), TCPPort);
        //    server.Start();
        //    Console.WriteLine("[TCP] 等待诊断仪连接...");

        //    while (true)
        //    {
        //        try
        //        {

        //            //// 等待客户端连接
        //            TcpClient client = server.AcceptTcpClient();
        //            Console.WriteLine($"[TCP] 新连接来自: {client.Client.RemoteEndPoint}");

        //            // 为每个客户端开启一个新线程处理
        //            Thread clientThread = new Thread(() => HandleClient(client));
        //            clientThread.IsBackground = true;
        //            clientThread.Start();

        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[TCP] 监听错误: {ex.Message}");
        //        }
        //    }
        //}


        static void StartTcpListener()
        {
            TcpListener server = new TcpListener(IPAddress.Parse(DoipInfo.IP), DoipInfo.TCPPort);
            server.Start();
            while (true)
            {
                //// 等待客户端连接
                TcpClient client = server.AcceptTcpClient();
                client.ReceiveTimeout = 50000;
                client.SendTimeout = 50000;
                LogHelper.Write($"[conn] 新连接来自: {client.Client.RemoteEndPoint}");
                NetworkStream stream = client.GetStream();
                try
                {
                    while (client.Connected)
                    {
                        // 读取 DoIP Header (8 bytes)
                        byte[]? header = ReadExact(stream, 6);
                        if (header == null)
                        {
                            LogHelper.Write($"[recv] DoIP Header接收失败,连接断开");
                            break;
                        }
                   

                        ushort payloadType = (ushort)((header[4] << 8) | header[5]);
                        int payloadLength = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];

                        // 读取 Payload
                        byte[]? payload = ReadExact(stream, payloadLength);
                        if (payload == null)
                        {
                            LogHelper.Write($"[recv] DoIP Payload接收失败,连接断开");
                            break;
                        }

                        byte [] fullMessage = new byte[header.Length + payload.Length];
                        Array.Copy(header, 0, fullMessage, 0, header.Length);
                        Array.Copy(payload, 0, fullMessage, header.Length, payload.Length);


                        LogHelper.Write($"[recv]", fullMessage);
                        // 处理不同类型的消息
                        //if (payloadType == 0x0001) // Diagnostic Message
                        //{
                        //SendDoipACK(stream, payload);
                        //ProcessDiagnosticMessage(stream, payload);
                        //}

                        
                        var listAns = EthernetData?.Where(x => fullMessage.SequenceEqual(x.RequestData));
                        if (listAns != null && listAns.Any())
                        {
                            DataGroup? dataGroup = listAns.FirstOrDefault();
                            if (dataGroup != null) 
                            {
                                foreach (var response in dataGroup.ResponseData)
                                {
                                    try
                                    {
                                        SendDoipMessage(stream, response);

                                    } 
                                    catch(Exception ex)
                                    {
                                        LogHelper.Write($"[send] 发送失败,客户端异常断开:{ex.Message}");
                                        break;
                                    }
                                    
                                }

                            }
      
                         
                        }

                        LogHelper.WritePlainText(Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Write($"[conn] 客户端异常断开: {ex.Message}");
                }
                finally
                {
                    LogHelper.Write($"[conn] 客户端关闭连接");
                    LogHelper.WritePlainText($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
                    client.Close();
                }

            }


        }

        //static void HandleClient(TcpClient client)
        //{
        //    NetworkStream stream = client.GetStream();
        //    try
        //    {
        //        while (true)
        //        {
        //            // 读取 DoIP Header (8 bytes)
        //            byte[]? header = ReadExact(stream, 6);
        //            if (header == null) break;

        //            ushort payloadType = (ushort)((header[4] << 8) | header[5]);
        //            int payloadLength = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];

        //            // 读取 Payload
        //            byte[]? payload = ReadExact(stream, payloadLength);
        //            if (payload == null) break;

        //            // 处理不同类型的消息
        //            if (payloadType == 0x0001) // Diagnostic Message
        //            {
        //                SendDoipACK(stream, payload);
        //                ProcessDiagnosticMessage(stream, payload);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[连接] 客户端异常断开: {ex.Message}");
        //    }
        //    finally
        //    {
        //        client.Close();
        //    }
        //}


        static bool ProcessRoutingActivation(NetworkStream stream, byte[] payload)
        {
            Console.WriteLine("[DoIP] 收到路由激活请求。");

 
            byte[] response = new byte[payload.Length + 6];
            response[0] = (byte)(payload.Length>>24);
            response[1] = (byte)(payload.Length >> 16);
            response[2] = (byte)(payload.Length >> 8);
            response[3] = (byte)(payload.Length >> 0);
            response[4] = 0x00;
            response[5] = 0x02;

            Array.Copy(payload, 0, response, 6, payload.Length);

            stream.Write(response, 0, response.Length);
            //Console.WriteLine("[DoIP] 路由激活成功。");
            return true;
        }


        /// <summary>
        /// DOIP ACK响应
        /// </summary>
        static void SendDoipACK(NetworkStream stream, byte[] payload)
        { 
            byte[] response = new byte[payload.Length + 6];
            response[0] = (byte)(payload.Length >> 24);
            response[1] = (byte)(payload.Length >> 16);
            response[2] = (byte)(payload.Length >> 8);
            response[3] = (byte)(payload.Length >> 0);
            response[4] = 0x00;
            response[5] = 0x02;

            Array.Copy(payload, 0, response, 6, payload.Length);

            stream.Write(response, 0, response.Length);
           
            LogHelper.Write($"[send]", response);
        }

        // 处理诊断消息 (UDS)
        static void ProcessDiagnosticMessage(NetworkStream stream, byte[] payload)
        {
            // 解析地址信息
            //F4 60 22 F1 50
            byte targetAddr = payload[1];
            byte sourceAddr = payload[0];

            // 提取 UDS 数据
            byte[] udsData = new byte[payload.Length - 2];
            Array.Copy(payload, 2, udsData, 0, udsData.Length);


            // 简单的 UDS 逻辑处理
            byte[]? responseData = null;

            // 0x22 10 03 (Read VIN) - 模拟响应
            if (udsData.Length >= 3 && udsData[0] == 0x22 && udsData[1] == 0xF1 && udsData[2] == 0x50)
            {
                responseData = new byte[] { 0x62,0xF1,0x50,0x0F,0x13,0x10 };

            } 
            else if(udsData.Length >= 3 && udsData[0] == 0x22 && udsData[1] == 0xF1 && udsData[2] == 0x90)
            {
                byte[] vinResponse = new byte[3 + DoipInfo.VIN.Length];
                vinResponse[0] = 0x62; // Service ID + 0x40
                vinResponse[1] = 0xF1; // DID High
                vinResponse[2] = 0x90; // DID Low
                Array.Copy(Encoding.ASCII.GetBytes(DoipInfo.VIN), 0, vinResponse, 3, DoipInfo.VIN.Length);
                responseData = vinResponse;
            }
            else
            {
                // 默认返回否定响应 0x7F 22 11 (Service Not Supported)
                responseData = new byte[] { 0x7F, 0x22, 0x11 };
            }

            // 发送 DoIP 响应
            SendDoipMessage(stream, sourceAddr, targetAddr, responseData);
        }

        static void SendDoipMessage(NetworkStream stream, byte target, byte source, byte[] udsData)
        {
            int payloadLen = 2 + udsData.Length;
            byte[] header = new byte[6];
            header[0] = (byte)(payloadLen>>24); 
            header[1] = (byte)(payloadLen >> 16);
            header[2] = (byte)(payloadLen >> 8);
            header[3] = (byte)(payloadLen >> 0);
            header[4] = 0x00;
            header[5] = 0x01;


            byte[] packet = new byte[header.Length + payloadLen];
            Array.Copy(header, packet, header.Length);

            // Address Info
            packet[6] = source;
            packet[7] = target;

            // UDS Data
            Array.Copy(udsData, 0, packet,8, udsData.Length);
            stream.Write(packet, 0, packet.Length);
            LogHelper.Write($"[send] ",packet);
        }


        static void SendDoipMessage(NetworkStream stream,byte[]? packet)
        {
            if (packet != null && packet.Length > 0) 
            {
                stream.Write(packet, 0, packet.Length);
                LogHelper.Write($"[send]", packet);
            }
      
        }


        // 辅助函数：精确读取
        static byte[]? ReadExact(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);
                if (read == 0) return null;
                offset += read;
            }
            return buffer;
        }
    }


    internal static class NetworkHelper
    {
        public static List<string> GetAllLocalIPv4()
        {
            List<string> ipList = new List<string>();

            // 1. 获取所有网络接口
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 2. 过滤：只选择正在运行 (Up) 且非回环 (Loopback) 的网卡
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // 3. 获取该网卡的 IP 属性
                    var ipProperties = ni.GetIPProperties();

                    // 4. 遍历单播地址 (UnicastAddresses)
                    foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
                    {
                        // 5. 只取 IPv4 地址
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipList.Add(ipInfo.Address.ToString());
                        }
                    }
                }
            }
            return ipList.Distinct().ToList(); // 去重
        }
    }

    // 调用示例
    // var ips = NetworkHelper.GetAllLocalIPv4();
    // ips.ForEach(ip => Console.WriteLine($"发现 IP: {ip}"));
}