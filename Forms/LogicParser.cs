using MathNet.Symbolics;
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
            string NewLogic = Logic.Replace("&", "*").Replace("|", "+");
            Dictionary<string, int> LetterToNum = new Dictionary<string, int>();
            foreach (var i in ExtractNumbers(Logic))
            {
                var Letter = IndexToColumn(i);
                LetterToNum.Add(Letter, i);
            }
            foreach (var i in LetterToNum)
            {
                NewLogic = NewLogic.Replace(i.Value.ToString(), i.Key);
            }
            Expression LogicSet = Infix.ParseOrThrow(NewLogic);
            var Output = Algebraic.Expand(LogicSet);
            string ExpandedLogic = Infix.Format(Output).Replace(" ", "");
            foreach (var i in LetterToNum)
            {
                ExpandedLogic = ExpandedLogic.Replace(i.Key, i.Value.ToString());
            }
            return ExpandedLogic.Split('+').Select(x => x.Split('*').Select(y => int.Parse(y)).ToArray()).ToArray(); ;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            foreach(var i in ExtractNumbers(textBox1.Text))
            {
                if (-1 < i && LogicEditor.EditorInstance.Logic.Count() > i && LogicEditor.EditorInstance.Logic.ElementAt(i) != null)
                {
                    var log = LogicEditor.EditorInstance.Logic[i];
                    string[] row1 = { log.DictionaryName, log.LocationName, log.ItemName };
                    listView1.Items.Add(log.ID.ToString()).SubItems.AddRange(row1);
                }
            }
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

        private void button1_Click(object sender, EventArgs e)
        {
            ItemSelect ItemSelectForm = new ItemSelect();
            ItemSelect.Function = 11;
            var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); ItemSelect.Function = 0; return; }
            textBox1.Text = textBox1.Text.Insert(textBox1.SelectionStart, Tools.CurrentSelectedItem.ID.ToString());
            Tools.CurrentSelectedItem = new LogicObjects.LogicEntry();
            ItemSelect.Function = 0;
        }
    }
}
