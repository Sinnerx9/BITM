using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BITM.BlazeSDK;
using static BITM.BlazeSDK.Enums;
using Newtonsoft.Json;

namespace BITM.BlazeSDK
{
    public static class Blaze
    {

        public static int pos = 0;

        #region Classes and Structs
        public class Packet
        {
            public int Length;         //4
            public ushort extLength;    //2
            public ushort Component;    //2
            public ushort Command;      //2
            public byte Error;        //1
            public ushort ID;           //2
            public uint QType;        //2
            public byte Unknown;      //1
            public byte[] Content;

            public Packet()
            {
                Length = 0;
                extLength = 0;
                Component = 0;
                Command = 0;
                Error = 0;
                //IDExtra = 0x0000;
                ID = 0;
                QType = 0;
                //Unknown = 0;
            }
        }
        public struct DoubleVal
        {
            public long v1;
            public long v2;
            public DoubleVal(long V1, long V2)
            {
                v1 = V1;
                v2 = V2;
            }
        }
        public struct TrippleVal
        {
            public long v1;
            public long v2;
            public long v3;
            public TrippleVal(long V1, long V2, long V3)
            {
                v1 = V1;
                v2 = V2;
                v3 = V3;
            }
        }
        public class Tdf
        {
            public string Label;
            [JsonIgnore]
            public uint Tag;
            [JsonIgnore]
            public byte Type;
            //public TreeNode ToTree()
            //{
            //    return new TreeNode(Label + " : " + Type);
            //}
            public void Set(string label, byte type)
            {
                Label = label;
                Type = type;
                Tag = 0;
                byte[] buff = Label2Tag(label);
                Tag |= (uint)(buff[0] << 24);
                Tag |= (uint)(buff[1] << 16);
                Tag |= (uint)(buff[2] << 8);
            }
        }
        public class TdfInteger : Tdf
        {
            public long Value;
            public static TdfInteger Create(string Label, long value)
            {
                TdfInteger res = new TdfInteger();
                res.Set(Label, 0);
                res.Value = value;
                return res;
            }
            public static TdfInteger Create(string Label, long value, byte type)
            {
                TdfInteger res = new TdfInteger();
                res.Set(Label, type);
                res.Value = value;
                return res;
            }
        }
        public class TdfBlob : Tdf
        {
            public long Length;
            public byte[] data;
            public static TdfBlob Create(string Label, byte[] buff)
            {
                TdfBlob res = new TdfBlob();
                res.Set(Label, 0x2);
                res.Length = buff.Length;
                res.data = buff;
                return res;
            }
        }
        public class TdfFloat : Tdf
        {
            public float Value;
            public static TdfFloat Create(string Label, float value)
            {
                TdfFloat res = new TdfFloat();
                res.Set(Label, 0xA);
                res.Value = value;
                return res;
            }
        }
        public class TdfString : Tdf
        {
            public string Value;
            public static TdfString Create(string Label, string value)
            {
                TdfString res = new TdfString();
                res.Set(Label, 1);
                res.Value = value;
                return res;
            }

        }

        public class TdfStruct : Tdf
        {
            public List<Tdf> Values;
            public bool startswith2;
            public bool endswith0;
            public static TdfStruct Create(string Label, List<Tdf> list, bool start2 = false, bool end0 = false)
            {
                TdfStruct res = new TdfStruct();
                res.startswith2 = start2;
                res.endswith0 = end0;
                res.Set(Label, 3);
                res.Values = list;
                return res;
            }
            public TdfStruct DeepCopy()
            {
                TdfStruct res = (TdfStruct)this.MemberwiseClone();
                return res;
            }
        }
        public class TdfList : Tdf
        {
            public byte SubType;
            public int Count;
            public object List;
            public static TdfList Create(string Label, byte subtype, int count, object list)
            {
                TdfList res = new TdfList();
                res.Set(Label, 4);
                res.SubType = subtype;
                res.Count = count;
                res.List = list;
                return res;
            }
            public TdfList DeepCopy()
            {
                TdfList res = (TdfList)this.MemberwiseClone();
                return res;
            }
        }
        public class TdfIntegerList : Tdf
        {
            public int Count;
            public List<long> List;
            public static TdfIntegerList Create(string Label, int count, List<long> list)
            {
                TdfIntegerList res = new TdfIntegerList();
                res.Set(Label, 7);
                res.Count = count;
                res.List = list;
                return res;
            }
        }

        /// <summary>
        /// TdfMap
        /// </summary>
        public class TdfDoubleList : Tdf
        {
            public byte SubType1;
            public byte SubType2;
            public int Count;
            public object List1;
            public object List2;
            public bool shit;
            public static TdfDoubleList Create(string Label, byte subtype1, byte subtype2, object list1, object list2, int count, bool shit = false)
            {
                TdfDoubleList res = new TdfDoubleList();
                res.Set(Label, 5);
                res.SubType1 = subtype1;
                res.SubType2 = subtype2;
                res.List1 = list1;
                res.List2 = list2;
                res.Count = count;
                res.shit = shit;
                return res;
            }
        }

        /// <summary>
        /// TDFVector2
        /// </summary>
        public class TdfDoubleVal : Tdf
        {
            public DoubleVal Value;
            public static TdfDoubleVal Create(string Label, DoubleVal v)
            {
                TdfDoubleVal res = new TdfDoubleVal();
                res.Set(Label, 8);
                res.Value = v;
                return res;
            }
        }
        /// <summary>
        /// TDFVector3
        /// </summary>
        public class TdfTrippleVal : Tdf
        {
            public TrippleVal Value;
            public static TdfTrippleVal Create(string Label, TrippleVal v)
            {
                TdfTrippleVal res = new TdfTrippleVal();
                res.Set(Label, 9);
                res.Value = v;
                return res;
            }
        }
        #endregion

