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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.clockTown = new System.Windows.Forms.Button();
            this.termina = new System.Windows.Forms.Button();
            this.romaniRanch = new System.Windows.Forms.Button();
            this.woodfall = new System.Windows.Forms.Button();
            this.dekuPalace = new System.Windows.Forms.Button();
            this.zoraCape = new System.Windows.Forms.Button();
            this.greatBayCoast = new System.Windows.Forms.Button();
            this.snowhead = new System.Windows.Forms.Button();
            this.mountainVillage = new System.Windows.Forms.Button();
            this.goronShrine = new System.Windows.Forms.Button();
            this.ikana = new System.Windows.Forms.Button();
            this.stoneTower = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // clockTown
            // 
            this.clockTown.BackColor = System.Drawing.Color.Transparent;
            this.clockTown.FlatAppearance.BorderSize = 0;
            this.clockTown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clockTown.ForeColor = System.Drawing.SystemColors.ControlText;
            this.clockTown.Location = new System.Drawing.Point(309, 147);
            this.clockTown.Name = "clockTown";
            this.clockTown.Size = new System.Drawing.Size(32, 50);
            this.clockTown.TabIndex = 0;
            this.clockTown.UseVisualStyleBackColor = false;
            // 
            // termina
            // 
            this.termina.BackColor = System.Drawing.Color.Transparent;
            this.termina.FlatAppearance.BorderSize = 0;
            this.termina.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.termina.Location = new System.Drawing.Point(283, 137);
            this.termina.Name = "termina";
            this.termina.Size = new System.Drawing.Size(97, 79);
            this.termina.TabIndex = 1;
            this.termina.UseVisualStyleBackColor = false;
            // 
            // romaniRanch
            // 
            this.romaniRanch.BackColor = System.Drawing.Color.Transparent;
            this.romaniRanch.FlatAppearance.BorderSize = 0;
            this.romaniRanch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.romaniRanch.Location = new System.Drawing.Point(217, 189);
            this.romaniRanch.Name = "romaniRanch";
            this.romaniRanch.Size = new System.Drawing.Size(97, 66);
            this.romaniRanch.TabIndex = 2;
            this.romaniRanch.UseVisualStyleBackColor = false;
            this.romaniRanch.Click += new System.EventHandler(this.romaniRanch_Click);
            // 
            // woodfall
            // 
            this.woodfall.BackColor = System.Drawing.Color.Transparent;
            this.woodfall.FlatAppearance.BorderSize = 0;
            this.woodfall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.woodfall.Location = new System.Drawing.Point(320, 231);
            this.woodfall.Name = "woodfall";
            this.woodfall.Size = new System.Drawing.Size(97, 66);
            this.woodfall.TabIndex = 3;
            this.woodfall.UseVisualStyleBackColor = false;
            // 
            // dekuPalace
            // 
            this.dekuPalace.BackColor = System.Drawing.Color.Transparent;
            this.dekuPalace.FlatAppearance.BorderSize = 0;
            this.dekuPalace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dekuPalace.Location = new System.Drawing.Point(264, 294);
            this.dekuPalace.Name = "dekuPalace";
            this.dekuPalace.Size = new System.Drawing.Size(97, 66);
            this.dekuPalace.TabIndex = 4;
            this.dekuPalace.UseVisualStyleBackColor = false;
            // 
            // zoraCape
            // 
            this.zoraCape.BackColor = System.Drawing.Color.Transparent;
            this.zoraCape.FlatAppearance.BorderSize = 0;
            this.zoraCape.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zoraCape.Location = new System.Drawing.Point(26, 166);
            this.zoraCape.Name = "zoraCape";
            this.zoraCape.Size = new System.Drawing.Size(136, 89);
            this.zoraCape.TabIndex = 5;
            this.zoraCape.UseVisualStyleBackColor = false;
            // 
            // greatBayCoast
            // 
            this.greatBayCoast.BackColor = System.Drawing.Color.Transparent;
            this.greatBayCoast.FlatAppearance.BorderSize = 0;
            this.greatBayCoast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.greatBayCoast.Location = new System.Drawing.Point(26, 72);
            this.greatBayCoast.Name = "greatBayCoast";
            this.greatBayCoast.Size = new System.Drawing.Size(136, 93);
            this.greatBayCoast.TabIndex = 6;
            this.greatBayCoast.UseVisualStyleBackColor = false;
            // 
            // snowhead
            // 
            this.snowhead.BackColor = System.Drawing.Color.Transparent;
            this.snowhead.FlatAppearance.BorderSize = 0;
            this.snowhead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.snowhead.Location = new System.Drawing.Point(217, 12);
            this.snowhead.Name = "snowhead";
            this.snowhead.Size = new System.Drawing.Size(144, 64);
            this.snowhead.TabIndex = 7;
            this.snowhead.UseVisualStyleBackColor = false;
            // 
            // mountainVillage
            // 
            this.mountainVillage.BackColor = System.Drawing.Color.Transparent;
            this.mountainVillage.FlatAppearance.BorderSize = 0;
            this.mountainVillage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mountainVillage.Location = new System.Drawing.Point(301, 47);
            this.mountainVillage.Name = "mountainVillage";
            this.mountainVillage.Size = new System.Drawing.Size(136, 94);
            this.mountainVillage.TabIndex = 8;
            this.mountainVillage.UseVisualStyleBackColor = false;
            // 
            // goronShrine
            // 
            this.goronShrine.BackColor = System.Drawing.Color.Transparent;
            this.goronShrine.FlatAppearance.BorderSize = 0;
            this.goronShrine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.goronShrine.Location = new System.Drawing.Point(433, 27);
            this.goronShrine.Name = "goronShrine";
            this.goronShrine.Size = new System.Drawing.Size(130, 58);
            this.goronShrine.TabIndex = 10;
            this.goronShrine.UseVisualStyleBackColor = false;
            // 
            // ikana
            // 
            this.ikana.BackColor = System.Drawing.Color.Transparent;
            this.ikana.FlatAppearance.BorderSize = 0;
            this.ikana.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ikana.Location = new System.Drawing.Point(433, 160);
            this.ikana.Name = "ikana";
            this.ikana.Size = new System.Drawing.Size(85, 56);
            this.ikana.TabIndex = 11;
            this.ikana.UseVisualStyleBackColor = false;
            // 
            // stoneTower
            // 
            this.stoneTower.BackColor = System.Drawing.Color.Transparent;
            this.stoneTower.FlatAppearance.BorderSize = 0;
            this.stoneTower.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stoneTower.Location = new System.Drawing.Point(472, 91);
            this.stoneTower.Name = "stoneTower";
            this.stoneTower.Size = new System.Drawing.Size(136, 60);
            this.stoneTower.TabIndex = 12;
            this.stoneTower.UseVisualStyleBackColor = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.Transparent;
            this.checkBox1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.checkBox1.Location = new System.Drawing.Point(12, 8);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 17);
            this.checkBox1.TabIndex = 13;
            this.checkBox1.Text = "Locations";
            this.checkBox1.UseVisualStyleBackColor = false;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.BackColor = System.Drawing.Color.Transparent;
            this.checkBox2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.checkBox2.Location = new System.Drawing.Point(90, 8);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(74, 17);
            this.checkBox2.TabIndex = 14;
            this.checkBox2.Text = "Entrances";
            this.checkBox2.UseVisualStyleBackColor = false;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.BackColor = System.Drawing.Color.Transparent;
            this.checkBox3.ForeColor = System.Drawing.Color.White;
            this.checkBox3.Location = new System.Drawing.Point(168, 8);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(118, 17);
            this.checkBox3.TabIndex = 15;
            this.checkBox3.Text = "Checked Locations";
            this.checkBox3.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.BackgroundImage = global::MMR_Tracker.Properties.Resources.Termina;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(620, 361);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.snowhead);
            this.Controls.Add(this.mountainVillage);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.stoneTower);
            this.Controls.Add(this.ikana);
            this.Controls.Add(this.goronShrine);
            this.Controls.Add(this.greatBayCoast);
            this.Controls.Add(this.zoraCape);
            this.Controls.Add(this.dekuPalace);
            this.Controls.Add(this.woodfall);
            this.Controls.Add(this.romaniRanch);
            this.Controls.Add(this.clockTown);
            this.Controls.Add(this.termina);
            this.Name = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void romaniRanch_Click(object sender, EventArgs e)
        {

        }
    }
}
