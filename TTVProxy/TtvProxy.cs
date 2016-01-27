using CryptoLibrary;
using Microsoft.Win32;
using SimpleLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using TTVProxy.Http.Server;
using TTVProxy.TTVApi;

namespace TTVProxy
{
    public class TtvProxy
    {
        public static string ExeDir = "";
        private object locker = new object();
        private readonly List<TorrentStream> _tsPool = new List<TorrentStream>();
        public const int AGE_CACHE = 325;
        public static Logger Log;
        public static SettingManager MySettings;
        public TTVProxyDevice Device;
        private VlcClient Vlc;
        public static bool Debug;
        private WebServer _web;
        private static string _appdata;
        private readonly bool _console;
        private System.Timers.Timer _tm;

        public Auth SessionState { get; protected set; }

        public static string ApplicationDataFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_appdata))
                {
                    _appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TTVProxyData");
                    if (!Directory.Exists(_appdata))
                        Directory.CreateDirectory(_appdata);
                }
                return _appdata;
            }
        }

        public static string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
            }
        }

        public bool IsWorking { get; set; }

        public event Notify Notifyed;

        public TtvProxy(bool console = false)
        {
            this._console = console;
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException);
            ExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log = new Logger(Path.Combine(ApplicationDataFolder, string.Format("TTVProxy_{0:yyyyMMdd}.log", DateTime.Now)), _console);
            MySettings = new SettingManager(Path.Combine(ApplicationDataFolder, "settings.xml"));
            this.IsWorking = true;
        }

        protected virtual void OnNotifyed(string text, int icon)
        {
            Notify notify = this.Notifyed;
            TypeMessage type = icon == 1 ? TypeMessage.Error : (icon == 2 ? TypeMessage.Warning : TypeMessage.Info);
            Log.Write(text, type);
            if (notify == null)
                return;
            notify(text, icon);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Log != null)
                Log.WriteError((e.ExceptionObject as Exception).Message);
            if (!(e.ExceptionObject is VlcConnectError))
                return;
            this.OnNotifyed("Не возможно запустить VLC", 1);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    this.Start();
                    break;
                case PowerModes.Suspend:
                    this.Stop();
                    break;
            }
            Log.WriteInfo("TTVProxy is " + (object)e.Mode);
        }

        public void Start()
        {
            Log.WriteInfo("Start TTVProxy");
            int setting = MySettings.GetSetting("web", "port", 8081);
            try
            {
                this.Vlc = new VlcClient();
                this.Vlc.OnEvent += new VlcClient.VlcEvent(this.VlcOnOnEvent);
                this.Vlc.ConnectAsync();
            }
            catch (Exception ex)
            {
                this.OnNotifyed("Не удалось запустить VLC-сервер. Дальнейшая работа программы не возможна", 2);
                this.IsWorking = false;
                return;
            }
            Log.WriteInfo("[TTVProxy] Starting Web-server");
            this._web = new WebServer(Convert.ToUInt16(setting));
            this._web.Start();
            this.Login();
            this.Device = new TTVProxyDevice(this._web, this);
            this.Device.Start();
            this.IsWorking = true;
        }

        public void Stop()
        {
            this._web.Close();
            this._tm.Close();
            this._tm.Dispose();
            this.Vlc.Close();
            this._tsPool.ForEach((Action<TorrentStream>)(stream => stream.Disconnect(true)));
            this._tsPool.Clear();
            this.Device.Stop();
        }

        private void VlcOnOnEvent(string text)
        {
            this.OnNotifyed(text, 0);
        }

        public void Close()
        {
            try
            {
                SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
                Log.Close();
                this.Vlc.Close();
                List<TorrentStream> list = this._tsPool;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter((object)list, ref lockTaken);
                    foreach (TorrentStream torrentStream in this._tsPool)
                        torrentStream.Disconnect(true);
                    this._tsPool.Clear();
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit((object)list);
                }
                this.Device.Stop();
                this._web.Close();
                this.IsWorking = false;
            }
            catch (Exception ex)
            {
            }
        }

        public bool Login()
        {
            string setting1 = TtvProxy.MySettings.GetSetting("torrent-tv.ru", "login", "anonymous");
            string setting2 = TtvProxy.MySettings.GetSetting("torrent-tv.ru", "password", "anonymous");
            string password;
            try
            {
                password = CryptoHelper.Decrypt<AesCryptoServiceProvider>(setting2 ?? "", Environment.MachineName, "4<_I'nQ");
            }
            catch
            {
                this.OnNotifyed("Неверный логин/пароль", 1);
                this.SessionState = new Auth();
                this.SessionState.error = ApiError.incorrect;
                return false;
            }
            this.SessionState = Auth.Run(setting1, password);
            if (!this.SessionState.IsSuccess())
            {
                this.OnNotifyed("Ошибка авторизации:" + this.SessionState.error.ToString(), 1);
                return false;
            }
            this.OnNotifyed("Авторизация успешна", 0);
            return true;
        }

        public TorrentStream GetTsClient(string key)
        {
            List<TorrentStream> list = this._tsPool;
            bool lockTaken = false;
            try
            {
                Monitor.Enter((object)list, ref lockTaken);
                return Enumerable.FirstOrDefault<TorrentStream>((IEnumerable<TorrentStream>)this._tsPool, 
                    (Func<TorrentStream, bool>)(c => c.PlayedFile == key));
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit((object)list);
            }
        }

        public void AddToTsPool(TorrentStream ts)
        {
            List<TorrentStream> list = this._tsPool;
            bool lockTaken = false;
            try
            {
                Monitor.Enter((object)list, ref lockTaken);
                this._tsPool.Add(ts);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit((object)list);
            }
        }

        public void RemoveFromTsPoos(TorrentStream ts)
        {
            List<TorrentStream> list = this._tsPool;
            bool lockTaken = false;
            try
            {
                Monitor.Enter((object)list, ref lockTaken);
                if (!this._tsPool.Contains(ts))
                    return;
                ts.Disconnect(true);
                this._tsPool.Remove(ts);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit((object)list);
            }
        }

        public string FindBroadcastUrl(string file)
        {
            object obj = this.locker;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                return this.Vlc.GetBroadcastUrl(file);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public string StartBroadcastStream(string file, string transcode = null)
        {
            object obj = this.locker;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                this.Vlc.Start(file, transcode);
                return this.Vlc.GetBroadcastUrl(file);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public void AddVlcBroadcastClient(string file, TcpClient client)
        {
            object obj = this.locker;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                this.Vlc.AddRoutedClient(file, client);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public Dictionary<string, string> GetBroadcasts()
        {
            object obj = this.locker;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                return this.Vlc.GetBroadcasts();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public int RoutedClientsCount(string url)
        {
            object obj = this.locker;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                return this.Vlc.RoutedClientsCount(url);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public bool StopBroadcast(string broadcast, string file)
        {
            object obj = this.locker;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                if (this.Vlc.RoutedClientsCount(broadcast) != 0)
                    return false;
                this.Vlc.Stop(file);
                return true;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public delegate void Notify(string text, int icon);
    }
}