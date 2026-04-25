using DOIPUtils;
using Microsoft.VisualBasic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace DoipSimulator
{
    public partial class MainWindow : Form
    {

        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            LoadDirectory("DataDB");
            initIPList();
        }

        void initIPList()
        {

            ComboBoxIP.Items.Clear();
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
            ipList = ipList.Distinct().ToList(); // 去重
            ComboBoxIP.Items.AddRange(ipList.ToArray());
            if (ipList.Count > 0)
                ComboBoxIP.SelectedIndex = 0;
        }

        private void LoadDirectory(string path)
        {
            treeViewFiles.Nodes.Clear();
            DirectoryInfo dir = new DirectoryInfo(path);
            TreeNode rootNode = new TreeNode(dir.Name); // 根目录名
            rootNode.Tag = dir.FullName;
            FillTreeNode(rootNode, dir);
            treeViewFiles.Nodes.Add(rootNode);
            rootNode.Expand(); // 默认展开
        }


        private void FillTreeNode(TreeNode node, DirectoryInfo dir)
        {
            try
            {
                // 添加文件
                foreach (FileInfo file in dir.GetFiles())
                {
                    var treeNode = new TreeNode(file.Name);
                    treeNode.Tag = file.FullName;
                    node.Nodes.Add(treeNode);
                }

                // 递归添加子文件夹
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeNode subNode = new TreeNode(subDir.Name);
                    subNode.Tag = subDir.FullName;
                    node.Nodes.Add(subNode);
                    FillTreeNode(subNode, subDir); // 递归调用
                }
            }
            catch (Exception)
            {
                // 处理权限不足等异常
                node.Nodes.Add(new TreeNode("Access Denied"));
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if(treeViewFiles.SelectedNode == null)
            {
                MessageBox.Show("请选择一个有效的文件！");
                return;
            }

            var strPath = treeViewFiles.SelectedNode.Tag as string;
            if (strPath == null || !File.Exists(strPath))
            {
                MessageBox.Show("请选择一个有效的文件！");
                return;
            }

            var listData = EthernetDataParser.ParseFile(strPath);
            var info = new DOIP.Information();
            string? IP = ComboBoxIP.SelectedItem as string;
            if(string.IsNullOrEmpty(IP))
            {
                MessageBox.Show("请选择一个有效的IP地址！");
                return;
            }

            info.IP = IP;
            string? port = textBoxUdpPort.Text;
            if (string.IsNullOrEmpty(port) || !int.TryParse(port, out int udpPort)) 
            {
                MessageBox.Show("udp端口配置无效！");
                return;
            }
            info.UDPPort = udpPort;
           
            port = textBoxTcpPort.Text;
            if (string.IsNullOrEmpty(port) || !int.TryParse(port, out int tcpPort))
            {
                MessageBox.Show("tcp端口配置无效！");
                return;
            }
            info.TCPPort = tcpPort;

            string VIN = textBoxVIN.Text;

            if(string.IsNullOrEmpty(textBoxVIN.Text) || textBoxVIN.Text.Length != 17)
            {
                MessageBox.Show("VIN配置无效！");
                return;
            }
            info.VIN = VIN;


            string mac = textBoxMAC.Text;

            if (string.IsNullOrEmpty(mac) || mac.Length != 12)
            {
                MessageBox.Show("MAC配置无效！");
                return;
            }

            info.MAC = mac;

            DOIP.StartDoipServer(info);
        }

        private void buttonUpdateIP_Click(object sender, EventArgs e)
        {
            initIPList();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