        #region Functions
        public static byte[] CreatePacket(ushort Component, ushort Command, byte Error, Enums.MessageType QType, ushort ID, byte Unknown, List<Tdf> Content)
        {
            MemoryStream s = new MemoryStream(); //  Return stream

            MemoryStream content = new MemoryStream();//  body stream

            foreach (Tdf tdf in Content)
                WriteTdf(tdf, content);

            int len = (int)content.ToArray().Length;

            /*WriteUShort4bytes(s, len);
            WriteUShort2bytes(s, 0);
            WriteUShort2bytes(s, Component);
            WriteUShort2bytes(s, Command);
            WriteUShort1byte(s, (byte)Error);
            WriteUShort1byte(s, IDExtra);
            WriteUShort1byte(s, ID);
            WriteUShort1byte(s, (byte)QType);
            WriteUShort2bytes(s, 0);*/

            if (QType == Enums.MessageType.ERROR_REPLY)
            {
                Writebyte(s, 0);
                Writebyte(s, 0);
                Writebyte(s, 0);
                Writebyte(s, 0);
                Writebyte(s, (byte)((len & 0xFFFF) >> 8));
                Writebyte(s, (byte)(len & 0xFF));
            }
            else
            {
                if (len < 0xFFFF)
                {
                    Writebyte(s, 0);
                    Writebyte(s, 0);
                    Writebyte(s, (byte)((len & 0xFFFF) >> 8));
                    Writebyte(s, (byte)(len & 0xFF));
                    Writebyte(s, 0);
                    Writebyte(s, 0);
                }
                else
                {
                    WriteInt(s, len);
                    Writebyte(s, 0);
                    Writebyte(s, 0);
                }
            }


            WriteUShort(s, Component);
            WriteUShort(s, Command);
            Writebyte(s, (byte)Error);
            WriteUShort(s, ID);
            WriteUShort(s, (ushort)QType);
            Writebyte(s, Unknown);

            s.Write(content.ToArray(), 0, len);
            return s.ToArray();
        }
        public static string getCommandString(BITM.BlazeSDK.Enums.Component component, ushort command)
        {
            switch (component)
            {
                case Component.AUTHENTICATION:
                    return ((c_Authentication)command).ToString();
                case Component.GAMEMANAGER:
                    return ((c_GameManager)command).ToString();
                case Component.UTIL:
                    return ((c_Util)command).ToString();
                case Component.USERSESSIONS:
                    return ((c_UserSessions)command).ToString();
                default:
                    return command.ToString();
            }
        }
        public static string Packets2Json(Packet[] packets, Formatting f = Formatting.None)
        {

            var components = new List<IBlazeComponent>();
            for (int i = 0; i < packets.Length; i++)
            {
                if (packets[i].Component != 0)
                {
                    var cmp = new IBlazeComponent();
                    cmp.Component = packets[i].Component.ToString();
                    cmp.Command = packets[i].Command.ToString();
                    cmp.Data = ReadPacketContent(packets[i]);
                    components.Add(cmp);
                }
            }
            if (components.Count == 0)
            {
                return "";
            }
            return JsonConvert.SerializeObject(components, f);
        }
        public static Packet[] FetchAllPacket(Stream s)
        {
            List<Packet> res = new List<Packet>();
            while (s.Position < s.Length)
            {
                res.Add(ReadBlazePacket(s));
            }
            return res.ToArray();
        }
        public static Packet ReadBlazePacket(Stream s)
        {
            Blaze.Packet packet = new Blaze.Packet();
            packet.Length = (int)ReadUShort(s);
            packet.Component = ReadUShort(s);
            packet.Command = ReadUShort(s);
            packet.Error = (byte)ReadUShort(s);
            packet.QType = ReadUShort(s);
            packet.ID = ReadUShort(s);
            if ((packet.QType & 16) != 0)
            {
                packet.extLength = Blaze.ReadUShort(s);
            }
            else
            {
                packet.extLength = 0;
            }
            int num = (int)(packet.Length + (uint)((uint)packet.extLength << 16));
            packet.Content = new byte[num];
            s.Read(packet.Content, 0, num);
            return packet;
            // Packet res = new Packet();
            // res.Length = (int)ReadUint(s);        //0-3

            // if (res.Length >= 0x8000)
            // {
            //     res.Length |= ReadUShort(s) << 16;
            // }
            // else
            // {
            //     res.extLength = ReadUShort(s); //4-5
            // }

            // res.Component = (BITM.BlazeSDK.Enums.Component)ReadUShort(s); //6-7
            // res.Command = ReadUShort(s); //8-9
            // res.Error = ReadByte(s);  //10

            // res.ID = ReadUShort(s);     //11-12
            // res.QType = ReadUShort(s);  //13-14
            //// res.Unknown = ReadByte(s); //15

            // int len = (int)res.Length + res.extLength;

            // res.Content = new byte[len];
            // s.Read(res.Content, 0, len);

            // return res;
        }

        public static Packet ReadBlazePacket(MemoryStream s)
        {
            Blaze.Packet packet = new Blaze.Packet();
            packet.Length = (int)ReadUShort(s);
            packet.Component = ReadUShort(s);
            packet.Command = ReadUShort(s);
            packet.Error = (byte)ReadUShort(s);
            packet.QType = ReadUShort(s);
            packet.ID = ReadUShort(s);
            if ((packet.QType & 16) != 0)
            {
                packet.extLength = Blaze.ReadUShort(s);
            }
            else
            {
                packet.extLength = 0;
            }
            int num = (int)(packet.Length + (uint)((uint)packet.extLength << 16));
            packet.Content = new byte[num];
            s.Read(packet.Content, 0, num);
            return packet;
            //Packet res = new Packet();
            //res.Length = (int)ReadUint(s);        //0-3

            //if (res.Length >= 0x8000)
            //{
            //    res.Length |= ReadUShort((s)) << 16;
            //}
            //else
            //{
            //    res.extLength = ReadUShort((s)); //4-5
            //}

            //res.Component = (BITM.BlazeSDK.Enums.Component)ReadUShort((s)); //6-7
            //res.Command = ReadUShort((s)); //8-9
            //res.Error = ReadByte((s));  //10

            //res.ID = ReadUShort((s));     //11-12
            //res.QType = ReadUShort((s));  //13-14
            //res.Unknown = ReadByte((s)); //15

            //int len = (int)res.Length + res.extLength;

            //res.Content = new byte[len];
            //s.Read(res.Content, 0, len);
            //return res;
        }
        /*public static List<Packet> FetchAllBlazePackets(Stream s)
        {
            List<Packet> res = new List<Packet>();
            s.Seek(0, 0);
            while (s.Position < s.Length)
            {
                try
                {
                    res.Add(ReadBlazePacket(s));
                }
                catch (Exception)
                {
                    s.Position = s.Length;
                }
            }
            return res;
        }*/
        public static ushort ReadUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return (ushort)((buff[0] << 8) + buff[1]);
        }
        public static ushort ReadUShort(BinaryReader s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return (ushort)((buff[0] << 8) + buff[1]);
        }
        public static byte ReadByte(Stream s)
        {
            byte[] buff = new byte[1];
            s.Read(buff, 0, 1);
            return (byte)(buff[0]);
        }
        public static void Writebyte(Stream s, ushort u)
        {
            s.WriteByte((byte)(u & 0xFF));
        }

