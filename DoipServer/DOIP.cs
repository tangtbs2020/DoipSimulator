using System.Text;

namespace DOIPUtils
{
    public class DOIP
    {
        public class Information
        {
            public string IP { get; set; } = string.Empty;
            public string VIN { get; set; } = "LBV8A9406GMF25307";
            public string MAC { get; set; } = string.Empty;
            public int UDPPort { get; set; } = 6811;
            public int TCPPort { get; set; } = 6801;
        }

        public static event Action<byte[]>? OnDataReceived;
        public static event Action<byte[]>? OnDataSent;
        public static event Action<byte[]>? OnAutoReplySent;
        public static event Action<byte[]>? OnUdsAutoReplySent;

        internal static void RaiseDataReceived(byte[] data) => OnDataReceived?.Invoke(data);
        internal static void RaiseDataSent(byte[] data) => OnDataSent?.Invoke(data);
        internal static void RaiseAutoReplySent(byte[] data) => OnAutoReplySent?.Invoke(data);
        internal static void RaiseUdsAutoReplySent(byte[] data) => OnUdsAutoReplySent?.Invoke(data);

        public static void SetEthernetData(List<DataGroup> dataGroups)
        {
            DoIPServer.SetEthernetData(dataGroups);
        }

        public static void StartDoipServer(Information info)
        {
            DoIPServer.StartServer(info);
        }

        public static void StopDoipServer()
        {
            DoIPServer.StopServer();
        }


        public static void SetAutoReply(bool enabled)
        {
            DoIPServer.SetAutoReply(enabled);
        }

        public static void SetUdsAutoReply(bool enabled)
        {
            DoIPServer.SetUdsAutoReply(enabled);
        }
    }
}
