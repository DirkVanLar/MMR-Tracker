using MathNet.Symbolics;
using MMR_Tracker.Class_Files;
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
            int testNum;
            List<string> Sets = new List<string>();
            string num = "";
            foreach (var i in x)
            {
                if (Char.IsNumber(i))
                {
                    num += i;
                }
                else
                {
                    if (num != "")
                    {
                        if (int.TryParse(num, out testNum)) { Sets.Add(num); }
                        num = "";
                    }
                }
            }
            if (int.TryParse(num, out testNum)) { Sets.Add(num); }
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
                Debugging.Log(i.ToString());
                NewLogic = NewLogic.Replace(i.Value.ToString(), i.Key);
            }
            Debugging.Log(NewLogic);
            Expression LogicSet = Infix.ParseOrThrow(NewLogic);
            var Output = Algebraic.Expand(LogicSet);
            string ExpandedLogic = Infix.Format(Output).Replace(" ", "");
            Debugging.Log(ExpandedLogic);

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
            foreach (var i in ExtractNumbers(textBox1.Text))
            {
                if (i < 0 || i >= LogicEditor.EditorInstance.Logic.Count() && LogicEditor.EditorInstance.Logic.ElementAt(i) == null)
                {
                    MessageBox.Show($"Logic Expression Not Valid. {i} is not a valid index in your logic.");
                }
            }
            try
            {
                Conditionals = ConvertLogicToConditional(textBox1.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Logic Expression Not Valid. Check syntax.");
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
                if (-1 < i && LogicEditor.EditorInstance.Logic.Count() > i && LogicEditor.EditorInstance.Logic.ElementAt(i) != null)
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
            LogicObjects.LogicEntry E = listBox1.SelectedItem as LogicObjects.LogicEntry;
            textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, E.ID.ToString());
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, listView1.SelectedItems[0].Tag.ToString());
            }
        }

        private void btnNames_Click(object sender, EventArgs e)
        {
            var OrderedLogic = Utility.CloneLogicList(LogicEditor.EditorInstance.Logic).OrderBy(x => x.DictionaryName.Count()).Reverse();
            foreach (var i in OrderedLogic)
            {
                Debugging.Log(i.DictionaryName);
                if (textBox1.Text.Contains(i.DictionaryName))
                {
                    textBox1.Text = textBox1.Text.Replace(i.DictionaryName, i.ID.ToString());
                }
            }
        }

        private void textBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { textBox2.Clear(); }
        }
    }
}
