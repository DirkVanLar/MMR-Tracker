using MMR_Tracker.Forms;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Class_Files
{
    class UserSettings
    {
        class WebPresetData
        {
            public string Name { get; set; } = "";
            public string Logic { get; set; } = "";
            public string Dictionary { get; set; } = "";
        }

        public static void HandleUserPreset(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Clear();
            MainInterface.CurrentProgram.changeLogicToolStripMenuItem.DropDownItems.Clear();
            if (LogicEditor.EditorForm != null)
            {
                LogicEditor.EditorForm.templatesToolStripMenuItem.DropDownItems.Clear();
            }
            List<ToolStripMenuItem> NewPresets = new List<ToolStripMenuItem>();
            List<ToolStripMenuItem> RecreatePresets = new List<ToolStripMenuItem>();
            List<ToolStripMenuItem> LogicEditorPresets = new List<ToolStripMenuItem>();
            int counter = 0;

            ImportPresetFiles();
            AddOtherGamePresets();
            ImportWebPresets();
            ApplyPresetsToMenuItems();

            void ImportPresetFiles()
            {
                if (!Directory.Exists(@"Recources\Other Files\Custom Logic Presets"))
                {
                    try
                    {
                        Directory.CreateDirectory((@"Recources\Other Files\Custom Logic Presets"));
                    }
                    catch { }
                }
                else
                {
                    foreach (var i in Directory.GetFiles(@"Recources\Other Files\Custom Logic Presets").Where(x => x.EndsWith(".txt") && !x.Contains("Web Presets.txt")))
                    {
                        ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem
                        {
                            Name = $"PresetNewLogic{counter}",
                            Size = new System.Drawing.Size(180, 22),
                            Text = Path.GetFileName(i).Replace(".txt", "")
                        };
                        counter++;
                        CustomLogicPreset.Click += (s, ee) => LoadMainLogicPreset(i, "", s, ee, "");
                        NewPresets.Add(CustomLogicPreset);

                        ToolStripMenuItem CustomLogicPresetRecreate = new ToolStripMenuItem
                        {
                            Name = $"PresetChangeLogic{counter}",
                            Size = new System.Drawing.Size(180, 22),
                            Text = Path.GetFileName(i).Replace(".txt", "")
                        };
                        counter++;
                        CustomLogicPresetRecreate.Click += (s, ee) => LoadMainLogicPreset(i, "", s, ee, "", false);
                        RecreatePresets.Add(CustomLogicPresetRecreate);

                        ToolStripMenuItem CustomLogicPresetLogicEditor = new ToolStripMenuItem
                        {
                            Name = $"PresetLogicEditor{counter}",
                            Size = new System.Drawing.Size(180, 22),
                            Text = Path.GetFileName(i).Replace(".txt", "")
                        };
                        counter++;
                        CustomLogicPresetLogicEditor.Click += (s, ee) => LoadEditorLogicPreset(i, "", s, ee);
                        LogicEditorPresets.Add(CustomLogicPresetLogicEditor);
                    }
                }
            }
            void ImportWebPresets()
            {
                if (File.Exists(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt"))
                {
                    var TextFile = File.ReadAllLines(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt");
                    AddPersonalPresets(TextFile);

                    WebPresetData WebEntry = null;

                    foreach (var i in TextFile)
                    {
                        if (i.StartsWith("Name:"))
                        {
                            AddWebEntry();
                            WebEntry = new WebPresetData();
                            WebEntry.Name = Regex.Replace(i, "Name:", "", RegexOptions.IgnoreCase).Trim();
                        }
                        if (i.StartsWith("Dictionary:"))
                        {
                            WebEntry.Dictionary = Regex.Replace(i, "Dictionary:", "", RegexOptions.IgnoreCase).Trim();
                        }
                        if (i.StartsWith("Address:"))
                        {
                            WebEntry.Logic = Regex.Replace(i, "Address:", "", RegexOptions.IgnoreCase).Trim();
                        }
                        counter++;
                    }
                    AddWebEntry();

                    void AddWebEntry()
                    {
                        if (WebEntry != null)
                        {
                            WebPresetData ClickData = new WebPresetData
                            {
                                Logic = WebEntry.Logic,
                                Dictionary = WebEntry.Dictionary
                            };

                            ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem();
                            CustomLogicPreset.Name = $"PresetNewLogic{counter}";
                            CustomLogicPreset.Text = WebEntry.Name;
                            CustomLogicPreset.Click += (s, ee) => LoadMainLogicPreset("", ClickData.Logic, s, ee, ClickData.Dictionary);
                            NewPresets.Add(CustomLogicPreset);

                            ToolStripMenuItem CustomLogicPresetRecreate = new ToolStripMenuItem();
                            CustomLogicPresetRecreate.Name = $"PresetChangeLogic{counter}";
                            CustomLogicPresetRecreate.Text = WebEntry.Name;
                            CustomLogicPresetRecreate.Click += (s, ee) => LoadMainLogicPreset("", ClickData.Logic, s, ee, ClickData.Dictionary, false);
                            RecreatePresets.Add(CustomLogicPresetRecreate);

                            ToolStripMenuItem CustomLogicPresetEditor = new ToolStripMenuItem();
                            CustomLogicPresetEditor.Name = $"PresetEditorLogic{counter}";
                            CustomLogicPresetEditor.Text = WebEntry.Name;
                            CustomLogicPresetEditor.Click += (s, ee) => LoadEditorLogicPreset("", ClickData.Logic, s, ee, ClickData.Dictionary);
                            LogicEditorPresets.Add(CustomLogicPresetEditor);
                            //Console.WriteLine($"Adding Web Entry \nName: {CustomLogicPreset.Text}\nDic: {WebEntry.Dictionary}\nLogic: {WebEntry.Logic}");
                        }
                    }

                }
            }
            void ApplyPresetsToMenuItems()
            {
                if (NewPresets.Count() < 1)
                {
                    ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem
                    {
                        Name = "newToolStripMenuItem",
                        Size = new System.Drawing.Size(180, 22),
                        Text = "No Presets Found (Open Folder)"
                    };
                    CustomLogicPreset.Click += (s, ee) => MainInterface.CurrentProgram.presetsToolStripMenuItem_Click(s, ee);
                    MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Add(CustomLogicPreset);
                }
                else
                {
                    foreach (var i in NewPresets.OrderBy(x => x.Text))
                    {
                        MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Add(i);
                    }
                    foreach (var i in RecreatePresets.OrderBy(x => x.Text))
                    {
                        MainInterface.CurrentProgram.changeLogicToolStripMenuItem.DropDownItems.Add(i);
                    }
                    if (LogicEditor.EditorForm != null)
                    {
                        foreach (var i in LogicEditorPresets.OrderBy(x => x.Text))
                        {
                            LogicEditor.EditorForm.templatesToolStripMenuItem.DropDownItems.Add(i);
                        }
                    }
                    ToolStripMenuItem newRecreatePreset = new ToolStripMenuItem
                    {
                        Name = $"PresetChangeLogic{counter + 1}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = "Browse"
                    };
                    newRecreatePreset.Click += (s, ee) => LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance);
                    MainInterface.CurrentProgram.changeLogicToolStripMenuItem.DropDownItems.Add(newRecreatePreset);
                }
            }
            void AddPersonalPresets(string[] TextFile)
            {
                if (Debugging.ISDebugging || Environment.MachineName == "DESKTOP-HBDL7AN")
                {
                    if (!TextFile.Contains("Name: Thedrummonger Glitched Logic"))
                    {
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nName: Thedrummonger Glitched Logic");
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nAddress: https://raw.githubusercontent.com/Thedrummonger/MMR-Logic/master/Logic%20File.txt");
                    }
                    if (!TextFile.Contains("Name: Thedrummonger Entrance Rando"))
                    {
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nName: Thedrummonger Entrance Rando");
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nAddress: https://raw.githubusercontent.com/Thedrummonger/MMR-Logic/Entrance-Radno-Logic/Logic%20File.txt");
                    }
                }
            }
            void AddOtherGamePresets()
            {
                bool AllowOtherGame = false;
                JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };
                LogicObjects.DefaultTrackerOption TrackerDefaultOptions = new LogicObjects.DefaultTrackerOption();
                if (File.Exists("options.txt"))
                {
                    try 
                    { 
                        TrackerDefaultOptions = JsonConvert.DeserializeObject<LogicObjects.DefaultTrackerOption>(File.ReadAllText("options.txt"), _jsonSerializerOptions);
                        AllowOtherGame = TrackerDefaultOptions.OtherGamesOK;
                    }
                    catch { AllowOtherGame = false; }
                }

                if (Debugging.ISDebugging) { AllowOtherGame = true; }

                if (!AllowOtherGame || !Directory.Exists(@"Recources\Other Files\Other Game Premade Logic")) { return; }
                foreach (var i in Directory.GetFiles(@"Recources\Other Files\Other Game Premade Logic").Where(x => x.EndsWith(".txt") && !x.Contains("Web Presets.txt")))
                {
                    ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem
                    {
                        Name = $"PresetNewLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "")
                    };
                    counter++;
                    CustomLogicPreset.Click += (s, ee) => LoadMainLogicPreset(i, "", s, ee, "");
                    NewPresets.Add(CustomLogicPreset);

                    ToolStripMenuItem CustomLogicPresetRecreate = new ToolStripMenuItem
                    {
                        Name = $"PresetChangeLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "")
                    };
                    counter++;
                    CustomLogicPresetRecreate.Click += (s, ee) => LoadMainLogicPreset(i, "", s, ee, "", false);
                    RecreatePresets.Add(CustomLogicPresetRecreate);

                    ToolStripMenuItem CustomLogicPresetEditor = new ToolStripMenuItem
                    {
                        Name = $"PresetChangeLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "")
                    };
                    counter++;
                    CustomLogicPresetEditor.Click += (s, ee) => LoadEditorLogicPreset(i, "", s, ee);
                    LogicEditorPresets.Add(CustomLogicPresetEditor);
                }
            }
        }

        public static void LoadMainLogicPreset(string Path, string WebPath, object sender, EventArgs e, string WebDicOverride, bool New = true)
        {
            Console.WriteLine(WebDicOverride + "Was Dic");
            try
            {
                if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
                string[] Lines = null;
                if (File.Exists(Path))
                {
                    Lines = File.ReadAllLines(Path);
                    Debugging.Log(Path);
                }
                else
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    string webData = wc.DownloadString(WebPath);
                    Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    Debugging.Log(WebPath);
                }

                LogicObjects.LogicDictionary DicOverride = null;
                if (WebDicOverride != "")
                {
                    try
                    {
                        System.Net.WebClient wc = new System.Net.WebClient();
                        string webData = wc.DownloadString(WebDicOverride);
                        var DicLines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        var csv = Utility.ConvertCsvFileToJsonObject(DicLines);
                        DicOverride = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(csv);
                        Debugging.Log(WebDicOverride);
                    }
                    catch (Exception j)
                    {
                        Console.WriteLine("Dictionary Invalid\n" + j);
                        DicOverride = null;
                    }
                }
                if (New)
                {
                    LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();
                    LogicObjects.MainTrackerInstance.LogicDictionary = DicOverride;
                    Tools.CreateTrackerInstance(LogicObjects.MainTrackerInstance, Lines.ToArray());
                }
                else
                {
                    LogicObjects.MainTrackerInstance.LogicDictionary = DicOverride;
                    LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance, Lines);
                }
                MainInterface.CurrentProgram.FormatMenuItems();
                MainInterface.CurrentProgram.ResizeObject();
                MainInterface.CurrentProgram.PrintToListBox();
                MainInterface.FireEvents(sender, e);
                Tools.UpdateTrackerTitle();
            }
            catch
            {
                MessageBox.Show("Preset File Invalid! If you have not tampered with the preset files in \"Recources\\Other Files\\\" Please report this issue. Otherwise, redownload or delete those files.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LoadEditorLogicPreset(string Path, string WebPath, object sender, EventArgs e, string WebDicOverride = "")
        {
            try
            {
                if (LogicEditor.EditorForm == null) { return; }

                var LEditorForm = LogicEditor.EditorForm;

                if (!LEditorForm.PromptSave()) { return; }
                string[] Lines = null;
                if (File.Exists(Path))
                {
                    Lines = File.ReadAllLines(Path);
                    Debugging.Log(Path);
                }
                else
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    string webData = wc.DownloadString(WebPath);
                    Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    Debugging.Log(WebPath);
                }

                LogicObjects.LogicDictionary DicOverride = null;
                if (WebDicOverride != "")
                {
                    try
                    {
                        System.Net.WebClient wc = new System.Net.WebClient();
                        string webData = wc.DownloadString(WebDicOverride);
                        var DicLines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        var csv = Utility.ConvertCsvFileToJsonObject(DicLines);
                        DicOverride = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(csv);
                        Debugging.Log(WebDicOverride);
                    }
                    catch { DicOverride = null; }
                }

                LogicEditor.EditorInstance = new LogicObjects.TrackerInstance();
                LogicEditor.EditorInstance.LogicDictionary = DicOverride;
                Tools.CreateTrackerInstance(LogicEditor.EditorInstance, Lines.ToArray());

                LogicEditor.AssignUniqueItemnames(LogicEditor.EditorInstance.Logic);
                if (LogicEditor.EditorInstance.Logic.Count < Convert.ToInt32(LEditorForm.nudIndex.Value)) 
                { LEditorForm.nudIndex.Value = LogicEditor.EditorInstance.Logic.Count - 1; }
                LogicEditor.EditorForm.FormatForm(Convert.ToInt32(LEditorForm.nudIndex.Value));
            }
            catch
            {
                MessageBox.Show("Preset File Invalid! If you have not tampered with the preset files in \"Recources\\Other Files\\\" Please report this issue. Otherwise, redownload or delete those files.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
