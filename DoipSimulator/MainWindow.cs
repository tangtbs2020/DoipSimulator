using DOIPUtils;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DoipSimulator
{
    public partial class MainWindow : Form
    {
        private bool _serverRunning = false;
        private bool _logHidden = false;
        private string _dataDbPath = "";

        public MainWindow()
        {
            InitializeComponent();
            init();
            FormClosing += MainWindow_FormClosing;
            DOIP.OnDataReceived += OnDataReceived;
            DOIP.OnDataSent += OnDataSent;
            DOIP.OnAutoReplySent += OnAutoReplySent;
            DOIP.OnUdsAutoReplySent += OnUdsAutoReplySent;

            // 初始状态：UDS默认开启，ACK随之强制开启并禁用
            checkBoxAutoReply.Enabled = false;
            treeViewFiles.AfterSelect += TreeViewFiles_AfterSelect;
        }

        void init()
        {
            var config = AppConfig.Load();
            if (string.IsNullOrEmpty(config.DataDB))
            {
                MessageBox.Show("配置文件中未设置 DataDB 路径，请在 config.json 中配置 DataDB 字段。",
                    "配置缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _dataDbPath = config.DataDB;
            if (!Path.IsPathRooted(_dataDbPath))
                _dataDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dataDbPath);

            if (!Directory.Exists(_dataDbPath))
            {
                MessageBox.Show($"DataDB 目录不存在:\n{_dataDbPath}\n\n请检查 config.json 中的 DataDB 配置。",
                    "路径无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadDirectory(_dataDbPath);
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

        private void buttonConnect_Click(object? sender, EventArgs e)
        {
            if (_serverRunning)
            {
                // 断开连接
                DOIP.StopDoipServer();
                _serverRunning = false;
                SetControlsEnabled(true);
                buttonConnect.Text = "连接";
                AppendStatus("服务已停止。");
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

            var listData = EthernetDataParser.ParseFile(strPath, checkBoxLengthHeader.Checked);
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

            // 每次连接时重新加载自动回复配置
            var autoReplyCfg = AppConfig.BuildAutoReplyConfig();
            if (autoReplyCfg != null)
            {
                DOIP.SetAutoReplyConfig(autoReplyCfg.Value.general, autoReplyCfg.Value.special);
            }

            DOIP.StartDoipServer(info);
            _serverRunning = true;
            SetControlsEnabled(false);
            buttonConnect.Text = "断开";
            AppendStatus($"服务已启动 → {IP}:{info.TCPPort} (TCP) / {info.UDPPort} (UDP)");
            AppendStatus($"数据文件: {Path.GetFileName(strPath)}");
        }

        // 连接后禁用所有控件，仅保留连接按钮和日志面板可用
        private void SetControlsEnabled(bool enabled)
        {
            treeViewFiles.Enabled = enabled;
            ComboBoxIP.Enabled = enabled;
            buttonUpdateIP.Enabled = enabled;
            textBoxUdpPort.Enabled = enabled;
            textBoxTcpPort.Enabled = enabled;
            textBoxVIN.Enabled = enabled;
            textBoxMAC.Enabled = enabled;
            // ACK复选框在UDS自动回复开启时强制禁用
            checkBoxAutoReply.Enabled = enabled && !checkBoxUdsAutoReply.Checked;
            checkBoxUdsAutoReply.Enabled = enabled;
            checkBoxLengthHeader.Enabled = enabled;
        }

        private void MainWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_serverRunning)
            {
                AppendStatus("正在停止服务...");
                DOIP.StopDoipServer();
            }
        }

        private void checkBoxUdsAutoReply_CheckedChanged(object? sender, EventArgs e)
        {
            bool enabled = checkBoxUdsAutoReply.Checked;
            DOIP.SetUdsAutoReply(enabled);

            // UDS自动回复开启时，强制开启ACK自动回复并禁用其复选框
            if (enabled)
            {
                checkBoxAutoReply.Checked = true;
                checkBoxAutoReply.Enabled = false;
            }
            else
            {
                checkBoxAutoReply.Enabled = true;
            }
        }
        private void checkBoxAutoReply_CheckedChanged(object? sender, EventArgs e)
        {
            DOIP.SetAutoReply(checkBoxAutoReply.Checked);
        }

        private void buttonClear_Click(object? sender, EventArgs e)
        {
            richTextBoxContent.Clear();
        }

        private void TreeViewFiles_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            var parts = new List<string>();
            TreeNode? node = e.Node;
            while (node != null)
            {
                parts.Add(node.Text);
                node = node.Parent;
            }
            parts.Reverse();
            labelFilePath.Text = string.Join(" / ", parts);
        }

        private void toolStripMenuItemRefresh_Click(object? sender, EventArgs e)
        {
            LoadDirectory(_dataDbPath);
        }

        private void buttonHide_Click(object? sender, EventArgs e)
        {
            _logHidden = !_logHidden;
            buttonHide.Text = _logHidden ? "显示" : "隐藏";
        }

        private void buttonUpdateIP_Click(object? sender, EventArgs e)
        {
            initIPList();
        }

        private void OnDataReceived(byte[] data)
        {
            if (_logHidden) return;
            if (InvokeRequired)
            {
                Invoke(() => OnDataReceived(data));
                return;
            }
            string hex = string.Join(" ", data.Select(b => b.ToString("X2")));
            AppendColoredText($"Req: {hex}{Environment.NewLine}", Color.Blue);
        }

        private void OnDataSent(byte[] data)
        {
            if (_logHidden) return;
            if (InvokeRequired)
            {
                Invoke(() => OnDataSent(data));
                return;
            }
            string hex = string.Join(" ", data.Select(b => b.ToString("X2")));
            AppendColoredText($"Ans: {hex}{Environment.NewLine}", Color.DarkGreen);
        }

        private void OnAutoReplySent(byte[] data)
        {
            if (_logHidden) return;
            if (InvokeRequired)
            {
                Invoke(() => OnAutoReplySent(data));
                return;
            }
            string hex = string.Join(" ", data.Select(b => b.ToString("X2")));
            AppendColoredText($"自动: {hex}{Environment.NewLine}", Color.DarkOrange);
        }

        private void OnUdsAutoReplySent(byte[] data)
        {
            if (_logHidden) return;
            if (InvokeRequired)
            {
                Invoke(() => OnUdsAutoReplySent(data));
                return;
            }
            string hex = string.Join(" ", data.Select(b => b.ToString("X2")));
            AppendColoredText($"自动: {hex}{Environment.NewLine}", Color.DodgerBlue);
        }

        private void AppendColoredText(string text, Color color)
        {
            richTextBoxContent.SelectionStart = richTextBoxContent.TextLength;
            richTextBoxContent.SelectionLength = 0;
            richTextBoxContent.SelectionColor = color;
            richTextBoxContent.AppendText(text);
            richTextBoxContent.SelectionColor = richTextBoxContent.ForeColor;
        }

        private void AppendStatus(string msg)
        {
            if (_logHidden) return;
            if (InvokeRequired)
            {
                Invoke(() => AppendStatus(msg));
                return;
            }
            richTextBoxContent.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }
    }
}
