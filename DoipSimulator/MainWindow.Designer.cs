namespace DoipSimulator
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            richTextBoxContent = new RichTextBox();
            treeViewFiles = new TreeView();
            contextMenuStrip = new ContextMenuStrip(components);
            toolStripMenuItemRefresh = new ToolStripMenuItem();
            buttonConnect = new Button();
            ComboBoxIP = new ComboBox();
            buttonUpdateIP = new Button();
            label1 = new Label();
            groupBox1 = new GroupBox();
            textBoxTcpPort = new TextBox();
            textBoxMAC = new TextBox();
            textBoxVIN = new TextBox();
            textBoxUdpPort = new TextBox();
            label5 = new Label();
            label3 = new Label();
            label4 = new Label();
            label2 = new Label();
            checkBoxAutoReply = new CheckBox();
            buttonClear = new Button();
            buttonHide = new Button();
            labelFilePath = new Label();
            contextMenuStrip.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBoxContent
            // 
            richTextBoxContent.Location = new Point(3, 183);
            richTextBoxContent.Name = "richTextBoxContent";
            richTextBoxContent.Size = new Size(573, 292);
            richTextBoxContent.TabIndex = 1;
            richTextBoxContent.Text = "";
            // 
            // treeViewFiles
            // 
            treeViewFiles.ContextMenuStrip = contextMenuStrip;
            treeViewFiles.Location = new Point(582, 12);
            treeViewFiles.Name = "treeViewFiles";
            treeViewFiles.Size = new Size(323, 463);
            treeViewFiles.TabIndex = 2;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemRefresh });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(101, 26);
            // 
            // toolStripMenuItemRefresh
            // 
            toolStripMenuItemRefresh.Name = "toolStripMenuItemRefresh";
            toolStripMenuItemRefresh.Size = new Size(100, 22);
            toolStripMenuItemRefresh.Text = "刷新";
            toolStripMenuItemRefresh.Click += toolStripMenuItemRefresh_Click;
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(582, 485);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(75, 23);
            buttonConnect.TabIndex = 3;
            buttonConnect.Text = "连接";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;
            // 
            // ComboBoxIP
            // 
            ComboBoxIP.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxIP.FormattingEnabled = true;
            ComboBoxIP.Location = new Point(53, 20);
            ComboBoxIP.Name = "ComboBoxIP";
            ComboBoxIP.Size = new Size(129, 25);
            ComboBoxIP.TabIndex = 4;
            // 
            // buttonUpdateIP
            // 
            buttonUpdateIP.Location = new Point(199, 20);
            buttonUpdateIP.Name = "buttonUpdateIP";
            buttonUpdateIP.Size = new Size(109, 25);
            buttonUpdateIP.TabIndex = 3;
            buttonUpdateIP.Text = "更新";
            buttonUpdateIP.UseVisualStyleBackColor = true;
            buttonUpdateIP.Click += buttonUpdateIP_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 25);
            label1.Name = "label1";
            label1.Size = new Size(43, 17);
            label1.TabIndex = 5;
            label1.Text = "可用IP";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(textBoxTcpPort);
            groupBox1.Controls.Add(textBoxMAC);
            groupBox1.Controls.Add(textBoxVIN);
            groupBox1.Controls.Add(textBoxUdpPort);
            groupBox1.Controls.Add(buttonUpdateIP);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(ComboBoxIP);
            groupBox1.Controls.Add(checkBoxAutoReply);
            groupBox1.Location = new Point(3, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(573, 165);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "设置";
            // 
            // textBoxTcpPort
            // 
            textBoxTcpPort.Location = new Point(426, 74);
            textBoxTcpPort.MaxLength = 6;
            textBoxTcpPort.Name = "textBoxTcpPort";
            textBoxTcpPort.Size = new Size(141, 23);
            textBoxTcpPort.TabIndex = 6;
            textBoxTcpPort.Text = "6801";
            // 
            // textBoxMAC
            // 
            textBoxMAC.Location = new Point(381, 23);
            textBoxMAC.MaxLength = 12;
            textBoxMAC.Name = "textBoxMAC";
            textBoxMAC.Size = new Size(144, 23);
            textBoxMAC.TabIndex = 6;
            textBoxMAC.Text = "001A3727DF22";
            // 
            // textBoxVIN
            // 
            textBoxVIN.Location = new Point(40, 71);
            textBoxVIN.MaxLength = 17;
            textBoxVIN.Name = "textBoxVIN";
            textBoxVIN.Size = new Size(142, 23);
            textBoxVIN.TabIndex = 6;
            textBoxVIN.Text = "LBV8A9406GMF25307";
            // 
            // textBoxUdpPort
            // 
            textBoxUdpPort.Location = new Point(256, 74);
            textBoxUdpPort.MaxLength = 6;
            textBoxUdpPort.Name = "textBoxUdpPort";
            textBoxUdpPort.Size = new Size(95, 23);
            textBoxUdpPort.TabIndex = 6;
            textBoxUdpPort.Text = "6811";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(339, 23);
            label5.Name = "label5";
            label5.Size = new Size(36, 17);
            label5.TabIndex = 5;
            label5.Text = "MAC";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(370, 74);
            label3.Name = "label3";
            label3.Size = new Size(50, 17);
            label3.TabIndex = 5;
            label3.Text = "tcp端口";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(9, 74);
            label4.Name = "label4";
            label4.Size = new Size(30, 17);
            label4.TabIndex = 5;
            label4.Text = "VIN";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(195, 74);
            label2.Name = "label2";
            label2.Size = new Size(55, 17);
            label2.TabIndex = 5;
            label2.Text = "udp端口";
            // 
            // checkBoxAutoReply
            // 
            checkBoxAutoReply.AutoSize = true;
            checkBoxAutoReply.Location = new Point(9, 120);
            checkBoxAutoReply.Name = "checkBoxAutoReply";
            checkBoxAutoReply.Size = new Size(99, 21);
            checkBoxAutoReply.TabIndex = 7;
            checkBoxAutoReply.Text = "ACK自动回复";
            checkBoxAutoReply.UseVisualStyleBackColor = true;
            checkBoxAutoReply.CheckedChanged += checkBoxAutoReply_CheckedChanged;
            // 
            // buttonClear
            // 
            buttonClear.Location = new Point(663, 485);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(75, 23);
            buttonClear.TabIndex = 8;
            buttonClear.Text = "清除";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += buttonClear_Click;
            // 
            // buttonHide
            // 
            buttonHide.Location = new Point(744, 485);
            buttonHide.Name = "buttonHide";
            buttonHide.Size = new Size(75, 23);
            buttonHide.TabIndex = 9;
            buttonHide.Text = "隐藏";
            buttonHide.UseVisualStyleBackColor = true;
            buttonHide.Click += buttonHide_Click;
            // 
            // labelFilePath
            // 
            labelFilePath.Location = new Point(3, 485);
            labelFilePath.Name = "labelFilePath";
            labelFilePath.Size = new Size(573, 30);
            labelFilePath.TabIndex = 10;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(910, 544);
            Controls.Add(groupBox1);
            Controls.Add(buttonConnect);
            Controls.Add(buttonClear);
            Controls.Add(buttonHide);
            Controls.Add(treeViewFiles);
            Controls.Add(richTextBoxContent);
            Controls.Add(labelFilePath);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainWindow";
            Text = "Doip模拟器";
            contextMenuStrip.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private RichTextBox richTextBoxContent;
        private TreeView treeViewFiles;
        private Button buttonConnect;
        private ComboBox ComboBoxIP;
        private Button buttonUpdateIP;
        private Label label1;
        private GroupBox groupBox1;
        private TextBox textBoxTcpPort;
        private TextBox textBoxUdpPort;
        private Label label3;
        private Label label2;
        private TextBox textBoxVIN;
        private Label label4;
        private TextBox textBoxMAC;
        private Label label5;
        private CheckBox checkBoxAutoReply;
        private Button buttonClear;
        private Button buttonHide;
        private Label labelFilePath;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem toolStripMenuItemRefresh;
    }
}