        public static void WriteUShort(Stream s, ushort u)
        {
            s.WriteByte((byte)((u & 0xFFFF) >> 8));
            s.WriteByte((byte)(u & 0xFF));

        }
        /*public static void WriteUShortUnknown2bytes(Stream s, ushort u)
        {
            if (u > 0)
            {
                Logger.Log("Unknown = " + u.ToString());
                Logger.Log("byte1 = " + ((byte)((u & 0xFFFF) >> 8)).ToString("X"));
                Logger.Log("byte2 = " + ((byte)(u & 0xFF)).ToString("X"));
            }
            s.WriteByte((byte)(u & 0xFF)); // reverse order
            s.WriteByte((byte)((u & 0xFFFF) >> 8));
        }*/
        public static uint ReadUint(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return (uint)((buff[0] << 24) + (buff[1] << 16) + (buff[2] << 8) + buff[3]);
        }

        public static void WriteInt(Stream s, int value)
        {
            s.WriteByte((byte)((value & 0xFF000000) >> 24));
            s.WriteByte((byte)((value & 0x00FF0000) >> 16));
            s.WriteByte((byte)((value & 0xFFFF) >> 8));
            s.WriteByte((byte)(value & 0xFF));

            //s.WriteByte((byte)(u >> 24));
            //s.WriteByte((byte)(u >> 16));
            //s.WriteByte((byte)(u >> 8));
            //s.WriteByte((byte)(u & 0xFF));
        }
        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            byte[] buffr = new byte[4];
            s.Read(buff, 0, 4);
            for (int i = 0; i < 4; i++)
                buffr[i] = buff[3 - i];
            return BitConverter.ToSingle(buffr, 0);
        }
        public static void WriteFloat(Stream s, float f)
        {
            byte[] buff = BitConverter.GetBytes(f);
            byte[] buffr = new byte[4];
            s.Read(buff, 0, 4);
            for (int i = 0; i < 4; i++)
                buffr[i] = buff[3 - i];
            s.Write(buffr, 0, 4);
        }
        public static string TagToLabel(uint Tag)
        {
            string s = "";
            List<byte> buff = new List<byte>(BitConverter.GetBytes(Tag));
            buff.Reverse();
            byte[] res = new byte[4];
            res[0] |= (byte)((buff[0] & 0x80) >> 1);
            res[0] |= (byte)((buff[0] & 0x40) >> 2);
            res[0] |= (byte)((buff[0] & 0x30) >> 2);
            res[0] |= (byte)((buff[0] & 0x0C) >> 2);

            res[1] |= (byte)((buff[0] & 0x02) << 5);
            res[1] |= (byte)((buff[0] & 0x01) << 4);
            res[1] |= (byte)((buff[1] & 0xF0) >> 4);

            res[2] |= (byte)((buff[1] & 0x08) << 3);
            res[2] |= (byte)((buff[1] & 0x04) << 2);
            res[2] |= (byte)((buff[1] & 0x03) << 2);
            res[2] |= (byte)((buff[2] & 0xC0) >> 6);

            res[3] |= (byte)((buff[2] & 0x20) << 1);
            res[3] |= (byte)((buff[2] & 0x1F));

            for (int i = 0; i < 4; i++)
            {
                if (res[i] == 0)
                    res[i] = 0x20;
                s += (char)res[i];
            }
            return s;
        }
        public static byte[] Label2Tag(string Label)
        {
            byte[] res = new byte[3];

            if (Label == "?CON")
            {
                res[0] = 0x46;
                res[1] = 0x3B;
                res[2] = 0xEE;
                return res;
            }

            while (Label.Length < 4)
                Label += '\0';
            if (Label.Length > 4)
                Label = Label.Substring(0, 4);
            byte[] buff = new byte[4];
            for (int i = 0; i < 4; i++)
                buff[i] = (byte)Label[i];
            res[0] |= (byte)((buff[0] & 0x40) << 1);
            res[0] |= (byte)((buff[0] & 0x10) << 2);
            res[0] |= (byte)((buff[0] & 0x0F) << 2);
            res[0] |= (byte)((buff[1] & 0x40) >> 5);
            res[0] |= (byte)((buff[1] & 0x10) >> 4);

            res[1] |= (byte)((buff[1] & 0x0F) << 4);
            res[1] |= (byte)((buff[2] & 0x40) >> 3);
            res[1] |= (byte)((buff[2] & 0x10) >> 2);
            res[1] |= (byte)((buff[2] & 0x0C) >> 2);

            res[2] |= (byte)((buff[2] & 0x03) << 6);
            res[2] |= (byte)((buff[3] & 0x40) >> 1);
            res[2] |= (byte)((buff[3] & 0x1F));
            return res;
        }
        public static long DecompressInteger(Stream s) ////////????????????????????????????????
        {
            List<byte> tmp = new List<byte>();
            byte b;
            while ((b = (byte)s.ReadByte()) >= 0x80)
                tmp.Add(b);
            tmp.Add(b);
            byte[] buff = tmp.ToArray();
            int currshift = 6;
            ulong result = (ulong)(buff[0] & 0x3F);
            for (int i = 1; i < buff.Length; i++)
            {
                byte curbyte = buff[i];
                ulong l = (ulong)(curbyte & 0x7F) << currshift;
                result |= l;
                currshift += 7;
            }
            return (long)result;
        }

        public static void CompressInteger(long l, Stream s)
        {
            //Console.WriteLine(l.ToString());
            if (l < 0x40)
            {
                s.WriteByte((byte)(l & 0xFF));
            }
            else
            {
                byte curbyte = (byte)((l & 0x3F) | 0x80);
                s.WriteByte(curbyte);
                long currshift = l >> 6;
                while (currshift >= 0x80)
                {
                    curbyte = (byte)((currshift & 0x7F) | 0x80);
                    currshift >>= 7;
                    s.WriteByte(curbyte);
                }
                s.WriteByte((byte)currshift);
            }
        }
        public static void CompressInteger(long l, Stream s, bool shit)
        {
            //Console.WriteLine(l.ToString());
            List<byte> result = new List<byte>();
            if (l < 0x40)
            {
                result.Add((byte)(l & 0xFF));
            }
            else
            {
                byte curbyte = (byte)((l & 0x3F) | 0x80);
                result.Add(curbyte);
                long currshift = l >> 6;
                while (currshift >= 0x80)
                {
                    curbyte = (byte)((currshift & 0x7F) | 0x80);
                    currshift >>= 7;
                    result.Add(curbyte);
                }
                result.Add((byte)currshift);
            }

            for (int i = 0; i < result.Count; i++)
            {
                if ((i == 0) && (shit == true))
                    s.WriteByte((byte)66);
                else
                    s.WriteByte(result[i]);

            }
        }
        public static string ReadString(Stream s)
        {
            int len = (int)DecompressInteger(s);
            string res = "";
            for (int i = 0; i < len - 1; i++)
                res += (char)s.ReadByte();
            s.ReadByte();
            return res;
        }
        public static void WriteString(string value, Stream s)
        {
            CompressInteger((long)value.Length + 1, s); // +1 0xA

            foreach (char c in value)
                s.WriteByte((byte)c);

            s.WriteByte(0);
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        public static uint GetUnixTimeStamp()
        {
            return (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;
            char[] HexChars = "0123456789ABCDEF".ToCharArray();
            int firstHexColumn = 11;
            int firstCharColumn = firstHexColumn + bytesPerLine * 3 + (bytesPerLine - 1) / 8 + 2;
            int lineLength = firstCharColumn + bytesPerLine + Environment.NewLine.Length;
            char[] line = (new String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);
            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];
                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;
                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
        public static Blaze.TdfStruct CreateStructStub(List<Tdf> tdfs, bool has2 = false)
        {
            Blaze.TdfStruct res = new TdfStruct();
            res.Values = tdfs;
            res.startswith2 = has2;
            return res;
        }
        #endregion

        #region Reading
        public static Tdf ReadTdf(Stream s)
        {

            Tdf res = new Tdf();
            uint Head = (uint)ReadUint(s);
            res.Tag = (Head & 0xFFFFFF00);
            res.Label = TagToLabel(res.Tag);
            res.Type = (byte)(Head & 0xFF);

            //Logger.Log("Label: " + res.Label);
            //Logger.Log("Type: " + res.Type.ToString());

            switch (res.Type)
            {
                case 2:
                    return ReadTdfBlob(res, s);
                case 0:
                case 6:
                    return ReadTdfInteger(res, s);
                case 1:
                    return ReadTdfString(res, s);
                case 3:
                    return ReadTdfStruct(res, s);
                case 4:
                    return ReadTdfList(res, s);
                case 5:
                    return ReadTdfDoubleList(res, s);
                case 7:
                    return ReadTdfIntegerList(res, s);
                case 8:
                    return ReadTdfDoubleVal(res, s);
                case 9:
                    return ReadTdfTrippleVal(res, s);
                case 0xA:
                    return ReadTdfFloat(res, s);
                default:
                    throw new Exception("Unknown Tdf Type: " + res.Type);
            }
        }
        public static TdfFloat ReadTdfFloat(Tdf head, Stream s)
        {
            TdfFloat res = new TdfFloat();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            res.Value = BitConverter.ToSingle(buff, 0);
            return res;
        }
        public static TdfBlob ReadTdfBlob(Tdf head, Stream s)
        {
            TdfBlob res = new TdfBlob();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.Length = (long)DecompressInteger(s);
            res.data = new byte[res.Length];
            for (int i = 0; i < res.Length; i++)
                res.data[i] = (byte)s.ReadByte();
            return res;
        }
        public static TdfInteger ReadTdfInteger(Tdf head, Stream s)
        {
            TdfInteger res = new TdfInteger();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.Value = (long)DecompressInteger(s);
            return res;
        }
        public static TdfString ReadTdfString(Tdf head, Stream s)
        {
            TdfString res = new TdfString();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.Value = ReadString(s);
            return res;
        }

        public static TdfStruct ReadTdfStruct(Tdf head, Stream s)
        {
            TdfStruct res = new TdfStruct();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            bool has2 = false;
            res.Values = ReadStruct(s, out has2, res.Label);
            res.startswith2 = has2;
            return res;
        }
        public static TdfTrippleVal ReadTdfTrippleVal(Tdf head, Stream s)
        {
            TdfTrippleVal res = new TdfTrippleVal();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.Value = ReadTrippleVal(s);
            return res;
        }
        public static TdfDoubleVal ReadTdfDoubleVal(Tdf head, Stream s)
        {
            TdfDoubleVal res = new TdfDoubleVal();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.Value = ReadDoubleVal(s);
            return res;
        }
        public static TdfList ReadTdfList(Tdf head, Stream s)
        {
            TdfList res = new TdfList();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.SubType = (byte)s.ReadByte();
            res.Count = (int)DecompressInteger(s);
            for (int i = 0; i < res.Count; i++)
            {
                switch (res.SubType)
                {
                    case 0:
                        if (res.List == null)
                            res.List = new List<long>();
                        List<long> l1 = (List<long>)res.List;
                        l1.Add(DecompressInteger(s));
                        res.List = l1;
                        break;
                    case 1:
                        if (res.List == null)
                            res.List = new List<string>();
                        List<string> l2 = (List<string>)res.List;
                        l2.Add(ReadString(s));
                        res.List = l2;
                        break;
                    case 3:
                        if (res.List == null)
                            res.List = new List<TdfStruct>();
                        List<TdfStruct> l3 = (List<TdfStruct>)res.List;
                        Blaze.TdfStruct tmp = new TdfStruct();
                        tmp.startswith2 = false;
                        tmp.Values = ReadStruct(s, out tmp.startswith2, res.Label);
                        l3.Add(tmp);
                        res.List = l3;
                        break;
                    case 9:
                        if (res.List == null)
                            res.List = new List<TrippleVal>();
                        List<TrippleVal> l4 = (List<TrippleVal>)res.List;
                        l4.Add(ReadTrippleVal(s));
                        res.List = l4;
                        break;
                    default:
                        throw new Exception("Unknown Tdf Type in List: " + res.Type);
                }
            }
            return res;
        }
        public static TdfIntegerList ReadTdfIntegerList(Tdf head, Stream s)
        {
            TdfIntegerList res = new TdfIntegerList();
            res.Label = head.Label;
            res.Tag = head.Tag;
            res.Type = head.Type;
            res.Count = (int)DecompressInteger(s);
            for (int i = 0; i < res.Count; i++)
            {
                if (res.List == null)
                    res.List = new List<long>();
                List<long> l1 = (List<long>)res.List;
                l1.Add(DecompressInteger(s));
                res.List = l1;
            }
            return res;
        }
        public static TdfDoubleList ReadTdfDoubleList(Tdf head, Stream s)
        {
            TdfDoubleList res = new TdfDoubleList();
            res.Label = head.Label;
            //Logger.Log("TdfDoubleList::Tdf Label:" + res.Label, 1);

            res.Tag = head.Tag;
            res.Type = head.Type;
            res.SubType1 = (byte)s.ReadByte();
            //Logger.Log("TdfDoubleList::SubType1:" + res.SubType1, 1);

            res.SubType2 = (byte)s.ReadByte();
            //Logger.Log("TdfDoubleList::SubType2:" + res.SubType2, 1);

            res.Count = (int)DecompressInteger(s);
            for (int i = 0; i < res.Count; i++)
            {
                switch (res.SubType1)
                {
                    case 0:
                        if (res.List1 == null)
                            res.List1 = new List<long>();
                        List<long> l1 = (List<long>)res.List1;
                        l1.Add(DecompressInteger(s));
                        res.List1 = l1;
                        break;
                    case 1:
                        if (res.List1 == null)
                            res.List1 = new List<string>();
                        List<string> l2 = (List<string>)res.List1;
                        l2.Add(ReadString(s));
                        res.List1 = l2;
                        break;
                    case 3:
                        if (res.List1 == null)
                            res.List1 = new List<TdfStruct>();
                        List<TdfStruct> l3 = (List<TdfStruct>)res.List1;
                        Blaze.TdfStruct tmp = new TdfStruct();
                        tmp.startswith2 = false;
                        tmp.Values = ReadStruct(s, out tmp.startswith2, res.Label);
                        l3.Add(tmp);
                        res.List1 = l3;
                        break;
                    case 0xA:
                        if (res.List1 == null)
                            res.List1 = new List<float>();
                        List<float> lf3 = (List<float>)res.List1;
                        lf3.Add(ReadFloat(s));
                        res.List1 = lf3;
                        break;
                    default:
                        throw new Exception("Unknown Tdf Type in Double List: " + res.SubType1);
                }
                switch (res.SubType2)
                {
                    case 0:
                        if (res.List2 == null)
                            res.List2 = new List<long>();
                        List<long> l1 = (List<long>)res.List2;
                        l1.Add(DecompressInteger(s));
                        res.List2 = l1;
                        break;
                    case 1:
                        if (res.List2 == null)
                            res.List2 = new List<string>();
                        List<string> l2 = (List<string>)res.List2;
                        l2.Add(ReadString(s));
                        res.List2 = l2;
                        break;
                    case 3:
                        if (res.List2 == null)
                            res.List2 = new List<TdfStruct>();
                        List<TdfStruct> l3 = (List<TdfStruct>)res.List2;
                        Blaze.TdfStruct tmp = new TdfStruct();
                        tmp.startswith2 = false;
                        tmp.Values = ReadStruct(s, out tmp.startswith2, res.Label);

                        //Logger.Log("tmp.Values.Count = " + tmp.Values.Count.ToString());

                        l3.Add(tmp);
                        res.List2 = l3;



                        break;
                    case 0xA:
                        if (res.List2 == null)
                            res.List2 = new List<float>();
                        List<float> lf3 = (List<float>)res.List2;
                        lf3.Add(ReadFloat(s));
                        res.List2 = lf3;
                        break;
                    default:
                        throw new Exception("Unknown Tdf Type in Double List: " + res.SubType2);
                }
            }
            return res;
        }
        public static List<Tdf> ReadStruct(Stream s, out bool has2, string Label)
        {
            //Logger.Log("ReadStruct::Tdf Label:" + Label, 1);
            //Logger.Log("1");
            List<Tdf> res = new List<Tdf>();
            byte b = 0;
            bool reshas2 = false;

            b = (byte)s.ReadByte();
            //Logger.Log("2");
            if ((b == 0) && (Label == "RRST"))
            {
                //Logger.Log("3_1");
                s.Seek(6, SeekOrigin.Current);

                b = (byte)s.ReadByte();
                if (b == 0xCE)
                    s.Seek(-1, SeekOrigin.Current);

                b = (byte)s.ReadByte();
                if (b == 0xCE)
                    s.Seek(-1, SeekOrigin.Current);

                b = (byte)s.ReadByte();
                if (b == 0xCE)
                    s.Seek(-1, SeekOrigin.Current);

                b = (byte)s.ReadByte();
                if (b == 0xCE)
                    s.Seek(-1, SeekOrigin.Current);

            }
            else
            {
                //Logger.Log("3_2");
                s.Seek(-1, SeekOrigin.Current);
            }

            while ((b = (byte)s.ReadByte()) != 0)
            {

                if (b != 2)
                {
                    //Logger.Log("4_1");
                    s.Seek(-1, SeekOrigin.Current);
                }
                else
                {
                    //Logger.Log("4_2");
                    reshas2 = true;
                }



                Tdf var = ReadTdf(s);
                //Logger.Log("5");

                if (var != null)
                {
                    //Logger.Log("6");
                    res.Add(var);
                    //Logger.Log("7");
                    //Logger.Log("res.Add(var): " + var.Label);
                    //Logger.Log("8");

                    if ((Label == "RRST") && var.Label == "XSES")
                    {
                        //Logger.Log("9");
                        //Logger.Log("((Label == RRST) && var.Label == XSES)");
                        break;
                    }

                }

                /*res.Add(ReadTdf(s));*/
            }
            has2 = reshas2;
            //Logger.Log("10");
            return res;
        }
        public static List<Tdf> ReadPacketContent(Packet p)
        {
            List<Tdf> res = new List<Tdf>();
            MemoryStream m = (new MemoryStream(p.Content));
            m.Seek(0, 0);
            try
            {
                while (m.Position < m.Length - 4)
                    res.Add(ReadTdf(m));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n@:" + m.Position.ToString("X"));
            }
            return res;
        }
        public static DoubleVal ReadDoubleVal(Stream s)
        {
            DoubleVal res = new DoubleVal();
            res.v1 = DecompressInteger(s);
            res.v2 = DecompressInteger(s);
            return res;
        }
        public static TrippleVal ReadTrippleVal(Stream s)
        {
            TrippleVal res = new TrippleVal();
            res.v1 = DecompressInteger(s);
            res.v2 = DecompressInteger(s);
            res.v3 = DecompressInteger(s);
            return res;
        }
        #endregion

        #region Writing
        public static void WriteTdf(Tdf tdf, Stream s)
        {
            if (tdf.Label == "?CON")
            {
                s.WriteByte(0x46);
                s.WriteByte(0x3B);
                s.WriteByte(0xEE);
            }
            else
            {
                s.WriteByte((byte)(tdf.Tag >> 24));
                s.WriteByte((byte)(tdf.Tag >> 16));
                s.WriteByte((byte)(tdf.Tag >> 8));
            }

            s.WriteByte(tdf.Type);
            switch (tdf.Type)
            {
                case 2:
                    TdfBlob tb = (TdfBlob)tdf;
                    CompressInteger(tb.Length, s);
                    s.Write(tb.data, 0, (int)tb.Length);
                    break;
                case 0:
                    TdfInteger ti = (TdfInteger)tdf;
                    CompressInteger(ti.Value, s);
                    break;
                case 6:
                    TdfInteger tu = (TdfInteger)tdf;
                    s.WriteByte((byte)tu.Value);
                    break;
                case 1:
                    TdfString ts = (TdfString)tdf;
                    WriteString(ts.Value, s);
                    break;
                case 3:
                    TdfStruct tst = (TdfStruct)tdf;
                    if (tst.startswith2)
                        s.WriteByte(2);
                    foreach (Tdf ttdf in tst.Values)
                        WriteTdf(ttdf, s);

                    if (tst.endswith0)
                        s.WriteByte(0);

                    s.WriteByte(0);
                    break;
                case 4:
                    WriteTdfList((TdfList)tdf, s);
                    break;
                case 5:
                    WriteTdfDoubleList((TdfDoubleList)tdf, s);
                    break;
                case 7:
                    TdfIntegerList til = (TdfIntegerList)tdf;
                    CompressInteger(til.Count, s);
                    if (til.Count != 0)
                        foreach (long l in til.List)
                            CompressInteger(l, s);
                    break;
                case 8:
                    WriteDoubleValue(((TdfDoubleVal)tdf).Value, s);
                    break;
                case 9:
                    WriteTrippleValue(((TdfTrippleVal)tdf).Value, s);
                    break;
                case 0xA:
                    TdfFloat tf = (TdfFloat)tdf;
                    WriteFloat(s, tf.Value);
                    break;
            }
        }
        public static void WriteTdfList(TdfList tdf, Stream s)
        {
            s.WriteByte(tdf.SubType);
            CompressInteger(tdf.Count, s);
            for (int i = 0; i < tdf.Count; i++)
                switch (tdf.SubType)
                {
                    case 0:
                        CompressInteger(((List<long>)tdf.List)[i], s);
                        break;
                    case 1:
                        WriteString(((List<string>)tdf.List)[i], s);
                        break;
                    case 3:
                        Blaze.TdfStruct str = ((List<Blaze.TdfStruct>)tdf.List)[i];
                        if (str.startswith2)
                            s.WriteByte(2);
                        foreach (Tdf ttdf in str.Values)
                            WriteTdf(ttdf, s);
                        s.WriteByte(0);
                        break;
                    case 9:
                        WriteTrippleValue(((List<TrippleVal>)tdf.List)[i], s);
                        break;
                }
        }
        public static void WriteTdfDoubleList(TdfDoubleList tdf, Stream s)
        {
            s.WriteByte(tdf.SubType1);
            s.WriteByte(tdf.SubType2);
            CompressInteger(tdf.Count, s);
            for (int i = 0; i < tdf.Count; i++)
            {
                switch (tdf.SubType1)
                {
                    case 0:
                        CompressInteger(((List<long>)(tdf.List1))[i], s);
                        break;
                    case 1:
                        WriteString(((List<string>)(tdf.List1))[i], s);
                        break;
                    case 3:
                        Blaze.TdfStruct str = ((List<Blaze.TdfStruct>)tdf.List1)[i];
                        if (str.startswith2)
                            s.WriteByte(2);
                        foreach (Tdf ttdf in str.Values)
                            WriteTdf(ttdf, s);
                        s.WriteByte(0);
                        break;
                    case 9:
                        WriteTrippleValue(((List<TrippleVal>)tdf.List1)[i], s);
                        break;
                    case 0xA:
                        WriteFloat(s, ((List<float>)(tdf.List1))[i]);
                        break;
                }
                switch (tdf.SubType2)
                {
                    case 0:
                        if (tdf.shit == true)
                            CompressInteger(((List<long>)(tdf.List2))[i], s, true);
                        else
                            CompressInteger(((List<long>)(tdf.List2))[i], s);
                        break;
                    case 1:
                        WriteString(((List<string>)(tdf.List2))[i], s);
                        break;
                    case 3:
                        Blaze.TdfStruct str = ((List<Blaze.TdfStruct>)tdf.List2)[i];
                        if (str.startswith2)
                            s.WriteByte(2);
                        foreach (Tdf ttdf in str.Values)
                            WriteTdf(ttdf, s);
                        s.WriteByte(0);
                        break;
                    case 9:
                        WriteTrippleValue(((List<TrippleVal>)tdf.List2)[i], s);
                        break;
                    case 0xA:
                        WriteFloat(s, ((List<float>)(tdf.List2))[i]);
                        break;
                }
            }
        }
        public static void WriteTrippleValue(TrippleVal v, Stream s)
        {
            CompressInteger(v.v1, s);
            CompressInteger(v.v2, s);
            CompressInteger(v.v3, s);
        }
        public static void WriteDoubleValue(DoubleVal v, Stream s)
        {
            CompressInteger(v.v1, s);
            CompressInteger(v.v2, s);
        }
        #endregion

        #region Logger
        public static void Log(string msg)
        {
            //string s = ", " + "\"" + msg + "\"";
            if (!File.Exists("PacketViewer.txt"))
                File.WriteAllBytes("PacketViewer.txt", new byte[0]);
            File.AppendAllText("PacketViewer.txt", msg);
        }
        #endregion



        #region Describers
        public static string PacketToDescriber(Blaze.Packet p)
        {
            string t = p.Command.ToString("X");
            string t2 = p.Component.ToString("X");
            string[] lines = ComponentNames.Split(',');
            string cname = "";
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && parts[1] == t2)
                {
                    cname = parts[0];
                    break;
                }
            }
            switch ((int)p.Component)
            {
                case 0xB:
                    for (int i = 0; i < DescComponentB.Length / 2; i++)
                        if (DescComponentB[i * 2] == t)
                            return cname + " : " + DescComponentB[i * 2 + 1];
                    break;
                case 0x1:
                    for (int i = 0; i < DescComponent1.Length / 2; i++)
                        if (DescComponent1[i * 2] == t)
                            return cname + " : " + DescComponent1[i * 2 + 1];
                    break;
                case 0x4:
                    for (int i = 0; i < DescComponent4.Length / 2; i++)
                        if (DescComponent4[i * 2] == t)
                            return cname + " : " + DescComponent4[i * 2 + 1];
                    break;
                case 0x7:
                    for (int i = 0; i < DescComponent7.Length / 2; i++)
                        if (DescComponent7[i * 2] == t)
                            return cname + " : " + DescComponent7[i * 2 + 1];
                    break;
                case 0x9:
                    for (int i = 0; i < DescComponent9.Length / 2; i++)
                        if (DescComponent9[i * 2] == t)
                            return cname + " : " + DescComponent9[i * 2 + 1];
                    break;
                case 0xF:
                    for (int i = 0; i < DescComponentF.Length / 2; i++)
                        if (DescComponentF[i * 2] == t)
                            return cname + " : " + DescComponentF[i * 2 + 1];
                    break;
                case 0x19:
                    for (int i = 0; i < DescComponent19.Length / 2; i++)
                        if (DescComponent19[i * 2] == t)
                            return cname + " : " + DescComponent19[i * 2 + 1];
                    break;
                case 0x1C:
                    for (int i = 0; i < DescComponent1C.Length / 2; i++)
                        if (DescComponent1C[i * 2] == t)
                            return cname + " : " + DescComponent1C[i * 2 + 1];
                    break;
                case 0x7802:
                    for (int i = 0; i < DescComponent7802.Length / 2; i++)
                        if (DescComponent7802[i * 2] == t)
                            return cname + " : " + DescComponent7802[i * 2 + 1];
                    break;
                case 0x0802:
                    for (int i = 0; i < DescComponent0802.Length / 2; i++)
                        if (DescComponent0802[i * 2] == t)
                            return cname + " : " + DescComponent0802[i * 2 + 1];
                    break;




            }
            return cname + " : " + p.Command.ToString("X");
        }
        public static string ComponentNames = "Authentication Component=1,Example Component=3,Game Manager Component=4,Redirector Component=5,Play Groups Component=6,Stats Component=7,Util Component=9,Census Data Component=A,Clubs Component=B,Game Report Lagacy Component=C,League Component=D,Mail Component=E,Messaging Component=F,Locker Component=14,Rooms Component=15,Tournaments Component=17,Commerce Info Component=18,Association Lists Component=19,GPS Content Controller Component=1B,Game Reporting Component=1C,Dynamic Filter Component=7D0,RSP Component=801,XPMultiplierComponent=802, UserSessions=7802";
        public static string[] DescComponentB = { "044C","createClub",
      "4B0","getClubs",
      "514","findClubs",
      "51E","findClubs2",
      "578","removeMember",
      "5DC","sendInvitation",
      "640","getInvitations",
      "6A4","revokeInvitation",
      "708","acceptInvitation",
      "76C","declineInvitation",
      "7D0","getMembers",
      "834","promoteToGM",
      "866","demoteToMember",
      "898","updateClubSettings",
      "8FC","postNews",
      "960","getNews",
      "992","setNewsItemHidden",
      "9C4","setMetadata",
      "9CE","setMetadata2",
      "A28","getClubsComponentSettings",
      "A5A","transferOwnership",
      "A8C","getClubMembershipForUsers",
      "AF0","sendPetition",
      "B54","getPetitions",
      "BB8","acceptPetition",
      "C1C","declinePetition",
      "C80","revokePetition",
      "CE4","joinClub",
      "CEE","joinOrPetitionClub",
      "D48","getClubRecordbook",
      "D52","resetClubRecords",
      "DAC","updateMemberOnlineStatus",
      "E10","getClubAwards",
      "E74","updateMemberMetadata",
      "ED8","findClubsAsync",
      "EE2","findClubs2Async",
      "F3C","listRivals",
      "FA0","getClubTickerMessages",
      "1004","setClubTickerMessagesSubscription",
      "1068","changeClubStrings",
      "10CC","countMessages",
      "1130","getMembersAsync",
      "1194","getClubBans",
      "11F8","getUserBans",
      "125C","banMember",
      "12C0","unbanMember",
      "1324","GetClubsComponentInfo",
      "1388","disbandClub",
      "13EC","getNewsForClubs",
      "1450","getPetitionsForClubs",
      "14B4","getClubTickerMessagesForClubs",
      "1518","countMessagesForClubs",
      "157C","getMemberOnlineStatus",
      "15E0","getMemberStatusInClub"};
        public static string[] DescComponent1 = { "A", "createAccount",
                                                  "14", "updateAccount",
                                                  "1C", "updateParentalEmail",
                                                  "1D", "listUserEntitlements2",
                                                  "1E", "getAccount",
                                                  "1F", "grantEntitlement",
                                                  "20", "listEntitlements",
                                                  "21", "hasEntitlement",
                                                  "22", "getUseCount",
                                                  "23", "decrementUseCount",
                                                  "24", "getAuthToken",
                                                  "25", "getHandoffToken",
                                                  "26", "getPasswordRules",
                                                  "27", "grantEntitlement2",
                                                  "28", "login",
                                                  "29", "acceptTos",
                                                  "2A", "getTosInfo",
                                                  "2B", "modifyEntitlement2",
                                                  "2C", "consumecode",
                                                  "2D", "passwordForgot",
                                                  "2E", "getTermsAndConditionsContent",
                                                  "2F", "getPrivacyPolicyContent",
                                                  "30", "listPersonaEntitlements2",
                                                  "32", "silentLogin",
                                                  "33", "checkAgeReq",
                                                  "34", "getOptIn",
                                                  "35", "enableOptIn",
                                                  "36", "disableOptIn",
                                                  "3C", "expressLogin",
                                                  "46", "logout",
                                                  "50", "createPersona",
                                                  "5A", "getPersona",
                                                  "64", "listPersonas",
                                                  "6E", "loginPersona",
                                                  "78", "logoutPersona",
                                                  "8C", "deletePersona",
                                                  "8D", "disablePersona",
                                                  "8F", "listDeviceAccounts",
                                                  "96", "xboxCreateAccount",
                                                  "98", "originLogin",
                                                  "A0", "xboxAssociateAccount",
                                                  "AA", "xboxLogin",
                                                  "B4", "ps3CreateAccount",
                                                  "BE", "ps3AssociateAccount",
                                                  "C8", "ps3Login",
                                                  "D2", "validateSessionKey",
                                                  "E6", "createWalUserSession",
                                                  "F1", "acceptLegalDocs",
                                                  "F2", "getLegalDocsInfo",
                                                  "F6", "getTermsOfServiceContent",
                                                  "12C", "deviceLoginGuest" };
        public static string[] DescComponent4 = { "1", "createGame",
                                                  "2", "destroyGame",
                                                  "3", "advanceGameState",
                                                  "4", "setGameSettings",
                                                  "5", "setPlayerCapacity",
                                                  "6", "setPresenceMode",
                                                  "7", "setGameAttributes",
                                                  "8", "setPlayerAttributes",
                                                  "9", "joinGame",
                                                  "B", "removePlayer",
                                                  "D", "startMatchmaking",
                                                  "E", "cancelMatchmaking",
                                                  "F", "finalizeGameCreation",
                                                  "11", "listGames",
                                                  "12", "setPlayerCustomData",
                                                  "13", "replayGame",
                                                  "14", "NotifyGameSetup",
                                                  "15", "NotifyPlayerJoining",
                                                  "16", "leaveGameByGroup",
                                                  "17", "migrateGame",
                                                  "18", "updateGameHostMigrationStatus",
                                                  "19", "resetDedicatedServer",
                                                  "1A", "updateGameSession",
                                                  "1B", "banPlayer",
                                                  "1D", "updateMeshConnection",
                                                  "1F", "removePlayerFromBannedList",
                                                  "20", "clearBannedList",
                                                  "21", "getBannedList",
                                                  "26", "addQueuedPlayerToGame",
                                                  "27", "updateGameName",
                                                  "28", "ejectHost",
                                                  "29", "setGameModRegister",
                                                  "47", "NotifyPlatformHostInitialized",
                                                  "50", "NotifyGameAttribChange",
                                                  "64", "NotifyGameStateChange",
                                                  "65", "getGameListSubscription",
                                                  "66", "destroyGameList",
                                                  "67", "getFullGameData",
                                                  "68", "getMatchmakingConfig",
                                                  "69", "getGameDataFromId",
                                                  "6A", "addAdminPlayer",
                                                  "6B", "removeAdminPlayer",
                                                  "6C", "setPlayerTeam",
                                                  "6D", "changeGameTeamId",
                                                  "6E", "migrateAdminPlayer",
                                                  "6F", "NotifyGameCapacityChange",
                                                  "70", "swapPlayersTeam",
                                                  "96", "registerDynamicDedicatedServerCreator",
                                                  "97", "unregisterDynamicDedicatedServerCreator" };
        public static string[] DescComponent5 = { "1", "getServerInstance" };
        public static string[] DescComponent7 = { "1", "getStatDescs",
                                                  "2", "getStats",
                                                  "3", "getStatGroupList",
                                                  "4", "getStatGroup",
                                                  "5", "getStatsByGroup",
                                                  "6", "getDateRange",
                                                  "7", "getEntityCount",
                                                  "A", "getLeaderboardGroup",
                                                  "B", "getLeaderboardFolderGroup",
                                                  "C", "getLeaderboard",
                                                  "D", "getCenteredLeaderboard",
                                                  "E", "getFilteredLeaderboard",
                                                  "F", "getKeyScopesMap",
                                                  "10", "getStatsByGroupAsync",
                                                  "11", "getLeaderboardTreeAsync",
                                                  "12", "getLeaderboardEntityCount",
                                                  "13", "getStatCategoryList",
                                                  "14", "getPeriodIds",
                                                  "15", "getLeaderboardRaw",
                                                  "16", "getCenteredLeaderboardRaw",
                                                  "17", "getFilteredLeaderboardRaw",
                                                  "18", "changeKeyscopeValue" };
        public static string[] DescComponent9 = { "1", "fetchClientConfig",
                                                  "2", "ping",
                                                  "3", "setClientData",
                                                  "4", "localizeStrings",
                                                  "5", "getTelemetryServer",
                                                  "6", "getTickerServer",
                                                  "7", "preAuth",
                                                  "8", "postAuth",
                                                  "A", "userSettingsLoad",
                                                  "B", "userSettingsSave",
                                                  "C", "userSettingsLoadAll",
                                                  "E", "deleteUserSettings",
                                                  "14", "filterForProfanity",
                                                  "15", "fetchQosConfig",
                                                  "16", "setClientMetrics",
                                                  "17", "setConnectionState",
                                                  "18", "getPssConfig",
                                                  "19", "getUserOptions",
                                                  "1A", "setUserOptions",
                                                  "1B", "suspendUserPing" };
        public static string[] DescComponentF = { "1", "sendMessage",
                                                  "2", "fetchMessages",
                                                  "3", "purgeMessages",
                                                  "4", "touchMessages",
                                                  "5", "getMessages" };
        public static string[] DescComponent19 = { "1", "addUsersToList",
                                                   "2", "removeUsersFromList",
                                                   "3", "clearLists",
                                                   "4", "setUsersToList",
                                                   "5", "getListForUser",
                                                   "6", "getLists",
                                                   "7", "subscribeToLists",
                                                   "8", "unsubscribeFromLists",
                                                   "9", "getConfigListsInfo" };
        public static string[] DescComponent1C = { "1", "submitGameReport",
                                                   "2", "submitOfflineGameReport",
                                                   "3", "submitGameEvents",
                                                   "4", "getGameReportQuery",
                                                   "5", "getGameReportQueriesList",
                                                   "6", "getGameReports",
                                                   "7", "getGameReportView",
                                                   "8", "getGameReportViewInfo",
                                                   "9", "getGameReportViewInfoList",
                                                   "A", "getGameReportTypes",
                                                   "B", "updateMetric",
                                                   "C", "getGameReportColumnInfo",
                                                   "D", "getGameReportColumnValues",
                                                   "64", "submitTrustedMidGameReport",
                                                   "65", "submitTrustedEndGameReport" };
        public static string[] DescComponent23 = { "A", "getUserSessionFromAuth" };
        public static string[] DescComponent803 = { "5", "drainConsumeable",
                                                    "6", "getTemplate"};
        public static string[] DescComponent7802 = { "1", "UserSessionExtendedDataUpdate",
                                                     "2", "UserAdded",
                                                     "3", "fetchExtendedData",
                                                     "5", "UserUpdated",
                                                     "8", "UpdateHardwareFlags",
                                                     "C", "lookupUser",
                                                     "D", "lookupUsers",
                                                     "E", "lookupUsersByPrefix",
                                                     "14", "updateNetworkInfo",
                                                     "17", "lookupUserGeoIPData",
                                                     "18", "overrideUserGeoIPData",
                                                     "19", "updateUserSessionClientData",
                                                     "1A", "setUserInfoAttribute",
                                                     "1B", "resetUserGeoIPData",
                                                     "20", "lookupUserSessionId",
                                                     "21", "fetchLastLocaleUsedAndAuthError",
                                                     "22", "fetchUserFirstLastAuthTime",
                                                     "23", "resumeSession" };
        public static string[] DescComponent0802 = { "1", "getMultiplierListRequest" };
        #endregion
    }
}