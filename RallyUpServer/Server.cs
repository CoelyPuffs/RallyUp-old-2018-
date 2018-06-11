using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RallyUpServer
{
    class Server
    {
        static void Main()
        {
            var serverSocket = new TcpListener(IPAddress.Any, 3292);
            serverSocket.Start();
            Console.WriteLine("Rally Up! Server Started.");
            while (true)
            {
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                LilClient newLil = new LilClient(clientSocket);
                new Thread(newLil.runClientThread).Start();
            }
        }
    }

    public static class TcpClientExtension
    {
        public static void WriteString(this TcpClient tcpClient, string msg)
        {
            msg += '\0';
            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            var stream = tcpClient.GetStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public static string getString(this TcpClient tcpClient)
        {
            var bytes = new byte[tcpClient.ReceiveBufferSize];
            var stream = tcpClient.GetStream();
            stream.Read(bytes, 0, tcpClient.ReceiveBufferSize);
            var msg = Encoding.ASCII.GetString(bytes);
            return msg.Substring(0, msg.IndexOf("\0", StringComparison.Ordinal));
        }
    }
}
