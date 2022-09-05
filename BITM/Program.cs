using BITM.Network;
using Newtonsoft.Json;
using System.Net.Security;
using System.Net.Sockets;

namespace BITM
{
    class Program
    {
        public static int Port = 43129;
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine(args.Length);

                foreach (string arg in args)
                {
                    Console.WriteLine(arg);
                    if (File.Exists(arg))
                    {
                        try
                        {
                            byte[] data = File.ReadAllBytes(arg);
                            var pp = BITM.BlazeSDK.Blaze.FetchAllPacket(new MemoryStream(data));
                            var test = BITM.BlazeSDK.Blaze.Packets2Json(pp, Newtonsoft.Json.Formatting.Indented);
                            var root = Path.GetDirectoryName(arg);
                            if (!Directory.Exists(Path.Combine(root, "json")))
                                Directory.CreateDirectory(Path.Combine(root, "json"));
                            var newfile = Path.Combine(Path.Combine(root, "json"), $"{Path.GetFileNameWithoutExtension(arg)}.json");
                            File.WriteAllText(newfile, test);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Console.ReadLine();
                            continue;
                        }

                    }

                }

                Console.ReadLine();
                return;
            }
            //new Task(() =>
            //{
            //    new REDI(42230).Start();
            //}).Start();
            Listener listener = new Listener(Port);
            listener.Start();

        }
    }
}



//List<BITM.BlazeSDK.IBlazeComponent[]> packets = new List<BITM.BlazeSDK.IBlazeComponent[]>();
//for (int i = 0; i <= 7; i++)
//{
//    byte[] data = File.ReadAllBytes($"dump\\CLI_{i}.bin");
//    var pp = BITM.BlazeSDK.Blaze.FetchAllPacket(new MemoryStream(data));
//    var test = BITM.BlazeSDK.Blaze.Packets2Json(pp, Newtonsoft.Json.Formatting.Indented);

//    byte[] data2 = File.ReadAllBytes($"dump\\SRV_{i}.bin");
//    var pp2 = BITM.BlazeSDK.Blaze.FetchAllPacket(new MemoryStream(data2));
//    var test2 = BITM.BlazeSDK.Blaze.Packets2Json(pp2, Newtonsoft.Json.Formatting.Indented);

//    Directory.CreateDirectory($"json\\packet_{i}");
//    File.WriteAllText($"json\\packet_{i}\\CLI.json", test);
//    File.WriteAllText($"json\\packet_{i}\\SRV.json", test2);

//}


//Console.ReadLine();