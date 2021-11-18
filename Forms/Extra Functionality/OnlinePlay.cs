using MMR_Tracker.Class_Files;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms
{
    public partial class OnlinePlay : Form
    {
        public OnlinePlay()
        {
            InitializeComponent();
        }

        public static bool Listening = false;
        public static bool Sending = false;
        public static bool Updating = false;
        public static OnlinePlay CurrentOpenForm = null;
        public static IPAddress MyIP;
        public static List<LogicObjects.IPDATA> IPS = new List<LogicObjects.IPDATA>();
        public static Socket listener;

        //Sending Data

        public static LogicObjects.MMRTpacket CreateNetData(int Type)
        {

            bool isValidSyncable(LogicObjects.LogicEntry x)
            {
                if (x.IsFake) { return false; }
                if (!x.HasRandomItem(true)) { return false; }
                return true;
            } 

            List<LogicObjects.NetData> NetData = new List<LogicObjects.NetData>();
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => isValidSyncable(x)))
            {
                if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld && (!i.Checked || i.ItemBelongsToMe())) { continue; }
                NetData.Add(new LogicObjects.NetData { ID = i.ID, PI = i.PlayerData.ItemBelongedToPlayer ,Ch = i.Checked, RI = i.RandomizedItem });
            }
            LogicObjects.MMRTpacket Pack = new LogicObjects.MMRTpacket
            {
                LogicData = (Type == 1) ? new List<LogicObjects.NetData>() : NetData,
                PlayerID = LogicObjects.MainTrackerInstance.Options.MyPlayerID,
                RequestingUpdate = Type,
                IPData = new LogicObjects.IPDATA
                {
                    IP = MyIP,
                    PORT = LogicObjects.MainTrackerInstance.Options.PortNumber
                }
            };
            return Pack;
        }

        public static void StartClient(string M, LogicObjects.IPDATA ip)
        {
            try
            {
                //IPEndPoint ipEndPoint = new IPEndPoint(ip, 8000);
                IPEndPoint ipEndPoint = new IPEndPoint(ip.IP, ip.PORT);
                //Debugging.Log("Starting: Creating Socket object");
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(ipEndPoint);
                Debugging.Log($"Successfully connected to {sender.RemoteEndPoint}" );
                string sendingMessage = M;
                //string sendingMessage = "Hello World Socket Test";
                //Debugging.Log("Creating message:{0}", sendingMessage);
                byte[] forwardMessage = Encoding.Default.GetBytes(sendingMessage);
                sender.Send(forwardMessage);
                //int totalBytesReceived = sender.Receive(receivedBytes);
                //Debugging.Log("Message provided from server: {0}", Encoding.ASCII.GetString(receivedBytes, 0, totalBytesReceived));
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Debugging.Log($"ArgumentNullException : {ane}");
            }
            catch (SocketException se)
            {
                Debugging.Log($"SocketException : {se}");
            }
            catch (Exception e)
            {
                Debugging.Log($"Unexpected exception : {e}");
            }
        }

        public async static void SendData(List<LogicObjects.IPDATA> SendList, int TYPE = 0)
        {
            if (!Sending && TYPE != 1) { return; }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IPConverter());

            string m = JsonConvert.SerializeObject(CreateNetData(TYPE), jsonSettings);
            foreach (var ip in SendList)
            {
                await Task.Run(() => StartClient(m, ip));
            }
        }

        //RecievingData

        public static string RecieveData()
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, LogicObjects.MainTrackerInstance.Options.PortNumber));
                listener.Listen(10);
                Socket socket = listener.Accept();
                Debugging.Log("Data Recieved.");
                string receivedValue = string.Empty;
                byte[] bytes = null;
                int counter = 0;
                while (counter < 4096)
                {
                    counter++;
                    bytes = new byte[socket.Available];
                    int bytesRec = socket.Receive(bytes);
                    receivedValue += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (receivedValue.IndexOf("]}") > -1)
                    {
                        break;
                    }
                }

                Debugging.Log($"Received value: {receivedValue}. Size: {counter}" );
                //string replyValue = "Message successfully received.";
                //byte[] replyMessage = Encoding.Default.GetBytes(replyValue);
                //socket.Send(replyMessage);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                listener.Close();
                return receivedValue;
            }
            catch (Exception e)
            {
                listener.Close();
                Debugging.Log($"Errored: { e}");
                return "";
            }
        }

        public static async void StartServer()
        {
            if (Listening) { MessageBox.Show("Net client already started!"); return; }
            Listening = true;
            while (Listening)
            {
                Debugging.Log("Listening for data...");
                string Logic = await Task.Run(() => RecieveData());
                Debugging.Log(Logic);

                LogicObjects.MMRTpacket NetData = new LogicObjects.MMRTpacket();
                if (Logic != "")
                {
                    try
                    {
                        var jsonSettings = new JsonSerializerSettings();
                        jsonSettings.Converters.Add(new IPConverter());
                        NetData = JsonConvert.DeserializeObject<LogicObjects.MMRTpacket>(Logic, jsonSettings);
                        ManageNetData(NetData);
                        Debugging.Log($"Data Processed");
                    }
                    catch (Exception e) { Debugging.Log($"Data Invalid: {e}"); }
                }
            }
            Debugging.Log("Server Shutting Down");
            return;
        }

        //Port Handeling

        private void NudYourPort_ValueChanged(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.PortNumber = (int)NudYourPort.Value;
            chkListenForData.Checked = false;
        }

        private void OnlinePlay_FormClosing(object sender, FormClosingEventArgs e)
        {
            CurrentOpenForm = null;
        }

        //Options

        private void AutoAddIncomingIPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections = !LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections;
            autoAddIncomingIPsToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections);
        }

        private void CopyNetDataToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IPConverter());
            Clipboard.SetText(JsonConvert.SerializeObject(CreateNetData(0), jsonSettings));
        }

        private void SendingDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendData(IPS);
        }

        private void RequestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendData(IPS, 1);
        }

        private void FullSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendData(IPS, 2);
        }

        private void PortForwardingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay { DebugFunction = 5 };
            DebugScreen.Show();
        }

        //Controls

        private void BtnAddIP_Click_1(object sender, EventArgs e)
        {
            
            LogicObjects.IPDATA NewIP = new LogicObjects.IPDATA();
            string IPText = txtIP.Text;
            IPAddress IP;
            try { IP = IPAddress.Parse(IPText); } catch { IP = null; }
            if (IP == null) { try { IP = Dns.GetHostEntry(IPText).AddressList[0]; } catch { IP = null; } }
            if (IP == null) 
            { 
                MessageBox.Show("IP Address not valid");
                txtIP.Focus();
                txtIP.SelectAll(); 
                return; 
            }
            NewIP.IP = IP;

            Debugging.Log(IP.ToString());
            NewIP.PORT = (int)NudPort.Value;
            NewIP.DisplayName = $"{IPText}:{NewIP.PORT}";
            IPS.Add(NewIP);
            UpdateFormItems();
            txtIP.Focus();
            txtIP.SelectAll();
        }

        private void BtnRemoveIP_Click(object sender, EventArgs e)
        {
            foreach (var i in LBIPAdresses.SelectedItems)
            {
                IPS.RemoveAt(IPS.IndexOf(i as LogicObjects.IPDATA));
            }
            UpdateFormItems();
        }

        private void ChkListenForData_CheckedChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            if (chkListenForData.Checked)
            {
                NudYourPort.Enabled = false;
                StartServer();
                Debugging.Log("Server Started");
            }
            else
            {
                NudYourPort.Enabled = true;
                Listening = false;
                try { listener.Close(); } catch { }
            }
            UpdateFormItems();
        }

        private void ChkSendData_CheckedChanged(object sender, EventArgs e)
        {
            Sending = chkSendData.Checked;
            UpdateFormItems();
        }

        //Other

        private void OnlinePlay_Load(object sender, EventArgs e)
        {
            MyIP = GetWanIP();
            txtPulbicIP.Text = MyIP.ToString();
            ChangeGameMode(sender, e);
        }

        private static void SendRequestedUpdate(LogicObjects.MMRTpacket Data, bool IPInSendingList)
        {
            if (IPInSendingList)
            {
                try
                {
                    SendData(new List<LogicObjects.IPDATA> { new LogicObjects.IPDATA { IP = Data.IPData.IP, PORT = Data.IPData.PORT } }, 0);
                }
                catch (Exception e) { Debugging.Log($"Could not send requested data\nReason: Send Request Errored with\n{e}"); }
            }
            else { Debugging.Log($"Could not send requested data\nReason: User not in send list {Data.IPData.IP}:{Data.IPData.PORT}"); }

        }

        public static void ManageNetData(LogicObjects.MMRTpacket Data)
        {
            var log = LogicObjects.MainTrackerInstance.Logic;
            var Instance = LogicObjects.MainTrackerInstance;
            var IPInSendingList = IPS.FindIndex(f => f.IP.ToString() == Data.IPData.IP.ToString() && f.PORT == Data.IPData.PORT) > -1;
            //bool SameLAN = (Data.IPData.IP == MyIP.ToString() && Data.IPData.PORT != LogicObjects.MainTrackerInstance.Options.PortNumber);

            if (!IPInSendingList)
            {
                if (LogicObjects.MainTrackerInstance.Options.StrictIP) { return; }
                if (LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections) { OnlinePlay_TriggerAddRemoteToIPList(Data); }
            }

            if (Data.RequestingUpdate != 0) { SendRequestedUpdate(Data, IPInSendingList); }
            
            if (Data.RequestingUpdate == 1) { return; }

            bool ChangesMade = false;

            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { CleanMultiWorldData(Instance, Data); }
            
            ListBox ItemsToCheck = new ListBox();

            foreach (var i in Data.LogicData)
            {

                if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld)
                {
                    if (i.PI != LogicObjects.MainTrackerInstance.Options.MyPlayerID || i.Ch == false || !Instance.ItemInRange(i.RI) || log[i.RI].IsEntrance()) { continue; }
                    var entry = new LogicObjects.LogicEntry { ID = -1, Checked = false, RandomizedItem = i.RI, SpoilerRandom = i.RI, Options = 0};
                    ItemsToCheck.Items.Add(entry);
                    ChangesMade = true;
                }
                else if (Instance.ItemInRange(i.ID) && !log[i.ID].Checked)
                {
                    var entry = log[i.ID];
                    if (!entry.HasRandomItem(false))
                    {
                        entry.RandomizedItem = (entry.SpoilerRandom > -2 ? entry.SpoilerRandom : i.RI);
                        ChangesMade = true;
                    }
                    if (LogicObjects.MainTrackerInstance.Options.AllowCheckingItems && i.Ch) 
                    {
                        ItemsToCheck.Items.Add(entry);
                        ChangesMade = true;
                    }
                }
            }

            if (!ChangesMade) { return; }

            for (int i = 0; i < ItemsToCheck.Items.Count; i++)
            {
                ItemsToCheck.SetSelected(i, true);
            }
            MainInterface.CurrentProgram.CheckItemSelected(ItemsToCheck, true, 0, false, Data.PlayerID, true);
        }

        public static void CleanMultiWorldData(LogicObjects.TrackerInstance Instance, LogicObjects.MMRTpacket Data)
        {
            foreach (var i in Instance.Logic.Where(x => x.Aquired && x.PlayerData.ItemCameFromPlayer == Data.PlayerID))
            {
                i.Aquired = false;
                i.PlayerData.ItemCameFromPlayer = -1;
            }

            var log = LogicObjects.MainTrackerInstance.Logic;
            var itemsAquired = new List<LogicObjects.LogicEntry>();
            var itemsInUse = new List<LogicObjects.LogicEntry>();

            foreach (var i in log)
            {
                if (i.IsFake) { continue; }
                if (i.LogicItemAquired()) { itemsAquired.Add(i); itemsInUse.Add(i); }
                if (i.ItemHasBeenPlaced(Instance.Logic)) { itemsInUse.Add(i); }
            }

            foreach (var i in Data.LogicData)
            {
                if (i.PI != LogicObjects.MainTrackerInstance.Options.MyPlayerID || i.Ch == false || !Instance.ItemInRange(i.RI) || log[i.RI].IsEntrance()) { continue; }

                if (itemsInUse.Where(x => x.ID == i.RI).Any())
                {
                    Debugging.Log($"{log[i.RI].DictionaryName} was in use elsewhere");

                    var MatchingItems = log.Where(x => x.SpoilerItem.Intersect(log[i.RI].SpoilerItem).Any());
                    var FindUnusedMatchingItem = MatchingItems.Where(x => !itemsInUse.Where(y => y.ID == x.ID).Any());
                    var FindUnAquiredMatchingItem = MatchingItems.Where(x => !itemsAquired.Where(y => y.ID == x.ID).Any());

                    if (FindUnusedMatchingItem.Any())
                    {
                        Debugging.Log($"Unused Matching Item found: {FindUnusedMatchingItem.ToArray()[0].DictionaryName}");
                        var newItem = FindUnusedMatchingItem.First();
                        i.RI = newItem.ID;
                        itemsAquired.Add(newItem);
                        itemsInUse.Add(newItem);
                    }
                    else if (FindUnAquiredMatchingItem.Any())
                    {
                        Debugging.Log($"No Unused Matching Items Were Found, getting unaquired matching item.");
                        Debugging.Log($"Matching UnAquired Item Found: {FindUnAquiredMatchingItem.First().DictionaryName}");
                        var newItem = FindUnAquiredMatchingItem.First();
                        i.RI = newItem.ID;
                        itemsAquired.Add(newItem);
                        itemsInUse.Add(newItem);
                    }
                    else { Debugging.Log($"No Unused items were found. This is an error and could cause Issues."); }
                }
            }
        }

        public static void OnlinePlay_TriggerAddRemoteToIPList(LogicObjects.MMRTpacket Data)
        {
            LogicObjects.IPDATA NewIP = new LogicObjects.IPDATA();
            NewIP.IP = Data.IPData.IP;
            NewIP.PORT = Data.IPData.PORT;
            NewIP.DisplayName = $"{NewIP.IP}:{NewIP.PORT}";
            IPS.Add(NewIP); 
            if (CurrentOpenForm != null)
            {
                CurrentOpenForm.UpdateFormItems();
            }
            
        }

        private void SaveIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

            List<LogicObjects.IPDATA> SaveIPS = new List<LogicObjects.IPDATA> { new LogicObjects.IPDATA { IP = MyIP, PORT = LogicObjects.MainTrackerInstance.Options.PortNumber } };
            foreach(var i in IPS)
            {
                SaveIPS.Add(new LogicObjects.IPDATA { IP = i.IP, PORT = i.PORT, DisplayName = i.DisplayName });
            }
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IPConverter());
            File.WriteAllText(saveDialog.FileName, JsonConvert.SerializeObject(SaveIPS, jsonSettings));
        }

        private void LoadIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select An IP List", "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP");
            if (file == "") { return; }
            List<LogicObjects.IPDATA> LoadData = new List<LogicObjects.IPDATA>();
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IPConverter());
            try { LoadData = JsonConvert.DeserializeObject<List<LogicObjects.IPDATA>>(File.ReadAllText(file), jsonSettings); }
            catch (Exception f)
            {
                Console.WriteLine(f);
                MessageBox.Show("File Invalid!");
                return;
            }
            Debugging.Log($"File Valid");
            foreach (var i in LoadData)
            {
                Debugging.Log($"Checking {i.IP}");
                if (i.IP.ToString() != MyIP.ToString() && IPS.FindIndex(f => f.IP.ToString() == i.IP.ToString() && f.PORT.ToString() == i.PORT.ToString()) < 0)
                {
                    string dist = (i.DisplayName == null || i.DisplayName == "") ? $"{i.IP}:{i.PORT}" : i.DisplayName;
                    IPS.Add(new LogicObjects.IPDATA { IP = i.IP, PORT = i.PORT, DisplayName = dist });
                    Debugging.Log($"{i.IP} Added");
                }
            }
            UpdateFormItems();
        }

        private void OnlyAcceptDataFromSendingListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.StrictIP = !LogicObjects.MainTrackerInstance.Options.StrictIP;
            UpdateFormItems();
        }

        private void LBIPAdresses_MouseMove(object sender, MouseEventArgs e)
        {
            ShowtoolTip(e, sender as ListBox);
        }

        public void ShowtoolTip(MouseEventArgs e, ListBox lb)
        {
            int index = lb.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(lb.Items[index] is LogicObjects.IPDATA)) { return; }
            string DisplayName = (lb.Items[index] as LogicObjects.IPDATA).IP.ToString();
            if (toolTip1.GetToolTip(lb) == DisplayName) { return; }
            if (Utility.IsDivider(DisplayName)) { return; }
            toolTip1.SetToolTip(lb, DisplayName);
        }

        private void ChangeGameMode(object sender, EventArgs e)
        {

            bool wasMultiWorld = LogicObjects.MainTrackerInstance.Options.IsMultiWorld;

            if (sender != this)
            {
                LogicObjects.MainTrackerInstance.Options.IsMultiWorld = (sender == multiworldToolStripMenuItem);
                LogicObjects.MainTrackerInstance.Options.AllowCheckingItems = (sender == onlineSyncedToolStripMenuItem);
            }

            bool MultiworldOFF() { return !LogicObjects.MainTrackerInstance.Options.IsMultiWorld && wasMultiWorld; }
            bool MultiworldON() { return LogicObjects.MainTrackerInstance.Options.IsMultiWorld && !wasMultiWorld; }

            multiworldToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.IsMultiWorld);
            onlineSyncedToolStripMenuItem.Checked = (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld && LogicObjects.MainTrackerInstance.Options.AllowCheckingItems);
            coopToolStripMenuItem.Checked = (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld && !LogicObjects.MainTrackerInstance.Options.AllowCheckingItems);

            if (MultiworldON())
            {
                this.Height += 25;
            }
            else if (MultiworldOFF() || (sender == this && !LogicObjects.MainTrackerInstance.Options.IsMultiWorld)) //Sender is "this" when called from form load event
            {
                this.Height -= 25;
            }

            UpdateFormItems();

            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { LogicObjects.MainTrackerInstance.Options.MyPlayerID = (int)nudPlayerID.Value; }
            else { LogicObjects.MainTrackerInstance.Options.MyPlayerID = -1; }

            MainInterface.CurrentProgram.FormatMenuItems();
        }

        private void UpdateFormItems()
        {
            Updating = true;

            LBIPAdresses.Items.Clear();
            foreach (var i in IPS) { LBIPAdresses.Items.Add(i); }
            chkListenForData.Checked = Listening;
            chkSendData.Checked = Sending;
            NudYourPort.Value = LogicObjects.MainTrackerInstance.Options.PortNumber;
            NudPort.Value = LogicObjects.MainTrackerInstance.Options.PortNumber;
            autoAddIncomingIPsToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections);
            autoAddIncomingIPsToolStripMenuItem.Visible = (!LogicObjects.MainTrackerInstance.Options.StrictIP);
            onlyAcceptDataFromSendingListToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.StrictIP);
            copyNetDataToClipboardToolStripMenuItem.Visible = Debugging.ISDebugging;
            multiworldToolStripMenuItem.Checked = LogicObjects.MainTrackerInstance.Options.IsMultiWorld;
            nudPlayerID.Enabled = LogicObjects.MainTrackerInstance.Options.IsMultiWorld && !chkListenForData.Checked && !chkSendData.Checked;

            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) 
            { 
                nudPlayerID.Value = (LogicObjects.MainTrackerInstance.Options.MyPlayerID < 0) ? 0 : LogicObjects.MainTrackerInstance.Options.MyPlayerID; 
            }

            Updating = false;
        }

        private void NudPlayerID_ValueChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            LogicObjects.MainTrackerInstance.Options.MyPlayerID = (int)nudPlayerID.Value;
        }

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Co-op: In this mode, any locations you check or mark will be marked on your parties trackers. It will only ever mark a location on your parties tracker, even if you did a full check.\nUsefull for when you are playing Co-op runs using the same seed.\n\n" +
                "Online (Synced): In this mode, any locations you check or mark will apply the same action to your parties tracker.\nUsefull for when you are playing an Online game through Modloader64 or other similar programs where items you obtain are synced between all players.\n\n" +
                "Multiworld: In this mode, when you do a check you will assign what player the item is going to. That item will be marked as obtained for that player only.\nUseful when playing multiworld game modes such as OOT Randomizer Multiworld.", "Game Mode Info");
        }

        private void LblYourIP_Click(object sender, EventArgs e)
        {
            if (lblYourIP.Text.Contains("Public"))
            {
                var LocalIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
                if (LocalIP == null) { LocalIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetworkV6); }
                if (LocalIP == null) { LocalIP = IPAddress.Parse("169.254.0.1"); }
                txtPulbicIP.Text = LocalIP.ToString();
                lblYourIP.Text = "Your Local IP Address:";
            }
            else
            {
                txtPulbicIP.Text = MyIP.ToString();
                lblYourIP.Text = "Your Public IP Address:";
            }
        }

        public static IPAddress GetWanIP()
        {   //Prioritize an IPv4, if none can be found try to use the first ipv6 that was found. If no v6 were found return an apipa address.
            List<IPAddress> BackupIpV6 = new List<IPAddress>();

            try
            {
                string externalip = new WebClient().DownloadString("https://ipinfo.io/ip").Trim();
                try
                {
                    var ip = IPAddress.Parse(externalip);
                    if (ip.AddressFamily == AddressFamily.InterNetwork) { return ip; }
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6) { BackupIpV6.Add(ip); }
                }
                catch { }

                externalip = new WebClient().DownloadString("http://icanhazip.com").Trim();
                try
                {
                    var ip = IPAddress.Parse(externalip);
                    if (ip.AddressFamily == AddressFamily.InterNetwork) { return ip; }
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6) { BackupIpV6.Add(ip); }
                }
                catch { }

                externalip = new WebClient().DownloadString("http://checkip.dyndns.org").Trim().Split(':')[1].Split('<')[0].Trim();
                try
                {
                    var ip = IPAddress.Parse(externalip);
                    if (ip.AddressFamily == AddressFamily.InterNetwork) { return ip; }
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6) { BackupIpV6.Add(ip); }
                }
                catch { }

                if (BackupIpV6.Count() > 0) { return BackupIpV6[0]; }
            }
            catch { }

            return IPAddress.Parse("169.254.0.1");
        }
    }
}
