using static BITM.Utility.Logger;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BITM.Network
{
    class HttpParser
    {
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Version { get; set; }
        public string Host { get; set; }
        public string Cookie { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string content { get; set; }

        public void Parse(string raw)
        {
            Headers = new Dictionary<string, string>();
            string[] lines = raw.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] magicHeader = lines[0].Split(' ');
            try
            {
                Method = magicHeader[0];
                Uri = magicHeader[1];
                Version = magicHeader[2];
            }
            catch
            {
                Console.WriteLine("[HttpParser] Invalid Http Request");
                return;
            }
            int i = 1;
            for (i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i].Trim()))
                    break;
                var head = lines[i].Split(':', 2);
                switch (head[0].ToLower().Trim())
                {
                    case "host":
                        Host = head[1].ToLower().Trim();
                        break;
                    case "cookie":
                        Cookie = head[1].Trim();
                        break;
                    default:
                        Headers.Add(head[0].Trim(), head[1].Trim());
                        break;
                }
            }
            while (i < lines.Length && Method == "POST")
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    content += lines[i] + Environment.NewLine;
                }
                i++;
            }
        }

    }
    class REDIHelpers
    {
        public static string ReadContent(Stream stream, int buff_len)
        {

            byte[] buffer = new byte[buff_len];
            int read = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, read);
        }
        public static byte[] FixSocketLeftovers(byte[] data)
        {
            MemoryStream res = new MemoryStream(data);
            bool flag = res.Length >= 5L;
            if (flag)
            {
                byte[] test = res.ToArray();
                bool flag2 = test[0] == 23 && test[1] == 3;
                if (flag2)
                {
                    res = new MemoryStream();
                    res.Write(test, 5, test.Length - 5);
                }
            }
            return res.ToArray();
        }
    }
    public class REDI
    {
        private string RESP_HEADERS { get; set; } = @"HTTP/1.1 200 OK
X-Blaze-Command: getServerInstance
Server: TheLegends-REDI
X-Blaze-Component: redirector
Connection: close
X-Blaze-Seqno: 0
Date: Tue, 07 Jun 2022 20:21:49 GMT
Content-Type: application/xml
Content-Length: $$LENGTH$$

";
        private string RESP_CONTENT { get; set; } = @"<?xml version='1.0' encoding='UTF-8'?>
 <serverinstanceinfo>
   <address member='0'>
     <valu>
       <hostname>$$IP$$</hostname>
       <ip>$$DEC_IP$$</ip>
       <port>$$PORT$$</port>
     </valu>
   </address>
   <secure>0</secure>
   <trialservicename></trialservicename>
   <defaultdnsaddress>0</defaultdnsaddress>
 </serverinstanceinfo>
".Replace("$$IP$$", "127.0.0.1")
            .Replace("$$DEC_IP$$", ((uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(IPAddress.Parse("127.0.0.1").GetAddressBytes(), 0))).ToString())
            .Replace("$$PORT$$", Program.Port.ToString());
        private TcpListener Listener { get; set; }
        private TcpListener Listener443 { get; set; }
        public REDI(int port)
        {
            Listener = new TcpListener(IPAddress.Any, port);
            Listener443 = new TcpListener(IPAddress.Any, 443);
        }
        public void Start()
        {
            Log($"[GOS-REDI] Started Successfuly <{Listener.LocalEndpoint.ToString()}>", 0);
            Log($"[GOS-REDI] Started Successfuly <{Listener443.LocalEndpoint.ToString()}>", 0);
            Listener.Start();
            Listener443.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            });
            while (true)
            {
                TcpClient client = Listener443.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }
        public void HandleClient(TcpClient client)
        {
            try
            {
                Log($"[GOS-REDI] Client Connected <{client.Client.RemoteEndPoint.ToString()}>", 0);
                SslStream stream = new SslStream(client.GetStream(), false);
                stream.AuthenticateAsServer(new X509Certificate2($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}SSL.pfx", "123456"), false, false);
                Log($"[GOS-REDI] Client <{client.Client.RemoteEndPoint.ToString()}> Verified Successfuly!", 0);
                string r_request = REDIHelpers.ReadContent(stream, client.ReceiveBufferSize);
                if (r_request.Length > 0 && !string.IsNullOrWhiteSpace(r_request))
                {
                    //Console.WriteLine(r_request);
                    HttpParser parser = new HttpParser();
                    parser.Parse(r_request);
                    if (parser.Method == "POST" && parser.Uri == "/redirector/getServerInstance")
                    {
                        byte[] data = Encoding.UTF8.GetBytes($"{RESP_HEADERS.Replace("$$LENGTH$$", RESP_CONTENT.Length.ToString())}{RESP_CONTENT}");
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        Log($"[GOS-REDI] <{client.Client.RemoteEndPoint.ToString()}> Redirected Successfuly", 0);
                    }
                    else if (parser.Uri == "/pinEvents")
                    {
                        string response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nConnection: keep-alive\r\nContent-Length: 15\r\n\r\n{\"status\":\"ok\"}";
                        byte[] data = Encoding.UTF8.GetBytes(response);
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        Log($"[GOS-REDI] <{client.Client.RemoteEndPoint.ToString()}> Pin-River Handled Successfuly", 0);
                    }
                    else
                    {
                        byte[] data = Encoding.UTF8.GetBytes($"{RESP_HEADERS.Replace("$$LENGTH$$", RESP_CONTENT.Length.ToString())}{RESP_CONTENT}");
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        Log($"[GOS-REDI] <{client.Client.RemoteEndPoint.ToString()}> Redirected Successfuly", 0);
                        Log($"[GOS-REDI] Unkown Packet `https://{parser.Host}{parser.Uri}` From <{client.Client.RemoteEndPoint.ToString()}>", 0);
                    }
                }
                else
                {
                    Log($"[GOS-REDI] Malformed Packet From <{client.Client.RemoteEndPoint.ToString()}>", 0);

                }
            }
            catch (Exception ex)
            {
                Log($"[GOS-REDI] Unkown ERROR \\ Connection FROM <{client.Client.RemoteEndPoint.ToString()}>", 0);
                Log(ex.ToString(), 0);

            }


        }


        private string ReadTCPString(NetworkStream stream)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num;
            while ((num = stream.ReadByte()) != -1 && num != 0)
            {
                stringBuilder.Append((char)num);
            }
            return stringBuilder.ToString();
        }
    }
}
