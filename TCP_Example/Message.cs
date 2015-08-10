using TCP_Example;
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

namespace TCP_Example
{
    public struct Message
    {
        public Int64 IMEI;
        public uint OneRecordLenth;
        public uint MessageCount;
        
        public DateTime DTime;
        public Int32 Longitude;
        public Int32 Latitude;
        public Int32 Speed;
        public Message(Int64 IMEI_, uint OneRecordLenth_, uint MessageCount_, Int32 Lingitude_, Int32 Latitude_, Int16 Speed_, DateTime DTime_)
        {
            IMEI = IMEI_;
            OneRecordLenth = OneRecordLenth_;
            MessageCount = MessageCount_;
            Longitude = Lingitude_;
            Latitude = Latitude_;
            Speed = Speed_;
            DTime = DTime_;
        }
        public DateTime ByteToDateTime(uint index, byte[] Buff)
        {
            byte[] tempBuff = new byte[sizeof(Int64)];
            Buffer.BlockCopy(Buff, (int)index, tempBuff, 0, 8);
            //String str = BitConverter.ToString(tempBuff);
            //Console.WriteLine(str);
            tempBuff = tempBuff.Reverse().ToArray();
            Int64 result = BitConverter.ToInt64(tempBuff, 0);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.Add(new TimeSpan(result * 10000 + 10800000L * 10000L));
        }
        public Int32 ByteToInt(uint index, byte[] Buff)
        {
            byte[] tempBuff = new byte[sizeof(int)];
            Buffer.BlockCopy(Buff, (int)index, tempBuff, 0, 4);
            tempBuff = tempBuff.Reverse().ToArray();
            int result = BitConverter.ToInt32(tempBuff, 0);
            return result;
        }
        public Int16 ByteToShort(uint index, byte[] Buff)
        {
            byte[] tempBuff = new byte[sizeof(short)];
            Buffer.BlockCopy(Buff, (int)index, tempBuff, 0, 2);
            tempBuff = tempBuff.Reverse().ToArray();
            short result = BitConverter.ToInt16(tempBuff, 0);
            return result;
        }
        public Int64 GetIMEI(uint BytesCount, byte[] Buff)
        {
            Int64 IMEI = 0;
            String Request = Encoding.ASCII.GetString(Buff, 0, (int)BytesCount);
            Request = Request.Substring(2);
            IMEI = Convert.ToInt64(Request);
            return IMEI;
        }
        public uint GetOffset()
        {
            uint value = MessageCount * OneRecordLenth;
            return value;
        }
        public void InitComponents(byte[] Buff)
        {
            OneRecordLenth = 32;
            MessageCount = Buff[9];
        }
    }
}
