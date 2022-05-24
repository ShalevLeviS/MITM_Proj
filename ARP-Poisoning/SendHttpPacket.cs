using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARP_Poisoning
{
    class SendHttpPacket
    {
        //static SharpPcap.ICaptureDevice device;

        string srcMac;
        string dstMac;
        string IpDst;
        string IpSrc;
        ushort SourcePort;

        public SendHttpPacket(string srcMac, string dstMac, string IpDst, string IpSrc, ushort SourcePort)
        {
            this.srcMac = srcMac;
            this.dstMac = dstMac;
            this.IpDst = IpDst;
            this.IpSrc = IpSrc;
            this.SourcePort = SourcePort;
        }

        public void SendPacket()
        {
            srcMac = ChangeString(srcMac);
            dstMac = ChangeString(dstMac);

            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                //("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            // Print the list
            
            PacketDevice selectedDevice = allDevices[allDevices.Count - 1];
            PacketCommunicator communicator = selectedDevice.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000);
            communicator.SendPacket(BuildHttpPacket());//Redirect
        }

        private Packet BuildHttpPacket()
        {

            EthernetLayer ethernetLayer =
                        new EthernetLayer
                            {
                                Source = new MacAddress(srcMac),
                                Destination = new MacAddress(dstMac),
                                EtherType = EthernetType.None, // Will be filled automatically.
                            };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                    {
                        Source = new IpV4Address(this.IpSrc),
                        CurrentDestination = new IpV4Address(this.IpDst),//dst
                        Fragmentation = IpV4Fragmentation.None,
                        HeaderChecksum = null, // Will be filled automatically.
                        Identification = 123,
                        Options = IpV4Options.None,
                        Protocol = null, // Will be filled automatically.
                        Ttl = 100,
                        TypeOfService = 0,
                    };

            TcpLayer tcpLayer =
                new TcpLayer
                    {
                        SourcePort = 4050,
                        DestinationPort = SourcePort,
                        Checksum = null, // Will be filled automatically.
                        SequenceNumber = 100,
                        AcknowledgmentNumber = 50,
                        ControlBits = TcpControlBits.Acknowledgment,
                        Window = 100,
                        UrgentPointer = 0,
                        Options = TcpOptions.None,
                    };

            HttpRequestLayer httpLayer =
                new HttpRequestLayer
                    {
                        Version = HttpVersion.Version11,
                        Header = new HttpHeader(new HttpContentLengthField(11)),
                        Body = new Datagram(Encoding.ASCII.GetBytes("Hello World")),
                        Method = new HttpRequestMethod(HttpRequestKnownMethod.Get),
                        Uri = @"https://www.facebook.com/",
                    };
            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, tcpLayer, httpLayer);

            return builder.Build(DateTime.Now);
        }

        public string ChangeString(string str)
        {
            string[] cs = new string[6];
            
            cs[0] = str.Substring(0, 2);
            cs[1] = str.Substring(2, 2);
            cs[2] = str.Substring(4, 2);
            cs[3] = str.Substring(6, 2);
            cs[4] = str.Substring(8, 2);
            cs[5] = str.Substring(10, 2);

           string result ="";
            for (int i = 0; i < 6; i++)
            {

                if (i == 5)
                    result += cs[i];
                else
                    result += cs[i] + ":";
            }
   
            return result;
        }

    }
}
