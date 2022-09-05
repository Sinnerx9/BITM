using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using static BITM.Utility.Logger;
namespace BITM.Network
{
    public class Listener
    {
        private TcpListener _TcpListener { get; set; }
        public Listener(int port) => this._TcpListener = new TcpListener(IPAddress.Any, port);

        public void Start()
        {
            this._TcpListener.Start();
            while (true)
            {
                TcpClient client = this._TcpListener.AcceptTcpClient();
                Task.Run(() => ClientPool(client));
            }
        }
        private void ClientPool(TcpClient client)
        {
            Log($"({client.Client.RemoteEndPoint?.ToString()}) Client connected", LogLevel.Info);
            Stream stream = client.GetStream();
            #region Authenticate Connected Client
            //try
            //{
            //    ssl.AuthenticateAsServer(
            //        new X509Certificate2("", ""),
            //        false,
            //        false);
            //    Log($"({client.Client.RemoteEndPoint?.ToString()}) Authentication Succeeded", LogLevel.Info);
            //}
            //catch
            //{
            //    Log($"({client.Client.RemoteEndPoint?.ToString()}) Authentication Fail", LogLevel.Fatal);
            //    ssl.Dispose();
            //    client.Dispose();
            //    GC.Collect();
            //    return;
            //}
            #endregion
            Task.Run(() => new PacketHandler(stream, client));
        }
    }
}
