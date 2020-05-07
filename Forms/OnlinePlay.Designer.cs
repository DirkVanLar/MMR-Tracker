namespace MMR_Tracker.Forms
{
    partial class OnlinePlay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OnlinePlay));
            this.LBIPAdresses = new System.Windows.Forms.ListBox();
            this.btnAddIP = new System.Windows.Forms.Button();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.btnRemoveIP = new System.Windows.Forms.Button();
            this.chkListenForData = new System.Windows.Forms.CheckBox();
            this.chkSendData = new System.Windows.Forms.CheckBox();
            this.NudPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPulbicIP = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.NudYourPort = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveIPListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadIPListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowFullCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowAutoPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAddIncomingIPsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyNetDataToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.NudPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NudYourPort)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LBIPAdresses
            // 
            this.LBIPAdresses.FormattingEnabled = true;
            this.LBIPAdresses.Location = new System.Drawing.Point(7, 90);
            this.LBIPAdresses.Name = "LBIPAdresses";
            this.LBIPAdresses.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBIPAdresses.Size = new System.Drawing.Size(211, 277);
            this.LBIPAdresses.TabIndex = 0;
            // 
            // btnAddIP
            // 
            this.btnAddIP.Location = new System.Drawing.Point(7, 411);
            this.btnAddIP.Name = "btnAddIP";
            this.btnAddIP.Size = new System.Drawing.Size(103, 23);
            this.btnAddIP.TabIndex = 1;
            this.btnAddIP.Text = "Add IP";
            this.btnAddIP.UseVisualStyleBackColor = true;
            this.btnAddIP.Click += new System.EventHandler(this.btnAddIP_Click_1);
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(7, 385);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(141, 20);
            this.txtIP.TabIndex = 2;
            // 
            // btnRemoveIP
            // 
            this.btnRemoveIP.Location = new System.Drawing.Point(115, 411);
            this.btnRemoveIP.Name = "btnRemoveIP";
            this.btnRemoveIP.Size = new System.Drawing.Size(103, 23);
            this.btnRemoveIP.TabIndex = 3;
            this.btnRemoveIP.Text = "Remove IP";
            this.btnRemoveIP.UseVisualStyleBackColor = true;
            this.btnRemoveIP.Click += new System.EventHandler(this.btnRemoveIP_Click);
            // 
            // chkListenForData
            // 
            this.chkListenForData.AutoSize = true;
            this.chkListenForData.BackColor = System.Drawing.Color.Transparent;
            this.chkListenForData.ForeColor = System.Drawing.SystemColors.Control;
            this.chkListenForData.Location = new System.Drawing.Point(7, 444);
            this.chkListenForData.Name = "chkListenForData";
            this.chkListenForData.Size = new System.Drawing.Size(95, 17);
            this.chkListenForData.TabIndex = 4;
            this.chkListenForData.Text = "Listen for Data";
            this.chkListenForData.UseVisualStyleBackColor = false;
            this.chkListenForData.CheckedChanged += new System.EventHandler(this.chkListenForData_CheckedChanged);
            // 
            // chkSendData
            // 
            this.chkSendData.AutoSize = true;
            this.chkSendData.BackColor = System.Drawing.Color.Transparent;
            this.chkSendData.ForeColor = System.Drawing.SystemColors.Control;
            this.chkSendData.Location = new System.Drawing.Point(115, 444);
            this.chkSendData.Name = "chkSendData";
            this.chkSendData.Size = new System.Drawing.Size(77, 17);
            this.chkSendData.TabIndex = 5;
            this.chkSendData.Text = "Send Data";
            this.chkSendData.UseVisualStyleBackColor = false;
            this.chkSendData.CheckedChanged += new System.EventHandler(this.chkSendData_CheckedChanged);
            // 
            // NudPort
            // 
            this.NudPort.Location = new System.Drawing.Point(154, 385);
            this.NudPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NudPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NudPort.Name = "NudPort";
            this.NudPort.Size = new System.Drawing.Size(63, 20);
            this.NudPort.TabIndex = 6;
            this.NudPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(6, 370);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "IP Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(151, 370);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(3, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Your IP Address:";
            // 
            // txtPulbicIP
            // 
            this.txtPulbicIP.Location = new System.Drawing.Point(6, 51);
            this.txtPulbicIP.Name = "txtPulbicIP";
            this.txtPulbicIP.ReadOnly = true;
            this.txtPulbicIP.Size = new System.Drawing.Size(139, 20);
            this.txtPulbicIP.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(151, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Your Port";
            // 
            // NudYourPort
            // 
            this.NudYourPort.Location = new System.Drawing.Point(154, 51);
            this.NudYourPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NudYourPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NudYourPort.Name = "NudYourPort";
            this.NudYourPort.Size = new System.Drawing.Size(63, 20);
            this.NudYourPort.TabIndex = 11;
            this.NudYourPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NudYourPort.ValueChanged += new System.EventHandler(this.NudYourPort_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(6, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(200, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Sending Data to the following addresses:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(228, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveIPListToolStripMenuItem,
            this.loadIPListToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowFullCheckToolStripMenuItem,
            this.allowAutoPortToolStripMenuItem,
            this.autoAddIncomingIPsToolStripMenuItem,
            this.copyNetDataToClipboardToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // saveIPListToolStripMenuItem
            // 
            this.saveIPListToolStripMenuItem.Name = "saveIPListToolStripMenuItem";
            this.saveIPListToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveIPListToolStripMenuItem.Text = "Save IP List";
            this.saveIPListToolStripMenuItem.Click += new System.EventHandler(this.saveIPListToolStripMenuItem_Click);
            // 
            // loadIPListToolStripMenuItem
            // 
            this.loadIPListToolStripMenuItem.Name = "loadIPListToolStripMenuItem";
            this.loadIPListToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.loadIPListToolStripMenuItem.Text = "Load IP List";
            this.loadIPListToolStripMenuItem.Click += new System.EventHandler(this.loadIPListToolStripMenuItem_Click);
            // 
            // allowFullCheckToolStripMenuItem
            // 
            this.allowFullCheckToolStripMenuItem.Name = "allowFullCheckToolStripMenuItem";
            this.allowFullCheckToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.allowFullCheckToolStripMenuItem.Text = "Allow Full Check";
            this.allowFullCheckToolStripMenuItem.Click += new System.EventHandler(this.allowFullCheckToolStripMenuItem_Click);
            // 
            // allowAutoPortToolStripMenuItem
            // 
            this.allowAutoPortToolStripMenuItem.Name = "allowAutoPortToolStripMenuItem";
            this.allowAutoPortToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.allowAutoPortToolStripMenuItem.Text = "Allow Auto Port";
            this.allowAutoPortToolStripMenuItem.Click += new System.EventHandler(this.allowAutoPortToolStripMenuItem_Click);
            // 
            // autoAddIncomingIPsToolStripMenuItem
            // 
            this.autoAddIncomingIPsToolStripMenuItem.Name = "autoAddIncomingIPsToolStripMenuItem";
            this.autoAddIncomingIPsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.autoAddIncomingIPsToolStripMenuItem.Text = "Auto Add Incoming IPs";
            this.autoAddIncomingIPsToolStripMenuItem.Click += new System.EventHandler(this.autoAddIncomingIPsToolStripMenuItem_Click);
            // 
            // copyNetDataToClipboardToolStripMenuItem
            // 
            this.copyNetDataToClipboardToolStripMenuItem.Name = "copyNetDataToClipboardToolStripMenuItem";
            this.copyNetDataToClipboardToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.copyNetDataToClipboardToolStripMenuItem.Text = "Copy Net Data to Clipboard";
            this.copyNetDataToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyNetDataToClipboardToolStripMenuItem_Click);
            // 
            // OnlinePlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(228, 470);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.NudYourPort);
            this.Controls.Add(this.txtPulbicIP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NudPort);
            this.Controls.Add(this.chkSendData);
            this.Controls.Add(this.chkListenForData);
            this.Controls.Add(this.btnRemoveIP);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.btnAddIP);
            this.Controls.Add(this.LBIPAdresses);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "OnlinePlay";
            this.Text = "OnlinePlay";
            this.Load += new System.EventHandler(this.OnlinePlay_Load);
            ((System.ComponentModel.ISupportInitialize)(this.NudPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NudYourPort)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBIPAdresses;
        private System.Windows.Forms.Button btnAddIP;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Button btnRemoveIP;
        private System.Windows.Forms.CheckBox chkListenForData;
        private System.Windows.Forms.CheckBox chkSendData;
        private System.Windows.Forms.NumericUpDown NudPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPulbicIP;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown NudYourPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveIPListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadIPListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowFullCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowAutoPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAddIncomingIPsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyNetDataToClipboardToolStripMenuItem;
    }
}