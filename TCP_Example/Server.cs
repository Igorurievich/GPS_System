using TCP_Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP_Example
{
    class Server
    {
        TcpListener Listener; // Объект, принимающий TCP-клиентов
        public Server(int port, IPAddress ipAddress)
        {
            IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 5050);
            Listener = new TcpListener(ipLocalEndPoint);
            Listener.Start();
            Console.WriteLine("Server starting...");
            while (true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
            }
        }
        void ClientThread(Object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }
        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
        }
    }
}
