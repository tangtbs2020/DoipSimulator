using System.Text;

namespace DOIPUtils
{
    public class DOIP
    {
        //static string foundVehicleIp = string.Empty;            // 发现的车辆 IP

         public class Information
        {
            public string IP { get; set; } = string.Empty;
            public string VIN { get; set; } = "LBV8A9406GMF25307";
            public string MAC { get; set; } = string.Empty;
            public int UDPPort { get; set; } = 6811;
            public int TCPPort { get; set; } = 6801;



        }

        public static void SetEthernetData(List<DataGroup> dataGroups)
        {
            DoIPServer.SetEthernetData(dataGroups);
        }



        public static void StartDoipServer(Information info)
        {
            //var listIP = NetworkHelper.GetAllLocalIPv4();
            //string? strIP = listIP.FirstOrDefault();
            //var listIP2 = listIP.Where(x => x.StartsWith("169.254."));
            //if(listIP2.Any())
                //strIP = listIP2.First();
            //if(!string.IsNullOrEmpty(strIP))
                DoIPServer.StartServer(info);
            //else
            //{
                 //LogHelper.Write("未发现有效的IP地址，DoIPServer启动失败");
            //}
               
        }

        //static void Main(string[] args)
        //{

        //    EthernetData = EthernetDataParser.ParseFile(@"D:\EcuSimulator\EcuSimulator\DataDB\工单\317212\ethernet.txt");
        //    var list = NetworkHelper.GetAllLocalIPv4();
        //    //Console.Write(list);

        //    //doipTestClient();
        //    string? strAddr = list.FirstOrDefault();
        //    if (!string.IsNullOrEmpty(strAddr))
        //    {
        //        StartDoipServer(strAddr, 6801, 6811);
        //    }

        //}



        //// --- 2. 路由激活 ---
        //static bool SendRoutingActivation(NetworkStream stream)
        //{
        //    byte[] header = new byte[16];
        //    header[0] = 0x02; header[1] = 0xFD;
        //    header[2] = 0x00; header[3] = 0x05; // Routing Activation Request
        //    header[7] = 0x07; // Length
        //    header[8] = 0x0E; header[9] = 0x00; // Source Address (Tester)
        //    header[10] = 0x00; // Activation Type (Default)

        //    stream.Write(header, 0, header.Length);

        //    // 读取响应
        //    byte[]? response = ReadExact(stream, 16);
        //    if (response != null)
        //    {
        //        ushort pType = (ushort)((response[2] << 8) | response[3]);
        //        if (pType == 0x0200) // Routing Activation Response (Success)
        //        {
        //            Console.WriteLine("路由激活成功。");
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //// --- 3. 发送 UDS 消息 ---
        //static void SendUdsMessage(NetworkStream stream, ushort srcAddr, ushort tgtAddr, byte[] udsData)
        //{
        //    int payloadLen = 4 + udsData.Length;
        //    byte[] header = new byte[8];
        //    header[0] = 0x02; header[1] = 0xFD;
        //    header[2] = 0x80; header[3] = 0x01; // Diagnostic Message
        //    // Length (Big Endian)
        //    header[4] = (byte)(payloadLen >> 24);
        //    header[5] = (byte)(payloadLen >> 16);
        //    header[6] = (byte)(payloadLen >> 8);
        //    header[7] = (byte)(payloadLen);

        //    // 组合包
        //    byte[] packet = new byte[8 + payloadLen];
        //    Array.Copy(header, packet, 8);

        //    // Address Info
        //    packet[8] = (byte)(tgtAddr >> 8); packet[9] = (byte)(tgtAddr);
        //    packet[10] = (byte)(srcAddr >> 8); packet[11] = (byte)(srcAddr);

        //    // UDS Data
        //    Array.Copy(udsData, 0, packet, 12, udsData.Length);

        //    stream.Write(packet, 0, packet.Length);
        //}

        //// --- 4. 接收 UDS 响应 ---
        //static void ReceiveUdsResponse(NetworkStream stream)
        //{
        //    // 简单读取 8 字节头
        //    byte[]? header = ReadExact(stream, 8);
        //    if (header != null)
        //    {
        //        // 读取长度
        //        int len = (header[4] << 24) | (header[5] << 16) | (header[6] << 8) | header[7];
        //        byte[]? payload = ReadExact(stream, len);

        //        if (payload != null)
        //        {
        //            // 打印 UDS 数据 (跳过 4 字节地址信息)
        //            byte[] udsBytes = new byte[len - 4];
        //            Array.Copy(payload, 4, udsBytes, 0, len - 4);
        //            Console.WriteLine($"收到 UDS 数据: {BitConverter.ToString(udsBytes)}");
        //        }
        //    }
        //}

        //// 辅助：精确读取指定字节数
        //static byte[]? ReadExact(NetworkStream stream, int length)
        //{
        //    byte[] buffer = new byte[length];
        //    int offset = 0;
        //    while (offset < length)
        //    {
        //        int read = stream.Read(buffer, offset, length - offset);
        //        if (read == 0) return null; // 断开
        //        offset += read;
        //    }
        //    return buffer;
        //}
    }
}