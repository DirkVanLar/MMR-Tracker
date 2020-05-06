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
        public OnlinePlay()
        {
            InitializeComponent();
            MainInterface.LocationChecked += MainInterface_LocationChecked;
        }

        public static bool Listening = false;
        public static bool Sending = false;
        public static bool Updating = false;
        public static List<IPAddress> IPS = new List<IPAddress>();
        public static Socket listener;

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

        public static void StartClient(string M)
        {
            byte[] bytes = new byte[1024];

            foreach (var ip in IPS)
            {
                try
                {
                    // Connect to a Remote server  
                    // Get Host IP Address that is used to establish a connection  
                    // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                    // If a host has multiple addresses, you will get a list of addresses  
                    IPHostEntry host = Dns.GetHostEntry("localhost");
                    IPAddress ipAddress = host.AddressList[0];
                    //IPAddress ipAddress = IPAddress.Parse("192.168.1.1");
                    //IPAddress ipAddress = ip;
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                    // Create a TCP/IP  socket.    
                    Socket sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect the socket to the remote endpoint. Catch any errors.    
                    try
                    {
                        // Connect to Remote EndPoint  
                        sender.Connect(remoteEP);

                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());

                        // Encode the data string into a byte array.    
                        byte[] msg = Encoding.ASCII.GetBytes($"{M}<EOF>");

                        // Send the data through the socket.    
                        int bytesSent = sender.Send(msg);

                        // Receive the response from the remote device.    
                        int bytesRec = sender.Receive(bytes);
                        Console.WriteLine("Echoed test = {0}",
                            Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        // Release the socket.    
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
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private async void MainInterface_LocationChecked(object sender, EventArgs e)
        {
            string message = JsonConvert.SerializeObject(createNetData());
            await Task.Run(() => StartClient(message));
        }

        //RecievingData

        public static async void startServer()
        {
            if (Listening) { MessageBox.Show("Net client already started!"); return; }
            Listening = true;
            while (Listening)
            {
                string Logic = await Task.Run(() => Listenfordata());

                Logic = Logic.Replace("<EOF>", "");
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

        public static string Listenfordata()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try
            {

                // Create a Socket that will use Tcp protocol      
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 10 requests at a time  
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();
                Console.WriteLine("Data Recieved");

                // Incoming data from the client.    
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                listener.Close();

                Console.WriteLine("Text received : {0}", data);

                byte[] msg = Encoding.ASCII.GetBytes(data);
                //handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Errored: { e.ToString()}");
                return "";
            }

        }

        //Controls

        private void btnAddIP_Click_1(object sender, EventArgs e)
        {
            IPAddress NewIP;
            try { NewIP = IPAddress.Parse(txtIP.Text); } catch { MessageBox.Show("IP Address not valid"); return; }
            IPS.Add(NewIP);
            updateLB();
        }

        private void btnRemoveIP_Click(object sender, EventArgs e)
        {
            foreach (var i in LBIPAdresses.SelectedItems)
            {
                IPS.RemoveAt(IPS.IndexOf(i as IPAddress));
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
                listener.Close();
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
            Updating = false;
        }

        public void updateLB()
        {
            LBIPAdresses.Items.Clear();
            foreach (var i in IPS) { LBIPAdresses.Items.Add(i); }
        }
    }
}
