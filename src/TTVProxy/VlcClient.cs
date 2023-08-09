// Decompiled with JetBrains decompiler
// Type: TTVProxy.VlcClient
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using SimpleLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using XmlSettings;

namespace TTVProxy
{
    public class VlcClient
    {
        public static readonly string WRONGPASS = "Wrong password";
        public static readonly string AUTHOK = "Welcome, Master";
        public static readonly string BROADCASTEXISTS = "Name already in use";
        public static readonly string SYNTAXERR = "Wrong command syntax";
        public static readonly string STARTOK = "new";
        public static readonly string STOPOK = "del";
        public static readonly string STOPERR = "media unknown";
        public static readonly string SHUTDOWN = "Bye-bye!";
        public static int Cache = 2000;
        public static int MuxCache = 0;
        private readonly Dictionary<string, string> _vods = new Dictionary<string, string>();
        public Dictionary<string, List<TcpClient>> Clients = new Dictionary<string, List<TcpClient>>();
        private Socket _sock;
        private readonly string _passw;
        private readonly int _vlcport;
        private readonly int _broadcastport;
        private readonly bool _extvlc;
        private readonly string _extvlcpath;
        private readonly Dictionary<string, string> _broadcasts;
        private readonly int _rtspport;
        private bool _inproccess;
        private Process _mainproc;

        public bool IsConnected
        {
            get
            {
                return this._sock.Connected;
            }
        }

        public int RtspPort
        {
            get
            {
                return this._rtspport;
            }
        }

        public event VlcEvent OnEvent;

        public VlcClient()
        {
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            Settings settings = new Settings(Path.Combine(TtvProxy.ApplicationDataFolder, "settings.xml"));

            if (!int.TryParse(settings.GetValue("vlc", "vlcport"), out _vlcport))
            {
                settings.SetValue("vlc", "vlcport", "4212");
                _vlcport = 4212;
            }

            _passw = settings.GetValue("vlc", "vlcpassw");
            if (string.IsNullOrEmpty(_passw))
            {
                _passw = "admin";
                settings.SetValue("vlc", "vlcpassw", _passw);
            }

            if (!int.TryParse(settings.GetValue("vlc", "vlcbroadcastport"), out _broadcastport))
            {
                _broadcastport = 8082;
                settings.SetValue("vlc", "vlcbradcastport", "8082");
            }

            if (!int.TryParse(settings.GetValue("vlc", "vlccache"), out VlcClient.Cache))
            {
                VlcClient.Cache = 2000;
                settings.SetValue("vlc", "vlccache", "4000");
            }

            if (!int.TryParse(settings.GetValue("vlc", "vlcmuxcache"), out VlcClient.MuxCache))
            {
                VlcClient.MuxCache = 0;
                settings.SetValue("vlc", "vlcmuxcache", "0");
            }

            if (!int.TryParse(settings.GetValue("vlc", "rtspport"), out _rtspport))
            {
                _rtspport = 5554;
                settings.SetValue("vlc", "rtspport", _rtspport.ToString());
            }

            bool.TryParse(settings.GetValue("vlc", "vlcext"), out _extvlc);
            _extvlcpath = settings.GetValue("vlc", "vlcpath");
            if (string.IsNullOrEmpty(_extvlcpath) || !File.Exists(_extvlcpath))
                _extvlc = false;
            _broadcasts = new Dictionary<string, string>();
        }

        public void RaiseEvent(string text)
        {
            if (OnEvent == null)
                return;
            OnEvent(text);
        }

