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
            this.SuspendLayout();
            // 
            // LBIPAdresses
            // 
            this.LBIPAdresses.FormattingEnabled = true;
            this.LBIPAdresses.Location = new System.Drawing.Point(13, 13);
            this.LBIPAdresses.Name = "LBIPAdresses";
            this.LBIPAdresses.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBIPAdresses.Size = new System.Drawing.Size(211, 329);
            this.LBIPAdresses.TabIndex = 0;
            // 
            // btnAddIP
            // 
            this.btnAddIP.Location = new System.Drawing.Point(13, 374);
            this.btnAddIP.Name = "btnAddIP";
            this.btnAddIP.Size = new System.Drawing.Size(103, 23);
            this.btnAddIP.TabIndex = 1;
            this.btnAddIP.Text = "Add IP";
            this.btnAddIP.UseVisualStyleBackColor = true;
            this.btnAddIP.Click += new System.EventHandler(this.btnAddIP_Click_1);
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(13, 348);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(211, 20);
            this.txtIP.TabIndex = 2;
            // 
            // btnRemoveIP
            // 
            this.btnRemoveIP.Location = new System.Drawing.Point(121, 374);
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
            this.chkListenForData.Location = new System.Drawing.Point(13, 407);
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
            this.chkSendData.Location = new System.Drawing.Point(121, 407);
            this.chkSendData.Name = "chkSendData";
            this.chkSendData.Size = new System.Drawing.Size(77, 17);
            this.chkSendData.TabIndex = 5;
            this.chkSendData.Text = "Send Data";
            this.chkSendData.UseVisualStyleBackColor = false;
            this.chkSendData.CheckedChanged += new System.EventHandler(this.chkSendData_CheckedChanged);
            // 
            // OnlinePlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(236, 436);
            this.Controls.Add(this.chkSendData);
            this.Controls.Add(this.chkListenForData);
            this.Controls.Add(this.btnRemoveIP);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.btnAddIP);
            this.Controls.Add(this.LBIPAdresses);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OnlinePlay";
            this.Text = "OnlinePlay";
            this.Load += new System.EventHandler(this.OnlinePlay_Load);
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
    }
}