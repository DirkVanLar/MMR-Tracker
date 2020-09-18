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

        public class entranceTable
        {
            public string Entrance { get; set; }
            public string To { get; set; }
            public string Exit { get; set; }
            public string From { get; set; }
        }

        private void SpoilerLogConverter_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Select a Spoiler Log");
            comboBox1.Items.Add("Ocarina Of Time Rando");
            //comboBox1.Items.Add("Wind Waker Rando");
            //comboBox1.Items.Add("Twilight Princess Rando");
            //comboBox1.Items.Add("Link to the past Rando");
            //comboBox1.Items.Add("Minish Cap Rando");
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1) { HandleOOTRSpoilerLog(); }
        }

        private void HandleOOTRSpoilerLog()
        {
            OpenFileDialog SelectedFile = new OpenFileDialog
            {
                Title = $"Select OOTR Spoiler Log",
                Filter = "OOTR Spoiler Log (*.json)|*.json",
                FilterIndex = 1,
                Multiselect = false
            };
            if (SelectedFile.ShowDialog() != DialogResult.OK) { return; }

            //Handle Entraces
            List<entranceTable> SpoilerEntranceTable = new List<entranceTable>();
            dynamic array = JsonConvert.DeserializeObject(File.ReadAllText(SelectedFile.FileName));
            foreach (dynamic item in array.entrances)
            {
                var entry = new entranceTable();
                string line = item.ToString();
                line = line.Replace(',', ':');
                line = line.Replace('"', '{');
                line = line.Replace("{", "");
                line = line.Replace("}", "");
                var lines = line.Split(':').Select(x => x.Trim()).ToArray();
                var front = lines[0].Split(new string[] { "->" }, StringSplitOptions.None);
                entry.Entrance = front[0].Trim();
                entry.To = front[1].Trim();
                entry.Exit = (lines[1].Contains("region")) ? lines[2].Trim() : lines[1].Trim();
                entry.From = (lines[1].Contains("region")) ? lines[4].Trim() : "";
                SpoilerEntranceTable.Add(entry);
            }

            List<string> FileContent = new List<string>();

            foreach (var i in SpoilerEntranceTable)
            {
                FileContent.Add($"{i.Entrance}>{i.To}->{i.Exit}" + (i.From == "" ? "" : $"<{i.From}"));
            }

            bool IsOneWay(string i)
            {
                var j = i.Split(new string[] { "->" }, StringSplitOptions.None)[0];
                var k = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation == j);
                return string.IsNullOrWhiteSpace(k.EntrancePair) || i.Contains("Adult Spawn") || i.Contains("Child Spawn");
            }

            //Attempt to add Entrances that were left out of the spoiler log
            foreach (var i in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Entrance"))
            {
                var e = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[0]) == i.SpoilerLocation);
                if (e == null)
                {
                    Console.WriteLine($"===========================================================");
                    Console.WriteLine($"{i.SpoilerLocation} Was not found");
                    var reverse = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == i.EntrancePair);
                    if (reverse == null) 
                    {
                        Console.WriteLine($"{i.SpoilerLocation} Did not have a pair. Setting it vanilla.");
                        FileContent.Add(i.SpoilerLocation + "->" + i.SpoilerItem); 
                    }
                    else
                    {
                        var f = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[1]) == reverse.SpoilerItem && !IsOneWay(x));
                        if (f != null)
                        {
                            Console.WriteLine($"{i.SpoilerLocation} Reverse Data found at {f}");
                            var g = f.Split(new string[] { "->" }, StringSplitOptions.None);
                            var h0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation == g[0] && !x.SpoilerLocation.Contains("Spawn"));
                            var j0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerItem == g[1] && !x.SpoilerLocation.Contains("Spawn"));
                            var h = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == h0.EntrancePair);
                            var j = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == j0.EntrancePair);

                            if (h == null || j == null) { Console.WriteLine($"{f} Did not have reverse Data! This is an error!"); continue; }

                            FileContent.Add(j.SpoilerLocation + "->" + h.SpoilerItem);
                        }
                        else
                        {
                            Console.WriteLine($"{i.SpoilerLocation} Did not have reverse Data. Setting it Vanilla.");
                            FileContent.Add(reverse.SpoilerLocation + "->" + reverse.SpoilerItem);
                        }
                    }
                }
            }

            //Handle Items
            Dictionary<string, string> ItemNames = new Dictionary<string, string>();
            foreach (dynamic item in array.locations)
            {
                string line = item.ToString();
                line = line.Replace(',', ':');
                line = line.Replace('"', '{');
                line = line.Replace("{", "");
                line = line.Replace("}", "");
                var lines = line.Split(':');
                for (var i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();
                }

                if (lines[1].Contains("item"))
                {
                    ItemNames.Add(lines[0].Trim(), lines[2].Trim());
                }
                else
                {
                    ItemNames.Add(lines[0].Trim(), lines[1].Trim());
                }

            }

            foreach (var i in ItemNames)
            {
                FileContent.Add($"{i.Key}->{i.Value}");
            }


            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Spoiler Log (*.txt)|*.txt",
                FilterIndex = 1,
                FileName = SelectedFile.FileName.Replace(".json", " MMRT Converted")
            };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

            File.WriteAllLines(saveDialog.FileName, FileContent);
        }

    }
}
