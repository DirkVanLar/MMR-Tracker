using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class Utility
    {
        public static string ConvertCsvFileToJsonObject(string path)
        {
            var csv = new List<string[]>();
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }
        public static List<LogicObjects.LogicEntry> CloneLogicList(List<LogicObjects.LogicEntry> logic)
        {
            //Create a deep copy of a logic object by converting it to a json and coverting it back.
            //I have no idea why this works and it seems silly but whatever.
            return JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(JsonConvert.SerializeObject(logic));
        }
        public static LogicObjects.TrackerInstance CloneLogicInstance(LogicObjects.TrackerInstance instance)
        {
            //Create a deep copy of a logic object by converting it to a json and coverting it back.
            //I have no idea why this works and it seems silly but whatever.
            return JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(JsonConvert.SerializeObject(instance));
        }
        public static string FileSelect(string title, string filter)
        {
            OpenFileDialog SelectedFile = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                FilterIndex = 1,
                Multiselect = false
            };
            if (SelectedFile.ShowDialog() != DialogResult.OK) { return ""; }
            return SelectedFile.FileName;
        }
        public static bool FilterSearch(LogicObjects.LogicEntry logic, string searchTerm, string NameToCompare, LogicObjects.LogicEntry RandomizedItem = null)
        {
            if (searchTerm == "") { return true; }
            string[] searchTerms = searchTerm.Split('|');
            foreach (string term in searchTerms)
            {
                string[] subTerms = term.Split(',');
                bool valid = true;
                foreach (string subterm in subTerms)
                {
                    if (subterm == "") { continue; }
                    if (subterm[0] == '#')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (!logic.LocationArea.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '@')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (!logic.ItemSubType.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '$')
                    {
                        if (subterm.Substring(1) == "" || logic.ItemName == null) { continue; }
                        if (!logic.ItemName.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '%')
                    {
                        if (subterm.Substring(1) == "" || logic.LocationName == null) { continue; }
                        if (!logic.LocationName.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '&')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (RandomizedItem == null) { valid = false; }
                        else if (!RandomizedItem.ItemName.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else
                    {
                        if (!NameToCompare.ToLower().Contains(subterm.ToLower())) { valid = false; }
                    }
                }
                if (valid) { return true; }
            }
            return false;
        }
        public static bool CheckforSpoilerLog(List<LogicObjects.LogicEntry> Logic, bool full = false)
        {
            bool fullLog = true;
            bool Spoiler = false;
            foreach (var i in Logic)
            {
                if (i.IsFake) { continue; }
                if (i.SpoilerRandom > -1) 
                { 
                    Spoiler = true;
                    //if (!full) { Console.WriteLine(i.DictionaryName + " Had SpoilerData"); }
                }
                if (i.SpoilerRandom < 0) 
                { 
                    fullLog = false;
                    //if (full) { Console.WriteLine(i.DictionaryName + " Does not have SpoilerData"); }
                }
            }
            return (full) ? fullLog : Spoiler;
        }
        public static bool IsDivider(string text)
        {
            if (text == null || text == "") { return false; }
            int occurences = 0;
            foreach (var i in text)
            {
                if (i == '=') { occurences++; }
            }
            return (occurences >= 5);
        }
        public static bool CheckForRandomEntrances(LogicObjects.TrackerInstance Instance, int validEntranceCount = 6)
        {
            if (!Instance.IsEntranceRando()) { return false; }
            int count = 0;
            foreach (var i in Instance.Logic)
            {
                if (i.IsEntrance() && (i.Options == 0 || i.Options == 2)) { count += 1; }
                if (count >= validEntranceCount) { return true; }
            }
            return false;
        }
        public static int BoolToInt(bool Bool, bool FalseFirst = true)
        {
            return Bool ? (FalseFirst ? 1 : 0 ) : (FalseFirst ? 0 : 1);
        } 
        public static List<string> SeperateStringByMeasurement(ListBox container, string Measure, string indent = "    ")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

            if (IsDivider(Measure) || (int)g.MeasureString(Measure, font).Width < width) { return new List<string> { Measure }; }

            List<string> ShortenedStrings = new List<string>();
            string[] words = Measure.Split(' ');
            string ShortenedString = "";
            foreach (string word in words)
            {
                string testString = ShortenedString + word + " ";
                int Stringwidth = (int)g.MeasureString(testString, font).Width;
                if (Stringwidth > width)
                {
                    ShortenedStrings.Add(ShortenedString);
                    ShortenedString = indent + word + " ";
                }
                else
                {
                    ShortenedString = ShortenedString + word + " ";
                }
            }
            ShortenedStrings.Add(ShortenedString);
            return ShortenedStrings;
        }
        public static string CreateDivider(ListBox container, string DividerText = "")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

            int marks = 1;
            string Divider = "";
            while((int)g.MeasureString(Divider, font).Width <= width)
            {
                string Section = "";
                for (var i = 0; i < marks; i++)
                {
                    Section += "=";
                }
                Divider = Section + DividerText + Section;
                marks++;
            }
            return Divider;
        }
    }
}
