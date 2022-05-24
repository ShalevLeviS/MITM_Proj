using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ARP_Poisoning
{
    class SendARP
    {
        [System.Runtime.InteropServices.DllImport("Iphlpapi.dll", EntryPoint = "SendARP")]
        internal extern static Int32 SendArp(Int32 destIpAddress, Int32 srcIpAddress,
                    byte[] macAddress, ref Int32 macAddressLength);
        IPAddress ip;
        IPAddress myIp;
        ListViewItem item1;
        Form1 f;
        int range = 0;

        public SendARP(Form1 f)
        {
            this.f = f;
        }
        /// <summary>
        /// משיג את הככתובת הפיזית של המחשב
        /// </summary>
        /// <param name="pIPAddress"></param>
        /// <returns></returns>
        public String GetMACFromNetworkComputer(IPAddress pIPAddress)
        {
            String lRetVal = String.Empty;
            Int32 lConvertedIPAddr = 0;
            
            byte[] lMACArray;
            int lByteArrayLen = 0;
           
            if (pIPAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("The remote system only supports IPv4 addresses");
            
            lConvertedIPAddr = ConvertIPToInt32(pIPAddress);
            
            lMACArray = new byte[6]; // 48 bit
            lByteArrayLen = lMACArray.Length;
            
            SendArp(lConvertedIPAddr, 0, lMACArray, ref lByteArrayLen);

            //return the MAC address in a PhysicalAddress format
            for (int i = 0; i < lMACArray.Length; i++)
            {
                lRetVal += String.Format("{0}", lMACArray[i].ToString("X2"));
                lRetVal += (i != lMACArray.Length - 1) ? "-" : "";
            } // for (in...

            return (lRetVal);
        
        }
        /// <summary>
        /// ממיר את האי.פי
        /// </summary>
        /// <param name="pIPAddr"></param>
        /// <returns></returns>
        private Int32 ConvertIPToInt32(IPAddress pIPAddr)
        {
            byte[] lByteAddress = pIPAddr.GetAddressBytes();
            return BitConverter.ToInt32(lByteAddress, 0);
        }

        /// <summary>
        /// מוסיף כתובת פיזית וכתובת אי.פי לרשימה רק אם הוא קיים
        /// </summary>
        /// <param name="ip"></param>
        public Task Send(string ip)
        {
            return Task.Run(() =>
                {
                    string lClientMAC = GetMACFromNetworkComputer(IPAddress.Parse(ip));
                    item1 = new ListViewItem(new string[] { ip.ToString(), lClientMAC, "Has not been poisoned" });
                    
                    if (lClientMAC.ToString() != "00-00-00-00-00-00")
                    {
                        f.AddToListView(item1);
                    }
                });    
        }
    }
}
