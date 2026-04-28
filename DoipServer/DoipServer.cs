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

        // 用于优雅退出的取消机制
        private static CancellationTokenSource? _cts;
        private static UdpClient? _udpClient;
        private static TcpListener? _tcpServer;
        private static readonly List<TcpClient> _activeClients = [];
        private static bool _autoReplyEnabled = true;
        private static bool _udsAutoReplyEnabled = true;

        public static void SetEthernetData(List<DataGroup> dataGroups)
        {
            EthernetData = dataGroups;
        }

        public static void SetAutoReply(bool enabled)
        {
            _autoReplyEnabled = enabled;
        }
        public static void SetUdsAutoReply(bool enabled)
        {
            _udsAutoReplyEnabled = enabled;
        }

        public static void StartServer(DOIP.Information info)
        {
            DoipInfo = info;
            _cts = new CancellationTokenSource();
            LogHelper.InitLogFile();

            LogHelper.Write("DoIP ECU 模拟器启动");
            LogHelper.Write($"本 机 IP: {DoipInfo.IP}");
            LogHelper.Write($"TCP 端口: {DoipInfo.TCPPort}");
            LogHelper.Write($"UDP 端口: {DoipInfo.UDPPort}");
            LogHelper.WritePlainText($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");

            var token = _cts.Token;

            // UDP 监听线程
            Thread udpThread = new Thread(() => StartUdpListener(token));
            udpThread.IsBackground = true;
            udpThread.Start();

            // TCP 监听线程（后台运行，不阻塞调用方）
            Thread tcpThread = new Thread(() => StartTcpListener(token));
            tcpThread.IsBackground = true;
            tcpThread.Start();
        }

        public static void StopServer()
        {
            _cts?.Cancel();
            _udpClient?.Close();
            _tcpServer?.Stop();

            lock (_activeClients)
            {
                foreach (var client in _activeClients)
                {
                    try { client.Close(); } catch { /* 忽略关闭异常 */ }
                }
                _activeClients.Clear();
            }

            _udpClient = null;
            _tcpServer = null;
            _cts?.Dispose();
            _cts = null;

            LogHelper.Write("DoIP 服务已停止");
        }

        // --- UDP 部分：响应车辆发现 ---
        static void StartUdpListener(CancellationToken token)
        {
            try
            {
                _udpClient = new UdpClient(DoipInfo.UDPPort);
                IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

                Console.WriteLine("[UDP] 开始监听广播...");

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        byte[] receivedBytes = _udpClient.Receive(ref remoteEndpoint);

                        // 解析 DoIP 头
                        if (receivedBytes.Length >= 6)
                        {
                            ushort payloadType = (ushort)((receivedBytes[4] << 8) | receivedBytes[5]);

                            // 0x0011: 自定义车辆识别请求
                            if (payloadType == 0x0011)
                            {
                                Console.WriteLine($"[UDP] 发现请求来自: {remoteEndpoint.Address}");
                                SendVehicleAnnouncement(_udpClient, remoteEndpoint);
                            }
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        break; // UDP 客户端被关闭，正常退出
                    }
                    catch (SocketException)
                    {
                        break; // Socket 关闭，正常退出
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UDP] 错误: {ex.Message}");
            }
            finally
            {
                _udpClient?.Dispose();
                _udpClient = null;
            }
        }

        // 发送车辆公告消息
        static void SendVehicleAnnouncement(UdpClient client, IPEndPoint endpoint)
        {
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

            client.Send(response, response.Length, endpoint);
        }

        // --- TCP 部分：处理诊断会话 ---
        static void StartTcpListener(CancellationToken token)
        {
            try
            {
                _tcpServer = new TcpListener(IPAddress.Parse(DoipInfo.IP), DoipInfo.TCPPort);
                _tcpServer.Start();
                LogHelper.Write("[TCP] 等待诊断仪连接...");

                while (!token.IsCancellationRequested)
                {
                    TcpClient? client = null;
                    try
                    {
                        client = _tcpServer.AcceptTcpClient();
                    }
                    catch (SocketException)
                    {
                        break; // 服务端已停止
                    }

                    if (client == null) continue;

                    // 注册客户端，以便停止时关闭
                    lock (_activeClients) { _activeClients.Add(client); }

                    bool shouldBreak = false;
                    try
                    {
                        client.ReceiveTimeout = 50000;
                        client.SendTimeout = 50000;
                        LogHelper.Write($"[conn] 新连接来自: {client.Client.RemoteEndPoint}");

                        using (NetworkStream stream = client.GetStream())
                        {
                            while (client.Connected && !token.IsCancellationRequested)
                            {
                                byte[]? header = ReadExact(stream, 6);
                                if (header == null)
                                {
                                    LogHelper.Write($"[recv] DoIP Header接收失败,连接断开");
                                    break;
                                }

                                ushort payloadType = (ushort)((header[4] << 8) | header[5]);
                                int payloadLength = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];

                                byte[]? payload = ReadExact(stream, payloadLength);
                                if (payload == null)
                                {
                                    LogHelper.Write($"[recv] DoIP Payload接收失败,连接断开");
                                    break;
                                }

                                byte[] fullMessage = new byte[header.Length + payload.Length];
                                Array.Copy(header, 0, fullMessage, 0, header.Length);
                                Array.Copy(payload, 0, fullMessage, header.Length, payload.Length);

                                LogHelper.Write($"[recv]", fullMessage);
                                DOIP.RaiseDataReceived(fullMessage);

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
                                            catch (Exception ex)
                                            {
                                                LogHelper.Write($"[send] 发送失败,客户端异常断开:{ex.Message}");
                                                shouldBreak = true;
                                                break;
                                            }
                                        }
                                        if (shouldBreak) break;

                                        // 数据文件匹配到但没有UDS诊断响应(type=0x0001)时，UDS自动回复补上
                                        if (payloadType == 0x0001 && _udsAutoReplyEnabled)
                                        {
                                            bool hasUdsResponse = dataGroup.ResponseData.Any(
                                                r => r.Length >= 6 && r[4] == 0x00 && r[5] == 0x01);
                                            if (!hasUdsResponse)
                                            {
                                                SendUdsAutoReply(stream, payload);
                                            }
                                        }
                                    }
                                }
                                else if (payloadType == 0x0001 && _udsAutoReplyEnabled)
                                {
                                    SendDoipACK(stream, payload);
                                    SendUdsAutoReply(stream, payload);
                                }
                                else if (_autoReplyEnabled)
                                {
                                    SendDoipACK(stream, payload);
                                }

                                LogHelper.WritePlainText(Environment.NewLine);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        LogHelper.Write($"[conn] 客户端异常断开: {ex.Message}");
                    }
                    catch (SocketException ex)
                    {
                        if (token.IsCancellationRequested) break;
                        LogHelper.Write($"[conn] 连接异常: {ex.Message}");
                    }
                    finally
                    {
                        LogHelper.Write($"[conn] 客户端关闭连接");
                        LogHelper.WritePlainText($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
                        try { client.Close(); } catch { /* 忽略 */ }
                        lock (_activeClients) { _activeClients.Remove(client); }
                    }
                }
            }
            catch (SocketException)
            {
                if (token.IsCancellationRequested)
                    LogHelper.Write("[TCP] 服务正常停止");
            }
            catch (Exception ex)
            {
                LogHelper.Write($"[TCP] 监听错误: {ex.Message}");
            }
            finally
            {
                _tcpServer?.Stop();
                _tcpServer = null;
            }
        }

        static bool ProcessRoutingActivation(NetworkStream stream, byte[] payload)
        {
            Console.WriteLine("[DoIP] 收到路由激活请求。");

            byte[] response = new byte[payload.Length + 6];
            response[0] = (byte)(payload.Length >> 24);
            response[1] = (byte)(payload.Length >> 16);
            response[2] = (byte)(payload.Length >> 8);
            response[3] = (byte)(payload.Length >> 0);
            response[4] = 0x00;
            response[5] = 0x02;

            Array.Copy(payload, 0, response, 6, payload.Length);

            stream.Write(response, 0, response.Length);
            DOIP.RaiseDataSent(response);
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
            DOIP.RaiseAutoReplySent(response);
        }

        /// <summary>
        /// UDS诊断消息自动回复：解析SID并生成肯定应答
        /// Payload结构: [source 1B] [target 1B] [UDS数据...]
        /// </summary>
        static void SendUdsAutoReply(NetworkStream stream, byte[] payload)
        {
            if (payload.Length < 3)
            {
                // 载荷太短，无法解析UDS，回退到ACK
                SendDoipACK(stream, payload);
                return;
            }

            const int udsOffset = 2; // 跳过source和target地址
            int udsLen = payload.Length - udsOffset;
            byte[] udsData = new byte[udsLen];
            Array.Copy(payload, udsOffset, udsData, 0, udsLen);

            byte[]? udsResponse = BuildUdsResponse(udsData);
            if (udsResponse == null)
            {
                SendDoipACK(stream, payload);
                return;
            }

            // 构造诊断响应载荷: [source 1B] [target 1B] [UDS响应]
            // 交换源/目标地址
            byte[] responsePayload = new byte[2 + udsResponse.Length];
            responsePayload[0] = payload[1]; // 新source = 旧target
            responsePayload[1] = payload[0]; // 新target = 旧source
            Array.Copy(udsResponse, 0, responsePayload, 2, udsResponse.Length);

            // 构造DoIP头 (6字节: 4B长度 + 2B类型)
            int totalLen = responsePayload.Length;
            byte[] response = new byte[6 + totalLen];
            response[0] = (byte)(totalLen >> 24);
            response[1] = (byte)(totalLen >> 16);
            response[2] = (byte)(totalLen >> 8);
            response[3] = (byte)(totalLen);
            response[4] = 0x00;
            response[5] = 0x01; // 诊断消息类型

            Array.Copy(responsePayload, 0, response, 6, totalLen);

            stream.Write(response, 0, response.Length);
            LogHelper.Write($"[send]", response);
            DOIP.RaiseUdsAutoReplySent(response);
        }

        /// <summary>
        /// 根据UDS SID生成肯定应答（全部返回正响应）
        /// </summary>
        static byte[]? BuildUdsResponse(byte[] udsData)
        {
            if (udsData.Length == 0)
                return null;

            byte sid = udsData[0];
            byte[] reqData = new byte[udsData.Length - 1];
            if (reqData.Length > 0)
                Array.Copy(udsData, 1, reqData, 0, reqData.Length);

            switch (sid)
            {
                case 0x22:
                    // DID=F1 90 → VIN特殊处理
                    if (reqData.Length >= 2 && reqData[0] == 0xF1 && reqData[1] == 0x90)
                    {
                        byte[] vinBytes = Encoding.ASCII.GetBytes(DoipInfo.VIN);
                        byte[] resp = new byte[3 + vinBytes.Length];
                        resp[0] = 0x62;
                        resp[1] = 0xF1;
                        resp[2] = 0x90;
                        Array.Copy(vinBytes, 0, resp, 3, vinBytes.Length);
                        return resp;
                    }
                    return BuildPositiveResponse(0x22, reqData, 6);
                case 0x2E:
                    return BuildPositiveResponse(0x2E, reqData, 3);
                case 0x31:
                    return BuildPositiveResponse(0x31, reqData, 3);
                case 0x27:
                    return BuildPositiveResponse(0x27, reqData, 6);
                default:
                    return BuildPositiveResponse(sid, reqData, 6);
            }
        }

        /// <summary>
        /// 通用肯定应答构造：SID+0x40 + 请求数据 + 不足用0x31起填充
        /// </summary>
        static byte[] BuildPositiveResponse(byte sid, byte[] reqData, int targetSize)
        {
            byte[] resp = new byte[targetSize];
            resp[0] = (byte)(sid + 0x40);
            int copyLen = Math.Min(reqData.Length, targetSize - 1);
            Array.Copy(reqData, 0, resp, 1, copyLen);
            byte pad = 0x31;
            for (int i = 1 + copyLen; i < targetSize; i++)
                resp[i] = pad++;
            return resp;
        }

        static void SendDoipMessage(NetworkStream stream, byte[]? packet)
        {
            if (packet != null && packet.Length > 0)
            {
                stream.Write(packet, 0, packet.Length);
                LogHelper.Write($"[send]", packet);
                DOIP.RaiseDataSent(packet);
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

}
