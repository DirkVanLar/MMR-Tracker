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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms
{
    public partial class OnlinePlay : Form
    {
        public class IPDATA
        {
            public IPAddress IP { get; set; }
            public int PORT { get; set; }
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }
        public class IPDATASerializable
        {
            public string IP { get; set; }
            public int PORT { get; set; }
        }
        public class MMRTpacket
        {
            public int PlayerID { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public List<LogicObjects.NetData> LogicData { get; set; }

        }
        public OnlinePlay()
        {
            InitializeComponent();
            MainInterface.LocationChecked += MainInterface_LocationChecked;
        }

        public static event EventHandler NetDataProcessed = delegate { };
        public static event Action<MMRTpacket> TriggerAddRemoteToIPList = delegate { };

        public static bool Listening = false;
        public static bool Sending = false;
        public static bool Updating = false;
        public static bool FormOpen = false;
        public static IPAddress MyIP;
        public static int PortNumber = 2112;
        public static bool AllowCheckingItems = true;
        public static bool AllowAutoPortForward = false;
        public static bool AutoAddIncomingConnections = false;
        public static List<IPDATA> IPS = new List<IPDATA>();
        public static Socket listener;
        public static string ThisPC = Dns.GetHostName();
        public static NATUPNPLib.UPnPNATClass upnpnat = new NATUPNPLib.UPnPNATClass();
        public static NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;

        //Sending Data

        public static MMRTpacket createNetData()
        {
            List<LogicObjects.NetData> ClipboardNetData = new List<LogicObjects.NetData>();
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && x.HasRandomItem(true)))
            {
                ClipboardNetData.Add(new LogicObjects.NetData { ID = i.ID, Checked = i.Checked, RandomizedItem = i.RandomizedItem });
            }
            MMRTpacket Pack = new MMRTpacket();
            Pack.LogicData = ClipboardNetData;
            Pack.IP = MyIP.ToString();
            Pack.Port = PortNumber;
            Pack.PlayerID = 0;
            return Pack;
        }

        public static void StartClient(string M, IPDATA ip)
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

        private async void MainInterface_LocationChecked(object sender, EventArgs e)
        {
            if (!Sending) { return; }
            Console.WriteLine("Logic updated");
            string m = JsonConvert.SerializeObject(createNetData());
            foreach (var ip in IPS)
            { 
                await Task.Run(() => StartClient(m, ip)); 
            }
        }

        //RecievingData

        public static string RecieveData()
        {
            try
            {
                //Console.WriteLine("Starting: Creating Socket object");
                listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
                listener.Listen(10);

                //Console.WriteLine("Waiting for connection on port 2112");
                Socket socket = listener.Accept();
                string receivedValue = string.Empty;
                /*
                while (true)
                {
                    Console.WriteLine(socket.Available);
                    if (socket.Available > 0)
                    {
                        do
                        {
                            var receivedBytes = new byte[socket.Available];
                            socket.Receive(receivedBytes);
                            Console.WriteLine("Receiving...");
                            receivedValue += Encoding.Default.GetString(receivedBytes);
                        } while (socket.Available > 0);
                        break;
                    }
                }
                */
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

                Console.WriteLine("Received value: {0}", receivedValue);
                Console.WriteLine(counter);
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
                Console.WriteLine($"Errored: { e.ToString()}");
                return "";
            }
        }

        public static async void startServer()
        {
            if (Listening) { MessageBox.Show("Net client already started!"); return; }
            Listening = true;
            AddPort(PortNumber);
            while (Listening)
            {
                Console.WriteLine("About to Recieve data");
                string Logic = await Task.Run(() => RecieveData());
                Console.WriteLine(Logic);

                MMRTpacket NetData = new MMRTpacket();
                if (Logic != "")
                {
                    try
                    {
                        NetData = JsonConvert.DeserializeObject<MMRTpacket>(Logic);
                        ManageNetData(NetData);
                    }
                    catch { Console.WriteLine("Data Invalid"); }
                }
            }
            DeletePort(PortNumber);
            Console.WriteLine("Server Shutting Down");
            return;
        }

        //Port Handeling

        private void NudYourPort_ValueChanged(object sender, EventArgs e)
        {
            DeletePort(PortNumber);
            PortNumber = (int)NudYourPort.Value;
            Listening = false;
            try { listener.Close(); } catch { }
            if (chkListenForData.Checked)
            {
                startServer();
                Console.WriteLine("Server Started");

            }
        }

        private void OnlinePlay_Closing(object sender, EventArgs e)
        {
            DeletePort(PortNumber);
            FormOpen = false;
        }

        public static void AddPort(int port)
        {
            if (!AllowAutoPortForward) { return; }
            // Opening up TCP Port
            mappings.Add(port, "TCP", port, ThisPC, true, "MMRTracker");
        }

        public static void DeletePort(int port)
        {
            // Remove TCP forwarding for Port
            mappings.Remove(port, "TCP");
        }

        //Options
        private void allowFullCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllowCheckingItems = !AllowCheckingItems;
            allowFullCheckToolStripMenuItem.Name = (AllowCheckingItems) ? "Disallow Full Check" : "Allow Full Check";
        }

        private void allowAutoPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllowAutoPortForward = !AllowAutoPortForward;
            allowAutoPortToolStripMenuItem.Name = (AllowAutoPortForward) ? "Disallow Auto Port Forward" : "Allow Auto Port Forward";
        }

        private void autoAddIncomingIPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoAddIncomingConnections = !AutoAddIncomingConnections;
            autoAddIncomingIPsToolStripMenuItem.Name = (AutoAddIncomingConnections) ? "Don't Add Incoming IPs" : "Auto Add Incoming IPs";
        }

        private void copyNetDataToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(JsonConvert.SerializeObject(createNetData()));
        }

        //Controls

        private void btnAddIP_Click_1(object sender, EventArgs e)
        {
            IPDATA NewIP = new IPDATA();
            try { NewIP.IP = IPAddress.Parse(txtIP.Text); } catch { MessageBox.Show("IP Address not valid"); return; }
            NewIP.PORT = (int)NudPort.Value;
            NewIP.DisplayName = $"{NewIP.IP}:{NewIP.PORT}";
            IPS.Add(NewIP);
            updateLB();
        }

        private void btnRemoveIP_Click(object sender, EventArgs e)
        {
            foreach (var i in LBIPAdresses.SelectedItems)
            {
                IPS.RemoveAt(IPS.IndexOf(i as IPDATA));
            }
            updateLB();
        }

        private void chkListenForData_CheckedChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            if (chkListenForData.Checked)
            {
                startServer();
                Console.WriteLine("Server Started");
            }
            else
            {
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
            TriggerAddRemoteToIPList += OnlinePlay_TriggerAddRemoteToIPList;
            Updating = false;
        }

        public void updateLB()
        {
            LBIPAdresses.Items.Clear();
            foreach (var i in IPS) { LBIPAdresses.Items.Add(i); }
        }

        public static void ManageNetData(MMRTpacket Data)
        {
            if (AutoAddIncomingConnections && IPS.FindIndex(f => f.IP.ToString() == Data.IP) < 0)
            {
                TriggerAddRemoteToIPList(Data);
            }

            foreach (var i in Data.LogicData)
            {
                if (LogicObjects.MainTrackerInstance.Logic.ElementAt(i.ID) != null && !LogicObjects.MainTrackerInstance.Logic[i.ID].Checked)
                {
                    var entry = LogicObjects.MainTrackerInstance.Logic[i.ID];

                    if (entry.HasRandomItem(true) || entry.SpoilerRandom > -1)
                    {
                        if (!AllowCheckingItems || i.Checked == false)
                        {
                            LogicEditing.MarkObject(entry);
                        }
                        else
                        {
                            LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance);
                        }
                    }
                    else
                    {
                        LogicObjects.MainTrackerInstance.Logic[i.ID].RandomizedItem = i.RandomizedItem;
                        if (AllowCheckingItems && i.Checked)
                        {
                            LogicEditing.CheckObject(entry, LogicObjects.MainTrackerInstance);
                        }
                    }
                }
            }
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            NetDataProcessed(null, null);
        }

        private void OnlinePlay_TriggerAddRemoteToIPList(MMRTpacket Data)
        {
            IPDATA NewIP = new IPDATA();
            try { NewIP.IP = IPAddress.Parse(Data.IP); } catch { return; }
            NewIP.PORT = Data.Port;
            NewIP.DisplayName = $"{NewIP.IP}:{NewIP.PORT}";
            IPS.Add(NewIP);
            updateLB();
        }

        private void saveIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

            List<IPDATASerializable> SaveIPS = new List<IPDATASerializable> { new IPDATASerializable { IP = MyIP.ToString(), PORT = PortNumber } };
            foreach(var i in IPS)
            {
                SaveIPS.Add(new IPDATASerializable { IP = i.IP.ToString(), PORT = i.PORT });
            }

            File.WriteAllText(saveDialog.FileName, JsonConvert.SerializeObject(SaveIPS));
        }

        private void loadIPListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select An IP List", "MMR Tracker IP List (*.MMRTIP)|*.MMRTIP");
            if (file == "") { return; }
            List<IPDATASerializable> LoadData = new List<IPDATASerializable>();
            Console.WriteLine($"File Found");
            try { LoadData = JsonConvert.DeserializeObject<List<IPDATASerializable>>(File.ReadAllText(file)); }
            catch
            {
                MessageBox.Show("File Invalid!");
                return;
            }
            Console.WriteLine($"File Valid");
            foreach (var i in LoadData)
            {
                Console.WriteLine($"Checking {i.IP.Trim()}");
                if (i.IP != MyIP.ToString() && IPS.FindIndex(f => f.IP.ToString() == i.IP) < 0)
                {
                    IPAddress NIP;
                    try { NIP = IPAddress.Parse(i.IP.Trim()); } catch { Console.WriteLine($"{i.IP.Trim()} Invalid"); continue; }
                    IPS.Add(new IPDATA { IP = NIP, PORT = i.PORT, DisplayName = $"{NIP}:{i.PORT}" });
                    Console.WriteLine($"{i.IP.Trim()} Added");
                }
            }
            updateLB();
        }
    }
}
