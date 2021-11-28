using Microsoft.VisualBasic;
using MMR_Tracker;
using MMR_Tracker.Forms;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using MMR_Tracker.Class_Files;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using MMR_Tracker.Forms.Sub_Forms;
using MMR_Tracker.Forms.Extra_Functionality;
using MMR_Tracker.Forms.Core_Tracker;

namespace MMR_Tracker_V2
{
    public partial class MainInterface : Form
    {

        public MainInterface()
        {
            InitializeComponent();
        }

        public static MainInterface CurrentProgram;

        public static PathFinder MainInterfacePathfinderInsance;

        //Event Triggers

        public static event EventHandler LogicStateUpdated = delegate { };


        #region Form Objects
        //Form Events---------------------------------------------------------------------------
        #region Form Events
        private void MainInterface_Load(object sender, EventArgs e)
        {
            //Since only one instance of the main interface should ever be open, We can store that instance in a variable to be called from static code.
            if (CurrentProgram != null) { Close(); return; }
            CurrentProgram = this;

            //Ensure the current directory is always the base directory in case the application is opened from a MMRTSave file elsewhere on the system
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            
            DateTime date = DateTime.Now;
            string DateString = date.ToString("dd-MM-yy-HH-mm-ss-ff");
            Debugging.LogFile = @"Recources\Logs\Log-" + DateString + ".txt";

            Debugging.ISDebugging = ((Control.ModifierKeys != Keys.Control) && Debugger.IsAttached);
            Debugging.ViewAsUserMode = ((Control.ModifierKeys == Keys.Control) && Debugger.IsAttached);

            Tools.CreateOptionsFile();
            if (VersionHandeling.GetLatestTrackerVersion()) { this.Close(); }
            HandleStartArgs(sender, e, Environment.GetCommandLineArgs());
            ResizeObject();
            FormatMenuItems();
            UserSettings.HandleUserPreset(sender, e);
            Tools.UpdateTrackerTitle();
        }

        private void MainInterface_ResizeEnd(object sender, EventArgs e) { ResizeObject(); }

        private void MainInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !Tools.PromptSave(LogicObjects.MainTrackerInstance);
        }

