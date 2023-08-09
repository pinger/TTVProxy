// Decompiled with JetBrains decompiler
// Type: TTVProxy.TorrentStream
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using Microsoft.Win32;
using SimpleLogger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTVProxy.TTVApi;

namespace TTVProxy
{
    public class TorrentStream
    {
        public static readonly string MSG_HELLOBG = "HELLOBG";
        public static readonly string MSG_HELLOTS = "HELLOTS";
        public static readonly string MSG_READY = "READY";
        public static readonly string MSG_LOADASYNC = "LOADASYNC";
        public static readonly string MSG_LOADRESP = "LOADRESP";
        public static readonly string MSG_START = "START";
        public static readonly string MSG_STOP = "STOP";
        public static readonly string MSG_GETPID = "GETPID";
        public static readonly string MSG_GETCID = "GETCID";
        public static readonly string MSG_GETADURL = "GETADURL";
        public static readonly string MSG_USERDATA = "USERDATA";
        public static readonly string MSG_SAVE = "SAVE";
        public static readonly string MSG_LIVESEEK = "LIVESEEK";
        public static readonly string MSG_SHUTDOWN = "SHUTDOWN";
        public static readonly string MSG_PLAY = "PLAY";
        public static readonly string MSG_PLAYAD = "PLAYAD";
        public static readonly string MSG_PLAYADI = "PLAYADI";
        public static readonly string MSG_PAUSE = "PAUSE";
        public static readonly string MSG_RESUME = "RESUME";
        public static readonly string MSG_DUR = "DUR";
        public static readonly string MSG_PLAYBACK = "PLAYBACK";
        public static readonly string MSG_EVENT = "EVENT";
        public static readonly string MSG_STATE = "STATE";
        public static readonly string MSG_INFO = "INFO";
        public static readonly string MSG_STATUS = "STATUS";
        public static readonly string MSG_AUTH = "AUTH";
        private readonly Socket _tssock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        private readonly PlatformID _curPlantform = Environment.OSVersion.Platform;
        private readonly string _defaultAddr = "127.0.0.1";
        private readonly int _port = 62062;
        private readonly List<TSMessage> _messagePool = new List<TSMessage>();
        private int _asTimeoute = 60;
        private static string _tsPath;
        private AsyncOperation _operation;
        private Task<string> _playdTask;
        public string ContentType;

        public bool Connected
        {
            get
            {
                return this._tssock.Connected;
            }
        }

        public string AceBroadcast { get; private set; }

        public string PlayedFile { get; private set; }

        public List<TcpClient> Owner { get; private set; }

        public bool Started { get; private set; }

        public static string AcePath
        {
            get
            {
                if (string.IsNullOrEmpty(_tsPath))
                {
                    RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\ACEStream");
                    if (registryKey != null)
                        _tsPath = (string)registryKey.GetValue("EnginePath");
                }
                string path = Path.GetDirectoryName(_tsPath);
                return path;
            }
        }

        public event TorrentStream.OnReceive OnReceived;

        public event TorrentStream.OnReceiveError OnError;

        protected TorrentStream()
        {
            this.LoadSettings();
        }

        public TorrentStream(TcpClient owner = null)
          : this()
        {
            this.Owner = new List<TcpClient>();
            if (owner != null)
                this.Owner.Add(owner);
            if (this._curPlantform == PlatformID.Win32NT)
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\ACEStream");
                if (registryKey != null)
                    TorrentStream._tsPath = (string)registryKey.GetValue("EnginePath");
                else
                    this.RaiseError((object)new Exception("AceStreamNotInstalled"));
                string path = Path.GetDirectoryName(TorrentStream._tsPath) + "/acestream.port";
                if (!System.IO.File.Exists(path))
                {
                    if (System.IO.File.Exists(TorrentStream._tsPath))
                    {
                        this.RunTS();
                    }
                    else
                    {
                        this.RaiseError((object)new Exception("AceStreamNotFound"));
                        return;
                    }
                }
                while (!System.IO.File.Exists(path))
                    Thread.Sleep(16);
                using (StreamReader streamReader = System.IO.File.OpenText(path))
                {
                    this._port = Convert.ToInt32(streamReader.ReadLine());
                    streamReader.Close();
                }
            }
            else
                this.RunTS();
        }

