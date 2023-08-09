// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVProxyDevice
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Text;
using TTVProxy.Http.Content;
using TTVProxy.Http.Server;

namespace TTVProxy
{
    public class TTVProxyDevice
    {
        public readonly TtvProxy Proxy;
        public ChannelContentProvider ChannelsProvider;
        private EpgContentProvider _epg;
        public WebServer Web;

        public TTVProxyDevice(WebServer webServer, TtvProxy proxy)
        {
            webServer.AddRouteUrl("/login", new Action<MyWebRequest>(this.LoginRequest), HttpMethod.Get);
            this.Web = webServer;
            this.Proxy = proxy;
        }

        private void LoginRequest(MyWebRequest obj)
        {
            this.Proxy.Login();
            MyWebResponse response = obj.GetResponse();
            byte[] bytes = Encoding.UTF8.GetBytes(this.Proxy.SessionState.ToString());
            int num1 = 1;
            string str1 = "text/xml";
            string charset1 = "utf-8";
            response.AddHeader((HttpHeader)num1, str1, charset1);
            int num2 = 0;
            string str2 = bytes.Length.ToString();
            string charset2 = "utf-8";
            response.AddHeader((HttpHeader)num2, str2, charset2);
            int code = 200;
            response.SendHeaders(code);
            response.GetStream().Write(bytes, 0, bytes.Length);
        }

        public void Start()
        {
            this.ChannelsProvider = new ChannelContentProvider(this.Web, this);
            this._epg = new EpgContentProvider(this);
        }

        public void Stop()
        {
        }

        public void AddRoute(string url, Action<MyWebRequest> route, HttpMethod method)
        {
            this.Web.AddRouteUrl(url, route, method);
        }
    }
}