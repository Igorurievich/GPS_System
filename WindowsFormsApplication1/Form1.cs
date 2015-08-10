using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Threading.Tasks;

namespace BackgroundWorkerSimple
{
    public partial class Form1 : Form
    {
        int port = 5050;
        IPAddress ipAddress = Dns.Resolve("192.168.10.216").AddressList[0];
        TcpListener Listener;
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            
            int MaxThreadsCount = Environment.ProcessorCount * 20;
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            ThreadPool.SetMinThreads(2, 2);
        }


        private void startAsyncButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void cancelAsyncButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                backgroundWorker1.CancelAsync();
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
           IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 5050);
            Listener = new TcpListener(ipLocalEndPoint);
            //Listener.ExclusiveAddressUse = true;
            Listener.Start(); // Запускаем его
            Console.WriteLine("Server starting...");
            // В бесконечном цикле
            while (true)
            {
                System.Windows.Forms.MessageBox.Show("START");
                
                //Console.WriteLine("Start!");
                // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                // с использованием пула потоков.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
                //Console.WriteLine("New client!");
                System.Windows.Forms.MessageBox.Show("FUCK");
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //resultLabel.Text = (e.ProgressPercentage.ToString() + "%");
            MessageBox.Show("One new client!");
        }
        // This event handler deals with the results of the background operation.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                resultLabel.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                resultLabel.Text = "Error: " + e.Error.Message;
            }
            else
            {
                resultLabel.Text = "Done!";
            }
        }
        protected void SetTBText(string text)
        {
            textBox1.Text = text;
        }

        void ClientThread(Object StateInfo)
        {
            Client cl = new Client(this,(TcpClient)StateInfo);
        }

        // Остановка сервера
        public void StopServer()
        {
            // Если "слушатель" был создан
            if (Listener != null)
            {
                // Остановим его
                Listener.Stop();
            }
        }
    }
}