        public TorrentStream(int port, TcpClient owner = null)
          : this()
        {
            this._port = port;
            this.Owner = new List<TcpClient>();
            if (owner != null)
                this.Owner.Add(owner);
            if (!this.IsRunning())
                return;
            this.RunTS();
        }

        public TorrentStream(string addr, int port, TcpClient owner = null)
          : this()
        {
            this._defaultAddr = addr;
            this._port = port;
            this.Owner = new List<TcpClient>();
            if (owner != null)
                this.Owner.Add(owner);
            if (!this.IsRunning())
                return;
            this.RunTS();
        }

        public Task<string> GetPlayTask()
        {
            return this._playdTask;
        }

        public Task<string> Play(string file, StreamSource source, int index = 0)
        {
            if (source == StreamSource.torrent)
            {
                if (string.IsNullOrEmpty(file))
                {
                    TtvProxy.Log.Write("Не верная ссылка на торрент", TypeMessage.Error);
                    throw new Exception("Не верная ссылка на торрент");
                }
                file = new Uri(file).ToString();
            }
            string str1 = string.Format("START {0} {1} ", (object)source.ToString().ToUpper(), (object)file);
            string str2 = source != StreamSource.contentid ? str1 + (object)index + " 0 0 0" : str1 + (object)index;
            this._playdTask = new Task<string>(new Func<string>(this.WaytingPlay));
            this._playdTask.Start();
            this.SendMessage(str2.Replace("CONTENTID", "PID"));
            this.PlayedFile = file;
            return this._playdTask;
        }

        private void LoadSettings()
        {
            this._asTimeoute = TtvProxy.MySettings.GetSetting("system", "as_timeout", 60);
        }

