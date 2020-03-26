using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class OOT_Support
    {
        public static bool isOOT = false;
        public static void CreateOOTFiles()
        {
            var file = Utility.FileSelect("Select OOTR Spoiler Log", "Logic File (*.json)|*.json");
            var LogicFile = new List<string>();
            var Dictionary = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem,ItemNameDump" };
            var Group = 0;
            int version = 0;
            bool Begin = false;
            foreach (var i in File.ReadAllLines(file))
            {
                string line = i.Replace("\"", "");
                line = line.Replace("-", "=");
                line = line.Trim();
                if (line.StartsWith(":version")) 
                {
                    version = Int32.Parse(line.Split(':')[2].Split('.')[0].Trim());
                    LogicFile.Add("-versionOOT " + line.Split(':')[2].Split('.')[0].Trim()); 
                }
                if (line.StartsWith("entrances:")) { Begin = true; Group = 1; continue; }
                if (line.StartsWith("locations:")) { Begin = true; Group = 2; continue; }
                if (line.Contains("Item 1") || line.Contains("Item 2") || line.Contains("Item 3") || line.Contains("Item 4")) { continue; }
                if (line.Contains(":woth_locations")) { break; }
                if (line.StartsWith("}")) { Begin = false; continue; }
                if (!Begin) { continue; }

                var info = line.Split(':');
                info[0] = info[0].Trim();
                LogicFile.Add("- " + info[0]);
                for (var o = 0; o < 4; o++) { LogicFile.Add(""); }
                if (info.Count() < 2) { continue; }
                if (info.Count() > 2 && info[1].Contains("{item") && Group == 2)
                {
                    info[1] = info[2].Split(',')[0].Trim();
                }
                if (info.Count() > 2 && info[1].Contains("{region") && Group == 1)
                {
                    info[1] = info[2].Split(',')[0].Trim();
                }
                info[1] = info[1].Trim();

                string item = (Group == 2) ? info[0] : info[0].Replace("=", "<").Replace(">", "=");

                Dictionary.Add(string.Format("{0},{1},,,{2},{3},,{4}", info[0], info[0], (Group == 1) ? "Entrance" : (info[0].Contains("Medallion") || info[0].Contains("Sapphire") || info[0].Contains("Ruby") || info[0].Contains("Emerald")) ? "Boss Token" : "Item", info[0], item));
                //Console.WriteLine(string.Format("{0},{1},,,{2},{3},,{4}", info[0], info[0], (Group == 1) ? "Entrance" : (info[0].Contains("Medallion") || info[0].Contains("Sapphire") || info[0].Contains("Ruby") || info[0].Contains("Emerald")) ? "Boss Token" : "Item", info[0], item));
            }

            LogicFile.Add("- Prog Ocarina of Time");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog BombBag 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog BombBag 3");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Bottle 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Bottle 3");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Bottle 4");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Wallet 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Scale 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog HookShot 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Quiver 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Quiver 3");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Gauntlet 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Seed Pouch 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Seed Pouch 3");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Nut 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }
            LogicFile.Add("- Prog Stick 2");
            for (var i = 0; i < 4; i++) { LogicFile.Add(""); }


            SaveFileDialog saveLogic = new SaveFileDialog
            {
                Filter = "OOT Logic (*.txt)|*.txt",
                FilterIndex = 1,
                Title = "Save Logic File",
                FileName = "OOT Logic V" + version + ".txt"
            };
            saveLogic.ShowDialog();
            File.WriteAllLines(saveLogic.FileName, LogicFile);

            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Dictionary File",
                FileName = "OOTRDICTIONARYV" + version + ".csv"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, Dictionary);
        }
    }
}
