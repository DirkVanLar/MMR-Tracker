using MMR_Tracker.Class_Files;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;

namespace MMR_Tracker.Forms.Sub_Forms
{
    public partial class LogicEditorReorder : Form
    {
        public LogicEditorReorder()
        {
            InitializeComponent();
        }

        public List<LogicObjects.LogicEntry> ListContent = new List<LogicObjects.LogicEntry>();
        public List<LogicObjects.LogicEntry> SelectedItems = new List<LogicObjects.LogicEntry>();

        private void button2_Click(object sender, EventArgs e)
        {
            UpDownButtons(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpDownButtons(sender, e);
        }

        private void UpDownButtons(object sender, EventArgs e)
        {
            if (sender == button2) { MoveItem(-1); }
            else if (sender == button1) { MoveItem(1); }
        }

        public void MoveItem(int Direction)
        {
            ListBox LB = listBox1;
            bool up = Direction < 0;
            int Decrementor = Direction * -1;
            if ((Control.ModifierKeys & Keys.Shift) != 0) { Direction *= 5; }
            if ((Control.ModifierKeys & Keys.Control) != 0) { Direction *= 10; }

            bool CanMove = TestMove();

            while (!CanMove)
            {
                Direction += Decrementor;
                if (Direction == 0) { return; }
                CanMove = TestMove();
            }

            if (up) { for (var i = 0; i < LB.SelectedIndices.Count; i++) { MoveItem(i); } }
            else { for (var i = LB.SelectedIndices.Count - 1; i >= 0; i--) { MoveItem(i); } }
            void MoveItem(int ind)
            {
                int newIndex = LB.SelectedIndices[ind] + Direction;
                object selected = LB.SelectedItems[ind];
                LB.Items.Remove(selected);
                LB.Items.Insert(newIndex, selected);
                LB.SetSelected(newIndex, true);
            }

            bool inBounds(int ind)
            {
                if (LB.SelectedItems[ind] == null || LB.SelectedIndices[ind] < 0) { return false; }
                int newIndex = LB.SelectedIndices[ind] + Direction;
                if (newIndex < 0 || newIndex >= LB.Items.Count) { return false; }
                return true;
            }
            bool TestMove()
            {
                if (up) { for (var i = 0; i < LB.SelectedIndices.Count; i++) { if (!inBounds(i)) { return false; } } }
                else { for (var i = LB.SelectedIndices.Count - 1; i >= 0; i--) { if (!inBounds(i)) { return false; } } }
                return true;
            }
        }

        private void LogicEditorReorder_Load(object sender, EventArgs e)
        {
            int lastRealItem = -1;
            foreach (var i in ListContent)
            {
                if (!i.IsFake) { lastRealItem = i.ID; }
            }
            

            listBox1.Items.Clear();
            foreach (var i in ListContent)
            {
                LogicObjects.ListItem ListItem = new LogicObjects.ListItem();
                ListItem.LocationEntry = i;
                ListItem.DisplayName = i.DictionaryName;
                if (i.ID > lastRealItem) { listBox1.Items.Add(ListItem); }
                else { SelectedItems.Add(ListItem.LocationEntry); }

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            foreach (var i in listBox1.Items)
            {
                if (i is LogicObjects.ListItem)
                {
                    var item = i as LogicObjects.ListItem;
                    if (Utility.FilterSearch(item.LocationEntry, textBox1.Text, item.DisplayName)) 
                    {
                        listBox1.TopIndex = listBox1.Items.IndexOf(i);
                        return;
                    }
                }
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach(var i in listBox1.Items)
            {
                if (i is LogicObjects.ListItem)
                {
                    var item = i as LogicObjects.ListItem;
                    SelectedItems.Add(item.LocationEntry);
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { textBox1.Clear(); }
        }
    }
}
