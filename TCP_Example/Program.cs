using System;
using System.Net;
using System.Threading;
using System.Xml;

namespace TCP_Example
{
    class Program
    {
        static void Main()
        {
            int port = 0;
            IPAddress ipAddress = null;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("data.xml");
            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            // обход всех узлов в корневом элементе
            foreach (XmlNode xnode in xRoot)
            {
                // получаем атрибут name
                if (xnode.Attributes.Count > 0)
                {
                    XmlNode attr = xnode.Attributes.GetNamedItem("name");
                    if (attr != null)
                        Console.WriteLine(attr.Value);
                }
                // обходим все дочерние узлы элемента user
                foreach (XmlNode childnode in xnode.ChildNodes)
                {
                    // если узел - company
                    if (childnode.Name == "ip")
                    {
                        ipAddress = Dns.Resolve(childnode.InnerText).AddressList[0];
                        Console.WriteLine("IP: {0}", childnode.InnerText);
                    }
                    // если узел age
                    if (childnode.Name == "port")
                    {
                        port = Convert.ToInt32(childnode.InnerText);
                        Console.WriteLine("Port: {0}", childnode.InnerText);
                    }
                }
            }
            // Определим нужное максимальное количество потоков
            // Пусть будет по 4 на каждый процессор
            int MaxThreadsCount = Environment.ProcessorCount * 1;
            // Установим максимальное количество рабочих потоков
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            // Установим минимальное количество рабочих потоков
            ThreadPool.SetMinThreads(2, 2);
            // Создадим новый сервер на порту 5050
            Console.Title = String.Concat("TCP Server: ", ipAddress.ToString(), ":", port.ToString());
            new Server(port, ipAddress);   
        }
    }
}