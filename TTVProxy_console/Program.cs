using CryptoLibrary;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TTVProxy;

namespace TTVProxy_console
{
    internal class Program
    {
        private static TtvProxy app;

        private static void Main(string[] args)
        {
            Program.app = new TtvProxy(true);
            Dictionary<string, string> parameters = Program.GetParameters(args);
            if (parameters.Count >= 2)
            {
                TtvProxy.MySettings.SetSetting("torrent-tv.ru", "login", 
                    parameters.ContainsKey("-u") ? (object)parameters["-u"] : (object)parameters["0"]);
                TtvProxy.MySettings.SetSetting("torrent-tv.ru", "password", 
                    (object)CryptoHelper.Encrypt<AesCryptoServiceProvider>(parameters.ContainsKey("-p") ? parameters["-p"] : parameters["1"], Environment.MachineName, "4<_I'nQ"));
            }
            Program.app.Start();
            while (true)
                Thread.Sleep(1000);
        }

        private static Dictionary<string, string> GetParameters(string[] args)
        {
            int index = 0;
            Dictionary<string, string> dictionary 
                = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
            int num = 0;
            while (index < args.Length)
            {
                if (args[index].Contains("-"))
                {
                    if (!dictionary.ContainsKey(args[index]))
                    {
                        dictionary.Add(args[index++], index >= args.Length || args[index].Contains("-") ? "" : args[index++]);
                        ++num;
                    }
                }
                else
                {
                    dictionary.Add(num.ToString(), args[index++]);
                    ++num;
                }
            }
            return dictionary;
        }
    }
}