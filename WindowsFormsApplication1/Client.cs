using BackgroundWorkerSimple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.InteropServices;
using FirebirdSql.Data.FirebirdClient;
using System.Data;
using WindowsFormsApplication1;

namespace BackgroundWorkerSimple
{
   
    public class Client
    {
        private Form1 Form;
        FbConnectionStringBuilder fb_con = new FbConnectionStringBuilder();
        FbConnection fb;
        Int64 IMEI;
        int flag = 0;
        int offset = 0;
        //MethodContainer MethodContainer_ = new MethodContainer();
        public Client(Form1 form, TcpClient Client_)
        {

            this.Form = form;
            GetMessage("Hello from contructor!");
            Console.WriteLine("[" + DateTime.Now + "]");
            byte[] Buffer = new byte[1024];
            int BytesCount;
            while (true)
            {
                if ((BytesCount = Client_.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    if (flag == 0)
                    {
                        String Request = Encoding.ASCII.GetString(Buffer, 0, BytesCount);
                        Request = Request.Substring(2);
                        try
                        {
                            IMEI = Convert.ToInt64(Request);
                        }
                        catch
                        {
                            Console.WriteLine("[IMEI]Format execption was excepted! ");
                        }
                        byte[] bf = { 1 };
                        Client_.GetStream().Write(bf, 0, bf.Length);
                        Console.WriteLine("Client: " + IMEI + " connected!");
                        flag = 1;
                        continue;
                    }
                    else if (flag == 1)
                    {
                        offset = 33 * Buffer[8];
                        try
                        {
                            DBConnect();
                            for (int i = Buffer[8]; i > 0; i--)
                            {
                                DateTime DateTime_ = ByteToDateTime(Buffer, offset + 10);
                                /*Console.WriteLine("DateTime" + DateTime);*/
                                Int32 LON_E = ByteToInt(Buffer, offset + 19);
                                /*Console.WriteLine("LON_E" + LON_E);*/
                                Int32 LAT_N = ByteToInt(Buffer, offset + 23);
                                /*Console.WriteLine("LAT_N" + LAT_N);*/
                                Int16 Pspeed = ByteToShort(Buffer, offset + 32);
                                DBSendData(IMEI, DateTime_, LAT_N, LON_E, Pspeed);
                                offset = offset - 33;
                            }
                            fb.Close();
                            flag = 2;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            try
            {
                Client_.Close();
                Console.WriteLine("Client was closed." + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void DBSendData(Int64 IMEI, DateTime dt, Int32 latitude, Int32 longitude, Int32 speed)
        {
            //Console.WriteLine("Sending data to database...");
            FbCommand insertSQL = new FbCommand("insert into sms(sms.IMEI, sms.DATE_, sms.TIME_, sms.LATITUDE, sms.LONGITUDE, sms.SPEED) values('" + IMEI + "','" + dt.ToShortDateString() + "','" + dt.ToLongTimeString() + "','" + latitude + "', '" + longitude + "','" + speed + "');", fb);

            //Action action3 = () => tbQuery.AppendText("\r\n" + insertSQL.CommandText + "\r\n");
            if (fb.State == ConnectionState.Closed)
            {
                fb.Open();
            }
            FbTransaction fbt = fb.BeginTransaction();
            insertSQL.Transaction = fbt;
            try
            {
                int res = insertSQL.ExecuteNonQuery(); //для запросов, не возвращающих набор данных (insert, update, delete) надо вызывать этот метод
                fbt.Commit(); //если вставка прошла успешно - комитим транзакцию
                Console.WriteLine("Data sended to database successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            insertSQL.Dispose();

        }
        private void DBConnect()
        {
            fb_con.Charset = "WIN1251"; //кодировка
            fb_con.UserID = "sysdba"; //логин
            fb_con.Password = "masterkey"; //пароль
            fb_con.Database = @"D:\IB Expert 2.0/TESTBD"; //путь к файлу бд
            fb_con.ServerType = 0; //указываем тип сервера (0 - "полноценный Firebird" (classic или super server), 1 - встроенный (embedded))
            fb = new FbConnection(fb_con.ToString()); //передаем нашу строку подключения объекту класса FbConnection
            FbDatabaseInfo fb_inf = null; //информация о БД
            if (fb.State != ConnectionState.Connecting)
            {
                fb.Open(); //открываем БД
                fb_inf = new FbDatabaseInfo(fb); //информация о БД
                
                Console.WriteLine("Connecting to database...");//выводим тип и версию
            }
            else
            {
                return;
            }
        }
        public string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        public DateTime ByteToDateTime(byte[] Buff, int index)
        {
            byte[] tempBuff = new byte[sizeof(long)];
            Buffer.BlockCopy(Buff, index, tempBuff, 0, 8);
            tempBuff = tempBuff.Reverse().ToArray();
            long result = BitConverter.ToInt64(tempBuff, 0);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.Add(new TimeSpan(result * 10000 + 10800000L * 10000L));
        }
        public Int32 ByteToInt(byte[] Buff, int index)
        {
            byte[] tempBuff = new byte[sizeof(int)];
            Buffer.BlockCopy(Buff, index, tempBuff, 0, 4);
            tempBuff = tempBuff.Reverse().ToArray();
            int result = BitConverter.ToInt32(tempBuff, 0);
            return result;
        }
        public Int16 ByteToShort(byte[] Buff, int index)
        {
            byte[] tempBuff = new byte[sizeof(short)];
            Buffer.BlockCopy(Buff, index, tempBuff, 0, 2);
            tempBuff = tempBuff.Reverse().ToArray();
            short result = BitConverter.ToInt16(tempBuff, 0);
            return result;
        }
        public void GetMessage(string txt)
        {
            Action action3 = () =>this.Form.textBox1.Text += txt;
        }
    }
}