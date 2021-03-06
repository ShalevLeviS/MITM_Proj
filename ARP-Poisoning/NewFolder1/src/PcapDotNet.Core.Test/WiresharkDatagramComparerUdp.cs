using System;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

namespace PcapDotNet.Core.Test
{
    internal class WiresharkDatagramComparerUdp : WiresharkDatagramComparer
    {
        protected override string PropertyName
        {
            get { return "Udp"; }
        }

        protected override bool CompareField(XElement field, Datagram parentDatagram, Datagram datagram)
        {
            IpV4Datagram ipV4Datagram = (IpV4Datagram)parentDatagram;
            UdpDatagram udpDatagram = (UdpDatagram)datagram;

            switch (field.Name())
            {
                case "udp.srcport":
                    field.AssertShowDecimal(udpDatagram.SourcePort);
                    break;

                case "udp.dstport":
                    field.AssertShowDecimal(udpDatagram.DestinationPort);
                    break;

                case "udp.port":
                    Assert.IsTrue(ushort.Parse(field.Show()) == udpDatagram.SourcePort ||
                                  ushort.Parse(field.Show()) == udpDatagram.DestinationPort);
                    break;

                case "udp.length":
                    field.AssertShowDecimal(udpDatagram.TotalLength);
                    break;

                case "udp.checksum":
                    field.AssertShowHex(udpDatagram.Checksum);
                    if (udpDatagram.Checksum != 0)
                    {
                        foreach (var checksumField in field.Fields())
                        {
                            switch (checksumField.Name())
                            {
                                case "udp.checksum_good":
                                    checksumField.AssertShowDecimal(ipV4Datagram.IsTransportChecksumCorrect);
                                    break;

                                case "udp.checksum_bad":
                                    if (checksumField.Show() == "1")
                                        Assert.IsFalse(ipV4Datagram.IsTransportChecksumCorrect);
                                    else
                                        checksumField.AssertShowDecimal(0);
                                    break;
                            }
                        }
                    }
                    break;

                case "udp.checksum_coverage":
                    field.AssertShowDecimal(udpDatagram.TotalLength);
                    break;

                default:
                    throw new InvalidOperationException("Invalid udp field " + field.Name());
            }

            return true;
        }
    }
}