        public void AddRoutedClient(string url, TcpClient client)
        {
            if (!Enumerable.Any<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>)_broadcasts, (Func<KeyValuePair<string, string>, bool>)(pair => pair.Value == url)))
                return;
            if (!this.Clients.ContainsKey(url))
                this.Clients.Add(url, new List<TcpClient>());
            this.Clients[url].Add(client);
        }

        public int RoutedClientsCount(string url)
        {
            if (Enumerable.Any<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>)this._broadcasts, (Func<KeyValuePair<string, string>, bool>)(pair => pair.Value == url)) && this.Clients.ContainsKey(url))
            {
                this.Clients[url].RemoveAll((Predicate<TcpClient>)(client => !client.Connected));
                if (this.Clients[url].Count != 0)
                    return this.Clients[url].Count;
                this.Clients.Remove(url);
            }
            return 0;
        }

        public void ConnectAsync()
        {
            this._inproccess = true;
            ThreadPool.QueueUserWorkItem((WaitCallback)(e =>
           {
               try
               {
                   this.Connect();
               }
               catch (Exception ex)
               {
                   this.RaiseEvent("Ошибка подключения к VLC");
                   if (this._sock.Connected)
                   {
                       this._sock.Close();
                       this._sock.Dispose();
                   }
               }
               this._inproccess = false;
           }));
        }

        public void Connect()
        {
            try
            {
                _inproccess = true;
                if (_mainproc != null)
                    Close();
                _sock.Connect("127.0.0.1", _vlcport);
            }
            catch (Exception ex1)
            {
                this.RaiseEvent("Starting VLC");
                string arguments = string.Format("-I telnet --clock-jitter=0 --clock-synchro 0 --no-network-synchronisation --network-caching {0} --sout-mux-caching {3} --telnet-password {1} --telnet-port {2} --rtsp-host 0.0.0.0 --rtsp-port {4}", (object)VlcClient.Cache, (object)this._passw, (object)this._vlcport, (object)VlcClient.MuxCache, (object)this._rtspport);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    if (!this._extvlc)
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(TorrentStream.AcePath);
                        if (directoryInfo.Parent != null)
                            directoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.Parent.FullName, "player"));
                        this._mainproc = Process.Start(Path.Combine(directoryInfo.FullName, "ace_player.exe"), arguments);
                    }
                    else
                        this._mainproc = Process.Start(this._extvlcpath, arguments);
                }
                else
                    this._mainproc = Process.Start("vlc", arguments);
                this._mainproc.WaitForInputIdle();
                this._mainproc.EnableRaisingEvents = true;
                this._mainproc.Exited += new EventHandler(this.RestartVlc);
                int num = 0;
                while (!this._sock.Connected)
                {
                    if (num < 300)
                    {
                        ++num;
                        try
                        {
                            _sock.Connect("127.0.0.1", this._vlcport);
                        }
                        catch (Exception ex2)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                        break;
                }
            }
            if (!this._sock.Connected)
            {
                this._inproccess = false;
                this.RaiseEvent("Ошибка запуска VLC");
            }
            Thread.Sleep(4);
            byte[] numArray = new byte[1024];
            this._sock.ReceiveTimeout = 29893;
            this._sock.Receive(numArray);
            this.Send(this._passw);
            int count = this._sock.Receive(numArray);
            if (!Encoding.UTF8.GetString(numArray, 0, count).Replace(">", "").Contains(VlcClient.AUTHOK))
            {
                this.Send("shutdown");
                this.RaiseEvent("Ошибка подключения к VLC");
                this._sock.Close();
            }
            this._inproccess = false;
            this.RaiseEvent("VLC запущен");
        }

        private void RestartVlc(object sender, EventArgs args)
        {
            TtvProxy.Log.Write("VLC закрыт, перезапуск", TypeMessage.Info);
            this.Connect();
            this._inproccess = false;
        }

        private void Send(string msg)
        {
            if (!_sock.Connected)
                return;
            _sock.Send(Encoding.ASCII.GetBytes(msg + "\r\n"));
            Thread.Sleep(4);
        }

        private void Send(byte[] msg)
        {
            if (!_sock.Connected)
                return;
            _sock.Send(msg);
            _sock.Send(Encoding.ASCII.GetBytes("\r\n"));
            Thread.Sleep(4);
        }

        public void Start(string url, string transcode = null)
        {
            if (this._mainproc != null && this._mainproc.ProcessName != null)
                this._mainproc.Refresh();
            while (this._inproccess)
                Thread.Sleep(5);
            if (!this._sock.Connected)
            {
                this.Connect();
                this.Start(url, transcode);
            }
            else
            {
                string str1 = Guid.NewGuid().ToString();
                if (this._broadcasts.ContainsKey(url))
                    return;
                string str2 = string.Format("http{{mux={1},dst=:{2}/{0}}}", str1, "ts{use-key-frames}", _broadcastport);
                string str3 = string.Format("new \"{0}\" broadcast input \"{1}\" output #", str1, url) + str2 + " enabled";
                TtvProxy.Log.WriteInfo("[VlcClient] Запуск трансляции с параметрами: " + str3);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    this.Send(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(866), Encoding.UTF8.GetBytes(str3)));
                else
                    this.Send(str3);
                byte[] numArray = new byte[1024];
                int count = this._sock.Receive(numArray);
                string message = Encoding.UTF8.GetString(numArray, 0, count).Replace("> ", "");
                if (!message.Contains(VlcClient.STARTOK))
                    throw new VlcError(message);
                this._broadcasts.Add(url, string.Format("http://127.0.0.1:{0}/{1}", _broadcastport, str1));
                this.Send(string.Format("control {0} play", str1));
                this._sock.Receive(numArray);
            }
        }

        public string GetBroadcastUrl(string url)
        {
            if (!this._broadcasts.ContainsKey(url))
                return "";
            return this._broadcasts[url];
        }

        public Dictionary<string, string> GetBroadcasts()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(this._broadcasts.Count);
            foreach (KeyValuePair<string, string> keyValuePair in this._broadcasts)
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            return dictionary;
        }

        public void Stop(string key)
        {
            if (!this._sock.Connected || !this._broadcasts.ContainsKey(key))
                return;
            this.Send(string.Format("del \"{0}\"", (object)this._broadcasts[key]));
            byte[] numArray = new byte[1024];
            int count = this._sock.Receive(numArray);
            if (!Encoding.UTF8.GetString(numArray, 0, count).Replace("> ", "").Contains(VlcClient.STOPOK))
                throw new VlcStopError();
            this._broadcasts.Remove(key);
        }

        public void Close()
        {
            if (this._sock.Connected)
            {
                this.Send("del all");
                this.Send("shutdown");
                this._sock.Close();
                this._sock.Dispose();
                this._sock = (Socket)null;
                this._broadcasts.Clear();
                this._vods.Clear();
            }
            if (this._mainproc != null)
            {
                this._mainproc.Exited -= new EventHandler(this.RestartVlc);
                try
                {
                    this._mainproc.Kill();
                }
                catch (Exception ex)
                {
                }
                this._mainproc.Close();
                this._mainproc.Dispose();
            }
            GC.Collect();
        }

        public delegate void VlcEvent(string text);
    }
}