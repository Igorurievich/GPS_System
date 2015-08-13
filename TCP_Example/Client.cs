using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace TCP_Example
{

    class Client
    {
        static Dictionary<Int64, bool> Dictionary_ = new Dictionary<Int64, bool>();
        FbConnection fb;
        FbTransaction fbt;
        Message Msg;
        public byte[] Buff = new byte[1024];
        public Client(TcpClient Client_)
        {
            byte[] bf = { 1 };
            Client_.GetStream().Write(bf, 0, bf.Length);
            uint BytesCount;

            while (true)
            {
                try
                {
                    BytesCount = (uint)Client_.GetStream().Read(Buff, 0, Buff.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
                if (Msg.IMEI == 0 && Buff.Length > 0)
                {
                    try
                    {
                        Msg.IMEI = Msg.GetIMEI(BytesCount, Buff);
                        if (!Dictionary_.ContainsKey(Msg.IMEI))
                        {
                            Dictionary_.Add(Msg.IMEI, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        break;
                    }
                    Console.WriteLine("Client: " + Msg.IMEI + " connected!");
                    //foreach (KeyValuePair<Int64, bool> kvp in Dictionary)
                    //{
                    //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    //}
                    continue;
                }
                else if (Msg.IMEI > 0)
                {
                    Msg.InitComponents(Buff);
                    uint offset = Msg.GetOffset();
                    try
                    {
                        Console.WriteLine("[{0}] " + " [" + DateTime.Now + "]", Msg.IMEI);
                        DataBaseConnect();
                        fbt = fb.BeginTransaction();
                        List<Message> InvalidMsg = new List<Message>();
                        for (int i = 0; i < Msg.MessageCount; i++)
                        {
                            Msg.DTime = Msg.ByteToDateTime(offset + 2, Buff);
                            Msg.Longitude = Msg.ByteToInt(offset + 11, Buff);
                            Msg.Latitude = Msg.ByteToInt(offset + 15, Buff);
                            Msg.Speed = Msg.ByteToShort(offset + 24, Buff);
                            offset -= 33;
                            if (Msg.Speed == 0)
                            {
                                InvalidMsg.Add(Msg);
                                if (InvalidMsg.Count == Msg.MessageCount)
                                {
                                    Dictionary_[Msg.IMEI] = false;
                                    break;
                                }
                            }
                            else if (Msg.Speed != 0)
                            {
                                if (InvalidMsg.Count > 0)
                                {
                                    Int32 AVGLongitude = 0;
                                    Int32 AVGLatitude = 0;
                                    DateTime TempDateTime;
                                    TempDateTime = InvalidMsg.Last().DTime;
                                    //foreach (var item in InvalidMsg)
                                    //{
                                    //    AVGLongitude += item.Longitude;
                                    //    AVGLatitude += item.Latitude;
                                    //}
                                    //AVGLongitude = AVGLongitude / InvalidMsg.Count;
                                    //AVGLatitude = AVGLatitude / InvalidMsg.Count;
                                    AVGLongitude = InvalidMsg.Last().Longitude;
                                    AVGLatitude = InvalidMsg.Last().Latitude;
                                    DataBaseSendData(Msg.IMEI, TempDateTime, (Int32)AVGLatitude, (Int32)AVGLongitude, InvalidMsg.Last().Speed);
                                    InvalidMsg.Clear();
                                }
                                DataBaseSendData(Msg.IMEI, Msg.DTime, Msg.Latitude, Msg.Longitude, Msg.Speed);
                                Dictionary_[Msg.IMEI] = true;
                            }
                        }
                        fbt.Commit();
                        InvalidMsg.Clear();
                        fb.Close();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            try
            {
                Client_.Close();
                Console.WriteLine("Client was closed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void DataBaseSendData(Int64 IMEI, DateTime dt, Int32 latitude, Int32 longitude, Int32 speed)
        {
            //Console.WriteLine("Sending data to database...");
            FbCommand insertSQL = new FbCommand("insert into sms(sms.IMEI, sms.DATE_, sms.TIME_, sms.LATITUDE, sms.LONGITUDE, sms.SPEED) values('" + IMEI + "','" + dt.ToShortDateString() + "','" + dt.ToLongTimeString() + "','" + latitude + "', '" + longitude + "','" + speed + "');", fb);
            //Action action3 = () => tbQuery.AppendText("\r\n" + insertSQL.CommandText + "\r\n");
            if (fb.State == ConnectionState.Closed)
            {
                fb.Open();
            }
            
            insertSQL.Transaction = fbt;
            try
            {
                int res = insertSQL.ExecuteNonQuery(); //для запросов, не возвращающих набор данных (insert, update, delete) надо вызывать этот метод
                 //если вставка прошла успешно - комитим транзакцию
                //Console.WriteLine("[{0}]Data sended to database successfully!", IMEI);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            insertSQL.Dispose();
        }
        private void DataBaseConnect()
        {
            FbConnectionStringBuilder fb_con = new FbConnectionStringBuilder();
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
                //Console.WriteLine("[{0}]Connecting to database...", IMEI);//выводим тип и версию
            }
            else
            {
                return;
            }
        }
    }
}