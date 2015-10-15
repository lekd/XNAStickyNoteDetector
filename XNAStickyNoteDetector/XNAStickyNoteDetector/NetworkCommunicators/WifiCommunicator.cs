using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace XNAStickyNoteDetector.NetworkCommunicators
{
    public class WifiCommunicator
    {
        readonly static object syncToken = new object();
        static Socket _sender = null;
        public static void Connect(String ipaddress, int port = 2015)
        {
            /*IPHostEntry ipHostInfo = Dns.GetHostEntry(ipaddress);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);*/

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sender.BeginConnect(ipaddress,port, new AsyncCallback(ConnectCallback), sender);
            //sender.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), sender);


        }
        private static void ConnectCallback(IAsyncResult connectResult)
        {
            _sender = (Socket)connectResult.AsyncState;
        }
        public static void Send(byte[] data)
        {
            lock (syncToken)
            {
                if (_sender != null)
                {
                    _sender.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), _sender);
                }
            }
        }
        private static void SendCallback(IAsyncResult sendResult)
        {
            
        }
        static public void Close()
        {
            if (_sender != null)
            {
                _sender.Close();
            }
        }

    }
}
