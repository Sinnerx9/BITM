using BITM.BlazeSDK;
using BITM.Utility;
using Newtonsoft.Json;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using static BITM.BlazeSDK.Blaze;
using static BITM.BlazeSDK.Enums;
using static BITM.Utility.Logger;
namespace BITM.Network
{
    public class PacketHandler
    {

        #region CLIENT VARIABLES
        private TcpClient c_Client { get; set; }
        private Stream c_Stream { get; set; }
        private IPEndPoint c_Endpoint { get; set; }
        #endregion
        #region SERVER VARIBALES
        private TcpClient r_Client { get; set; }
        //private Stream r_Stream { get; set; }
        private SslStream r_Stream { get; set; }
        private List<byte> lastPacket { get; set; } = new List<byte>();
        #endregion

        public PacketHandler(Stream _stream, TcpClient c_Client)
        {
            this.c_Client = c_Client;
            this.c_Endpoint = (IPEndPoint)c_Client.Client.RemoteEndPoint;
            this.c_Stream = _stream;
            this.r_Client = new TcpClient();
            #region Reverse Client Establish
            try
            {
                // this.r_Client.Connect("65.20.149.126", 42139);
                this.r_Client.Connect("gsprodblapp-04.ea.com", 10025);
                Log("Reverse End Point Established", LogLevel.Info);
            }
            catch
            {
                Log("Couldn't Establish Reverse End Point", LogLevel.Fatal);
                this.r_Client.Dispose();
                GC.Collect();
                return;
            }
            #endregion
            this.r_Stream = new SslStream(this.r_Client.GetStream(), true, aa);
            //this.r_Stream = new this.r_Client.GetStream();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            #region Reverse Stream Authenticate
            try
            {
                this.r_Stream.AuthenticateAsClient("gsprodblapp-04.ea.com", null, System.Security.Authentication.SslProtocols.Tls11, false);
                Log("Reverse End Point Authenticated", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log("Couldn't Authenticate Reverse End Point", LogLevel.Fatal);
                this.r_Client.Dispose();
                this.r_Stream.Dispose();
                GC.Collect();
                return;
            }
            #endregion
            Handle();
        }

        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            if (this.lastPacket.Count > 0)
            {
                var SRV_Packet = this.lastPacket.ToArray();
                try
                {

                    Packet[] sp = Blaze.FetchAllPacket(SRV_Packet.toMemoryStream());
                    if (sp.Length > 1)
                    {
                        string filename = "[";
                        foreach (Packet s in sp)
                        {
                            //string scmd = Blaze.getCommandString(s.Component, s.Command);
                            string scmd = s.Command.ToString();
                            packetSID = packetGID != 0 ? packetGID - 1 : 0;
                            Log($"Recived {SRV_Packet.Length} From Server [{s.Component.ToString()}::{scmd}] ", LogLevel.Info);
                        }
                        File.WriteAllBytes($@"Dumps\({packetSID}) SRV [MultiPacket].bin", SRV_Packet);

                    }
                    else if (sp.Length == 1)
                    {
                        //string scmd = Blaze.getCommandString(sp[0].Component, sp[0].Command);
                        packetSID = packetGID != 0 ? packetGID - 1 : 0;
                        Log($"Recived {SRV_Packet.Length} From Server [MultiPacket] ", LogLevel.Info);

                        File.WriteAllBytes($@"Dumps\({packetSID}) SRV MultiPacket.bin", SRV_Packet);
                    }
                    else
                    {
                        File.WriteAllBytes($"Dumps\\({packetSID}) SRV_[Unkown].bin", SRV_Packet);
                        Log($"Recived {SRV_Packet.Length} From Server", LogLevel.Warn);
                    }


                }
                catch
                {
                    File.WriteAllBytes($"Dumps\\({packetSID}) SRV_[Unkown].bin", SRV_Packet);
                    Log($"Recived {SRV_Packet.Length} From Server", LogLevel.Warn);
                }
                this.lastPacket.Clear();
            }
        }

