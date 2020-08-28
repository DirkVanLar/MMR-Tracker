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

        public static int MyPlayerID = -1;
        public static bool IsMultiWorld = false;

        public static bool Listening = false;
        public static bool Sending = false;
        public static bool Updating = false;
        public static bool FormOpen = false;
        public static IPAddress MyIP;
        public static int PortNumber = 2112;
        public static bool AllowCheckingItems = true;
        public static bool AllowAutoPortForward = false;
        public static bool AutoAddIncomingConnections = false;
        public static bool StrictIP = false;
        public static List<LogicObjects.IPDATA> IPS = new List<LogicObjects.IPDATA>();
        public static Socket listener;

        //Sending Data

        public static LogicObjects.MMRTpacket createNetData(int Type)
        {
            List<LogicObjects.NetData> ClipboardNetData = new List<LogicObjects.NetData>();
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && x.HasRandomItem(true)))
            {
                ClipboardNetData.Add(new LogicObjects.NetData { ID = i.ID, PI = i.PlayerData.ItemBelongedToPlayer ,Ch = i.Checked, RI = i.RandomizedItem });
            }
            LogicObjects.MMRTpacket Pack = new LogicObjects.MMRTpacket();
            Pack.LogicData = ClipboardNetData;
            Pack.IPData.IP = MyIP.ToString();
            Pack.IPData.PORT = PortNumber;
            Pack.PlayerID = MyPlayerID;
            Pack.RequestingUpdate = Type;
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
            Console.WriteLine("Logic updated");
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
                listener.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
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
            PortNumber = (int)NudYourPort.Value;
            chkListenForData.Checked = false;
        }

        private void OnlinePlay_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormOpen = false;
        }

        //Options
        private void allowFullCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllowCheckingItems = !AllowCheckingItems;
            allowFullCheckToolStripMenuItem.Checked = (AllowCheckingItems);
        }

        private void autoAddIncomingIPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoAddIncomingConnections = !AutoAddIncomingConnections;
            autoAddIncomingIPsToolStripMenuItem.Checked = (AutoAddIncomingConnections);
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
            updateLB();
            txtIP.Focus();
            txtIP.SelectAll();
        }

        private void btnRemoveIP_Click(object sender, EventArgs e)
        {
            foreach (var i in LBIPAdresses.SelectedItems)
            {
                IPS.RemoveAt(IPS.IndexOf(i as LogicObjects.IPDATA));
            }
            updateLB();
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
        }

        private void chkSendData_CheckedChanged(object sender, EventArgs e)
        {
            Sending = chkSendData.Checked;
        }

        //Other

        private void OnlinePlay_Load(object sender, EventArgs e)
        {
            Updating = true;

            FormOpen = true;
            updateLB();
            chkListenForData.Checked = Listening;
            chkSendData.Checked = Sending;
            var MyIPString = new WebClient().DownloadString("http://icanhazip.com").Trim();
            Console.WriteLine(MyIPString);
            MyIP = IPAddress.Parse(MyIPString);
            txtPulbicIP.Text = MyIP.ToString();
            NudYourPort.Value = PortNumber;
            NudPort.Value = PortNumber;
            allowFullCheckToolStripMenuItem.Checked = (AllowCheckingItems);
            autoAddIncomingIPsToolStripMenuItem.Checked = (AutoAddIncomingConnections);
            onlyAcceptDataFromSendingListToolStripMenuItem.Checked = (StrictIP);
            copyNetDataToClipboardToolStripMenuItem.Visible = Debugging.ISDebugging;

            Updating = false;
        }

        public void updateLB()
        {
            LBIPAdresses.Items.Clear();
            foreach (var i in IPS) { LBIPAdresses.Items.Add(i); }
        }

        public static void ManageNetData(LogicObjects.MMRTpacket Data)
        {
            var log = LogicObjects.MainTrackerInstance.Logic;
            var IPInSendingList = IPS.FindIndex(f => f.IP.ToString() == Data.IPData.IP && f.PORT == Data.IPData.PORT) > -1;

            if (!IPInSendingList)
            {
                if (StrictIP) { return; }
                if (AutoAddIncomingConnections) { TriggerAddRemoteToIPList(Data); }
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
                if (IsMultiWorld)
                {
                    if (i.PI != MyPlayerID || i.Ch == false || i.RI < 0 || i.RI >= log.Count() || log[i.RI].IsEntrance()) { continue; }
                    var entry = new LogicObjects.LogicEntry { ID = -1, Checked = false, RandomizedItem = i.RI, SpoilerRandom = i.RI, Options = 0};
                    LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance, Data.PlayerID);
                }

                else if (log.ElementAt(i.ID) != null && !log[i.ID].Checked)
                {
                    var entry = log[i.ID];

                    if (entry.HasRandomItem(true) || entry.SpoilerRandom > -1)
                    {
                        if (!AllowCheckingItems || i.Ch == false)
                        {
                            if (entry.RandomizedItem < 0)
                            {
                                LogicEditing.MarkObject(entry);
                            }
                        }
                        else
                        {
                            LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance);
                        }
                    }
                    else
                    {
                        LogicObjects.MainTrackerInstance.Logic[i.ID].RandomizedItem = i.RI;
                        if (AllowCheckingItems && i.Ch)
                        {
                            LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance);
                        }
                    }
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
            updateLB();
        }

        private void saveIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

            List<LogicObjects.IPDATASerializable> SaveIPS = new List<LogicObjects.IPDATASerializable> { new LogicObjects.IPDATASerializable { IP = MyIP.ToString(), PORT = PortNumber } };
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
            updateLB();
        }

        private void onlyAcceptDataFromSendingListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StrictIP = !StrictIP;
            onlyAcceptDataFromSendingListToolStripMenuItem.Checked = (StrictIP);
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
    }
}
