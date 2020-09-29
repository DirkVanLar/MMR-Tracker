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
            MainInterface.LocationChecked += MainInterface_LocationChecked;
            TriggerAddRemoteToIPList += OnlinePlay_TriggerAddRemoteToIPList;
        }

        public static event EventHandler NetDataProcessed = delegate { };
        public static event Action<LogicObjects.MMRTpacket> TriggerAddRemoteToIPList = delegate { };
        public static object MultiworldToggle = new object();

        public static bool Listening = false;
        public static bool Sending = false;
        public static bool Updating = false;
        public static OnlinePlay CurrentOpenForm = null;
        public static IPAddress MyIP;
        public static List<LogicObjects.IPDATA> IPS = new List<LogicObjects.IPDATA>();
        public static Socket listener;

        //Sending Data

        public static LogicObjects.MMRTpacket createNetData(int Type)
        {

            bool isValidSyncable(LogicObjects.LogicEntry x)
            {
                if (x.IsFake) { return false; }
                if (!x.HasRandomItem(true)) { return false; }
                if (!Utility.StandardItemTypes.Contains(x.ItemSubType)) { return false; }
                return true;
            } 

            List<LogicObjects.NetData> NetData = new List<LogicObjects.NetData>();
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => isValidSyncable(x)))
            {
                NetData.Add(new LogicObjects.NetData { ID = i.ID, PI = i.PlayerData.ItemBelongedToPlayer ,Ch = i.Checked, RI = i.RandomizedItem });
            }
            LogicObjects.MMRTpacket Pack = new LogicObjects.MMRTpacket
            {
                LogicData = NetData,
                PlayerID = LogicObjects.MainTrackerInstance.Options.MyPlayerID,
                RequestingUpdate = Type,
                IPData = new LogicObjects.IPDATASerializable
                {
                    IP = MyIP.ToString(),
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
                //Console.WriteLine("Starting: Creating Socket object");
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(ipEndPoint);
                Console.WriteLine("Successfully connected to {0}", sender.RemoteEndPoint);
                string sendingMessage = M;
                //string sendingMessage = "Hello World Socket Test";
                //Console.WriteLine("Creating message:{0}", sendingMessage);
                byte[] forwardMessage = Encoding.Default.GetBytes(sendingMessage);
                sender.Send(forwardMessage);
                //int totalBytesReceived = sender.Receive(receivedBytes);
                //Console.WriteLine("Message provided from server: {0}", Encoding.ASCII.GetString(receivedBytes, 0, totalBytesReceived));
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        public async static void SendData(List<LogicObjects.IPDATA> SendList, int TYPE = 0)
        {
            if (!Sending && TYPE != 1) { return; }
            string m = JsonConvert.SerializeObject(createNetData(TYPE));
            foreach (var ip in SendList)
            {
                await Task.Run(() => StartClient(m, ip));
            }
        }

        private void MainInterface_LocationChecked(object sender, EventArgs e)
        {
            SendData(IPS);
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
                Console.WriteLine("Data Recieved.");
                string receivedValue = string.Empty;
                byte[] bytes = null;
                int counter = 0;
                while (counter < 1024)
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

                Console.WriteLine("Received value: {0}. Size: {1}", receivedValue, counter);
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
                Console.WriteLine($"Errored: { e.ToString() }");
                return "";
            }
        }

        public static async void startServer()
        {
            if (Listening) { MessageBox.Show("Net client already started!"); return; }
            Listening = true;
            while (Listening)
            {
                Console.WriteLine("Listening for data...");
                string Logic = await Task.Run(() => RecieveData());
                Console.WriteLine(Logic);

                LogicObjects.MMRTpacket NetData = new LogicObjects.MMRTpacket();
                if (Logic != "")
                {
                    try
                    {
                        NetData = JsonConvert.DeserializeObject<LogicObjects.MMRTpacket>(Logic);
                        ManageNetData(NetData);
                        Console.WriteLine($"Data Processed");
                    }
                    catch (Exception e) { Console.WriteLine($"Data Invalid: {e}"); }
                }
            }
            Console.WriteLine("Server Shutting Down");
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

        private void autoAddIncomingIPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections = !LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections;
            autoAddIncomingIPsToolStripMenuItem.Checked = (LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections);
        }

        private void copyNetDataToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(JsonConvert.SerializeObject(createNetData(0)));
        }

        private void sendingDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendData(IPS);
        }

        private void requestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendData(IPS, 1);
        }

        private void fullSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendData(IPS, 2);
        }

        private void portForwardingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            DebugScreen.DebugFunction = 5;
            DebugScreen.Show();
        }

        //Controls

        private void btnAddIP_Click_1(object sender, EventArgs e)
        {
            
            LogicObjects.IPDATA NewIP = new LogicObjects.IPDATA();
            string IPText = txtIP.Text;
            IPAddress IP = null;
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
            
            Console.WriteLine(IP);
            NewIP.PORT = (int)NudPort.Value;
            NewIP.DisplayName = $"{IPText}:{NewIP.PORT}";
            IPS.Add(NewIP);
            UpdateFormItems();
            txtIP.Focus();
            txtIP.SelectAll();
        }

        private void btnRemoveIP_Click(object sender, EventArgs e)
        {
            foreach (var i in LBIPAdresses.SelectedItems)
            {
                IPS.RemoveAt(IPS.IndexOf(i as LogicObjects.IPDATA));
            }
            UpdateFormItems();
        }

        private void chkListenForData_CheckedChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            if (chkListenForData.Checked)
            {
                NudYourPort.Enabled = false;
                startServer();
                Console.WriteLine("Server Started");
            }
            else
            {
                NudYourPort.Enabled = true;
                Listening = false;
                try { listener.Close(); } catch { }
            }
            UpdateFormItems();
        }

        private void chkSendData_CheckedChanged(object sender, EventArgs e)
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

        public static void ManageNetData(LogicObjects.MMRTpacket Data)
        {
            var log = LogicObjects.MainTrackerInstance.Logic;
            var IPInSendingList = IPS.FindIndex(f => f.IP.ToString() == Data.IPData.IP && f.PORT == Data.IPData.PORT) > -1;

            if (!IPInSendingList)
            {
                if (LogicObjects.MainTrackerInstance.Options.StrictIP) { return; }
                if (LogicObjects.MainTrackerInstance.Options.AutoAddIncomingConnections) { TriggerAddRemoteToIPList(Data); }
            }

            if (Data.RequestingUpdate !=0 && IPInSendingList)
            {
                try
                {
                    IPAddress NIP = IPAddress.Parse(Data.IPData.IP);
                    SendData(new List<LogicObjects.IPDATA> { new LogicObjects.IPDATA { IP = NIP, PORT = Data.IPData.PORT } }, 0);
                }
                catch { }
            }

            if (Data.RequestingUpdate == 1) { return; }

            foreach (var i in Data.LogicData)
            {
                var SyncedItemInMultiworld = (LogicObjects.MainTrackerInstance.Options.MultiWorldOnlineCombo && log.ElementAt(i.ID) != null && Data.PlayerID == i.PI && (log[i.ID].SpoilerRandom < -1 || log[i.ID].SpoilerRandom == i.RI));
                //This is used to theoretically allow for a combination of multiworld and Modloader64 online. If you recieve data from a player 
                //with a matching player ID and the data in the packet does not contridict the spoiler log data, Check the actual location on 
                //your tracker as if you were in Online (Synced) mode.

                if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld && !SyncedItemInMultiworld)
                {
                    if (i.PI != LogicObjects.MainTrackerInstance.Options.MyPlayerID || i.Ch == false || i.RI < -1 || i.RI >= log.Count() || (i.RI > -1 && log[i.RI].IsEntrance())) { continue; }
                    var entry = new LogicObjects.LogicEntry { ID = -1, Checked = false, RandomizedItem = i.RI, SpoilerRandom = i.RI, Options = 0};
                    LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance, Data.PlayerID);
                }
                else if (log.ElementAt(i.ID) != null && !log[i.ID].Checked)
                {
                    var entry = log[i.ID];
                    entry.RandomizedItem = entry.HasRandomItem(false) ? entry.RandomizedItem : (entry.SpoilerRandom > -2 ? entry.SpoilerRandom : i.RI);
                    if (LogicObjects.MainTrackerInstance.Options.AllowCheckingItems && i.Ch) { LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance); }
                }
            }
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            NetDataProcessed(null, null);
        }

        private void OnlinePlay_TriggerAddRemoteToIPList(LogicObjects.MMRTpacket Data)
        {
            LogicObjects.IPDATA NewIP = new LogicObjects.IPDATA();
            try { NewIP.IP = IPAddress.Parse(Data.IPData.IP); } catch { return; }
            NewIP.PORT = Data.IPData.PORT;
            NewIP.DisplayName = $"{NewIP.IP}:{NewIP.PORT}";
            IPS.Add(NewIP);
            UpdateFormItems();
        }

        private void saveIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

            List<LogicObjects.IPDATASerializable> SaveIPS = new List<LogicObjects.IPDATASerializable> { new LogicObjects.IPDATASerializable { IP = MyIP.ToString(), PORT = LogicObjects.MainTrackerInstance.Options.PortNumber } };
            foreach(var i in IPS)
            {
                SaveIPS.Add(new LogicObjects.IPDATASerializable { IP = i.IP.ToString(), PORT = i.PORT, DisplayName = i.DisplayName });
            }

            File.WriteAllText(saveDialog.FileName, JsonConvert.SerializeObject(SaveIPS));
        }

        private void loadIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select An IP List", "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP");
            if (file == "") { return; }
            List<LogicObjects.IPDATASerializable> LoadData = new List<LogicObjects.IPDATASerializable>();
            try { LoadData = JsonConvert.DeserializeObject<List<LogicObjects.IPDATASerializable>>(File.ReadAllText(file)); }
            catch
            {
                MessageBox.Show("File Invalid!");
                return;
            }
            Console.WriteLine($"File Valid");
            foreach (var i in LoadData)
            {
                Console.WriteLine($"Checking {i.IP.Trim()}");
                if (i.IP != MyIP.ToString() && IPS.FindIndex(f => f.IP.ToString() == i.IP && f.PORT == i.PORT) < 0)
                {
                    IPAddress NIP;
                    try { NIP = IPAddress.Parse(i.IP.Trim()); } catch { Console.WriteLine($"{i.IP.Trim()} Invalid"); continue; }
                    string dist = (i.DisplayName == null || i.DisplayName == "") ? $"{NIP}:{i.PORT}" : i.DisplayName;
                    IPS.Add(new LogicObjects.IPDATA { IP = NIP, PORT = i.PORT, DisplayName = dist });
                    Console.WriteLine($"{i.IP.Trim()} Added");
                }
            }
            UpdateFormItems();
        }

        private void onlyAcceptDataFromSendingListToolStripMenuItem_Click(object sender, EventArgs e)
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
                this.Height = this.Height + 25;
            }
            else if (MultiworldOFF() || (sender == this && !LogicObjects.MainTrackerInstance.Options.IsMultiWorld)) //Sender is "this" when called from form load event
            {
                this.Height = this.Height - 25;
            }

            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { LogicObjects.MainTrackerInstance.Options.MyPlayerID = (int)nudPlayerID.Value; }
            else { LogicObjects.MainTrackerInstance.Options.MyPlayerID = -1; }

            UpdateFormItems();

            NetDataProcessed(MultiworldToggle, null);
        }

        private void UpdateFormItems()
        {
            Updating = true;

            CurrentOpenForm = this;
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
            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { nudPlayerID.Value = (LogicObjects.MainTrackerInstance.Options.MyPlayerID < 0) ? 0 : LogicObjects.MainTrackerInstance.Options.MyPlayerID; }

            Updating = false;
        }

        private void nudPlayerID_ValueChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            LogicObjects.MainTrackerInstance.Options.MyPlayerID = (int)nudPlayerID.Value;
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Co-op: In this mode, any locations you check or mark will be marked on your parties trackers. It will only ever mark a location on your parties tracker, even if you did a full check.\nUsefull for when you are playing Co-op runs using the same seed.\n\n" +
                "Online (Synced): In this mode, any locations you check or mark will apply the same action to your parties tracker.\nUsefull for when you are playing an Online game through Modloader64 or other similar programs where items you obtain are synced between all players.\n\n" +
                "Multiworld: In this mode, when you do a check you will assign what player the item is going to. That item will be marked as obtained for that player only.\nUseful when playing multiworld game modes such as OOT Randomizer Multiworld.", "Game Mode Info");
        }

        private void lblYourIP_Click(object sender, EventArgs e)
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
