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


        public static List<string> GetEntries(string input)
        {
            return GetBrokenLogicString(input).Select(x => x.Item1.Trim()).ToList();
        }

        private static List<Tuple<string, int>> GetBrokenLogicString(string input)
        {
            List<Tuple<string, int>> BrokenString = new List<Tuple<string, int>>();

            bool InComment = false;
            int CharIndex = -1;

            string currentItem = "";
            foreach (var i in input)
            {
                char CurChar = i;
                CharIndex++;
                if (CurChar == '#') { InComment = true; }
                if (CurChar == '\n') { InComment = false; }
                if (InComment) { CurChar = ' '; }

                if (ISLogicChar(CurChar, false) && !InComment)
                {
                    if (currentItem != "")
                    {
                        BrokenString.Add(new Tuple<string, int>(currentItem, CharIndex));
                        currentItem = "";
                    }
                    BrokenString.Add(new Tuple<string, int>(CurChar.ToString(), CharIndex + 1));
                }
                else
                {
                    currentItem += CurChar.ToString();
                }
            }
            if (!string.IsNullOrWhiteSpace(currentItem)) { BrokenString.Add(new Tuple<string, int>(currentItem, CharIndex + 1)); }
            return BrokenString;
        }

        public static bool ISLogicChar(char i, bool ConsiderNewline = true)
        {
            if (i == '\n' && ConsiderNewline) { return true; }
            switch (i)
            {
                case '&':
                case '|':
                case '+':
                case '*':
                case '(':
                case ')':
                    return true;
                default:
                    return false;
            }

        }

        //Logic Processing
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

        public List<List<string>> ConvertLogicToConditional(string Logic)
        {
            Dictionary<string, string> LetterToNum = new Dictionary<string, string>();
            Dictionary<string, string> LetterToNumReverse = new Dictionary<string, string>();
            int Counter = 1;
            var LogicEntries = GetEntries(Logic);
            foreach (var i in LogicEntries)
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

            for (var i = 0; i < LogicEntries.Count(); i++)
            {
                string CleanedEntry = LogicEntries[i].Trim();
                if (string.IsNullOrWhiteSpace(CleanedEntry) || ISLogicChar(CleanedEntry[0]) || !LetterToNum.ContainsKey(CleanedEntry)) { continue; }

                LogicEntries[i] = LetterToNum[CleanedEntry];
            }

            var ReconstructedLoigc = string.Join(" ", LogicEntries).Replace("&&", "*").Replace("||", "+").Replace("&", "*").Replace("|", "+");

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

        private int[][] ConvertConditionalToNumbericList(List<List<string>> Conditional)
        {
            List<List<int>> IdConditional = new List<List<int>>();
            for (var i = 0; i < Conditional.Count(); i++)
            {
                var condentry = new List<int>();
                for (var j = 0; j < Conditional[i].Count(); j++)
                {
                    var LogicEntry = LogicEditor.EditorInstance.Logic.Find(x => x.DictionaryName.ToLower().Trim() == Conditional[i][j].ToLower().Trim());
                    if (LogicEntry == null) { Console.WriteLine(Conditional[i][j] + " Is not valid logic item"); return null; }
                    condentry.Add(LogicEntry.ID);
                }
                IdConditional.Add(condentry);
            }
            return IdConditional.Select(x => x.ToArray()).ToArray();
        }

        //List Box managment

        private void PrintToListVeiw()
        {
            lvReference.Items.Clear();
            foreach (var i in LogicEditor.EditorInstance.Logic)
            {
                string Name = i.DictionaryName;
                if (radItem.Checked) { Name = i.ItemName ?? i.DictionaryName; }
                if (radLoc.Checked) { Name = i.LocationName ?? i.DictionaryName; }
                if (Utility.FilterSearch(i, txtFilter.Text, Name))
                {
                    var log = i;
                    string[] row = { log.ID.ToString(), log.DictionaryName, log.LocationName, log.ItemName };
                    ListViewItem listViewItem = new ListViewItem(row) { Tag = log };
                    lvReference.Items.Add(listViewItem);
                }
            }
        }

        private void PrintToListBox(List<Tuple<string, int>> BrokenLogicString)
        {
            lbUsedlogic.Items.Clear();

            foreach (var i in BrokenLogicString)
            {
                var LogicItem = LogicEditor.EditorInstance.Logic.Find(io => io.DictionaryName.ToLower() == i.Item1.Trim().ToLower());
                if (LogicItem != null && !lbUsedlogic.Items.Contains(LogicItem))
                {
                    string Name = LogicItem.DictionaryName;
                    if (radItem.Checked) { Name = LogicItem.ItemName ?? LogicItem.DictionaryName; }
                    if (radLoc.Checked) { Name = LogicItem.LocationName ?? LogicItem.DictionaryName; }
                    LogicItem.DisplayName = Name;
                    lbUsedlogic.Items.Add(LogicItem);
                }
            }
        }

        //Text Box Formatting

        private void RecolorLogicItems(List<Tuple<string, int>> BrokenLogicString)
        {
            foreach (var i in BrokenLogicString)
            {
                if (i.Item1.Count() == 1 & ISLogicChar(i.Item1[0], false))
                {
                    txtInput.Select(i.Item2 - i.Item1.Length, i.Item2);
                    txtInput.SelectionColor = Color.Black;
                }
                else if (LogicEditor.EditorInstance.Logic.Any(io => io.DictionaryName.ToLower() == i.Item1.Trim().ToLower()))
                {
                    txtInput.Select(i.Item2 - i.Item1.Length, i.Item2);
                    txtInput.SelectionColor = Color.DarkGreen;
                }
                else
                {
                    txtInput.Select(i.Item2 - i.Item1.Length, i.Item2);
                    txtInput.SelectionColor = Color.Red;
                }
            }
        }

        private void RecolorComments()
        {
            int CommentStart = -1;
            int CommentEnd;
            int index = -1;
            foreach (var i in txtInput.Text)
            {
                index++;
                if (i == '#') { CommentStart = index; }
                if (i == '\n' || index >= txtInput.Text.Length - 1)
                {
                    CommentEnd = index+1;
                    if (CommentStart > -1)
                    {
                        txtInput.Select(CommentStart, CommentEnd - CommentStart);
                        txtInput.SelectionColor = Color.Gray;
                        CommentStart = -1;
                    }
                }
            }
        }

        //Form Controls

        private void textBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { txtFilter.Clear(); }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!(lbUsedlogic.SelectedItem is LogicObjects.LogicEntry)) { return; }
            LogicObjects.LogicEntry E = lbUsedlogic.SelectedItem as LogicObjects.LogicEntry;
            int CursorPos = txtInput.SelectionStart;
            int InsertLength = 0;
            if (lvReference.SelectedItems.Count > 0)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    InsertLength = E.DictionaryName.Count();
                    txtInput.Text = txtInput.Text.Insert(txtInput.SelectionStart, E.DictionaryName);
                }
                else
                {
                    InsertLength = E.ID.ToString().Count();
                    txtInput.Text = txtInput.Text.Insert(txtInput.SelectionStart, E.ID.ToString());
                }
            }
            txtInput.Focus();
            txtInput.SelectionStart = CursorPos + InsertLength;
            txtInput.SelectionLength = 0;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            int CursorPos = txtInput.SelectionStart;
            int InsertLength = 0;
            if (lvReference.SelectedItems.Count > 0)
            {
                var logicItem = (LogicObjects.LogicEntry)(lvReference.SelectedItems[0].Tag);
                InsertLength = logicItem.DictionaryName.Count();
                txtInput.Text = txtInput.Text.Insert(txtInput.SelectionStart, logicItem.DictionaryName);
            }
            txtInput.Focus();
            txtInput.SelectionStart = CursorPos + InsertLength;
            txtInput.SelectionLength = 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int CursorPos = txtInput.SelectionStart;
            var BrokenLogicString = GetBrokenLogicString(txtInput.Text);
            PrintToListBox(BrokenLogicString);
            RecolorLogicItems(BrokenLogicString);
            RecolorComments();

            txtInput.SelectionStart = CursorPos;
            txtInput.SelectionLength = 0;

            Console.WriteLine("====================");
            foreach (var i in BrokenLogicString)
            {
                Console.WriteLine($"{i.Item1} Index({i.Item2 - i.Item1.Length}, {i.Item2})");
            }
            Console.WriteLine(string.Join("", BrokenLogicString.Select(x => x.Item1.Trim())));
        }

        private void btnParseExpression_Click(object sender, EventArgs e)
        {
            var BrokenLogicString = GetBrokenLogicString(txtInput.Text);
            string TextBoxNewText = string.Join("", BrokenLogicString.Select(x => x.Item1.Trim()));

            try
            {
                var StringConditional = ConvertLogicToConditional(TextBoxNewText);
                Conditionals = ConvertConditionalToNumbericList(StringConditional);
                if (Conditionals == null)
                {
                    MessageBox.Show("Logic Expression Not Valid. Check syntax.");
                    return;
                }
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
            var BrokenLogicString = GetBrokenLogicString(txtInput.Text);
            PrintToListBox(BrokenLogicString);
        }

        private void LogicParser_Load(object sender, EventArgs e)
        {
            radDic.Checked = true;
        }

        private void LogicParser_Shown(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtInput.Focus();
        }

        private void RadChanged(object sender, EventArgs e)
        {
            PrintToListVeiw();
            var BrokenLogicString = GetBrokenLogicString(txtInput.Text);
            PrintToListBox(BrokenLogicString);
        }
    }
}