        private bool aa(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        private int packetGID = 0;
        private int packetSID = 1;
        private void Handle()
        {
            while (this.c_Client.Connected && this.r_Client.Connected)
            {
                #region CLIENT DATA
                new Task(() =>
                {
                    var CLI_Packet = getContent(this.c_Stream);
                    if (CLI_Packet.Length > 0)
                    {
                        this.setContent(this.r_Stream, CLI_Packet);
                        try
                        {
                            if (this.lastPacket.Count > 0)
                            {
                                var SRV_Packet = this.lastPacket.ToArray();
                                try
                                {

                                    Packet[] sp = Blaze.FetchAllPacket(SRV_Packet.toMemoryStream());
                                    if (sp.Length > 1)
                                    {
                                        string filename = "[";
                                        foreach (Packet s in sp)
                                        {
                                            //string scmd = Blaze.getCommandString(s.Component, s.Command);
                                            string scmd =  s.Command.ToString();
                                            packetSID = packetGID != 0 ? packetGID - 1 : 0;
                                            Log($"Recived {SRV_Packet.Length} From Server [{s.Component.ToString()}::{scmd}] ", LogLevel.Info);
                                            filename += $"{s.Component.ToString()}-{scmd}, ";
                                        }
                                        File.WriteAllBytes($@"Dumps\({packetSID}) SRV {filename.Remove(filename.Length - 2)}].bin", SRV_Packet);

                                    }
                                    else if (sp.Length == 1)
                                    {
                                        //string scmd = Blaze.getCommandString(sp[0].Component, sp[0].Command);
                                        packetSID = packetGID != 0 ? packetGID - 1 : 0;
                                        Log($"Recived {SRV_Packet.Length} From Server [MultiPacket] ", LogLevel.Info);

                                        File.WriteAllBytes($@"Dumps\({packetSID}) SRV MultiPacket.bin", SRV_Packet);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Logic ERROR ");
                                        File.WriteAllBytes($"Dumps\\({packetSID}) SRV_[Unkown].bin", SRV_Packet);
                                        Log($"Recived {SRV_Packet.Length} From Server", LogLevel.Warn);
                                    }


                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine("GGG >> " + ex.Message.ToString());
                                    File.WriteAllBytes($"Dumps\\({packetSID}) SRV_[Unkown].bin", SRV_Packet);
                                    Log($"Recived {SRV_Packet.Length} From Server", LogLevel.Warn);
                                }
                                this.lastPacket.Clear();
                            }
                            Packet p = Blaze.ReadBlazePacket(CLI_Packet.toMemoryStream());
                            //string cmd = Blaze.getCommandString(p.Component, p.Command);
                            string cmd = p.Command.ToString();
                            File.WriteAllBytes($@"Dumps\({packetGID}) CLI {p.Component.ToString()}-{cmd}.bin", CLI_Packet);
                            packetGID++;
                            Log($"Recived {CLI_Packet.Length} From Client [{p.Component.ToString()}::{cmd}] ", LogLevel.Info);
                        }
                        catch
                        {
                            File.WriteAllBytes($"Dumps\\({packetGID}) CLI_[Unkown].bin", CLI_Packet);
                            //packetGID++;
                            Log($"Recived {CLI_Packet.Length} From Client", LogLevel.Warn);
                        }
                    }
                }).Start();
                #endregion
                #region SERVER DATA
                new Task(() =>
                {
                    var SRV_Packet = getContent(this.r_Stream);
                    if (SRV_Packet.Length > 0)
                    {

                        try
                        {
                            this.setContent(this.c_Stream, SRV_Packet);


                            this.lastPacket.AddRange(SRV_Packet);
                            //Packet p = Blaze.ReadBlazePacket(SRV_Packet.toMemoryStream());
                            //string cmd = Blaze.getCommandString(p.Component, p.Command);
                            //packetSID = packetGID != 0 ? packetGID - 1 : 0;
                            //Log($"Recived {SRV_Packet.Length} From Server [{p.Component.ToString()}::{cmd}] ", LogLevel.Info);

                            //File.WriteAllBytes($@"Dumps\({packetSID}) SRV {p.Component.ToString()}-{cmd}.bin", SRV_Packet);
                        }
                        catch
                        {
                            //File.WriteAllBytes($"Dumps\\({packetSID}) SRV_[Unkown].bin", SRV_Packet);
                            //Log($"Recived {SRV_Packet.Length} From Server", LogLevel.Info);
                            this.lastPacket.AddRange(SRV_Packet);
                            //packetSID++;
                        }
                    }
                }).Start();
                #endregion
            }

        }
        private void setContent(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        private byte[] getContent(Stream stream)
        {
            MemoryStream res = new MemoryStream();
            byte[] buff = new byte[0x10000];
            int bytesRead;
            stream.ReadTimeout = 20;
            try
            {
                while ((bytesRead = stream.Read(buff, 0, 0x10000)) > 0)
                    res.Write(buff, 0, bytesRead);
            }
            catch { }
            stream.Flush();
            return res.ToArray();
        }
    }
}
