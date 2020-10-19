﻿using MMR_Tracker.Forms.Other_Games;
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

        private void LoadLogicData(string[] Logic, bool NewData)
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

            LogicEditor.EditorForm.LoadLogic(Logic);
            if (NewData) { LogicEditor.EditorForm.ClearLogicData(true); }
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
            return ReturnSpoiler;
        }

        private void CreateOOTR_Click(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.LoadLogicPreset("", "https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/OOTR%20Logic.txt", sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OcarinaOfTimeTools.HandleOOTRSpoilerLog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadLogicData(GetWebData("https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/OOTR%20Logic.txt"), true);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.LoadLogicPreset("", "https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/WWR%20Logic.txt", sender, e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WindWakerTools.HandleWWRSpoilerLog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadLogicData(GetWebData("https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker/master/Recources/Other%20Files/Custom%20Logic%20Presets/WWR%20Logic.txt"), true);
        }

        private void button9_Click(object sender, EventArgs e)
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
            LogicEditor.EditorForm.BtnNewLogic_Click(sender, e);
        }
    }
}
