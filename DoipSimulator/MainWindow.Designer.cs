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
            richTextBoxContent = new RichTextBox();
            treeViewFiles = new TreeView();
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
            treeViewFiles.Location = new Point(582, 12);
            treeViewFiles.Name = "treeViewFiles";
            treeViewFiles.Size = new Size(323, 463);
            treeViewFiles.TabIndex = 2;
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(582, 485);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(113, 23);
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
            buttonUpdateIP.Size = new Size(185, 25);
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
            groupBox1.Location = new Point(3, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(573, 165);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "设置";
            // 
            // textBoxTcpPort
            // 
            textBoxTcpPort.Location = new Point(243, 125);
            textBoxTcpPort.MaxLength = 6;
            textBoxTcpPort.Name = "textBoxTcpPort";
            textBoxTcpPort.Size = new Size(141, 23);
            textBoxTcpPort.TabIndex = 6;
            textBoxTcpPort.Text = "6801";
            // 
            // textBoxMAC
            // 
            textBoxMAC.Location = new Point(229, 71);
            textBoxMAC.MaxLength = 12;
            textBoxMAC.Name = "textBoxMAC";
            textBoxMAC.Size = new Size(155, 23);
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
            textBoxUdpPort.Location = new Point(71, 119);
            textBoxUdpPort.MaxLength = 6;
            textBoxUdpPort.Name = "textBoxUdpPort";
            textBoxUdpPort.Size = new Size(95, 23);
            textBoxUdpPort.TabIndex = 6;
            textBoxUdpPort.Text = "6811";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(187, 71);
            label5.Name = "label5";
            label5.Size = new Size(36, 17);
            label5.TabIndex = 5;
            label5.Text = "MAC";
            label5.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(187, 125);
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
            label4.Click += label2_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 119);
            label2.Name = "label2";
            label2.Size = new Size(55, 17);
            label2.TabIndex = 5;
            label2.Text = "udp端口";
            label2.Click += label2_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(910, 520);
            Controls.Add(groupBox1);
            Controls.Add(buttonConnect);
            Controls.Add(treeViewFiles);
            Controls.Add(richTextBoxContent);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainWindow";
            Text = "Doip模拟器";
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
    }
}
