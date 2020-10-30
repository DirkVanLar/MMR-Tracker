using MMR_Tracker.Forms.Other_Games;
using MMR_Tracker.Other_Games;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms
{
    public partial class SpoilerLogConverter : Form
    {

        public static string[] SpoilerLogConvertable = new string[] { "MMR", "SSR", "OOTR", "WWR" };
        public SpoilerLogConverter()
        {
            InitializeComponent();
        }

        private string[] GetWebData(string URL)
        {
            string[] Lines;
            try
            {
                string WebPath = URL;
                System.Net.WebClient wc = new System.Net.WebClient();
                string webData = wc.DownloadString(WebPath);
                Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                Debugging.Log(WebPath);
            }
            catch { return null; }
            return Lines;
        }

        private void LoadLogicData(string[] Logic = null)
        {
            if (LogicEditor.EditorForm == null)
            {
                LogicEditor.EditorForm = new LogicEditor();
                LogicEditor.EditorForm.Show();
            }
            else
            {
                LogicEditor.EditorForm.Show();
                LogicEditor.EditorForm.Focus();
            }

            if (Logic != null) { LogicEditor.EditorForm.LoadLogic(Logic); }
            else { LogicEditor.EditorForm.BtnNewLogic_Click(this, null); }
        }

        private void SpoilerLogConverter_Load(object sender, EventArgs e)
        {
            
        }

        public static string[] AutoConverter(LogicObjects.TrackerInstance instance, string[] Spoiler)
        {
            var ReturnSpoiler = Spoiler;
            if (instance.GameCode == "WWR" && !Spoiler[0].Contains("Converted WWR"))
            {
                ReturnSpoiler = WindWakerTools.HandleWWRSpoilerLog(Spoiler);
            }
            if (instance.GameCode == "OOTR" && !Spoiler[0].Contains("Converted OOTR"))
            {
                ReturnSpoiler = OcarinaOfTimeTools.HandleOOTRSpoilerLog(string.Join("", Spoiler));
            }
            if (instance.GameCode == "SSR" && !Spoiler[0].Contains("Converted SSR"))
            {
                ReturnSpoiler = SkywardSwordTools.HandleSSRSpoilerLog(Spoiler);
            }
            return ReturnSpoiler;
        }

        private void CreateOOTR_Click(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.LoadLogicPreset("", "https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/OOTR%20Logic.txt.dis", sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OcarinaOfTimeTools.HandleOOTRSpoilerLog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadLogicData(GetWebData("https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/OOTR%20Logic.txt.dis"));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.LoadLogicPreset("", "https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/WWR%20Logic.txt.dis", sender, e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WindWakerTools.HandleWWRSpoilerLog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadLogicData(GetWebData("https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/WWR%20Logic.txt.dis"));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            LoadLogicData();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            List<string> csv = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem,EntrancePair" };
            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Dictionary File",
                FileName = "XXXDICTIONARYV1.csv"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, csv);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.LoadLogicPreset("", "https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/SSR%20Logic.txt.dis", sender, e);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            SkywardSwordTools.HandleSSRSpoilerLog();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            LoadLogicData(GetWebData("https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/SSR%20Logic.txt.dis"));
        }
    }
}
