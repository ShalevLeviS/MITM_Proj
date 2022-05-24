using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;
using System.Net.NetworkInformation;

namespace ARP_Poisoning
{
    class DeviceUtill
    {
        SharpPcap.ICaptureDevice device;
        SharpPcap.CaptureDeviceList devices;
        int readTimeoutMilliseconds = 1000;


        /// <summary>
        /// open connection with our device
        /// </summary>
        /// <returns>The following devices are available on this machine </returns>
        public SharpPcap.ICaptureDevice OpenDevice()
        {
            // Print SharpPcap version
            string ver = SharpPcap.Version.VersionString;
            Console.WriteLine("SharpPcap {0}, Let's have some fun! ", ver);

            // Retrieve the device list
            devices = CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return null;
            }

            Console.WriteLine();
            Console.WriteLine("The following devices are available on this machine:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine();

            int i = 0;

            // Print out the devices
            foreach (var dev in devices)
            {
                Console.WriteLine("{0}) {1} {2}", i, dev.Name, dev.Description);
                i++;
            }

            Console.WriteLine();
            //  Console.Write("-- Please choose a device to capture: ");
            //i = int.Parse(Console.ReadLine());
            //i = 5;
            device = devices[0];
            return this.device;

        }// openDevice
    }
}
