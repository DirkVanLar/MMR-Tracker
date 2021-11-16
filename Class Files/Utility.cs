﻿using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class Utility
    {
        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                {
                    try
                    {
                        objResult.Add(properties[j], csv[i][j]);
                    }
                    catch { }
                }

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
        public static LogicObjects.TrackerInstance CloneTrackerInstance(LogicObjects.TrackerInstance instance)
        {
            //Create a deep copy of a tracker object by converting it to a json and coverting it back.
            return JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(JsonConvert.SerializeObject(instance));
        }
        public static LogicObjects.LogicEntry CloneLogicObject(LogicObjects.LogicEntry source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<LogicObjects.LogicEntry>(serialized);
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
            if (NameToCompare == null) { NameToCompare = logic.DictionaryName; }
            if (searchTerm == "") { return true; }
            if (searchTerm[0] == '^')
            {
                searchTerm = searchTerm.Substring(1);
                if (searchTerm == "") { return true; }
            }
            string[] searchTerms = searchTerm.Split('|');
            foreach (string term in searchTerms)
            {
                string[] subTerms = term.Split('&');
                bool valid = true;
                foreach (string i in subTerms)
                {
                    bool Inverse = false;
                    bool Perfect = false;
                    var subterm = i;
                    if (subterm == "") { continue; }
                    if (subterm[0] == '!')
                    {
                        Inverse = true;
                        subterm = subterm.Substring(1);
                    }
                    else if (subterm[0] == '=')
                    {
                        Perfect = true;
                        subterm = subterm.Substring(1);
                    }
                    if (subterm == "") { continue; }
                    if (subterm[0] == '#')//Search By Location Area
                    {
                        if (subterm.Substring(1) == "" || logic.LocationArea == null) { continue; }
                        if (logic.LocationArea.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.LocationArea.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '@')//Search By Item Type
                    {
                        if (subterm.Substring(1) == "" || logic.ItemSubType == null) { continue; }
                        if ((logic.ItemSubType.ToLower().Contains(subterm.Substring(1).ToLower())) == Inverse) { valid = false; }
                        if (Perfect && logic.ItemSubType.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '~')//Search By Dictionary Name
                    {
                        if (subterm.Substring(1) == "" || logic.DictionaryName == null) { continue; }
                        if ((logic.DictionaryName.ToLower().Contains(subterm.Substring(1).ToLower())) == Inverse) { valid = false; }
                        if (Perfect && logic.DictionaryName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '$')//Search By Item Name
                    {
                        if (subterm.Substring(1) == "" || logic.ItemName == null) { continue; }
                        if (logic.ItemName.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.ItemName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '%')//Search By Location Name
                    {
                        if (subterm.Substring(1) == "" || logic.LocationName == null) { continue; }
                        if (logic.LocationName.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.LocationName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '_')//Search By Randomized Item
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (RandomizedItem == null) { valid = false; }
                        else if ((RandomizedItem.ItemName ?? RandomizedItem.DictionaryName).ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.ItemName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '*')//Search Starred Items
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (NameToCompare.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && NameToCompare.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                        if (!logic.Starred) { valid = false; }
                    }
                    else //Search By "NameToCompare" variable
                    {
                        if (!NameToCompare.ToLower().Contains(subterm.ToLower()) == !Inverse) { valid = false; }
                        if (Perfect && NameToCompare.ToLower() != subterm.ToLower()) { valid = false; }
                    }
                }
                if (valid) { return true; }
            }
            return false;
        }
        public static bool CheckforSpoilerLog(List<LogicObjects.LogicEntry> Logic, bool full = false, bool FakeAllowed = true, bool Log = false)
        {
            bool fullLog = true;
            bool Spoiler = false;
            foreach (var i in Logic)
            {
                if (i.IsFake || string.IsNullOrWhiteSpace(i.LocationName) || i.Unrandomized(2)) { continue; }
                if (i.SpoilerRandom > (FakeAllowed ? -2 : -1)) 
                { 
                    Spoiler = true;
                    if (!full && Log) { Debugging.Log($"{string.Join(", ", i.SpoilerLocation[0])} Had SpoilerData: {string.Join(", ", i.SpoilerItem[0])}"); }
                }
                if (i.SpoilerRandom < (FakeAllowed ? -1 : 0)) 
                { 
                    fullLog = false;
                    if (full && Log) { Debugging.Log(string.Join(", ", i.SpoilerLocation[0]) + " Does not have SpoilerData"); }
                }
            }
            return (full) ? fullLog : Spoiler;
        }
        public static bool IsDivider(string text, int MinDividerLength = 5)
        {
            if (text == null || text == "") { return false; }
            string TestFor = "";
            for(var i = 0; i < MinDividerLength; i++) { TestFor += "="; }
            return text.Contains(TestFor);
        }
        public static string GetTextAfter(string Input, string After)
        {
            int Loc = Input.IndexOf(After);
            if (Loc < 0) { return ""; }
            int Rightof = Loc + After.Count();
            if (Rightof >= Input.Count()) { return ""; }
            return Input.Substring(Rightof);
        }
        public static bool CheckForRandomEntrances(LogicObjects.TrackerInstance Instance, bool Spoiler = false, int validEntranceCount = 6)
        {
            if (!Instance.EntranceRando) { return false; }
            int count = 0;
            foreach (var i in Instance.Logic.Where(x => x.IsEntrance()))
            {
                if (!Spoiler && i.AppearsInListbox()) { count += 1; }
                if (Spoiler && i.SpoilerRandom != i.ID && i.SpoilerRandom > -1) { count += 1; }
                if (count >= validEntranceCount) { return true; }
            }
            return false;
        }
        public static void FixSpoilerInconsistency(LogicObjects.TrackerInstance Instance)
        {
            var RandoOptionsContradictSpoiler = false;
            foreach (var i in Instance.Logic)
            {
                if (i.Unrandomized(2) && i.SpoilerRandom != i.ID && i.SpoilerRandom > -1) { RandoOptionsContradictSpoiler = true; break; }
            }
            if (RandoOptionsContradictSpoiler)
            {
                var AttemptFix = MessageBox.Show("You have marked items as unrandomized that are are radnomized according to your spoiler log. Would you like the tracker to attempt to correct this? This may leave items unrandomized that were simply place vanilla which could spoil your seed.", "Randomization Option Inconsistency", MessageBoxButtons.YesNo);
                if (AttemptFix == DialogResult.Yes)
                {
                    foreach (var i in Instance.Logic)
                    {
                        if (i.Unrandomized(2) && i.SpoilerRandom != i.ID && i.SpoilerRandom > -1) { i.Options = (i.StartingItem()) ? 4 : 0; }
                    }
                }
            }
        }
        public static int BoolToInt(bool Bool, bool FalseFirst = true)
        {
            return Bool ? (FalseFirst ? 1 : 0 ) : (FalseFirst ? 0 : 1);
        } 
        public static string RemoveCommentLines(string Line)
        {
            string[] lines = Line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] LinesNew = lines.Where(i => !i.Trim().StartsWith("#")).Select(i => (i.Contains("#")) ? i.Substring(0, i.IndexOf("#")) : i).ToArray();
            return string.Join(Environment.NewLine, LinesNew);
        }
        public static List<string> WrapStringInListBox(ListBox container, string Measure, string indent = "    ")
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
        public static long CountUniqueCombinations(int n, int r)
        {
            // naive: return Factorial(n) / (Factorial(r) * Factorial(n - r));
            return nPr(n, r) / Factorial(r);

            long nPr(int np, int rp)
            {
                // naive: return Factorial(n) / Factorial(n - r);
                return FactorialDivision(np, np - rp);
            }

            long FactorialDivision(int topFactorial, int divisorFactorial)
            {
                long result = 1;
                for (int i = topFactorial; i > divisorFactorial; i--)
                    result *= i;
                return result;
            }

            long Factorial(int i)
            {
                if (i <= 1)
                    return 1;
                return i * Factorial(i - 1);
            }
        }
        public static List<List<LogicObjects.LogicEntry>> GetProgressiveItemSets(LogicObjects.TrackerInstance instance)
        {
            var SW1 = instance.Logic.Find(x => x.DictionaryName == "Starting Sword" || x.DictionaryName == "StartingSword");
            var SW2 = instance.Logic.Find(x => x.DictionaryName == "Razor Sword" || x.DictionaryName == "UpgradeRazorSword");
            var SW3 = instance.Logic.Find(x => x.DictionaryName == "Gilded Sword" || x.DictionaryName == "UpgradeGildedSword");
            var MM1 = instance.Logic.Find(x => x.DictionaryName == "Great Fairy Magic Meter" || x.DictionaryName == "FairyMagic");
            var MM2 = instance.Logic.Find(x => x.DictionaryName == "Great Fairy Extended Magic" || x.DictionaryName == "FairyDoubleMagic");
            var WL1 = instance.Logic.Find(x => x.DictionaryName == "Town Wallet (200)" || x.DictionaryName == "UpgradeAdultWallet");
            var WL2 = instance.Logic.Find(x => x.DictionaryName == "Ocean Wallet (500)" || x.DictionaryName == "UpgradeGiantWallet");
            var BB1 = instance.Logic.Find(x => x.DictionaryName == "Bomb Bag (20)" || x.DictionaryName == "ItemBombBag");
            var BB2 = instance.Logic.Find(x => x.DictionaryName == "Town Bomb Bag (30)" || x.DictionaryName == "UpgradeBigBombBag");
            var BB3 = instance.Logic.Find(x => x.DictionaryName == "Mountain Bomb Bag (40)" || x.DictionaryName == "UpgradeBiggestBombBag");
            var BW1 = instance.Logic.Find(x => x.DictionaryName == "Hero's Bow" || x.DictionaryName == "ItemBow");
            var BW2 = instance.Logic.Find(x => x.DictionaryName == "Town Archery Quiver (40)" || x.DictionaryName == "UpgradeBigQuiver");
            var BW3 = instance.Logic.Find(x => x.DictionaryName == "Swamp Archery Quiver (50)" || x.DictionaryName == "UpgradeBiggestQuiver");

            List<List<LogicObjects.LogicEntry>> ProgressiveItemSets = new List<List<LogicObjects.LogicEntry>>
            {
                new List<LogicObjects.LogicEntry> { SW1, SW2, SW3 }.Where(x => x != null).ToList(),
                new List<LogicObjects.LogicEntry> { MM1, MM2 }.Where(x => x != null).ToList(),
                new List<LogicObjects.LogicEntry> { WL1, WL2 }.Where(x => x != null).ToList(),
                new List<LogicObjects.LogicEntry> { BB1, BB2, BB3 }.Where(x => x != null).ToList(),
                new List<LogicObjects.LogicEntry> { BW1, BW2, BW3 }.Where(x => x != null).ToList(),
            };
            return ProgressiveItemSets.Where(x => x.Any()).ToList();
        }

        public static void EditFont()
        {
            Form fontSelect = new Form();
            fontSelect.FormBorderStyle = FormBorderStyle.FixedSingle;
            fontSelect.Text = "Font";
            fontSelect.Width = (220);
            fontSelect.Height = (112);
            try { fontSelect.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon()); } catch { }
            //Font Size lable
            Label lbSize = new Label();
            lbSize.Text = "Font Size:";
            lbSize.Location = new Point(2, 2);
            lbSize.AutoSize = true;
            lbSize.Parent = fontSelect;
            fontSelect.Controls.Add(lbSize);
            //Font Size Selector
            NumericUpDown Size = new NumericUpDown();
            Size.Location = new Point(lbSize.Width + 6, 2);
            Size.Width += 20;
            Size.Parent = fontSelect;
            Size.DecimalPlaces = 2;
            Size.Value = (decimal)LogicObjects.MainTrackerInstance.Options.FormFont.Size;
            Size.ValueChanged += (s, ea) =>
            {
                var currentFont = LogicObjects.MainTrackerInstance.Options.FormFont;
                LogicObjects.MainTrackerInstance.Options.FormFont = new Font(currentFont.FontFamily, (float)Size.Value, FontStyle.Regular);
                UpdateFontInForms();
            };
            fontSelect.Controls.Add(Size);
            //Font Style Lable
            Label lbFont = new Label();
            lbFont.Text = "Font Style:";
            lbFont.Location = new Point(2, Size.Height + 2);
            lbFont.AutoSize = true;
            lbFont.Parent = fontSelect;
            fontSelect.Controls.Add(lbFont);
            //Create list of available fonts and find currently used Font
            List<string> FontStyles = new List<string>();
            int CurIndex = -1;
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                FontStyles.Add(font.Name);
                if (font.Name == LogicObjects.MainTrackerInstance.Options.FormFont.FontFamily.Name) { CurIndex = FontStyles.Count - 1; }
            }
            //Font Style Selector
            ComboBox cmbStyle = new ComboBox();
            cmbStyle.Location = new Point(lbSize.Width + 6, Size.Height + 2);
            cmbStyle.Parent = fontSelect;
            cmbStyle.DataSource = FontStyles;
            cmbStyle.Width = Size.Width;
            if (CurIndex > 0) { cmbStyle.SelectedIndex = CurIndex; }
            cmbStyle.SelectedIndexChanged += (s, ea) =>
            {
                var currentFont = LogicObjects.MainTrackerInstance.Options.FormFont;
                LogicObjects.MainTrackerInstance.Options.FormFont = new Font(cmbStyle.SelectedItem.ToString(), currentFont.Size, FontStyle.Regular);
                UpdateFontInForms();
            };
            fontSelect.Controls.Add(cmbStyle);
            //Default button
            Button Default = new Button();
            Default.Text = "Set to Default";
            Default.Location = new Point(2, Size.Height + 4 + cmbStyle.Height);
            Default.Width = lbSize.Width + Size.Width + 5;
            Default.Click += (s, ea) =>
            {
                var currentFont = LogicObjects.MainTrackerInstance.Options.FormFont;
                LogicObjects.MainTrackerInstance.Options.FormFont = SystemFonts.DefaultFont;
                currentFont = LogicObjects.MainTrackerInstance.Options.FormFont;
                try { cmbStyle.SelectedIndex = cmbStyle.Items.IndexOf(currentFont.FontFamily.Name); } catch { }
                Size.Value = (decimal)LogicObjects.MainTrackerInstance.Options.FormFont.Size;
                UpdateFontInForms();
            };
            fontSelect.Controls.Add(Default);
            fontSelect.Show();

            void UpdateFontInForms()
            {
                //Main Interface
                MainInterface.CurrentProgram.ResizeObject();

                //LogicEditor
                if (LogicEditor.EditorForm != null && LogicEditor.currentEntry.ID > -1)
                {
                    LogicEditor.EditorForm.WriteCurentItem(LogicEditor.currentEntry.ID);
                }
            }
        }
        //TODO Possible make this configurable? It's very niche but configuration will probably be needed in the case of custom logic
        public static string MutliPriceDisginguisingText(string input)
        {
            var AltNames = new Dictionary<string, string>
            {
                {"ShopItemGoronBomb10InWinter","Winter"},
                {"ShopItemGoronBomb10InSpring","Spring"},
                {"ShopItemGoronArrow10InWinter","Winter"},
                {"ShopItemGoronArrow10InSpring","Spring"},
                {"ShopItemGoronRedPotionInWinter","Winter"},
                {"ShopItemGoronRedPotionInSpring","Spring"},
                {"ItemTingleMapTownInTown","Town"},
                {"ItemTingleMapTownInCanyon","Canyon"},
                {"ItemTingleMapWoodfallInSwamp","Swamp"},
                {"ItemTingleMapWoodfallInTown","Town"},
                {"ItemTingleMapSnowheadInMountain","Mountain"},
                {"ItemTingleMapSnowheadInSwamp","Swamp"},
                {"ItemTingleMapRanchInRanch","Ranch"},
                {"ItemTingleMapRanchInMountain","Mountain"},
                {"ItemTingleMapGreatBayInOcean","Ocean"},
                {"ItemTingleMapGreatBayInRanch","Ranch"},
                {"ItemTingleMapStoneTowerInCanyon","Canyon"},
                {"ItemTingleMapStoneTowerInOcean","Ocean"}
            };
            return AltNames.ContainsKey(input) ? AltNames[input] : input;
        }

        public static string TrimPriceDisplay(string input)
        {
            if (input.IndexOf("(") > -1 && input.IndexOf(")") > -1 && input.IndexOf('(') < input.IndexOf(')'))
            {
                return input.Substring(input.IndexOf("(")+1, input.IndexOf(")") - input.IndexOf("(")-1);
            }
            return input;
        }

        public static string GetPriceText(LogicObjects.LogicEntry Entry, LogicObjects.TrackerInstance Instance)
        {
            if (Entry.Price > -1) { return $" (${Entry.Price})"; }

            int SubPricesFound = 0;
            string MutliPrice = " (";
            if (Entry.Required != null)
            {
                foreach(var i in Entry.Required)
                {
                    if (Instance.Logic[i].Price > -1 && Instance.Logic[i].IsFake)
                    {
                        SubPricesFound++;
                        string DisName = MutliPriceDisginguisingText(Instance.Logic[i].DictionaryName);
                        MutliPrice += $"{DisName}: ${Instance.Logic[i].Price}, ";
                    }
                }
            }
            if (Entry.Conditionals != null)
            {
                foreach(var conditional in Entry.Conditionals)
                {
                    foreach (var i in conditional)
                    {
                        if (Instance.Logic[i].Price > -1 && Instance.Logic[i].IsFake)
                        {
                            SubPricesFound++;
                            string DisName = MutliPriceDisginguisingText(Instance.Logic[i].DictionaryName);
                            MutliPrice += $"{DisName}: ${Instance.Logic[i].Price}, ";
                        }
                    }
                }
            }
            if (MutliPrice.EndsWith(", ")) { MutliPrice = MutliPrice.Substring(0, MutliPrice.Length - 2); }
            MutliPrice += ")";
            return SubPricesFound > 0 ? (SubPricesFound > 1 ? MutliPrice : $" (${Entry.Price})")  : "";
        }

        public static Dictionary<string, int> ReadSpoilerLogPriceLogicMap(LogicObjects.TrackerInstance Instance, Dictionary<string, int> Pricedata)
        {
            var SpoilerPriceDic = new Dictionary<string, int>();
            var TextFile = File.ReadAllLines(@"Recources\Other Files\SpoilerLogPriceLogicMap.txt");

            bool AtGame = true;
            foreach (var line in TextFile)
            {
                var x = line.Trim();
                x = Regex.Replace(x, @"\s+", " ");
                if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                if (x.Contains("//")) { x = x.Substring(0, x.IndexOf("//")); }
                if (x.ToLower().StartsWith("#gamecodestart:"))
                {
                    AtGame = x.ToLower().Replace("#gamecodestart:", "").Trim().Split(',').Select(y => y.Trim()).Contains(Instance.GameCode.ToLower());
                    continue;
                }
                if (x.ToLower().StartsWith("#gamecodeend:")) { AtGame = true; continue; }
                if (!AtGame) { continue; }

                var RestrictionSplit = x.Split('!');
                bool LineValid = true;
                if (RestrictionSplit.Count() > 1)
                {
                    for(var y = 1; y < RestrictionSplit.Count(); y++)
                    {
                        var RestrictionAndSplit = RestrictionSplit[y].Split('&');
                        bool AllItemsInLogic = true;
                        foreach (var j in RestrictionAndSplit) { if (!Instance.DicNameToID.ContainsKey(j.Trim())) { AllItemsInLogic = false; } }
                        if (AllItemsInLogic) { LineValid = false;  }
                    }
                }
                if (!LineValid) { continue; }

                var DicAndPriceData = RestrictionSplit[0].Split('|');
                if (DicAndPriceData.Count() < 2) { continue; }
                var dictionaryNameData = DicAndPriceData[0].Split(',');
                var SpoilerNamedata = DicAndPriceData[1].Split(',');

                string DictionaryName = null;
                int LowestPrice = -1;

                foreach(var i in dictionaryNameData)
                {
                    if (Instance.DicNameToID.ContainsKey(i.Trim()))
                    {
                        DictionaryName = i.Trim();
                        break;
                    }
                }
                foreach (var i in SpoilerNamedata)
                {
                    if (Pricedata.ContainsKey(i.Trim()))
                    {
                        if (LowestPrice == -1 || Pricedata[i.Trim()] < LowestPrice) { LowestPrice = Pricedata[i.Trim()]; }
                    }
                }

                if (DictionaryName != null && LowestPrice > -1 && !SpoilerPriceDic.ContainsKey(DictionaryName)) { SpoilerPriceDic.Add(DictionaryName, LowestPrice); }

            }
            return SpoilerPriceDic;

        }

        public static Dictionary<string, string[]> uselessLogicItems()
        {
            return new Dictionary<string, string[]>()
            {
                { "UpgradeBigBombBag", new string[] { "TradeItemKafeiLetter", "TradeItemPendant" } },
                { "MaskBlast", new string[] { "TradeItemKafeiLetter", "TradeItemPendant" } }
            };
        }

        public static Dictionary<string, string[]> BYOAmmoData()
        {
            return new Dictionary<string, string[]>()
            {
                { "UpgradeBigQuiver", new string[] { "UpgradeBigQuiver", "UpgradeBiggestQuiver" } },
                { "UpgradeBiggestQuiver", new string[] { "UpgradeBigQuiver", "UpgradeBiggestQuiver" } },
                { "HeartPieceSwampArchery", new string[] { "UpgradeBigQuiver", "UpgradeBiggestQuiver" } },
                { "HeartPieceTownArchery", new string[] { "UpgradeBigQuiver", "UpgradeBiggestQuiver" } },
                { "MaskRomani", new string[] { "OtherArrow", "MaskCircusLeader" } },
                { "HeartPieceHoneyAndDarling", new string[] { "ChestInvertedStoneTowerBombchu10", "ChestLinkTrialBombchu10", "ShopItemBombsBombchu10" } }
            };
        }
    }
}
