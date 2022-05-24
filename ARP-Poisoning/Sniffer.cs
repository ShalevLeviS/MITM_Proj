using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ARP_Poisoning
{
    public enum Protocol
    {
        NARP = 54,
        LARP = 91,
        TCP = 6,
        UDP = 17,
        Unknown = -1
    };


    public partial class Sniffer
    {
        string ipVictim = "";
        private Socket mainSocket;                          //The socket which captures all incoming packets
        private byte[] byteData = new byte[4096];
        private bool bContinueCapturing = false;            //A flag to check if packets are to be captured or not
        private IPHeader GetIPHeader;
        private delegate void AddTreeNode(TreeNode node);
        TreeView treeView = null;
        TreeNode tN = new TreeNode("shalev");
        ListView listView = new ListView();
        int i = 0;
        ListViewItem item1;
        string ipdst = "";
        string ipsrc = "";
        string protocol = "";
        string info = "";
        string myIpAddress = "";
        Form1 f1;
        string result;
        int startIndex;
        int endIndex;
        WebBrowser webbrowser;
        public void GetForm(Form1 f)
        {
            f1 = f;
        }
        public void SetMyIp(string ipaddress)
        {
            myIpAddress = ipaddress;
        }
        public void GetTreeView(TreeView tv)
        {
            treeView = tv;
        }
        public void IPVictim(string ip)
        {
            ipVictim = ip;
        }
        public void GetListView(ListView ls)
        {
            listView = ls;
        }
        public void GetWebBrowser(WebBrowser web)
        {
            webbrowser = web;
        }
        public void StartSniffing()
        {
            //if (cmbInterfaces.Text == "")
            //{
            //    MessageBox.Show("Select an Interface to capture the packets.", "ARP_Poisoning",
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            //MessageBox.Show(ipVictim);
            try
            {
                if (!bContinueCapturing)
                {
                    //Start capturing the packets...

                    //btnStart.Text = "&Stop";

                    bContinueCapturing = true;

                    //For sniffing the socket to capture the packets has to be a raw socket, with the
                    //address family being of type internetwork, and protocol being IP
                    mainSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Raw, ProtocolType.IP);

                    //Bind the socket to the selected IP address
                    mainSocket.Bind(new IPEndPoint(IPAddress.Parse(myIpAddress), 0));

                    //Set the socket  options
                    mainSocket.SetSocketOption(SocketOptionLevel.IP,            //Applies only to IP packets
                                               SocketOptionName.HeaderIncluded, //Set the include the header
                                               true);                           //option to true

                    byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
                    byte[] byOut = new byte[4] { 1, 0, 0, 0 }; //Capture outgoing packets

                    //Socket.IOControl is analogous to the WSAIoctl method of Winsock 2
                    mainSocket.IOControl(IOControlCode.ReceiveAll,              //Equivalent to SIO_RCVALL constant
                        //of Winsock 2
                                         byTrue,
                                         byOut);

                    //Start receiving the packets asynchronously
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
                else
                {
                    //btnStart.Text = "&Start";
                    bContinueCapturing = false;
                    //To stop capturing the packets close the socket
                    mainSocket.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ARP_Poisoning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int nReceived = mainSocket.EndReceive(ar);

                ParseData(byteData, nReceived);

                if (bContinueCapturing)
                {
                    byteData = new byte[4096];

                    //Another call to BeginReceive so that we continue to receive the incoming
                    //packets
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
                //MessageBox.Show( "ARP_Poisoning");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ARP_Poisoning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// return the ip src of the packet
        /// </summary>
        /// <param name="bytedata"></param>
        /// <param name="nReceived"></param>
        /// <returns></returns>
        private string PacketSourceIPAddress(byte[] bytedata, int nReceived)
        {
            IPHeader ipHeader = new IPHeader(bytedata, nReceived);
            return ipHeader.SourceAddress.ToString();
        }
        /// <summary>
        /// return the protocol type of the packet
        /// </summary>
        /// <param name="bytedata"></param>
        /// <param name="nReceived"></param>
        /// <returns></returns>
        private string Port(byte[] bytedata, int nReceived)
        {
            IPHeader ipHeader = new IPHeader(bytedata, nReceived);
            TCPHeader tcpHeader = new TCPHeader(ipHeader.Data,//IPHeader.Data stores the data being 
                //carried by the IP datagram
                                                           ipHeader.MessageLength);
            return tcpHeader.DestinationPort;
        }

        public void ShowData(byte[] data)
        {

            //MessageBox.Show(str.Substring(str.IndexOf("http"), str.IndexOf("com") - str.IndexOf("http") + 3));
        }
        /// <summary>
        /// מפרסר את המידע ומכניס אותו לתוך טבלת עץ
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="nReceived"></param>

        private void ParseData(byte[] byteData, int nReceived)
        {
            TreeNode rootNode = new TreeNode();

            //Since all protocol packets are encapsulated in the IP datagram
            //so we start by parsing the IP header and see what protocol data
            //is being carried by it
            IPHeader ipHeader = new IPHeader(byteData, nReceived);

            TreeNode ipNode = MakeIPTreeNode(ipHeader);
            rootNode.Nodes.Add(ipNode);
            //////////////////////////////////////////////////////////
            //ipdst = ipHeader.DestinationAddress;
            //ipsrc = ipHeader.SourceAddress;
            /////////////////////////////////////////////////////////
            //Now according to the protocol being carried by the IP datagram we parse 
            //the data field of the datagram

            switch (ipHeader.ProtocolType)
            {
                case Protocol.NARP:

                    MessageBox.Show("NARP");
                    break;

                case Protocol.LARP:

                    MessageBox.Show("LARP");
                    break;

                case Protocol.TCP:

                    TCPHeader tcpHeader = new TCPHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                        //carried by the IP datagram
                                                        ipHeader.MessageLength);//Length of the data field         


                    byte[] data = tcpHeader.Data;
                    string str = Encoding.UTF8.GetString(data, 0, data.Length);

                    //int endIndexCOM = str.LastIndexOf("com");
                    //protocol = "Tcp";
                    if (tcpHeader.DestinationPort == "80" && (str.Contains("Host") || str.Contains("Referer")))
                    {
                        startIndex = str.IndexOf("http:");
                        endIndex = str.LastIndexOf("/");
                        if (startIndex >= 0)
                        {
                            result = str.Substring(startIndex, endIndex - startIndex);

                            //MessageBox.Show(result);
                            //webbrowser.Navigate(result);
                            //Thread.Sleep(10000);
                            //MessageBox.Show("src: " + ipHeader.SourceAddress.ToString() + " dst: " + ipHeader.DestinationAddress.ToString() + " protocol: TCP  info:" + result);
                            item1 = new ListViewItem(new string[] { ipHeader.DestinationAddress.ToString(), "HTTP", result, ipHeader.SourceAddress.ToString() });
                            f1.AddToListView2(item1);
                        }
                        //result = str.Substring(startIndex, (endIndexCOM - startIndex));
                        //MessageBox.Show(str);
                        //MessageBox.Show(result);
                    }
                    TreeNode tcpNode = MakeTCPTreeNode(tcpHeader);

                    rootNode.Nodes.Add(tcpNode);

                    //If the port is equal to 53 then the underlying protocol is DNS
                    //Note: DNS can use either TCP or UDP thats why the check is done twice
                    if (tcpHeader.DestinationPort == "53" || tcpHeader.SourcePort == "53")
                    {
                        TreeNode dnsNode = MakeDNSTreeNode(tcpHeader.Data, (int)tcpHeader.MessageLength);
                        rootNode.Nodes.Add(dnsNode);
                    }

                    //item1 = new ListViewItem(new string[] { ipHeader.SourceAddress.ToString() , ipHeader.DestinationAddress.ToString() , "TCP", info });
                    //f1.AddToListView2(item1);
                    break;

                case Protocol.UDP:
                    //protocol = "Udp";
                    UDPHeader udpHeader = new UDPHeader(ipHeader.Data,              //IPHeader.Data stores the data being 
                        //carried by the IP datagram
                                                       (int)ipHeader.MessageLength);//Length of the data field                    

                    TreeNode udpNode = MakeUDPTreeNode(udpHeader);

                    rootNode.Nodes.Add(udpNode);

                    //If the port is equal to 53 then the underlying protocol is DNS
                    //Note: DNS can use either TCP or UDP thats why the check is done twice
                    if (udpHeader.DestinationPort == "53" || udpHeader.SourcePort == "53")
                    {

                        TreeNode dnsNode = MakeDNSTreeNode(udpHeader.Data,
                            //Length of UDP header is always eight bytes so we subtract that out of the total 
                            //length to find the length of the data
                                                           Convert.ToInt32(udpHeader.Length) - 8);
                        rootNode.Nodes.Add(dnsNode);
                    }
                    //item1 = new ListViewItem(new string[] { ipHeader.SourceAddress.ToString() , ipHeader.DestinationAddress.ToString() , "UDP", "" });
                    //f1.AddToListView2(item1);
                    break;

                case Protocol.Unknown:
                    break;
            }

            AddTreeNode addTreeNode = new AddTreeNode(OnAddTreeNode);
            if (ipHeader.SourceAddress.ToString() == ipVictim)
            {
                rootNode.Text = ipHeader.SourceAddress.ToString() + "-" +
                    ipHeader.DestinationAddress.ToString();

                //Thread safe adding of the nodes
                treeView.Invoke(addTreeNode, new object[] { rootNode });
                //MessageBox.Show("shalev"); 
                //item1 = new ListViewItem(new string[] { ipsrc.ToString(), ipdst.ToString(), protocol,"" });
                //f.AddToListView2(item1);
            }
        }


        //Helper function which returns the information contained in the IP header as a
        //tree node
        private TreeNode MakeIPTreeNode(IPHeader ipHeader)
        {
            TreeNode ipNode = new TreeNode();

            ipNode.Text = "IP";
            ipNode.Nodes.Add("Ver: " + ipHeader.Version);
            ipNode.Nodes.Add("Header Length: " + ipHeader.HeaderLength);
            ipNode.Nodes.Add("Differntiated Services: " + ipHeader.DifferentiatedServices);
            ipNode.Nodes.Add("Total Length: " + ipHeader.TotalLength);
            ipNode.Nodes.Add("Identification: " + ipHeader.Identification);
            ipNode.Nodes.Add("Flags: " + ipHeader.Flags);
            ipNode.Nodes.Add("Fragmentation Offset: " + ipHeader.FragmentationOffset);
            ipNode.Nodes.Add("Time to live: " + ipHeader.TTL);
            switch (ipHeader.ProtocolType)
            {
                case Protocol.TCP:
                    ipNode.Nodes.Add("Protocol: " + "TCP");
                    break;
                case Protocol.UDP:
                    ipNode.Nodes.Add("Protocol: " + "UDP");
                    break;
                case Protocol.Unknown:
                    ipNode.Nodes.Add("Protocol: " + "Unknown");
                    break;
            }
            ipNode.Nodes.Add("Checksum: " + ipHeader.Checksum);
            ipNode.Nodes.Add("Source: " + ipHeader.SourceAddress.ToString());
            ipNode.Nodes.Add("Destination: " + ipHeader.DestinationAddress.ToString());

            return ipNode;
        }

        //Helper function which returns the information contained in the TCP header as a
        //tree node
        private TreeNode MakeTCPTreeNode(TCPHeader tcpHeader)
        {
            TreeNode tcpNode = new TreeNode();

            tcpNode.Text = "TCP";

            tcpNode.Nodes.Add("Source Port: " + tcpHeader.SourcePort);
            tcpNode.Nodes.Add("Destination Port: " + tcpHeader.DestinationPort);
            tcpNode.Nodes.Add("Sequence Number: " + tcpHeader.SequenceNumber);

            if (tcpHeader.AcknowledgementNumber != "")
                tcpNode.Nodes.Add("Acknowledgement Number: " + tcpHeader.AcknowledgementNumber);

            tcpNode.Nodes.Add("Header Length: " + tcpHeader.HeaderLength);
            tcpNode.Nodes.Add("Flags: " + tcpHeader.Flags);
            tcpNode.Nodes.Add("Window Size: " + tcpHeader.WindowSize);
            tcpNode.Nodes.Add("Checksum: " + tcpHeader.Checksum);

            if (tcpHeader.UrgentPointer != "")
                tcpNode.Nodes.Add("Urgent Pointer: " + tcpHeader.UrgentPointer);
            if (result != "")

                tcpNode.Nodes.Add("Info: " + result);
            return tcpNode;
        }

        //Helper function which returns the information contained in the UDP header as a
        //tree node
        private TreeNode MakeUDPTreeNode(UDPHeader udpHeader)
        {
            TreeNode udpNode = new TreeNode();

            udpNode.Text = "UDP";
            udpNode.Nodes.Add("Source Port: " + udpHeader.SourcePort);
            udpNode.Nodes.Add("Destination Port: " + udpHeader.DestinationPort);
            udpNode.Nodes.Add("Length: " + udpHeader.Length);
            udpNode.Nodes.Add("Checksum: " + udpHeader.Checksum);

            return udpNode;
        }

        //Helper function which returns the information contained in the DNS header as a
        //tree node
        private TreeNode MakeDNSTreeNode(byte[] byteData, int nLength)
        {
            DNSHeader dnsHeader = new DNSHeader(byteData, nLength);

            TreeNode dnsNode = new TreeNode();

            dnsNode.Text = "DNS";
            dnsNode.Nodes.Add("Identification: " + dnsHeader.Identification);
            dnsNode.Nodes.Add("Flags: " + dnsHeader.Flags);
            dnsNode.Nodes.Add("Questions: " + dnsHeader.TotalQuestions);
            dnsNode.Nodes.Add("Answer RRs: " + dnsHeader.TotalAnswerRRs);
            dnsNode.Nodes.Add("Authority RRs: " + dnsHeader.TotalAuthorityRRs);
            dnsNode.Nodes.Add("Additional RRs: " + dnsHeader.TotalAdditionalRRs);

            return dnsNode;
        }

        private void OnAddTreeNode(TreeNode node)
        {
            treeView.Nodes.Add(node);
        }

        private void SnifferForm_Load(object sender, EventArgs e)
        {
            string strIP = null;

            IPHostEntry HosyEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HosyEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HosyEntry.AddressList)
                {
                    strIP = ip.ToString();
                    //cmbInterfaces.Items.Add(strIP);
                }
            }
        }

        private void SnifferForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bContinueCapturing)
            {
                mainSocket.Close();
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
