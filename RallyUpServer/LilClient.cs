using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using RallyUpLibrary;

namespace RallyUpServer
{
    // This is the client handler on the server side. 
    class LilClient
    {
        private TcpClient clientSocket;
        private string clientName;

        public void initializeClient(TcpClient clientSocket, string clientName)
        {
            this.clientName = clientName;
            this.clientSocket = clientSocket;
            Thread thread = new Thread(runClientThread);
            thread.Start();
        }

        private void runClientThread()
        {
            while (true)
            {
                try
                {
                    string clientData = clientSocket.ReadString();
                    Console.WriteLine(clientData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
