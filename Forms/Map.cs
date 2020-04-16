using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker
{
    public partial class Map : Form
    {
        public Map()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Map));
            this.clockTown = new System.Windows.Forms.Button();
            this.locations = new System.Windows.Forms.CheckBox();
            this.entrances = new System.Windows.Forms.CheckBox();
            this.checkedLocations = new System.Windows.Forms.CheckBox();
            this.Termina = new System.Windows.Forms.Button();
            this.Ranch = new System.Windows.Forms.Button();
            this.Coast = new System.Windows.Forms.Button();
            this.GreatBay = new System.Windows.Forms.Button();
            this.Mountain = new System.Windows.Forms.Button();
            this.Snowhead = new System.Windows.Forms.Button();
            this.StoneTower = new System.Windows.Forms.Button();
            this.Ikana = new System.Windows.Forms.Button();
            this.Swamp = new System.Windows.Forms.Button();
            this.Woodfall = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clockTown
            // 
            this.clockTown.BackColor = System.Drawing.Color.Transparent;
            this.clockTown.FlatAppearance.BorderSize = 0;
            this.clockTown.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.clockTown.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.clockTown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clockTown.ForeColor = System.Drawing.SystemColors.ControlText;
            this.clockTown.Location = new System.Drawing.Point(309, 147);
            this.clockTown.Name = "clockTown";
            this.clockTown.Size = new System.Drawing.Size(32, 50);
            this.clockTown.TabIndex = 0;
            this.clockTown.UseVisualStyleBackColor = false;
            // 
            // locations
            // 
            this.locations.AutoSize = true;
            this.locations.BackColor = System.Drawing.Color.Transparent;
            this.locations.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.locations.Location = new System.Drawing.Point(12, -3);
            this.locations.Name = "locations";
            this.locations.Size = new System.Drawing.Size(72, 17);
            this.locations.TabIndex = 13;
            this.locations.Text = "Locations";
            this.locations.UseVisualStyleBackColor = false;
            // 
            // entrances
            // 
            this.entrances.AutoSize = true;
            this.entrances.BackColor = System.Drawing.Color.Transparent;
            this.entrances.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.entrances.Location = new System.Drawing.Point(112, -3);
            this.entrances.Name = "entrances";
            this.entrances.Size = new System.Drawing.Size(74, 17);
            this.entrances.TabIndex = 14;
            this.entrances.Text = "Entrances";
            this.entrances.UseVisualStyleBackColor = false;
            // 
            // checkedLocations
            // 
            this.checkedLocations.AutoSize = true;
            this.checkedLocations.BackColor = System.Drawing.Color.Transparent;
            this.checkedLocations.ForeColor = System.Drawing.Color.White;
            this.checkedLocations.Location = new System.Drawing.Point(217, -3);
            this.checkedLocations.Name = "checkedLocations";
            this.checkedLocations.Size = new System.Drawing.Size(69, 17);
            this.checkedLocations.TabIndex = 15;
            this.checkedLocations.Text = "Checked";
            this.checkedLocations.UseVisualStyleBackColor = false;
            // 
            // Termina
            // 
            this.Termina.BackColor = System.Drawing.Color.Transparent;
            this.Termina.FlatAppearance.BorderSize = 0;
            this.Termina.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Termina.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Termina.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Termina.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Termina.Location = new System.Drawing.Point(232, 132);
            this.Termina.Name = "Termina";
            this.Termina.Size = new System.Drawing.Size(169, 87);
            this.Termina.TabIndex = 16;
            this.Termina.UseVisualStyleBackColor = false;
            // 
            // Ranch
            // 
            this.Ranch.BackColor = System.Drawing.Color.Transparent;
            this.Ranch.FlatAppearance.BorderSize = 0;
            this.Ranch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Ranch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Ranch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Ranch.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Ranch.Location = new System.Drawing.Point(232, 195);
            this.Ranch.Name = "Ranch";
            this.Ranch.Size = new System.Drawing.Size(80, 48);
            this.Ranch.TabIndex = 17;
            this.Ranch.UseVisualStyleBackColor = false;
            // 
            // Coast
            // 
            this.Coast.BackColor = System.Drawing.Color.Transparent;
            this.Coast.FlatAppearance.BorderSize = 0;
            this.Coast.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Coast.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Coast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Coast.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Coast.Location = new System.Drawing.Point(12, 147);
            this.Coast.Name = "Coast";
            this.Coast.Size = new System.Drawing.Size(214, 132);
            this.Coast.TabIndex = 18;
            this.Coast.UseVisualStyleBackColor = false;
            // 
            // GreatBay
            // 
            this.GreatBay.BackColor = System.Drawing.Color.Transparent;
            this.GreatBay.FlatAppearance.BorderSize = 0;
            this.GreatBay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.GreatBay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.GreatBay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GreatBay.ForeColor = System.Drawing.SystemColors.ControlText;
            this.GreatBay.Location = new System.Drawing.Point(22, 54);
            this.GreatBay.Name = "GreatBay";
            this.GreatBay.Size = new System.Drawing.Size(102, 87);
            this.GreatBay.TabIndex = 19;
            this.GreatBay.UseVisualStyleBackColor = false;
            // 
            // Mountain
            // 
            this.Mountain.BackColor = System.Drawing.Color.Transparent;
            this.Mountain.FlatAppearance.BorderSize = 0;
            this.Mountain.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Mountain.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Mountain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Mountain.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Mountain.Location = new System.Drawing.Point(297, 12);
            this.Mountain.Name = "Mountain";
            this.Mountain.Size = new System.Drawing.Size(255, 114);
            this.Mountain.TabIndex = 20;
            this.Mountain.UseVisualStyleBackColor = false;
            // 
            // Snowhead
            // 
            this.Snowhead.BackColor = System.Drawing.Color.Transparent;
            this.Snowhead.FlatAppearance.BorderSize = 0;
            this.Snowhead.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Snowhead.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Snowhead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Snowhead.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Snowhead.Location = new System.Drawing.Point(232, 12);
            this.Snowhead.Name = "Snowhead";
            this.Snowhead.Size = new System.Drawing.Size(131, 66);
            this.Snowhead.TabIndex = 21;
            this.Snowhead.UseVisualStyleBackColor = false;
            // 
            // StoneTower
            // 
            this.StoneTower.BackColor = System.Drawing.Color.Transparent;
            this.StoneTower.FlatAppearance.BorderSize = 0;
            this.StoneTower.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.StoneTower.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.StoneTower.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StoneTower.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StoneTower.Location = new System.Drawing.Point(465, 78);
            this.StoneTower.Name = "StoneTower";
            this.StoneTower.Size = new System.Drawing.Size(125, 72);
            this.StoneTower.TabIndex = 22;
            this.StoneTower.UseVisualStyleBackColor = false;
            // 
            // Ikana
            // 
            this.Ikana.BackColor = System.Drawing.Color.Transparent;
            this.Ikana.FlatAppearance.BorderSize = 0;
            this.Ikana.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Ikana.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Ikana.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Ikana.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Ikana.Location = new System.Drawing.Point(420, 147);
            this.Ikana.Name = "Ikana";
            this.Ikana.Size = new System.Drawing.Size(92, 69);
            this.Ikana.TabIndex = 23;
            this.Ikana.UseVisualStyleBackColor = false;
            // 
            // Swamp
            // 
            this.Swamp.BackColor = System.Drawing.Color.Transparent;
            this.Swamp.FlatAppearance.BorderSize = 0;
            this.Swamp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Swamp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Swamp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Swamp.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Swamp.Location = new System.Drawing.Point(267, 216);
            this.Swamp.Name = "Swamp";
            this.Swamp.Size = new System.Drawing.Size(147, 116);
            this.Swamp.TabIndex = 24;
            this.Swamp.UseVisualStyleBackColor = false;
            // 
            // Woodfall
            // 
            this.Woodfall.BackColor = System.Drawing.Color.Transparent;
            this.Woodfall.FlatAppearance.BorderSize = 0;
            this.Woodfall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Woodfall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Woodfall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Woodfall.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Woodfall.Location = new System.Drawing.Point(318, 225);
            this.Woodfall.Name = "Woodfall";
            this.Woodfall.Size = new System.Drawing.Size(45, 37);
            this.Woodfall.TabIndex = 25;
            this.Woodfall.UseVisualStyleBackColor = false;
            // 
            // Map
            // 
            this.AccessibleDescription = "";
            this.AccessibleName = "";
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::MMR_Tracker.Properties.Resources.Termina;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(614, 344);
            this.Controls.Add(this.Woodfall);
            this.Controls.Add(this.Ranch);
            this.Controls.Add(this.Swamp);
            this.Controls.Add(this.Ikana);
            this.Controls.Add(this.StoneTower);
            this.Controls.Add(this.Snowhead);
            this.Controls.Add(this.Mountain);
            this.Controls.Add(this.GreatBay);
            this.Controls.Add(this.Coast);
            this.Controls.Add(this.checkedLocations);
            this.Controls.Add(this.entrances);
            this.Controls.Add(this.locations);
            this.Controls.Add(this.clockTown);
            this.Controls.Add(this.Termina);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Map";
            this.Text = "Filter Map";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
            //public void clockTown_Click(object sender, EventArgs e)
            //{
            //    if ((ModifierKeys & Keys.Control) != Keys.Control)
            //    {
            //        if (locations.Checked == true)
            //        { MainInterface.TXTLocSearch.text = "#NorthClockTown,#SouthClockTown,#EastClockTown,#WestClockTown"; }
            //        if (entrances.Checked == true)
            //        { MainInterface.TXTEntSearch.text = "#NorthClockTown,#SouthClockTown,#EastClockTown,#WestClockTown"; }
            //        if (checkedLocations.Checked == true)
            //        { MainInterface.TXTCheckedSearch.text = "#NorthClockTown,#SouthClockTown,#EastClockTown,#WestClockTown"; }
            //    }
            //    else
            //    {
            //        if (locations.Checked == true)
            //        { MainInterface.TXTLocSearch.text = MainInterface.TXTLocSearch.text + ",#NorthClockTown,#SouthClockTown,#EastClockTown,#WestClockTown"; }
            //        if (entrances.Checked == true)
            //        { MainInterface.TXTEntSearch.text = MainInterface.TXTEntSearch.text + ",#NorthClockTown,#SouthClockTown,#EastClockTown,#WestClockTown"; }
            //        if (checkedLocations.Checked == true)
            //        { MainInterface.TXTCheckedSearch.text = MainInterface.TXTCheckedSearch.text + ",#NorthClockTown,#SouthClockTown,#EastClockTown,#WestClockTown"; }

            //    }
            //}
        }
}
