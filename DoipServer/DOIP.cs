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
    }
}
