using MathNet.Symbolics;
using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms.Other_Games;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms
{
    public partial class LogicParser : Form
    {
        public LogicParser()
        {
            InitializeComponent();
        }

        public static int[][] Conditionals = null;

        public string IndexToColumn(int index)
        {
            int ColumnBase = 26;
            int DigitMax = 7; // ceil(log26(Int32.Max))
            string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            if (index <= ColumnBase)
                return Digits[index - 1].ToString();

            var sb = new StringBuilder().Append(' ', DigitMax);
            var current = index;
            var offset = DigitMax;
            while (current > 0)
            {
                sb[--offset] = Digits[--current % ColumnBase];
                current /= ColumnBase;
            }
            return sb.ToString(offset, DigitMax - offset);
        }

        public int[] ExtractNumbers(string x)
        {
            List<string> Sets = new List<string>();
            string num = "";
            foreach (var i in x)
            {
                if (Char.IsNumber(i))
                {
                    num += i;
                }
                else if (num != "")
                {
                    if (int.TryParse(num, out int testNum1)) { Sets.Add(num); }
                    num = "";
                }
            }
            if (int.TryParse(num, out int testNum2)) { Sets.Add(num); }
            return Sets.Select(y => int.Parse(y)).Distinct().ToArray();
        }

        public int[][] ConvertLogicToConditional(string Logic)
        {
            string NewLogic = Logic.Replace("and", "*").Replace("or", "+");
            NewLogic = NewLogic.Replace("And", "*").Replace("Or", "+");
            NewLogic = NewLogic.Replace("AND", "*").Replace("OR", "+");
            NewLogic = NewLogic.Replace("A", "*").Replace("O", "+");
            NewLogic = NewLogic.Replace("a", "*").Replace("o", "+");
            NewLogic = NewLogic.Replace("&&", "*").Replace("||", "+");
            NewLogic = NewLogic.Replace("&", "*").Replace("|", "+");
            Dictionary<string, int> LetterToNum = new Dictionary<string, int>();
            foreach (var i in ExtractNumbers(Logic))
            {
                var Letter = IndexToColumn(i + 1);
                LetterToNum.Add(Letter, i);
            }
            LetterToNum = LetterToNum.OrderBy(x => x.Value).Reverse().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            foreach (var i in LetterToNum)
            {
                //Debugging.Log(i.ToString());
                NewLogic = NewLogic.Replace(i.Value.ToString(), i.Key);
            }
            //Debugging.Log(NewLogic);
            Expression LogicSet = Infix.ParseOrThrow(NewLogic);
            var Output = Algebraic.Expand(LogicSet);
            string ExpandedLogic = Infix.Format(Output).Replace(" ", "");
            //Debugging.Log(ExpandedLogic);

            foreach (var i in LetterToNum)
            {
                ExpandedLogic = ExpandedLogic.Replace(i.Key, i.Value.ToString());
            }
            ExpandedLogic = HandlePowers(ExpandedLogic.Replace(" ", ""));

            return ExpandedLogic.Split('+').Select(x => x.Split('*').Select(y => int.Parse(y)).ToArray()).ToArray();
        }

        public string SimplifyLetters(string Input)
        {
            string NewInput = Input;
            List<string> Output = new List<string>();
            string[] Sets = Input.Split('+');
            for (var i = 0; i < Sets.Length; i++)
            {
                string[] SubSets = Sets[i].Split('*');
                for (var j = 0; j < SubSets.Length; j++)
                {
                    Output.Add(SubSets[j]);
                }
            }
            string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Output = Output.Distinct().ToList();
            int count = 0;
            foreach(var i in Output)
            {
                NewInput = NewInput.Replace(i, Digits[count].ToString());
                count++;
            }
            return NewInput;
        }

        public string HandlePowers(string Input)
        {
            string Output = "";
            string[] Sets = Input.Split('+');
            for (var i = 0; i < Sets.Length; i++)
            {
                string[] SubSets = Sets[i].Split('*');
                for (var j = 0; j < SubSets.Length; j++)
                {
                    string line = SubSets[j];

                    if (line.Contains("^"))
                    {
                        line = line.Substring(0, line.IndexOf("^"));
                    }

                    Output = Output + line;
                    if (j != SubSets.Length - 1) { Output = Output + "*"; }
                    
                }
                if (i != Sets.Length - 1) { Output = Output + "+"; }
            }
            return Output;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        private void btnParseExpression_Click(object sender, EventArgs e)
        {
            string TextBoxNewText = Utility.RemoveCommentLines(textBox1.Text);

            foreach (var i in ExtractNumbers(TextBoxNewText))
            {
                if (!LogicEditor.EditorInstance.ItemInRange(i) || LogicEditor.EditorInstance.Logic.ElementAt(i) == null)
                {
                    MessageBox.Show($"Logic Expression Not Valid. {i} is not a valid index in your logic.");
                    return;
                }
            }
            try
            {
                Conditionals = ConvertLogicToConditional(TextBoxNewText);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Logic Expression Not Valid. Check syntax.");
                Conditionals = null;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            PrintToListVeiw();
            PrintToListBox();
        }

        private void LogicParser_Load(object sender, EventArgs e)
        {
            radDic.Checked = true;
        }

        private void RadChanged(object sender, EventArgs e)
        {
            PrintToListVeiw();
            PrintToListBox();
        }

        private void PrintToListVeiw()
        {
            listView1.Items.Clear();
            foreach (var i in LogicEditor.EditorInstance.Logic)
            {
                string Name = i.DictionaryName;
                if (radItem.Checked) { Name = i.ItemName ?? i.DictionaryName; }
                if (radLoc.Checked) { Name = i.LocationName ?? i.DictionaryName; }
                if (Utility.FilterSearch(i, textBox2.Text, Name))
                {
                    var log = i;
                    string[] row = { log.ID.ToString(), log.DictionaryName, log.LocationName, log.ItemName };
                    ListViewItem listViewItem = new ListViewItem(row) { Tag = log.ID };
                    listView1.Items.Add(listViewItem);
                }
            }
        }

        private void PrintToListBox()
        {
            listBox1.Items.Clear();
            foreach (var i in ExtractNumbers(textBox1.Text))
            {
                if (LogicEditor.EditorInstance.ItemInRange(i) && LogicEditor.EditorInstance.Logic.ElementAt(i) != null)
                {
                    var log = LogicEditor.EditorInstance.Logic[i];
                    string Name = log.DictionaryName;
                    if (radItem.Checked) { Name = log.ItemName ?? log.DictionaryName; }
                    if (radLoc.Checked) { Name = log.LocationName ?? log.DictionaryName; }
                    log.DisplayName = log.ID.ToString() + ": " + Name;
                    listBox1.Items.Add(log);
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem is LogicObjects.LogicEntry)) { return; }
            LogicObjects.LogicEntry E = listBox1.SelectedItem as LogicObjects.LogicEntry;
            int CursorPos = textBox1.SelectionStart;
            int InsertLength = 0;
            if (listView1.SelectedItems.Count > 0)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    InsertLength = E.DictionaryName.Count();
                    textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, E.DictionaryName);
                }
                else
                {
                    InsertLength = E.ID.ToString().Count();
                    textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, E.ID.ToString());
                }
            }
            textBox1.Focus();
            textBox1.SelectionStart = CursorPos + InsertLength;
            textBox1.SelectionLength = 0;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            int CursorPos = textBox1.SelectionStart;
            int InsertLength = 0;
            if (listView1.SelectedItems.Count > 0)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && int.TryParse(listView1.SelectedItems[0].Tag.ToString(), out int x) && LogicEditor.EditorInstance.ItemInRange(x))
                {
                    InsertLength = LogicEditor.EditorInstance.Logic[x].DictionaryName.Count();
                    textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, LogicEditor.EditorInstance.Logic[x].DictionaryName);
                }
                else
                {
                    InsertLength = listView1.SelectedItems[0].Tag.ToString().Count();
                    textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, listView1.SelectedItems[0].Tag.ToString());
                }
            }
            textBox1.Focus();
            textBox1.SelectionStart = CursorPos + InsertLength;
            textBox1.SelectionLength = 0;
        }

        private void btnNames_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> ReplacerList = new Dictionary<string, string>();

            var OrderedLogic = Utility.CloneLogicList(LogicEditor.EditorInstance.Logic);
            foreach (var i in OrderedLogic)
            {
                if (!ReplacerList.ContainsKey(i.DictionaryName))
                {
                    ReplacerList.Add(i.DictionaryName, i.ID.ToString());
                }
            }

            string TextBoxNewText = Utility.RemoveCommentLines(textBox1.Text);

            foreach (var i in ReplacerList.OrderBy(x => x.Key.Count()).Reverse())
            {
                if (TextBoxNewText.Contains(i.Key))
                {
                    textBox1.Text = textBox1.Text.Replace(i.Key, i.Value);
                }
            }

        }

        private void textBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { textBox2.Clear(); }
        }

        private void LogicParser_Shown(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.Focus();
        }
    }
}
