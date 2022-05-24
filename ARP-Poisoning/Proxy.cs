using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using PacketDotNet;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Collections;

namespace ARP_Poisoning
{
    class Proxy
    {
        public HttpListener http_listener;
        public HttpClient http_client;
        public IAsyncResult ar;
        public DestroyDelegate destroyer;
        public Socket client_Socket;
        
        public Proxy(IPAddress ip)
        {
           
            //Start Listening
            http_listener = new HttpListener(ip, 5555);
           
             http_listener.Start();

            

            //MessageBox.Show("Proxy");

            
        }  
    }
}