        private void MainInterface_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S)
            {
                SaveToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion Form Events
        //Menu Strip---------------------------------------------------------------------------
        #region Form Events
        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.Redo(LogicObjects.MainTrackerInstance, LogicObjects.MaintrackerInstanceUndoRedoData);
            PrintToListBox();
            FormatMenuItems();
            FireEvents(sender, e);
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.Undo(LogicObjects.MainTrackerInstance, LogicObjects.MaintrackerInstanceUndoRedoData);
            PrintToListBox();
            FormatMenuItems();
            FireEvents(sender, e);
        }
        #endregion Form Events
        //Menu Strip => File---------------------------------------------------------------------------
        #region File
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == saveAsToolStripMenuItem)
            {
                Tools.SaveInstance(LogicObjects.MainTrackerInstance, true);
            }
            else
            {
                Tools.SaveInstance(LogicObjects.MainTrackerInstance, true, Tools.SaveFilePath);
            }
            FormatMenuItems();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadMMRTSAVfile(sender, e);
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.fileToolStripMenuItem.HideDropDown();
            if (!Tools.ParseLogicFile()) { return; }
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
            FireEvents(sender, e);
            Tools.UpdateTrackerTitle();
            Tools.SaveFilePath = "";
        }

        public void presetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"Recources\Other Files\Custom Logic Presets");
        }
        #endregion File
        //Menu Strip => Options---------------------------------------------------------------------------
        #region Online Play Options
        private void onlinePlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OnlinePlay.CurrentOpenForm != null) { Debugging.Log("Form already open"); OnlinePlay.CurrentOpenForm.Focus(); return; }
            OnlinePlay.CurrentOpenForm = new OnlinePlay();
            OnlinePlay.CurrentOpenForm.Show();
        }
        #endregion Online Play Options
        //Menu Strip => Options => Logic Options---------------------------------------------------------------------------
        #region Logic Options
        private void EditRadnomizationOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);
            RandomizeOptions RandoOptionScreen = new RandomizeOptions();
            RandoOptionScreen.ShowDialog();
            bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);

            if (!LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
            {
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);
                LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
            }

            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void ImportSpoilerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instance = LogicObjects.MainTrackerInstance;
            if (Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic))
            {
                var result = MessageBox.Show("are you sure you want to remove all spoiler data?", "Remove Spoiler Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) { return; }

                foreach (var entry in LogicObjects.MainTrackerInstance.Logic) 
                { 
                    entry.SpoilerRandom = -2;
                    entry.Price = -1;
                    entry.GossipHint = "";
                }
                instance.CurrentSpoilerLog.type = null;
                instance.CurrentSpoilerLog.Log = null;
            }
            else
            {
                string file = (instance.GameCode == "MMR") ? 
                    Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*html)|*html") :
                    Utility.FileSelect("Select A Spoiler Log", "Spoiler Log(*.txt;*.json)|*.txt;*.json");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(instance, file);
                if (!Utility.CheckforSpoilerLog(instance.Logic)) { MessageBox.Show("No spoiler data found!"); return; }
                else if (!Utility.CheckforSpoilerLog(instance.Logic, true, true, true)) { MessageBox.Show("Not all checks have been assigned spoiler data!"); }

                bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(instance);
                Utility.FixSpoilerInconsistency(LogicObjects.MainTrackerInstance);
                bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(instance);
                if (!instance.Options.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
                {
                    instance.Options.EntranceRadnoEnabled = EntrancesRandoAfter;
                    instance.Options.OverRideAutoEntranceRandoEnable = (instance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
                }

                LogicEditing.CalculateItems(instance, fromScratch: true);
            }
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void StricterLogicHandelingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.StrictLogicHandeling = !LogicObjects.MainTrackerInstance.Options.StrictLogicHandeling;
            FormatMenuItems();
        }

        private void UpdateLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optionsToolStripMenuItem.HideDropDown();
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }
        //Menu Strip => Options => Logic Options = > Rando Options----------------------------------------------------------

        private void enableProgressiveItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.ProgressiveItems = !LogicObjects.MainTrackerInstance.Options.ProgressiveItems;
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void enableBringYourOwnAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.BringYourOwnAmmo = !LogicObjects.MainTrackerInstance.Options.BringYourOwnAmmo;
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }
        private void smallKeyDoorsAlwaysOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.Keysy["SmallKey"] = !LogicObjects.MainTrackerInstance.Options.Keysy["SmallKey"];
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void bossKeyDoorsAlwaysOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.Keysy["BossKey"] = !LogicObjects.MainTrackerInstance.Options.Keysy["BossKey"];
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        #endregion Logic Options
        //Menu Strip => Options => Entrance Rando---------------------------------------------------------------------------
        #region Entrance Rando

        private void ToggleEntranceRandoFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
            ResizeObject();
            PrintToListBox();
            FormatMenuItems();
        }

        private void IncludeItemLocationsAsDestinationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.IncludeItemLocations = !LogicObjects.MainTrackerInstance.Options.IncludeItemLocations;
            FormatMenuItems();
        }

        private void CoupleEntrancesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!LogicObjects.MainTrackerInstance.Options.CoupleEntrances) { if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; } }
            LogicObjects.MainTrackerInstance.Options.CoupleEntrances = !LogicObjects.MainTrackerInstance.Options.CoupleEntrances;
            if (!LogicObjects.MainTrackerInstance.Options.CoupleEntrances) { MessageBox.Show("Entrances will not uncouple automatically."); }
            if (LogicObjects.MainTrackerInstance.Options.CoupleEntrances)
            {
                Tools.SetUnsavedChanges(LogicObjects.MainTrackerInstance);
                Tools.SaveState(LogicObjects.MainTrackerInstance, new LogicObjects.SaveState() { Logic = LogicObjects.MainTrackerInstance.Logic }, LogicObjects.MaintrackerInstanceUndoRedoData);
                foreach (var entry in LogicObjects.MainTrackerInstance.Logic)
                {
                    if (entry.Checked && entry.RandomizedItem > -1)
                    {
                        LogicEditing.CheckEntrancePair(LogicObjects.MainTrackerInstance.Logic[entry.ID], LogicObjects.MainTrackerInstance, true);
                    }
                }
                LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance, true);
                PrintToListBox();
            }
            FormatMenuItems();
        }
        #endregion Entrance Rando
        //Menu Strip => Options => Dev---------------------------------------------------------------------------
        #region Dev
        private void CreateDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.CreateDictionary();
        }

        private void PrintLogicObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            Debugging.PrintLogicObject(LogicObjects.MainTrackerInstance.Logic);
            DebugScreen.DebugFunction = 1;
            DebugScreen.Show();
        }

        private void CodeTestingToolStripMenuItem_Click(object sender, EventArgs e) 
        { 
            Debugging.TestDumbStuff();
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            PrintToListBox();
        }

        private void viewAsUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Debugging.ISDebugging && !Debugging.ViewAsUserMode) { return; }
            else if (Debugging.ISDebugging) { Debugging.ISDebugging = false; Debugging.ViewAsUserMode = true; }
            else if (Debugging.ViewAsUserMode) { Debugging.ISDebugging = true; Debugging.ViewAsUserMode = false; }
            FormatMenuItems();
        }

        private void devToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Debugging.ISDebugging && Debugging.ViewAsUserMode)
            {
                Debugging.ISDebugging = true; Debugging.ViewAsUserMode = false;
                FormatMenuItems();
            }
        }
        #endregion Dev
        //Menu Strip => Options => MISC Options---------------------------------------------------------------------------
        #region MISC Options
        private void ShowEntryNameToolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip = !LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip;
            FormatMenuItems();
        }

        private void horizontalLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.HorizontalLayout = !LogicObjects.MainTrackerInstance.Options.HorizontalLayout;
            ResizeObject();
            FormatMenuItems();
        }

        private void SeperateMarkedItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom = !LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom;
            FormatMenuItems();
            PrintToListBox();
        }

        private void ChangeMiddleClickToStarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.MiddleClickStarNotMark = !LogicObjects.MainTrackerInstance.Options.MiddleClickStarNotMark;
            FormatMenuItems();
            PrintToListBox();
        }

        private void ChangeFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.EditFont();
        }
        #endregion MISC Options
        //Menu Strip => Tools---------------------------------------------------------------------------
        #region Tools
        private void SeedCheckerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { MessageBox.Show("Not compatible with multiworld seeds!", "Incompatible", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            SeedChecker SeedCheckerForm = new SeedChecker();
            SeedCheckerForm.Show();
        }

        private void WhatUnlockedThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MiscSingleItemSelect WhatUnlockedSelect = new MiscSingleItemSelect
            {
                Text = "Select Available Item",
                Function = 1,
                Display = 1,
                ListContent = LogicObjects.MainTrackerInstance.Logic.Where(x => x.Available && !x.IsFake).ToList()
            };
            WhatUnlockedSelect.Show();
            return;
        }

        private void LogicEditorToolStripMenuItem_Click(object sender, EventArgs e)
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
        }

        private void PopoutPathfinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PathFinder Poputpathfinder = new PathFinder();
            Poputpathfinder.Show();
        }

        private void FilterMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map FilterMap = new Map();
            FilterMap.Show();
        }

        private void ItemTrackerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemDisplay id = new ItemDisplay();
            id.Name = "ItemDisplay";
            id.MainInterfaceInstance = this;
            id.Show();

        }

        private void spoilerLogConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpoilerLogConverter spoilerLogConverter = new SpoilerLogConverter();
            spoilerLogConverter.Show();
        }
        #endregion Tools
        //Menu strip => Info---------------------------------------------------------------------------
        #region Info
        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            Debugging.PrintLogicObject(LogicObjects.MainTrackerInstance.Logic);
            DebugScreen.DebugFunction = 2;
            DebugScreen.Show();
        }

        private void IkanaWellMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //string link = "https://lh3.googleusercontent.com/C0lTSDAQVpM_AeYM_WAGsbFCXvOLHkrgw2pFjh5BGLKfyyIs-S8iUboYrapNpiHIYqEKdQTrLPSCkG-EBOztDKnhEfDNu-IqXspp5cjfmjumpEYqGb6u_-h0SpUsR28c41NljrXIJA";
                string link = "https://cdn.discordapp.com/attachments/547922736257957914/707453444398645278/wells.png";
                Form form = new Form();
                var request = WebRequest.Create(link);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
                form.Width = 500;
                form.Height = 500;
                form.BackgroundImageLayout = ImageLayout.Stretch;
                form.Text = "Showing Web page: " + link;
                form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                form.Show();
            }
            catch
            {
                MessageBox.Show("This image source is currently unavailable.\n\nTo avoid copyright and ensure creators are properly credited for their work, certain images are not hosted locally within the tracker and are pulled live from the web. You are seeing this message because the source of this image has become unavailable, please report this using the links in the about page.", $"Image source not available");
            }
        }

        private void WoodsOfMysteryRouteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string link = "https://gamefaqs.gamespot.com/n64/197770-the-legend-of-zelda-majoras-mask/map/761?raw=1";
                Form form = new Form();
                var request = WebRequest.Create(link);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
                form.Width = 500;
                form.Height = 500;
                form.BackgroundImageLayout = ImageLayout.Stretch;
                form.Text = "Showing Web page: " + link;
                form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                form.Show();
            }
            catch
            {
                try
                {
                    string link = "https://www.zeldadungeon.net/Zelda06/Walkthrough/02/2b~Mystery2.jpg";
                    Form form = new Form();
                    var request = WebRequest.Create(link);
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
                    form.Width = 500;
                    form.Height = 500;
                    form.BackgroundImageLayout = ImageLayout.Stretch;
                    form.Text = "Showing Web page: " + link;
                    form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                    form.Show();
                }
                catch
                {
                    MessageBox.Show("This image source is currently unavailable.\n\nTo avoid copyright and ensure creators are properly credited for their work, certain images are not hosted locally within the tracker and are pulled live from the web. You are seeing this message because the source of this image has become unavailable, please report this using the links in the about page.", $"Image source not available");
                }
            }
        }

        private void BombersCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = (LogicObjects.MainTrackerInstance.Options.BomberCode == "") ? "Enter your bombers code below." : "Bomber code: \n" + LogicObjects.MainTrackerInstance.Options.BomberCode + "\nEnter a new code to change it.";
            string name = Interaction.InputBox(text, "Bomber Code", "");
            if (name != "") { LogicObjects.MainTrackerInstance.Options.BomberCode = name; }
        }

        private void LotteryNumbersStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = (LogicObjects.MainTrackerInstance.Options.LotteryNumber == "") ? "Enter your Lottery Number(s) below." : "Lottery Number(s): \n" + LogicObjects.MainTrackerInstance.Options.LotteryNumber + "\nEnter Lottery Number(s) to change it.";
            string name = Interaction.InputBox(text, "Lottery Number(s)", "");
            if (name != "") { LogicObjects.MainTrackerInstance.Options.LotteryNumber = name; }
        }

        private void OcarinaSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form form = new Form
                {
                    BackgroundImage = Bitmap.FromFile(@"Recources\Images\Ocarina Songs.PNG"),
                    Width = 500,
                    Height = 500,
                    BackgroundImageLayout = ImageLayout.Stretch
                };
                form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                form.Show();
            }
            catch
            {
                MessageBox.Show("Recourse not available. Redownload the tracker and ensure you extract all of the contents.");
            }
        }

        private void curiosityShopBottlePurchaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() => MessageBox.Show("Blue Rupee (5):\nMushroom, Half Milk\n\nRed Rupee (20):\nFish, Bug, Fairy, Any Spring Water, Zora Egg, Any Potion, Milk\n\nPurple Rupee (50):\nSmall Poe\n\nGold Rupee(200):\nGold Dust, Chateau, Big Poe", "Curiosity Shop Bottle Purchases", MessageBoxButtons.OK));
            t.Start();
        }

        private void indexWarpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form form = new Form
                {
                    BackgroundImage = Bitmap.FromFile(@"Recources\Images\IndexWarp.PNG"),
                    Width = 250,
                    Height = 400,
                    BackgroundImageLayout = ImageLayout.Stretch
                };
                form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                form.Show();
            }
            catch
            {
                MessageBox.Show("Recourse not available. Redownload the tracker and ensure you extract all of the contents.");
            }
        }

        private void zoraTrialMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string link = "https://cdn.discordapp.com/attachments/547922736257957914/598641931484135483/unknown.png";
                Form form = new Form();
                var request = WebRequest.Create(link);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
                form.Width = 500;
                form.Height = 500;
                form.BackgroundImageLayout = ImageLayout.Stretch;
                form.Text = "Showing Web page: " + link;
                form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                form.Show();
            }
            catch
            {
                MessageBox.Show("This image source is currently unavailable.\n\nTo avoid copyright and ensure creators are properly credited for their work, certain images are not hosted locally within the tracker and are pulled live from the web. You are seeing this message because the source of this image has become unavailable, please report this using the links in the about page.", $"Image source not available");
            }
        }

        private void lensCavePathsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string link = "https://cdn.discordapp.com/attachments/547922736257957914/737152928334348328/Invisible-paths.png";
                Form form = new Form();
                var request = WebRequest.Create(link);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
                form.Width = 500;
                form.Height = 500;
                form.BackgroundImageLayout = ImageLayout.Stretch;
                form.Text = "Showing Web page: " + link;
                form.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon());
                form.Show();
            }
            catch
            {
                MessageBox.Show("This image source is currently unavailable.\n\nTo avoid copyright and ensure creators are properly credited for their work, certain images are not hosted locally within the tracker and are pulled live from the web. You are seeing this message because the source of this image has become unavailable, please report this using the links in the about page.", $"Image source not available");
            }
        }

        private void goronGraveLadderClimbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() => MessageBox.Show("From the ground climb:\n23 Up, \n8 right, \n26 up, \n6 left, \n26 up, \n15 right, \nup from there", "Lensless Goron Grave", MessageBoxButtons.OK));
            t.Start();
        }

        #endregion Info
        //Text Boxes---------------------------------------------------------------------------
        #region Text Boxes
        private void TXT_TextChanged(object sender, EventArgs e) 
        { 
            if (sender == TXTLocSearch) { PrintToListBox(1); }
            if (sender == TXTEntSearch) { PrintToListBox(2); }
            if (sender == TXTCheckedSearch) { PrintToListBox(3); }
        }

        private void TXT_MouseClick(object sender, MouseEventArgs e)
        {
            var TB = sender as TextBox;
            if (e.Button == MouseButtons.Middle) { TB.Clear(); }
        }
        #endregion Text Boxes
        //List Boxes---------------------------------------------------------------------------
        #region List Boxes
        private void LB_DoubleClick(object sender, EventArgs e) 
        { 
            if (sender as ListBox == LBPathFinder && MainInterfacePathfinderInsance != null)
            {
                MainInterfacePathfinderInsance.LBPathFinder_DoubleClick(sender, e);
            }
            else
            {
                CheckItemSelected(sender as ListBox, true);
            }
        }

        private void LB_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, sender as ListBox); }

        private void LB_MouseUp(object sender, MouseEventArgs e)
        {
            var LB = sender as ListBox;
            int index = LB.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(LB.Items[index] is LogicObjects.ListItem)) { return; }
            if (e.Button == MouseButtons.Middle)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { LB.SelectedItems.Clear(); }
                LB.SetSelected(index, true);
                if (LogicObjects.MainTrackerInstance.Options.MiddleClickStarNotMark || LB == LBCheckedLocations) { StarItemSelected(LB); }
                else { CheckItemSelected(LB, false); }
                
            }
            else if (e.Button == MouseButtons.Right)
            {
                LB.SelectedItems.Clear();
                LB.SetSelected(index, true);
                this.ActiveControl = LB;

            }
        }

        private void LB_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = LogicObjects.MainTrackerInstance.Options.FormFont;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;
            if (LB.Items[e.Index] is LogicObjects.ListItem ListEntry && sender != LBPathFinder)
            {
                var item = ListEntry.LocationEntry;
                if (item.ID < 0) { F = new Font(F.FontFamily, F.Size, FontStyle.Regular); }
                else if (item.HasRandomItem(false) && !item.Available && item.Starred) { F = new Font(F.FontFamily, F.Size, FontStyle.Bold | FontStyle.Strikeout); }
                else if (item.Starred) { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                else if (item.HasRandomItem(false) && !item.Available) { F = new Font(F.FontFamily, F.Size, FontStyle.Strikeout); }
            }
            e.Graphics.DrawString(LB.Items[e.Index].ToString(), F, brush, e.Bounds);

            e.DrawFocusRectangle();
        }
        #endregion List Boxes
        //Buttons---------------------------------------------------------------------------
        #region Buttons
        private void BTNSetItem_Click(object sender, EventArgs e) { CheckItemSelected(LBValidLocations, false); }

        private void BTNSetEntrance_Click(object sender, EventArgs e) { CheckItemSelected(LBValidEntrances, false); }

        private void BTNFindPath_Click(object sender, EventArgs e)
        {
            MainInterfacePathfinderInsance = new PathFinder();
            MainInterfacePathfinderInsance.GetAndPrintPaths(LBPathFinder, CMBStart, CMBEnd);

        }
        #endregion Buttons
        //Other---------------------------------------------------------------------------
        #region Other
        private void CHKShowAll_CheckedChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void CMBStart_DropDown(object sender, EventArgs e) 
        {
            PrintToComboBox(true); AdjustCMBWidth(sender);
        }

        private void CMBEnd_DropDown(object sender, EventArgs e) 
        {
            PrintToComboBox(false); AdjustCMBWidth(sender);
        }

        private void DestinationLabel_DoubleClick(object sender, EventArgs e)
        {
            LogicObjects.LogicEntry newStart = null;
            LogicObjects.LogicEntry newDest = null;
            try
            {
                int DestIndex = Int32.Parse(CMBEnd.SelectedValue.ToString());
                newStart = LogicObjects.MainTrackerInstance.Logic[DestIndex].PairedEntry(LogicObjects.MainTrackerInstance);
            }
            catch { }
            try
            {
                int StartIndex = Int32.Parse(CMBStart.SelectedValue.ToString());
                newDest = LogicObjects.MainTrackerInstance.Logic[StartIndex].PairedEntry(LogicObjects.MainTrackerInstance);
            }
            catch { }
            try
            {
                CMBStart.DataSource = new BindingSource(new Dictionary<int, string> { { newStart.ID, newStart.ItemName } }, null);
                CMBStart.DisplayMember = "Value";
                CMBStart.ValueMember = "key";
                CMBStart.SelectedIndex = 0;
            }
            catch { }
            try
            {
                CMBEnd.DataSource = new BindingSource(new Dictionary<int, string> { { newDest.ID, newDest.LocationName } }, null);
                CMBEnd.DisplayMember = "Value";
                CMBEnd.ValueMember = "key";
                CMBEnd.SelectedIndex = 0;
            }
            catch { }
        }
        private void PresetDropDownOpening(object sender, EventArgs e)
        {
            UserSettings.HandleUserPreset(sender, e);
        }
        #endregion Other
        #endregion Form Objects
        #region Functions
        //Context Menu Functions---------------------------------------------------------------------------
        #region Context Menu
        private void CreateMenu()
        {
            //LBValidLocations List Box
            ContextMenuStrip LocationContextMenu = new ContextMenuStrip();
            LocationContextMenu.Opening += (sender, e) => ContextMenu_Opening(LBValidLocations, e);
            ToolStripItem WhatUnlcoked = LocationContextMenu.Items.Add("What Unlocked This?");
            WhatUnlcoked.Click += (sender, e) => { RunMenuItems(0, LBValidLocations); };
            ToolStripItem ShowRequired = LocationContextMenu.Items.Add("Show Requirements");
            ShowRequired.Click += (sender, e) => { RunMenuItems(9, LBValidLocations); };
            if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando())
            {
                ToolStripItem NavigateHere = LocationContextMenu.Items.Add("Navigate to this check");
                NavigateHere.Click += (sender, e) => { RunMenuItems(5, LBValidLocations); };
            }
            ToolStripItem Filter = LocationContextMenu.Items.Add("Filter at this Location");
            Filter.Click += (sender, e) => { RunMenuItems(3, LBValidLocations); };
            ToolStripItem GroupFilter = LocationContextMenu.Items.Add("Filter at Locations near this area");
            GroupFilter.Click += (sender, e) => { RunMenuItems(4, LBValidLocations); };
            ToolStripItem Check = LocationContextMenu.Items.Add("Check This Item");
            Check.Click += (sender, e) => { RunMenuItems(1, LBValidLocations); };
            ToolStripItem Mark = LocationContextMenu.Items.Add("Mark This Item");
            Mark.Click += (sender, e) => { RunMenuItems(2, LBValidLocations); };
            ToolStripItem Star = LocationContextMenu.Items.Add("Star This Item");
            Star.Click += (sender, e) => { StarItemSelected(LBValidLocations); };
            ToolStripItem RemoveSpoiler = LocationContextMenu.Items.Add("Remove Spoiler Data");
            RemoveSpoiler.Click += (sender, e) => { RunMenuItems(7, LBValidLocations); };
            ToolStripItem SetItemPrice = LocationContextMenu.Items.Add("Set Check Price");
            SetItemPrice.Click += (sender, e) => { RunMenuItems(10, LBValidLocations); };
            LBValidLocations.ContextMenuStrip = LocationContextMenu;

            //LBValidEntrances List Box
            ContextMenuStrip EntranceContextMenu = new ContextMenuStrip();
            EntranceContextMenu.Opening += (sender, e) => ContextMenu_Opening(LBValidEntrances, e);
            ToolStripItem EWhatUnlcoked = EntranceContextMenu.Items.Add("What Unlocked This?");
            EWhatUnlcoked.Click += (sender, e) => { RunMenuItems(0, LBValidEntrances); };
            ToolStripItem EShowRequired = EntranceContextMenu.Items.Add("Show Requirements");
            EShowRequired.Click += (sender, e) => { RunMenuItems(9, LBValidEntrances); };
            if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando())
            {
                ToolStripItem ENavigateHere = EntranceContextMenu.Items.Add("Navigate to this entrance");
                ENavigateHere.Click += (sender, e) => { RunMenuItems(5, LBValidEntrances); };
            }
            ToolStripItem EFilter = EntranceContextMenu.Items.Add("Filter at this Location");
            EFilter.Click += (sender, e) => { RunMenuItems(3, LBValidEntrances); };
            ToolStripItem EGroupFilter = EntranceContextMenu.Items.Add("Filter at Locations near this area");
            EGroupFilter.Click += (sender, e) => { RunMenuItems(4, LBValidEntrances); };
            ToolStripItem ECheck = EntranceContextMenu.Items.Add("Check This Item");
            ECheck.Click += (sender, e) => { RunMenuItems(1, LBValidEntrances); };
            ToolStripItem EMark = EntranceContextMenu.Items.Add("Mark This Item");
            EMark.Click += (sender, e) => { RunMenuItems(2, LBValidEntrances); };
            ToolStripItem EStar = EntranceContextMenu.Items.Add("Star This Item");
            EStar.Click += (sender, e) => { StarItemSelected(LBValidEntrances); };
            ToolStripItem ERemoveSpoiler = EntranceContextMenu.Items.Add("Remove Spoiler Data");
            ERemoveSpoiler.Click += (sender, e) => { RunMenuItems(7, LBValidEntrances); };
            LBValidEntrances.ContextMenuStrip = EntranceContextMenu;

            //LBCheckedLocations List Box
            ContextMenuStrip CheckContextMenu = new ContextMenuStrip();
            CheckContextMenu.Opening += (sender, e) => ContextMenu_Opening(LBCheckedLocations, e);
            ToolStripItem CWhatUnlcoked = CheckContextMenu.Items.Add("What Unlocked This?");
            CWhatUnlcoked.Click += (sender, e) => { RunMenuItems(0, LBCheckedLocations); };
            ToolStripItem CShowRequired = CheckContextMenu.Items.Add("Show Requirements");
            CShowRequired.Click += (sender, e) => { RunMenuItems(9, LBCheckedLocations); };
            if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando())
            {
                ToolStripItem ENavigateHere = CheckContextMenu.Items.Add("Navigate to this check");
                ENavigateHere.Click += (sender, e) => { RunMenuItems(5, LBCheckedLocations); };
            }
            ToolStripItem CFilter = CheckContextMenu.Items.Add("Filter at this Location");
            CFilter.Click += (sender, e) => { RunMenuItems(3, LBCheckedLocations); };
            ToolStripItem CGroupFilter = CheckContextMenu.Items.Add("Filter at Locations near this area");
            CGroupFilter.Click += (sender, e) => { RunMenuItems(4, LBCheckedLocations); };
            ToolStripItem CCheck = CheckContextMenu.Items.Add("Un Check This Item");
            CCheck.Click += (sender, e) => { RunMenuItems(1, LBCheckedLocations); };
            ToolStripItem CMCheck = CheckContextMenu.Items.Add("Un Check This Item (Keep Marked)");
            CMCheck.Click += (sender, e) => { RunMenuItems(6, LBCheckedLocations); };
            ToolStripItem CStar = CheckContextMenu.Items.Add("Star This Item");
            CStar.Click += (sender, e) => { StarItemSelected(LBCheckedLocations); };
            ToolStripItem CReCheck = CheckContextMenu.Items.Add("Change Checked Item");
            CReCheck.Click += (sender, e) => { RunMenuItems(8, LBCheckedLocations); };
            LBCheckedLocations.ContextMenuStrip = CheckContextMenu;

            //Set Item Button
            ContextMenuStrip SetItemMenu = new ContextMenuStrip();
            ToolStripItem ItemSetAll = SetItemMenu.Items.Add("Set Only");
            ItemSetAll.Click += (sender, e) => { CheckItemSelected(LBValidLocations, false, 1); };
            ToolStripItem ItemUnSetAll = SetItemMenu.Items.Add("Unset Only");
            ItemUnSetAll.Click += (sender, e) => { CheckItemSelected(LBValidLocations, false, 2); };
            ToolStripItem ItemToggleStar = SetItemMenu.Items.Add("Toggle Star");
            ItemToggleStar.Click += (sender, e) => { StarItemSelected(LBValidLocations); };
            ToolStripItem ItemStar = SetItemMenu.Items.Add("Star");
            ItemStar.Click += (sender, e) => { StarItemSelected(LBValidLocations, 1); };
            ToolStripItem ItemUnStar = SetItemMenu.Items.Add("Unstar");
            ItemUnStar.Click += (sender, e) => { StarItemSelected(LBValidLocations, 2); };
            BTNSetItem.ContextMenuStrip = SetItemMenu;

            //Set Entrance Button
            ContextMenuStrip SetLocationMenu = new ContextMenuStrip();
            ToolStripItem EntranceSetAll = SetLocationMenu.Items.Add("Set Only");
            EntranceSetAll.Click += (sender, e) => { CheckItemSelected(LBValidEntrances, false, 1); };
            ToolStripItem EntranceUnSetAll = SetLocationMenu.Items.Add("Unset Only");
            EntranceUnSetAll.Click += (sender, e) => { CheckItemSelected(LBValidEntrances, false, 2); };
            ToolStripItem EntranceToggleStar = SetLocationMenu.Items.Add("Toggle Star");
            EntranceToggleStar.Click += (sender, e) => { StarItemSelected(LBValidEntrances); };
            ToolStripItem EntranceStar = SetLocationMenu.Items.Add("Star");
            EntranceStar.Click += (sender, e) => { StarItemSelected(LBValidEntrances, 1); };
            ToolStripItem EntranceUnStar = SetLocationMenu.Items.Add("Unstar");
            EntranceUnStar.Click += (sender, e) => { StarItemSelected(LBValidEntrances, 2); };
            BTNSetEntrance.ContextMenuStrip = SetLocationMenu;
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var LB = sender as ListBox;
                var index = LB.IndexFromPoint(LB.PointToClient(Cursor.Position));
                if (index < 0) { e.Cancel = true; }
                if (!(LB.Items[index] is LogicObjects.ListItem)) { e.Cancel = true; }
                if ((LB.Items[index] as LogicObjects.ListItem) == null) { e.Cancel = true; return; }
                if ((LB.Items[index] as LogicObjects.ListItem).LocationEntry.ID < 0) { e.Cancel = true; }

                //If the entry has spoiler data, the change checked item function does nothing
                if (sender is ListBox && (sender as ListBox).ContextMenuStrip != null)
                {
                    foreach (var j in (sender as ListBox).ContextMenuStrip.Items)
                    {
                        if (j is ToolStripItem && j.ToString() == "Change Checked Item")
                        {
                            (j as ToolStripItem).Visible = (LB.Items[index] as LogicObjects.ListItem).LocationEntry.SpoilerRandom < -1;
                        }
                    }
                }

            }
            catch { e.Cancel = true; }
        }

        private void RunMenuItems(int Function, ListBox ActiveListBox)
        {
            LogicObjects.LogicEntry ActiveItem;
            if ((ActiveListBox == LBValidLocations) && LBValidLocations.SelectedItem is LogicObjects.ListItem)
            { ActiveItem = (LBValidLocations.SelectedItem as LogicObjects.ListItem).LocationEntry; }
            else if ((ActiveListBox == LBValidEntrances) && LBValidEntrances.SelectedItem is LogicObjects.ListItem)
            { ActiveItem = (LBValidEntrances.SelectedItem as LogicObjects.ListItem).LocationEntry; }
            else if ((ActiveListBox == LBCheckedLocations) && LBCheckedLocations.SelectedItem is LogicObjects.ListItem)
            { ActiveItem = (LBCheckedLocations.SelectedItem as LogicObjects.ListItem).LocationEntry; }
            else { return; }

            switch (Function)
            {
                case 0:
                    Tools.WhatUnlockedThis(ActiveItem);
                    break;
                case 1:
                    CheckItemSelected(ActiveListBox, true);
                    break;
                case 2:
                    CheckItemSelected(ActiveListBox, false);
                    break;
                case 3:
                    if ((ActiveListBox == LBValidLocations)) { TXTLocSearch.Text = "=#" + ActiveItem.LocationArea; }
                    else if ((ActiveListBox == LBValidEntrances)) { TXTEntSearch.Text = "=#" + ActiveItem.LocationArea; }
                    else if ((ActiveListBox == LBCheckedLocations)) { TXTCheckedSearch.Text = "=#" + ActiveItem.LocationArea; }
                    break;
                case 4:
                    TextBox SearchBox;
                    if ((ActiveListBox == LBValidLocations)) { SearchBox = TXTLocSearch; }
                    else if ((ActiveListBox == LBValidEntrances)) { SearchBox = TXTEntSearch; }
                    else if ((ActiveListBox == LBCheckedLocations)) { SearchBox = TXTCheckedSearch; }
                    else { return; }
                    List<Map.LocationArea> LocationDic = new List<Map.LocationArea>();
                    Map MapUtils = new Map();
                    MapUtils.setLocationDic(LocationDic);
                    foreach (var i in LocationDic)
                    {
                        foreach (var j in i.SubAreas)
                        {
                            if (ActiveItem.LocationArea.ToLower() == j.Replace("#", "").ToLower())
                            {
                                SearchBox.Text = MapUtils.CreateFilter(0, i.SubAreas);
                                return;
                            }
                        }
                    }
                    break;
                case 5:
                    CMBEnd.DataSource = new BindingSource(new Dictionary<int, string> { { ActiveItem.ID, ActiveItem.LocationName } }, null);
                    CMBEnd.DisplayMember = "Value";
                    CMBEnd.ValueMember = "key";
                    CMBEnd.SelectedIndex = 0;
                    break;
                case 6:
                    CheckItemSelected(ActiveListBox, true, 0, true);
                    break;
                case 7:
                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove this items spoiler data? The only way to recover spoiler data is reimporting the spoiler log.", "Remove Spoiler Data?", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes) { ActiveItem.SpoilerRandom = -2; ActiveItem.GossipHint = ""; ActiveItem.Price = -1; }
                    break;
                case 8:
                    Tools.SaveState(LogicObjects.MainTrackerInstance, new LogicObjects.SaveState() { Logic = LogicObjects.MainTrackerInstance.Logic }, LogicObjects.MaintrackerInstanceUndoRedoData);
                    for (var i = 0; i < 2; i++)
                    {
                        ListBox ItemList = new ListBox();
                        ItemList.Items.Add(ActiveItem);
                        ItemList.SelectedItems.Add(ItemList.Items[0]);
                        CheckItemSelected(ItemList, true);
                    }
                    break;
                case 9:
                    RequirementCheck Req = new RequirementCheck
                    {
                        Instance = LogicObjects.MainTrackerInstance,
                        entry = ActiveItem
                    };
                    Req.Show();
                    break;
                case 10:
                    string CurrenPrice = ActiveItem.Price > -1 ? ActiveItem.Price.ToString() : "";
                    string input = Interaction.InputBox("Enter the price of this check. Leave blank for no price.", "Enter Price", CurrenPrice);
                    int CheckPrice = -1;
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        ActiveItem.Price = -1;
                    }
                    else
                    {
                        try { CheckPrice = int.Parse(input); }
                        catch { }
                        if (CheckPrice > -2) { ActiveItem.Price = CheckPrice; }
                        else { MessageBox.Show("Price not Valid, must be a number greater than 0"); }
                    }
                    LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
                    PrintToListBox();
                    break;
            }

        }
        #endregion Context Menu
        // List/combo Box Functions---------------------------------------------------------------------------
        #region List/combo Box

        public void PrintToListBox(int Container = 0)
        {
            if (!LogicObjects.MainTrackerInstance.Logic.Any(x => x.AppearsInListbox())) { return; }
            LBValidLocations.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            LBValidEntrances.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            LBCheckedLocations.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            var lbLocTop = LBValidLocations.TopIndex;
            var lbEntTop = LBValidEntrances.TopIndex;
            var lbCheckTop = LBCheckedLocations.TopIndex;
            int TotalLoc = 0;
            int TotalEnt = 0;
            int totalchk = 0;

            Dictionary<string, int> Groups = new Dictionary<string, int>();
            if (File.Exists(@"Recources\Other Files\Categories.txt"))
            {
                //Groups = File.ReadAllLines(@"Recources\Other Files\Categories.txt")
                //    .Select(x => x.ToLower().Trim()).Distinct()
                //    .Select((value, index) => new { value, index })
                //    .ToDictionary(pair => pair.value, pair => pair.index);

                bool AtGame = true;
                foreach (var i in File.ReadAllLines(@"Recources\Other Files\Categories.txt"))
                {
                    var x = i.ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                    if (x.StartsWith("#gamecodestart:"))
                    {
                        AtGame = x.Replace("#gamecodestart:", "").Trim().Split(',')
                            .Select(y => y.Trim()).Contains(LogicObjects.MainTrackerInstance.GameCode.ToLower()) ;
                        continue;
                    }
                    if (x.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                    //Console.WriteLine($"{x} Is Valid {AtGame}");

                    if (!Groups.ContainsKey(x) && AtGame)
                    {
                        Groups.Add(x, Groups.Count());
                    }
                }
                
            }

            var mi = LogicObjects.MainTrackerInstance;
            List<LogicObjects.ListItem> ListItems = new List<LogicObjects.ListItem>();
            foreach(var entry in LogicObjects.MainTrackerInstance.Logic)
            {
                //Add starting items to Checked Items
                if (entry.StartingItem())
                {
                    var MultiWorldEntry = new LogicObjects.LogicEntry
                    {
                        ID = -2,
                        DictionaryName = "Starting Item",
                        Checked = true,
                        RandomizedItem = entry.ID,
                        SpoilerRandom = entry.ID,
                        Options = 0,
                        LocationArea = "Starting Items",
                        ItemSubType = "Item"
                    };
                    var Name = createDisplayName(true, MultiWorldEntry, mi);
                    var LBItem = new LogicObjects.ListItem() { Container = 3, LocationEntry = MultiWorldEntry, ItemEntry = entry, DisplayName = Name, Header = MultiWorldEntry.LocationArea };
                    totalchk++;
                    if (Utility.FilterSearch(MultiWorldEntry, TXTCheckedSearch.Text, Name, entry)) { ListItems.Add(LBItem); }
                }
                if (!entry.AppearsInListbox() || entry.LocationName == null) { continue; }
                //Add Entry to Available Locations
                if (!entry.Checked && (entry.Available || entry.HasRandomItem(true) || CHKShowAll.Checked || TXTLocSearch.Text.StartsWith("^")))
                {
                    var Name = createDisplayName(false, entry, mi);
                    var LBItem = new LogicObjects.ListItem() { Container = 1, LocationEntry = entry, ItemEntry = entry.RandomizedEntry(mi, true), DisplayName = Name, Header = entry.LocationArea };
                    if ((!entry.IsEntrance() || !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled))
                    {
                        TotalLoc++;
                        if (Utility.FilterSearch(entry, TXTLocSearch.Text, Name, entry.RandomizedEntry(mi))) { ListItems.Add(LBItem); }
                    }
                }
                //Add Entry to Available Entrances
                if (!entry.Checked && (entry.Available || entry.HasRandomItem(true) || CHKShowAll.Checked || TXTEntSearch.Text.StartsWith("^")))
                {
                    var Name = createDisplayName(false, entry, mi);
                    var LBItem = new LogicObjects.ListItem() { Container = 2, LocationEntry = entry, ItemEntry = entry.RandomizedEntry(mi, true), DisplayName = Name, Header = entry.LocationArea };
                    if (entry.IsEntrance() && LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled)
                    {
                        TotalEnt++;
                        if (Utility.FilterSearch(entry, TXTEntSearch.Text, Name, entry.RandomizedEntry(mi))) { ListItems.Add(LBItem); }
                    }
                }
                //Add Entry to Checked Locations
                if (entry.Checked)
                {
                    var Name = createDisplayName(true, entry, mi);
                    var LBItem = new LogicObjects.ListItem() { Container = 3, LocationEntry = entry, ItemEntry = entry.RandomizedEntry(mi, true), DisplayName = Name, Header = entry.LocationArea };
                    totalchk++;
                    if (Utility.FilterSearch(entry, TXTCheckedSearch.Text, Name, entry.RandomizedEntry(mi))) { ListItems.Add(LBItem); }
                }
                //Add Items obtained via multiworld to Checked Items
                if (entry.Aquired && mi.Logic.Find(x => x.RandomizedItem == entry.ID && x.ItemBelongsToMe() && x.Checked) == null)
                {
                    var MultiWorldEntry = new LogicObjects.LogicEntry
                    {
                        ID = -1,
                        DictionaryName = (entry.PlayerData.ItemCameFromPlayer == -1 || entry.PlayerData.ItemCameFromPlayer == LogicObjects.MainTrackerInstance.Options.MyPlayerID) ? "Unknown" : $"Player {entry.PlayerData.ItemCameFromPlayer}",
                        Checked = true,
                        RandomizedItem = entry.ID,
                        SpoilerRandom = entry.ID,
                        Options = 0,
                        LocationArea = (entry.PlayerData.ItemCameFromPlayer == -1 || entry.PlayerData.ItemCameFromPlayer == LogicObjects.MainTrackerInstance.Options.MyPlayerID) ? "MISC" : "Multiworld",
                        ItemSubType = "Item"
                    };
                    var Name = createDisplayName(true, MultiWorldEntry, mi);
                    var LBItem = new LogicObjects.ListItem() { Container = 3, LocationEntry = MultiWorldEntry, ItemEntry = entry, DisplayName = Name, Header = MultiWorldEntry.LocationArea };
                    totalchk++;
                    if (Utility.FilterSearch(MultiWorldEntry, TXTCheckedSearch.Text, Name, entry)) { ListItems.Add(LBItem); }
                }

            }

            ListItems = ListItems
                .OrderBy(x => Utility.BoolToInt(x.LocationEntry.IsEntrance()))
                .ThenBy(x => Utility.BoolToInt(x.LocationEntry.HasRandomItem(false) && LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom))
                .ThenBy(x => (Groups.ContainsKey(x.LocationEntry.LocationArea.ToLower().Trim()) ? Groups[x.LocationEntry.LocationArea.ToLower().Trim()] : ListItems.Count() + 1))
                .ThenBy(x => x.LocationEntry.LocationArea)
                .ThenBy(x => x.DisplayName).ToList();

            var lastLocArea = "";
            var lastEntArea = "";
            var lastChkArea = "";

            int AvalableLocations = 0;
            int AvalableEntrances = 0;
            int CheckedLocations = 0;

            if (Container == 0 || Container == 1) { LBValidLocations.Items.Clear(); }
            if (Container == 0 || Container == 2) { LBValidEntrances.Items.Clear(); }
            if (Container == 0 || Container == 3) { LBCheckedLocations.Items.Clear(); }

            LBValidLocations.BeginUpdate();
            LBValidEntrances.BeginUpdate();
            LBCheckedLocations.BeginUpdate();

            foreach (var entry in ListItems)
            {
                if (entry.Container == 1 && (Container == 0 || Container == 1)) 
                { lastLocArea = WriteObject(entry, LBValidLocations, lastLocArea, entry.LocationEntry.HasRandomItem(false)); AvalableLocations++; }
                if (entry.Container == 2 && (Container == 0 || Container == 2)) 
                { lastEntArea = WriteObject(entry, LBValidEntrances, lastEntArea, entry.LocationEntry.HasRandomItem(false)); AvalableEntrances++; }
                if (entry.Container == 3 && (Container == 0 || Container == 3)) 
                { lastChkArea = WriteObject(entry, LBCheckedLocations, lastChkArea, false); CheckedLocations++; }
            }

            LBValidLocations.EndUpdate();
            LBValidEntrances.EndUpdate();
            LBCheckedLocations.EndUpdate();

            if (Container == 0 || Container == 1) { LBValidLocations.TopIndex = lbLocTop; }
            if (Container == 0 || Container == 2) { LBValidEntrances.TopIndex = lbEntTop; }
            if (Container == 0 || Container == 3) { LBCheckedLocations.TopIndex = lbCheckTop; }

            label1.Text = "Available Locations: " + ((AvalableLocations == TotalLoc) ? AvalableLocations.ToString() : (AvalableLocations.ToString() + "/" + TotalLoc.ToString()));
            label2.Text = "Checked locations: " + ((CheckedLocations == totalchk) ? CheckedLocations.ToString() : (CheckedLocations.ToString() + "/" + totalchk.ToString()));
            label3.Text = "Available Entrances: " + ((AvalableEntrances == TotalEnt) ? AvalableEntrances.ToString() : (AvalableEntrances.ToString() + "/" + TotalEnt.ToString()));
        }

        private string WriteObject(LogicObjects.ListItem ListEntry, ListBox lb, string lastArea, bool Marked)
        {
            var entry = ListEntry.LocationEntry;
            bool ShowMarked = (Marked && LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom);
            string returnLastArea = lastArea;
            if (entry.LocationArea != returnLastArea)
            {
                if (returnLastArea != "") { lb.Items.Add(Utility.CreateDivider(lb)); }

                string Header = entry.LocationArea.ToUpper();
                if (ShowMarked && entry.IsEntrance()) { Header += " SET EXITS"; }
                else if (ShowMarked && !entry.IsEntrance()) { Header += " SET ITEMS"; }
                else if (entry.IsEntrance()) { Header += " ENTRANCES"; }
                lb.Items.Add(Header + ":");
                returnLastArea = entry.LocationArea;
            }
            lb.Items.Add(ListEntry);
            return (returnLastArea);
        }


        public void ShowtoolTip(MouseEventArgs e, ListBox lb)
        {
            if (!LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip) { return; }
            int index = lb.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(lb.Items[index] is LogicObjects.LogicEntry || lb.Items[index] is LogicObjects.ListItem)) { return; }
            string DisplayName = lb.Items[index].ToString();
            if (toolTip1.GetToolTip(lb) == DisplayName) { return; }
            if (Utility.IsDivider(DisplayName)) { return; }
            toolTip1.SetToolTip(lb, DisplayName);
        }

        public void PrintToComboBox(bool start)
        {
            var UnsortedPathfinder = new Dictionary<int, string>();
            var UnsortedItemPathfinder = new Dictionary<int, string>();
            foreach (var entry in LogicObjects.MainTrackerInstance.Logic)
            {
                if (entry.IsEntrance())
                {
                    if ((start) ? entry.Aquired : entry.Available) { UnsortedPathfinder.Add(entry.ID, (start) ? entry.ItemName : entry.LocationName); }
                }
                if (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations && !entry.IsEntrance() && !entry.IsFake && entry.Available && !start)
                {
                    UnsortedItemPathfinder.Add(entry.ID, (entry.RandomizedItem > -1) ? entry.LocationName + ": " + LogicObjects.MainTrackerInstance.Logic[entry.RandomizedItem].ItemName : entry.LocationName);
                }
            }
            Dictionary<int, string> sortedPathfinder = UnsortedPathfinder.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

            if (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations && !start)
            {
                Dictionary<int, string> ItemPathFinder = new Dictionary<int, string>();
                Dictionary<int, string> sortedItemPathfinder = UnsortedItemPathfinder.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                ItemPathFinder.Add(-2, "ENTRANCES =============================");
                foreach (KeyValuePair<int, string> p in sortedPathfinder)
                {
                    ItemPathFinder.Add(p.Key, p.Value);
                }
                ItemPathFinder.Add(-1, "ITEM LOCATIONS =============================");
                foreach (KeyValuePair<int, string> p in sortedItemPathfinder)
                {
                    ItemPathFinder.Add(p.Key, p.Value);
                }
                sortedPathfinder = ItemPathFinder;
            }
            if (sortedPathfinder.Values.Count < 1) { return; }
            ComboBox cmb = (start) ? CMBStart : CMBEnd;
            cmb.DataSource = new BindingSource(sortedPathfinder, null);
            cmb.DisplayMember = "Value";
            cmb.ValueMember = "key";
        }

        #endregion List/combo Box
        //Other Functions---------------------------------------------------------------------------
        #region Other Functions
        public void AdjustCMBWidth(object sender)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth = (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

            int newWidth;

            if (senderComboBox.Items.Count < 1) { return; }

            foreach (KeyValuePair<int, string> dictionary in senderComboBox.Items)
            {
                newWidth = (int)g.MeasureString(dictionary.Value, font).Width + vertScrollBarWidth;
                if (width < newWidth) { width = newWidth; }
            }
            senderComboBox.DropDownWidth = width;
        }

        public void CheckItemSelected(ListBox LB, bool FullCheck, int SetFunction = 0, bool UncheckAndMark = false, int ItemsCameFromPlayer = -2, bool FromNet = false)
        {
            var TotalStartTime = System.DateTime.Now.Ticks;
            var StartTime = System.DateTime.Now.Ticks; //Timing how long this takes for testing purposes
            Console.WriteLine("Starting Check=========================================");
            //Set Function: 0 = none, 1 = Always Set, 2 = Always Unset
            if (TXTLocSearch.Text.ToLower() == "enabledev" && LB == LBValidLocations && !FullCheck)
            {
                Debugging.ISDebugging = !Debugging.ISDebugging;
                FormatMenuItems();
                TXTLocSearch.Clear();
                return;
            }
            Debugging.Log("Initial Formatting took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
            StartTime = System.DateTime.Now.Ticks;

            Tools.GetWhatchanged(LogicObjects.MainTrackerInstance.Logic, LB);

            Debugging.Log("Get What Changed 1 took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
            StartTime = System.DateTime.Now.Ticks;

            //We want to save logic at this point but don't want to comit to a full save state
            //var Templogic = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic);
            var CheckedItems = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic);

            Debugging.Log("populating Undo List 1 took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
            StartTime = System.DateTime.Now.Ticks;

            CheckItemForm CIF = new CheckItemForm
            {
                Instance = LogicObjects.MainTrackerInstance,
                LB = LB,
                FullCheck = FullCheck,
                SetFunction = SetFunction,
                KeepChecked = UncheckAndMark,
                FromNetPlayer = ItemsCameFromPlayer
            };
            CIF.BeginCheckItem();

            Debugging.Log("Check form took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
            StartTime = System.DateTime.Now.Ticks;

            Tools.GetWhatchanged(LogicObjects.MainTrackerInstance.Logic, LB, true);

            Debugging.Log("Get What Changed 2 took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
            StartTime = System.DateTime.Now.Ticks;

            if (!CIF.ItemStateChanged && ItemsCameFromPlayer == -2) { return; }
            if (FullCheck)
            {  
                //Now that we have successfully checked/Marked an object we can commit to a full save state
                Tools.SaveState(LogicObjects.MainTrackerInstance, new LogicObjects.SaveState() { Logic = CheckedItems }, LogicObjects.MaintrackerInstanceUndoRedoData, false);
                Tools.SetUnsavedChanges(LogicObjects.MainTrackerInstance);
                Debugging.Log("Save State Application took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
                StartTime = System.DateTime.Now.Ticks;
                LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance, fromScratch: false);
                Debugging.Log("Item Calculation took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
                StartTime = System.DateTime.Now.Ticks;
            }

            if (!FromNet) { OnlinePlay.SendData(OnlinePlay.IPS); }

            PrintToListBox();
            Debugging.Log("List Box Print took " + ((System.DateTime.Now.Ticks - StartTime) / 10000) + " Milisecconds");
            LogicStateUpdated(null, null);
            Debugging.Log("Total Time for Check " + ((System.DateTime.Now.Ticks - TotalStartTime) / 10000) + " Milisecconds");
        }

        public void StarItemSelected(ListBox LB, int SetFunction = 0)
        {
            foreach (var i in LB.SelectedItems)
            {
                if ((i is LogicObjects.ListItem))
                {
                    var j = (i as LogicObjects.ListItem).LocationEntry;
                    if (SetFunction == 0) { j.Starred = !j.Starred; }
                    if (SetFunction == 1) { j.Starred = true; }
                    if (SetFunction == 2) { j.Starred = false; }
                }
            }
            int TopIndex = LB.TopIndex;
            PrintToListBox();
            LB.TopIndex = TopIndex;
        }

        public void LoadMMRTSAVfile(object sender, EventArgs e, string Name = "")
        {
            Tools.LoadInstance(Name);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
            FireEvents(sender, e);
            Tools.UpdateTrackerTitle();
        }

        public void HandleStartArgs(object sender, EventArgs e, string[] args)
        {
            if (args.Length > 1)
            {
                if (args[1].EndsWith(".MMRTSAV"))
                {
                    LoadMMRTSAVfile(sender, e, args[1]);
                }
                else
                {
                    if (!Tools.ParseLogicFile(args[1])) { return; }
                    FormatMenuItems();
                    ResizeObject();
                    PrintToListBox();
                    FireEvents(sender, e);
                    Tools.UpdateTrackerTitle();
                }
            }
        }

        public void FormatMenuItems()
        {
            //Disable other Game support until I can find a way to handle it better.
            spoilerLogConverterToolStripMenuItem.Visible = false;

            saveAsToolStripMenuItem.Visible = (Tools.SaveFilePath != "");
            importSpoilerLogToolStripMenuItem.Text = (Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic)) ? "Remove Spoiler Log" : "Import Spoiler Log";
            enableProgressiveItemsToolStripMenuItem1.Checked = (LogicObjects.MainTrackerInstance.Options.ProgressiveItems);
            enableBringYourOwnAmmoToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.BringYourOwnAmmo);
            stricterLogicHandelingToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.StrictLogicHandeling);
            showEntryNameToolTipToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip);
            includeItemLocationsAsDestinationToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations);
            changeMiddleClickToStarToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.MiddleClickStarNotMark) ? "Make Middle Click Set Item" : "Make Middle Click Star Item";
            entranceRandoToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.EntranceRando;
            optionsToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            undoToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            redoToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            saveToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            seedCheckerToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0) && !LogicObjects.MainTrackerInstance.Options.IsMultiWorld && Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic);
            whatUnlockedThisToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            changeLogicToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            whatUnlockedThisToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            enableProgressiveItemsToolStripMenuItem1.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            smallKeyDoorsAlwaysOpenToolStripMenuItem.Checked = LogicObjects.MainTrackerInstance.Options.Keysy["SmallKey"];
            bossKeyDoorsAlwaysOpenToolStripMenuItem.Checked = LogicObjects.MainTrackerInstance.Options.Keysy["BossKey"];

            popoutPathfinderToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.EntranceRando);
            if (!LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable) 
            {
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = LogicObjects.MainTrackerInstance.EntranceRando; 
            }
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            coupleEntrancesToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            toggleEntranceRandoFeaturesToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled);
            coupleEntrancesToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.CoupleEntrances);
            seperateMarkedItemsToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom);
            coupleEntrancesToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            horizontalLayoutToolStripMenuItem.Visible = (!LogicObjects.MainTrackerInstance.EntranceRando || !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled);
            horizontalLayoutToolStripMenuItem.Checked = LogicObjects.MainTrackerInstance.Options.HorizontalLayout;

            //Manage Dev Menus
            devToolStripMenuItem.Visible = Debugging.ISDebugging || Debugging.ViewAsUserMode;
            devToolStripMenuItem.Text = (Debugging.ViewAsUserMode) ? "Run as Dev" : "Dev Options";
            foreach (ToolStripDropDownItem i in devToolStripMenuItem.DropDownItems) { i.Visible = Debugging.ISDebugging; }
            viewAsUserToolStripMenuItem.Checked = Debugging.ViewAsUserMode;

            CreateMenu();

            bool ShowMMOnly = LogicObjects.MainTrackerInstance.IsMM() || Debugging.ISDebugging;

            //MM specific Controls
            importSpoilerLogToolStripMenuItem.Visible = SpoilerLogConverter.SpoilerLogConvertable.Contains(LogicObjects.MainTrackerInstance.GameCode.ToUpper()) && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = ShowMMOnly && LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            FilterMapToolStripMenuItem.Visible = ShowMMOnly && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            itemTrackerToolStripMenuItem.Visible = ShowMMOnly && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            enableBringYourOwnAmmoToolStripMenuItem.Visible = ShowMMOnly && (LogicObjects.MainTrackerInstance.LogicVersion > 0);

            Tools_StateListChanged();
            LogicStateUpdated(null, null);
        }

        public void ResizeObject()
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;
            var Menuhieght = menuStrip1.Height;
            var FormHeight = this.Height - 40 - Menuhieght;
            var FormWidth = this.Width - 18;
            var FormHalfHeight = FormHeight / 2;
            var FormHalfWidth = FormWidth / 2;

            var locX = 2;
            var locY = 2 + Menuhieght;

            if (LogicObjects.MainTrackerInstance.LogicVersion == 0)
            {
                SetObjectVisibility(false, false);
            }
            else if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled)
            {
                SetObjectVisibility(true, true);
                UpperLeftLBL.Location = new Point(locX, locY + 2);
                BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                TXTLocSearch.Width = FormHalfWidth - 2;
                LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                LBValidLocations.Width = FormHalfWidth - 2;
                LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                UpperRightLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                BTNSetEntrance.Location = new Point(FormWidth - BTNSetEntrance.Width, Menuhieght + 1);
                TXTEntSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                TXTEntSearch.Width = FormHalfWidth - 2;
                LBValidEntrances.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                LBValidEntrances.Width = FormHalfWidth - 2;
                LBValidEntrances.Height = FormHalfHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;

                LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                CHKShowAll.Location = new Point(FormHalfWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                TXTCheckedSearch.Width = FormHalfWidth - 2;
                LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                LBCheckedLocations.Width = FormHalfWidth - 2;
                LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;

                LowerRightLBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY - 2);
                BTNFindPath.Location = new Point(FormWidth - BTNFindPath.Width, Menuhieght + FormHalfHeight - 3);
                LowerRight2LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 6);
                lblSwapPathfinder.Location = new Point(LowerRight2LBL.Location.X + LowerRight2LBL.Width + 4, LowerRight2LBL.Location.Y - 3 );
                LowerRight3LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 7 + CMBStart.Height);
                CMBStart.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + 2);
                CMBEnd.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + CMBStart.Height + 5);
                CMBStart.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                CMBEnd.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                LBPathFinder.Location = new Point(locX + FormHalfWidth, FormHalfHeight + locY + 8 + LowerRightLBL.Height + CMBStart.Height + CMBEnd.Height);
                LBPathFinder.Width = FormHalfWidth - 2;
                LBPathFinder.Height = LBCheckedLocations.Height - CMBEnd.Height - 5;
            }
            else
            {
                SetObjectVisibility(true, false);
                if (LogicObjects.MainTrackerInstance.Options.HorizontalLayout)
                {
                    UpperLeftLBL.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                    TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                    TXTLocSearch.Width = FormHalfWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormHalfWidth - 2;
                    LBValidLocations.Height = FormHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                    LowerLeftLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + 3);
                    TXTCheckedSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                    TXTCheckedSearch.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                    LBCheckedLocations.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Height = FormHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;
                }
                else
                {
                    UpperLeftLBL.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormWidth - BTNSetItem.Width, Menuhieght + 1);
                    TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                    TXTLocSearch.Width = FormWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormWidth - 2;
                    LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                    LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                    TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                    TXTCheckedSearch.Width = FormWidth - 2;
                    LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                    LBCheckedLocations.Width = FormWidth - 2;
                    LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;
                }
            }
            PrintToListBox();
            this.Refresh();

        }

        public void SetObjectVisibility(bool item, bool location)
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;

            UpperLeftLBL.Visible = item;
            BTNSetItem.Visible = item;
            TXTLocSearch.Visible = item;
            LBValidLocations.Visible = item;
            LowerLeftLBL.Visible = item;
            CHKShowAll.Visible = item;
            TXTCheckedSearch.Visible = item;
            LBCheckedLocations.Visible = item;

            UpperRightLBL.Visible = location;
            BTNSetEntrance.Visible = location;
            TXTEntSearch.Visible = location;
            LBValidEntrances.Visible = location;
            LowerRightLBL.Visible = location;
            BTNFindPath.Visible = location;
            LowerRight2LBL.Visible = location;
            LowerRight3LBL.Visible = location;
            CMBStart.Visible = location;
            CMBEnd.Visible = location;
            LBPathFinder.Visible = location;
            lblSwapPathfinder.Visible = location;
        }

        public static void FireEvents(object sender, EventArgs e, bool TrackerUpdated = true, bool LocationCheck = true)
        {

        }

        private string createDisplayName(bool Checked, LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance instance)
        {
            if (entry.IsGossipStone())
            {
                if (entry.Checked)
                {
                    if (entry.GossipHint != "") { return (entry.LocationName ?? entry.DictionaryName) + ": " + entry.GossipHint.Replace("$","") + ((entry.Starred) ? "*" : ""); }
                    else { return (entry.LocationName ?? entry.DictionaryName) + ": No Hint" + ((entry.Starred) ? "*" : ""); }
                }
                else { return (entry.LocationName ?? entry.DictionaryName) + ((entry.Starred) ? "*" : ""); }
            }
            var addPlayerName = entry.ItemBelongsToMe() ? "" : $" (Player {entry.PlayerData.ItemBelongedToPlayer})";
            var LocationName = entry.LocationName ?? entry.DictionaryName;
            var ItemName = (entry.HasRandomItem(false)) ? entry.RandomizedEntry(instance, true).ProgressiveItemName(instance) : "";
            var checkedName = (ItemName == "") ? LocationName : ItemName + addPlayerName + ": " + LocationName;
            var AvailableName = (ItemName == "") ? LocationName : LocationName + ": " + ItemName + addPlayerName;
            var fullName = (Checked) ? checkedName : AvailableName;
            fullName = fullName + ((entry.Starred) ? "*" : "");
            var CheckPrice = Utility.GetPriceText(entry, instance);
            fullName = fullName + ((entry.HasRandomItem(false)) ? CheckPrice : "");
            return fullName;
        }

        public void Tools_StateListChanged()
        {
            undoToolStripMenuItem.Enabled = LogicObjects.MaintrackerInstanceUndoRedoData.UndoList.Count > 0;
            redoToolStripMenuItem.Enabled = LogicObjects.MaintrackerInstanceUndoRedoData.RedoList.Count > 0;
        }


        #endregion Other Functions

        #endregion Functions

        private void LB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && Control.ModifierKeys == Keys.Control)
            {
                (sender as ListBox).BeginUpdate();
                for (int i = 0; i < (sender as ListBox).Items.Count; i++)
                {
                    (sender as ListBox).SetSelected(i, true);
                }
                (sender as ListBox).EndUpdate();
            }
        }

        private void preventKeyShortcuts(object sender, KeyPressEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control) { e.Handled = true; }
        }

        private void changeDefaultSetingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.CreateOptionsFile(true);
            ResizeObject();
            FormatMenuItems();
        }

        private void lblSwapPathfinder_Click(object sender, EventArgs e)
        {
            LogicObjects.LogicEntry newStart = null;
            LogicObjects.LogicEntry newDest = null;
            try
            {
                int DestIndex = Int32.Parse(CMBEnd.SelectedValue.ToString());
                newStart = LogicObjects.MainTrackerInstance.Logic[DestIndex].PairedEntry(LogicObjects.MainTrackerInstance);
            }
            catch { }
            try
            {
                int StartIndex = Int32.Parse(CMBStart.SelectedValue.ToString());
                newDest = LogicObjects.MainTrackerInstance.Logic[StartIndex].PairedEntry(LogicObjects.MainTrackerInstance);
            }
            catch { }

            if (newStart == null || newDest == null) { return; }

            try
            {
                CMBStart.DataSource = new BindingSource(new Dictionary<int, string> { { newStart.ID, newStart.ItemName } }, null);
                CMBStart.DisplayMember = "Value";
                CMBStart.ValueMember = "key";
                CMBStart.SelectedIndex = 0;
            }
            catch { }
            try
            {
                CMBEnd.DataSource = new BindingSource(new Dictionary<int, string> { { newDest.ID, newDest.LocationName } }, null);
                CMBEnd.DisplayMember = "Value";
                CMBEnd.ValueMember = "key";
                CMBEnd.SelectedIndex = 0;
            }
            catch { }
        }
    }
}
