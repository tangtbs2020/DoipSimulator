using DOIPUtils;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DoipSimulator
{
    public partial class MainWindow : Form
    {
        private bool _serverRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            init();
            FormClosing += MainWindow_FormClosing;
        }

        void init()
        {
            LoadDirectory("DataDB");
            initIPList();
            AppendStatus("就绪，请选择数据文件并配置参数后点击「连接」。");
        }

        void initIPList()
        {
            ComboBoxIP.Items.Clear();
            List<string> ipList = new List<string>();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var ipProperties = ni.GetIPProperties();

                    foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipList.Add(ipInfo.Address.ToString());
                        }
                    }
                }
            }
            ipList = ipList.Distinct().ToList();
            ComboBoxIP.Items.AddRange(ipList.ToArray());
            if (ipList.Count > 0)
                ComboBoxIP.SelectedIndex = 0;
        }

        private void LoadDirectory(string path)
        {
            treeViewFiles.Nodes.Clear();
            DirectoryInfo dir = new DirectoryInfo(path);
            TreeNode rootNode = new TreeNode(dir.Name);
            rootNode.Tag = dir.FullName;
            FillTreeNode(rootNode, dir);
            treeViewFiles.Nodes.Add(rootNode);
            rootNode.Expand();
        }

        private void FillTreeNode(TreeNode node, DirectoryInfo dir)
        {
            try
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    var treeNode = new TreeNode(file.Name);
                    treeNode.Tag = file.FullName;
                    node.Nodes.Add(treeNode);
                }

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeNode subNode = new TreeNode(subDir.Name);
                    subNode.Tag = subDir.FullName;
                    node.Nodes.Add(subNode);
                    FillTreeNode(subNode, subDir);
                }
            }
            catch (Exception)
            {
                node.Nodes.Add(new TreeNode("Access Denied"));
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_serverRunning)
            {
                MessageBox.Show("服务已在运行中！");
                return;
            }

            if (treeViewFiles.SelectedNode == null)
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
            DOIP.SetEthernetData(listData);
            var info = new DOIP.Information();
            string? IP = ComboBoxIP.SelectedItem as string;
            if (string.IsNullOrEmpty(IP))
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

            if (string.IsNullOrEmpty(textBoxVIN.Text) || textBoxVIN.Text.Length != 17)
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
            _serverRunning = true;
            buttonConnect.Enabled = false;
            AppendStatus($"服务已启动 → {IP}:{info.TCPPort} (TCP) / {info.UDPPort} (UDP)");
            AppendStatus($"数据文件: {Path.GetFileName(strPath)}");
        }

        private void MainWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_serverRunning)
            {
                AppendStatus("正在停止服务...");
                DOIP.StopDoipServer();
            }
        }

        private void buttonUpdateIP_Click(object sender, EventArgs e)
        {
            initIPList();
            AppendStatus("IP列表已刷新。");
        }

        private void AppendStatus(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendStatus(msg));
                return;
            }
            richTextBoxContent.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }
    }
}
