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
            string NewLogic = Logic.Replace("&&", "*").Replace("||", "+").Replace("&", "*").Replace("|", "+");
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

        public List<List<string>> ConvertLogicToConditionalString(string Logic)
        {
            Dictionary<string, string> LetterToNum = new Dictionary<string, string>();
            Dictionary<string, string> LetterToNumReverse = new Dictionary<string, string>();
            int Counter = 1;
            foreach (var i in GetEntries(Logic))
            {
                string CleanedEntry = i.Trim();
                if (string.IsNullOrWhiteSpace(CleanedEntry) || string.IsNullOrEmpty(CleanedEntry) || ISLogicChar(CleanedEntry[0]) || LetterToNum.ContainsKey(CleanedEntry)) 
                { continue; }
                var Letter = IndexToColumn(Counter);
                Counter++;
                LetterToNum.Add(CleanedEntry, Letter);
                LetterToNumReverse.Add(Letter, CleanedEntry);
            }
            LetterToNum = LetterToNum.OrderBy(x => x.Value).Reverse().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var LogicEntries = GetEntries(Logic);
            for (var i= 0; i < LogicEntries.Count(); i++)
            {
                string CleanedEntry = LogicEntries[i].Trim();
                if (string.IsNullOrWhiteSpace(CleanedEntry) || string.IsNullOrEmpty(CleanedEntry) || ISLogicChar(CleanedEntry[0]) || !LetterToNum.ContainsKey(CleanedEntry))
                { continue; }

                LogicEntries[i] = LetterToNum[CleanedEntry];
            }

            var ReconstructedLoigc = string.Join(" ", LogicEntries).Replace("&&", "*").Replace("||", "+").Replace("&", "*").Replace("|", "+");
            ReconstructedLoigc = ReconstructedLoigc.Replace("&&", "*").Replace("||", "+").Replace("&", "*").Replace("|", "+");

            if (string.IsNullOrEmpty(ReconstructedLoigc)) { return null; }

            //Debugging.Log("Reconstructed Logc = " + ReconstructedLoigc);
            Expression LogicSet = Infix.ParseOrThrow(ReconstructedLoigc);
            var Output = Algebraic.Expand(LogicSet);
            string ExpandedLogic = Infix.Format(Output).Replace(" ", "");
            //Debugging.Log(ExpandedLogic);

            ExpandedLogic = HandlePowers(ExpandedLogic.Replace(" ", ""));
            ExpandedLogic = ExpandedLogic.Replace("*", "&").Replace("+", "|");

            var ExpandedLogicEntries = GetEntries(ExpandedLogic);
            for (var i = 0; i < ExpandedLogicEntries.Count(); i++)
            {
                string CleanedEntry = ExpandedLogicEntries[i].Trim();

                if (string.IsNullOrWhiteSpace(CleanedEntry) || string.IsNullOrEmpty(CleanedEntry) || ISLogicChar(CleanedEntry[0]) || !LetterToNumReverse.ContainsKey(CleanedEntry))
                { continue; }

                ExpandedLogicEntries[i] = LetterToNumReverse[CleanedEntry];
            }

            var FinalLogic = string.Join(" ", ExpandedLogicEntries);

            return FinalLogic.Split('|').Select(x => x.Split('&').Select(y => y.Trim()).ToList()).ToList();
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
            string newInput = textBox1.Text.Replace(" and ", " & ").Replace(" And ", " & ").Replace(" AND ", " & ").Replace(" or ", " | ").Replace(" Or ", " | ").Replace(" OR ", " | ");

            var Entries = GetEntries(newInput);
            for (var i = 0; i < Entries.Count(); i++)
            {
                if (ISLogicChar(Entries[i][0]) || ISComment(Entries[i])) { continue; }
                string CleanEntry = Entries[i].ToLower().Trim();
                var LogicEntry = LogicEditor.EditorInstance.Logic.Find(x => x.DictionaryName.ToLower().Trim() == CleanEntry);
                if (int.TryParse(CleanEntry, out int ID) && LogicEditor.EditorInstance.ItemInRange(ID))
                {
                    var LogicItem = LogicEditor.EditorInstance.Logic[ID];
                    string Name = LogicItem.DictionaryName;
                    if (radItem.Checked) { Name = LogicItem.ItemName ?? LogicItem.DictionaryName; }
                    if (radLoc.Checked) { Name = LogicItem.LocationName ?? LogicItem.DictionaryName; }
                    LogicItem.DisplayName = LogicItem.ID.ToString() + ": " + Name;
                    listBox1.Items.Add(LogicItem);
                }
                else if (LogicEntry != null)
                {
                    var LogicItem = LogicEntry;
                    string Name = LogicItem.DictionaryName;
                    if (radItem.Checked) { Name = LogicItem.ItemName ?? LogicItem.DictionaryName; }
                    if (radLoc.Checked) { Name = LogicItem.LocationName ?? LogicItem.DictionaryName; }
                    LogicItem.DisplayName = LogicItem.ID.ToString() + ": " + Name;
                    listBox1.Items.Add(LogicItem);
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
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                textBox1.Text = ConvertIDToDicName(textBox1.Text);
            }
            else
            {
                textBox1.Text = ConvertDicNameToID(textBox1.Text);
            }
        }

        public static string ConvertDicNameToID(string Input)
        {
            var Entries = GetEntries(Input);
            for (var i = 0; i < Entries.Count(); i++)
            {
                if (ISLogicChar(Entries[i][0]) || ISComment(Entries[i])) { continue; }
                string CleanEntry = Entries[i].ToLower().Trim();
                var LogicEntry = LogicEditor.EditorInstance.Logic.Find(x => x.DictionaryName.ToLower().Trim() == CleanEntry);
                if (LogicEntry == null) { continue; }
                Entries[i] = LogicEntry.ID.ToString();
            }
            string newOutput = string.Join("", Entries);
            //newOutput = newOutput.Replace(" and ", " & ").Replace(" And ", " & ").Replace(" AND ", " & ").Replace(" or ", " | ").Replace(" Or ", " | ").Replace(" OR ", " | ");
            return newOutput;
        }

        public static string ConvertIDToDicName(string Input)
        {
            var Entries = GetEntries(Input);
            for (var i = 0; i < Entries.Count(); i++)
            {
                if (ISLogicChar(Entries[i][0]) || ISComment(Entries[i])) { continue; }
                string CleanEntry = Entries[i].Trim();
                if (int.TryParse(CleanEntry, out int ID) && LogicEditor.EditorInstance.ItemInRange(ID))
                {
                    Entries[i] = LogicEditor.EditorInstance.Logic[ID].DictionaryName;
                }
            }
            string newOutput = string.Join("", Entries);
            //newOutput = newOutput.Replace(" and ", " & ").Replace(" And ", " & ").Replace(" AND ", " & ").Replace(" or ", " | ").Replace(" Or ", " | ").Replace(" OR ", " | ");
            return newOutput;
        }

        public static List<string> GetEntries(string input)
        {
            List<string> BrokenString = new List<string>();

            bool InComment = false;

            string currentItem = "";
            foreach(var i in input)
            {
                if (i == '#') { InComment = true; }
                if (i == '\n') { InComment = false; }
                if (ISLogicChar(i) && !InComment)
                {
                    if (currentItem != "")
                    {
                        BrokenString.Add(currentItem);
                        currentItem = "";
                    }
                    BrokenString.Add(i.ToString());
                }
                else
                {
                    currentItem += i.ToString();
                }
            }
            if (currentItem != "") { BrokenString.Add(currentItem); }
            return BrokenString;

        }

        public static bool ISLogicChar(char i)
        {
            switch (i)
            {
                case '&':
                case '|':
                case '+':
                case '*':
                case '(':
                case ')':
                case '\n':
                    return true;
                default:
                    return false;
            }

        }

        public static bool ISComment(string i)
        {
            return i.Trim().StartsWith("#");
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
