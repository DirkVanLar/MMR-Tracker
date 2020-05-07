using MMR_Tracker.Class_Files;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        public OnlinePlay()
        {
            InitializeComponent();
            MainInterface.LocationChecked += MainInterface_LocationChecked;
        }

        public static bool Listening = false;
        public static bool Sending = false;
        public static bool Updating = false;
        public static int PortNumber = 2112;
        public static List<IPDATA> IPS = new List<IPDATA>();
        public static Socket listener;
        public static string localIP = Dns.GetHostName();
        public static NATUPNPLib.UPnPNATClass upnpnat = new NATUPNPLib.UPnPNATClass();
        public static NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;

        //Sending Data

        public static List<LogicObjects.NetData> createNetData()
        {
            List<LogicObjects.NetData> ClipboardNetData = new List<LogicObjects.NetData>();
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && x.HasRandomItem(true) && x.RandomizedEntry(LogicObjects.MainTrackerInstance).Aquired))
            {
                ClipboardNetData.Add(new LogicObjects.NetData { ID = i.ID, Checked = i.Checked, RandomizedItem = i.RandomizedItem });
            }
            return ClipboardNetData;
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
                    if (receivedValue.IndexOf("]") > -1)
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
            AddPort(PortNumber);
            if (Listening) { MessageBox.Show("Net client already started!"); return; }
            Listening = true;
            while (Listening)
            {
                Console.WriteLine("About to Recieve data");
                string Logic = await Task.Run(() => RecieveData());
                Console.WriteLine(Logic);

                List<LogicObjects.NetData> NetData = new List<LogicObjects.NetData>();
                if (Logic != "")
                {
                    try
                    {
                        NetData = JsonConvert.DeserializeObject<List<LogicObjects.NetData>>(Logic);
                        Tools.ManageNetData(NetData);
                    }
                    catch { Console.WriteLine("Data Invalid"); }
                }
            }
            Console.WriteLine("Server Shutting Down");
            return;
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
            updateLB();
            chkListenForData.Checked = Listening;
            chkSendData.Checked = Sending;
            txtPulbicIP.Text = new WebClient().DownloadString("http://icanhazip.com");
            NudYourPort.Value = PortNumber;
            NudPort.Value = PortNumber;
            Updating = false;
        }

        public void updateLB()
        {
            LBIPAdresses.Items.Clear();
            foreach (var i in IPS) { LBIPAdresses.Items.Add(i); }
        }

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
        }
        public static void AddPort(int port)
        {
            // Opening up TCP Port
            mappings.Add(port, "TCP", port, localIP, true, "MMRTracker");
        }
        public static void DeletePort(int port)
        {
            // Remove TCP forwarding for Port
            mappings.Remove(port, "TCP");
        }

    }
}
