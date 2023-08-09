// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Server.WebServer
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using SimpleLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TTVProxy;

namespace TTVProxy.Http.Server
{
    public class WebServer
    {
        private List<TcpClient> clients = new List<TcpClient>();
        public const int BUFFER_SIZE = 464;
        private TcpListener websock;
        private bool _closed;
        private readonly Dictionary<string, Action<MyWebRequest>> _routers;
        private readonly HttpMimeDictionary _mimes;

        public int Port { get; private set; }

        public WebServer(int port = 8080)
        {
            this.Port = port;
            this._routers = new Dictionary<string, Action<MyWebRequest>>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
            this._mimes = HttpMimeDictionary.GetDefaults();
        }

        public void Start()
        {
            this.websock = new TcpListener(IPAddress.Any, this.Port);
            this.websock.Start();
            new Thread(new ThreadStart(this.ClientAccept))
            {
                IsBackground = true
            }.Start();
        }

        public HttpMime GetMime(string type)
        {
            return this._mimes[type];
        }

        public string[] GetRoutes()
        {
            string[] array = new string[this._routers.Keys.Count];
            this._routers.Keys.CopyTo(array, 0);
            return array;
        }

        private void ClientAccept()
        {
            while (!this._closed)
            {
                try
                {
                    TcpClient tcpClient1 = this.websock.AcceptTcpClient();
                    this.clients.RemoveAll((Predicate<TcpClient>)(tcpClient => !tcpClient.Connected));
                    this.clients.Add(tcpClient1);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.ClientReceived), (object)tcpClient1);
                }
                catch (Exception ex)
                {
                    TtvProxy.Log.WriteError(ex.Message);
                }
            }
        }

        private void ClientReceived(object o)
        {
            TcpClient client = (TcpClient)o;
            if (!client.Connected)
                return;
            MyWebRequest req = (MyWebRequest)null;
            try
            {
                req = MyWebRequest.Create(client);
                if (req.Url != null)
                {
                    if (TtvProxy.Debug)
                    {
                        string logMsg = string.Format("WebServer::ClientRequest({0}): {1}", req.Client.Client.RemoteEndPoint, req.Url);
                        TtvProxy.Log.WriteInfo(logMsg);
                    }
                    if (this._routers.ContainsKey(req.Method.ToString() + "_" + req.Url))
                        this._routers[req.Method.ToString() + "_" + req.Url](req);
                    else
                        this.Send404(req);
                }
                client.Close();
            }
            catch (Exception ex)
            {
                string logMsg = string.Format("WebServer::ClientRequest({0}):{1}", req != null ? (req.Url + "?" + req.QueryString) : "", ex.Message);
                TtvProxy.Log.WriteError(logMsg);
                client.Close();
            }
        }

        public void Send404(MyWebRequest req)
        {
            MyWebResponse response = req.GetResponse();
            response.AddHeader(HttpHeader.ContentType, "text/html; charset=UTF-8", "utf-8");
            byte[] bytes = Encoding.UTF8.GetBytes("<h1>404. Файл не найден</h1>");
            response.AddHeader(HttpHeader.ContentLength, bytes.Length.ToString(), "utf-8");
            response.SendHeaders(200);
            try
            {
                response.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                TtvProxy.Log.Write(ex.Message, TypeMessage.Error);
            }
        }

        public void SendFile(MyWebRequest req, string fpath)
        {
            if (!System.IO.File.Exists(fpath))
                this.Send404(req);
            FileStream fileStream = System.IO.File.OpenRead(fpath);
            MyWebResponse response = req.GetResponse();
            response.AddHeader(HttpHeader.ContentType, this._mimes[Path.GetExtension(fpath)].ToString(), "utf-8");
            response.AddHeader(HttpHeader.ContentLength, fileStream.Length.ToString(), "utf-8");
            fileStream.CopyTo(response.GetStream());
        }

        public void Close()
        {
            this._closed = true;
            this.clients.ForEach((Action<TcpClient>)(client => client.Close()));
            this.clients.Clear();
            GC.Collect();
            this.websock.Stop();
        }

        public void AddRouteUrl(string url, Action<MyWebRequest> route, HttpMethod method)
        {
            string key = (string)method.ToString() + "_" + url;
            if (!this._routers.ContainsKey(key))
            {
                this._routers.Add(key, route);
            }
            else
            {
                TtvProxy.Log.Write(string.Format("URL {0} already routed", url), TypeMessage.Error);
            }
        }
    }
}