        private string WaytingPlay()
        {
            DateTime dateTime = DateTime.Now;
            dateTime = dateTime.AddSeconds((double)this._asTimeoute);
            while (this._tssock.Connected)
            {
                List<TcpClient> owner = this.Owner;
                Func<TcpClient, bool> func = (Func<TcpClient, bool>)(client => client.Connected);
                //Func<TcpClient, bool> predicate;
                //if (Enumerable.Any<TcpClient>((IEnumerable<TcpClient>) owner, predicate) && DateTime.Now < dateTime || this.Owner.Count == 0)
                if (Enumerable.Any<TcpClient>((IEnumerable<TcpClient>)owner, func) && DateTime.Now < dateTime || this.Owner.Count == 0)
                {
                    List<TSMessage> list = this._messagePool;
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter((object)list, ref lockTaken);
                        foreach (TSMessage tsMessage in this._messagePool)
                        {
                            if (tsMessage.Type == TorrentStream.MSG_START)
                            {
                                if (tsMessage.Parameters.ContainsKey("ad"))
                                {
                                    Stream responseStream = WebRequest.Create(this.AceBroadcast).GetResponse().GetResponseStream();
                                    byte[] buffer = new byte[384];
                                    for (int index = 0; index < 4 && responseStream != null && responseStream.Read(buffer, 0, buffer.Length) > 0; ++index)
                                    {
                                        Thread.Sleep(1000);
                                        switch (index)
                                        {
                                            case 0:
                                                this.SendMessage(string.Format("PLAYBACK {0} 0", (object)this.AceBroadcast));
                                                break;
                                            case 1:
                                                this.SendMessage(string.Format("PLAYBACK {0} 25", (object)this.AceBroadcast));
                                                break;
                                            case 2:
                                                this.SendMessage(string.Format("PLAYBACK {0} 50", (object)this.AceBroadcast));
                                                break;
                                            case 3:
                                                this.SendMessage(string.Format("PLAYBACK {0} 75", (object)this.AceBroadcast));
                                                break;
                                        }
                                    }
                                    this.SendMessage(string.Format("PLAYBACK {0} 100", (object)this.AceBroadcast));
                                    if (responseStream != null)
                                        responseStream.Close();
                                }
                                else
                                {
                                    this._messagePool.Clear();
                                    return this.AceBroadcast;
                                }
                            }
                        }
                        this._messagePool.Clear();
                    }
                    finally
                    {
                        if (lockTaken)
                            Monitor.Exit((object)list);
                    }
                    Thread.Sleep(4);
                }
                else
                    break;
            }
            return (string)null;
        }

        public void RunTS()
        {
            TtvProxy.Log.WriteInfo("[AceStream] Запуск AceStream");
            if (this._curPlantform == PlatformID.Win32NT)
                Process.Start(TorrentStream._tsPath);
            else
                Process.Start("acestreamengine", "--client-console");
        }

        public bool IsRunning()
        {
            if (this._curPlantform == PlatformID.Win32NT)
                return Enumerable.Any<Process>((IEnumerable<Process>)Process.GetProcessesByName("tsengine.exe"));
            return Enumerable.Any<Process>((IEnumerable<Process>)Process.GetProcesses(), 
                (Func<Process, bool>)(i => i.ProcessName.Contains("acestreamengine")));
        }

        public bool Connect()
        {
            try
            {
                TtvProxy.Log.WriteInfo("[AceStream] Попытка подключиться к AceStream");
                this._tssock.Connect(this._defaultAddr, this._port);
                TtvProxy.Log.WriteInfo("[AceStream] Подключение к AceStream успешно");
            }
            catch
            {
                this.RunTS();
                int num = 0;
                while (!this._tssock.Connected)
                {
                    if (num < 100)
                    {
                        try
                        {
                            this._tssock.Connect(this._defaultAddr, this._port);
                            break;
                        }
                        catch (Exception ex)
                        {
                            ++num;
                            Thread.Sleep(100);
                        }
                    }
                    else
                        break;
                }
            }
            if (!this._tssock.Connected)
                throw new Exception(SocketError.ConnectionRefused.ToString());
            this.SendMessage(TorrentStream.MSG_HELLOBG + " version=4");
            byte[] numArray = new byte[192];
            int num1 = this._tssock.Receive(numArray);
            Dictionary<string, string> dictionary = this.ParseParametersToDictionary(Encoding.Default.GetString(numArray, 0, num1 - 2));
            if (!dictionary.ContainsKey("key"))
            {
                this.Disconnect(true);
                return false;
            }
            this.SendMessage(string.Format("{2} key={0}-{1}", (object)"c1rvequTEgoyC06zTVz1-Yl12lvAzWyv-7WYVhxe7A4zR2fUNPZw8l1y-riKqspd5".Split("-".ToCharArray())[0], (object)BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(dictionary["key"] + "c1rvequTEgoyC06zTVz1-Yl12lvAzWyv-7WYVhxe7A4zR2fUNPZw8l1y-riKqspd5"))).Replace("-", "").ToLower(), (object)TorrentStream.MSG_READY));
            this._operation = AsyncOperationManager.CreateOperation((object)null);
            new Thread(new ThreadStart(this.Receive))
            {
                IsBackground = true
            }.Start();
            return this._tssock.Connected;
        }

        private Dictionary<string, string> ParseParametersToDictionary(string value)
        {
            IEnumerable<string[]> source = Enumerable.Select<string, string[]>(Enumerable.Skip<string>((IEnumerable<string>)value.Split(" ".ToCharArray()), 1), (Func<string, string[]>)(s => s.Split("=".ToCharArray())));
            Func<string[], string> func = (Func<string[], string>)(prm => prm[0]);
            //Func<string[], string> keySelector;
            //return Enumerable.ToDictionary<string[], string, string>(source, keySelector, (Func<string[], string>) (prm => prm[1]));
            return Enumerable.ToDictionary<string[], string, string>(source, func, (Func<string[], string>)(prm => prm[1]));
        }

        private void RaisReceived(object msg)
        {
            this._operation.Post((SendOrPostCallback)(state =>
           {
               // ISSUE: reference to a compiler-generated field
               if (this.OnReceived == null || msg == null)
                   return;
               // ISSUE: reference to a compiler-generated field
               this.OnReceived(this, (TSMessage)msg);
           }), (object)null);
        }

        private void RaiseError(object e)
        {
            this._operation.Post((SendOrPostCallback)(state =>
           {
               // ISSUE: reference to a compiler-generated field
               if (this.OnError == null)
                   return;
               // ISSUE: reference to a compiler-generated field
               this.OnError(this, (Exception)e);
           }), (object)null);
        }

        private void Receive()
        {
            byte[] numArray = new byte[(int)short.MaxValue];
            string str = string.Empty;
            while (this._tssock.Connected)
            {
                try
                {
                    if (this.Owner == null)
                    {
                        this._tssock.Close();
                        return;
                    }
                    int count = this._tssock.Receive(numArray);
                    if (count > 0)
                    {
                        str += Encoding.UTF8.GetString(numArray, 0, count);
                        if (str.Contains("\r\n"))
                        {
                            if (str.LastIndexOf("\r\n", StringComparison.Ordinal) >= str.Length - 2)
                            {
                                foreach (string msg in str.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                                {
                                    TSMessage tsMessage = TSMessage.Construct(msg);
                                    if (tsMessage.Type == TorrentStream.MSG_START)
                                    {
                                        this.Started = true;
                                        this.AceBroadcast = tsMessage.Parameters["url"];
                                    }
                                    else if (msg == "STATUS 0")
                                        this.Started = false;
                                    else if (tsMessage.Type == TorrentStream.MSG_EVENT && tsMessage.Text.Contains("getuserdata"))
                                    {
                                        this.SendMessage("USERDATA [{\"gender\": 1}, {\"age\": 1}]");
                                        this.Disconnect(true);
                                    }
                                    List<TSMessage> list = this._messagePool;
                                    bool lockTaken = false;
                                    try
                                    {
                                        Monitor.Enter((object)list, ref lockTaken);
                                        this._messagePool.Add(tsMessage);
                                    }
                                    finally
                                    {
                                        if (lockTaken)
                                            Monitor.Exit((object)list);
                                    }
                                    this.RaisReceived((object)tsMessage);
                                }
                                str = string.Empty;
                                Thread.Sleep(20);
                            }
                        }
                    }
                    else
                        break;
                }
                catch (SocketException ex)
                {
                    Socket socket = this._tssock;
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter((object)socket, ref lockTaken);
                        this._tssock.Close();
                        break;
                    }
                    finally
                    {
                        if (lockTaken)
                            Monitor.Exit((object)socket);
                    }
                }
            }
            if (!this._tssock.Connected)
                return;
            this.Disconnect(true);
        }

        public void SendMessage(string msg)
        {
            try
            {
                Socket socket = this._tssock;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter((object)socket, ref lockTaken);
                    if (this._tssock.Connected)
                        this._tssock.Send(Encoding.UTF8.GetBytes(msg + "\r\n"));
                    else
                        this.RaiseError((object)new Exception("AceStream is disconnected"));
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit((object)socket);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Disconnect(bool shutdown = true)
        {
            Socket socket = this._tssock;
            bool lockTaken = false;
            try
            {
                Monitor.Enter((object)socket, ref lockTaken);
                if (!this._tssock.Connected)
                    return;
                Thread.Sleep(6);
                this.SendMessage(TorrentStream.MSG_STOP);
                Thread.Sleep(12);
                this.SendMessage(TorrentStream.MSG_SHUTDOWN);
                Thread.Sleep(18);
                this._tssock.Close(1000);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit((object)socket);
            }
        }

        public string GetStreamUrl()
        {
            return string.Empty;
        }

        public string GetTorrentUrl()
        {
            return string.Empty;
        }

        public delegate void OnReceive(TorrentStream sender, TSMessage msg);

        public delegate void OnReceiveError(TorrentStream sender, Exception ex);
    }
}