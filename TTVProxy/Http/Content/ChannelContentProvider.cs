// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Content.ChannelContentProvider
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using SimpleLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTVProxy;
using TTVProxy.Http.Server;
using TTVProxy.TTVApi;

namespace TTVProxy.Http.Content
{
    public class ChannelContentProvider : ContentProvider
    {
        public static readonly string PLAYLIST_PATH = "/channels/";
        public static readonly string PLAY_PATH = "/channels/play";
        public static readonly string SCREEN_PATH = "/channels/screen";
        public static readonly string FAVOURITE_ADD = "/channels/favourite/add";
        public static readonly string FAVOURITE_DEL = "/channels/favourite/del";
        private readonly DateTime _lastUpdate = new DateTime(1, 1, 1);
        private readonly TimeSpan _maxAge = new TimeSpan(0, 0, 0, 0, 325);
        private readonly TTVProxyDevice _device;
        private AllTranslation _apires;

        public ChannelContentProvider(WebServer server, TTVProxyDevice device)
        {
            server.AddRouteUrl(ChannelContentProvider.PLAYLIST_PATH, new Action<MyWebRequest>(((ContentProvider)this).SendResponse), HttpMethod.Get);
            server.AddRouteUrl(ChannelContentProvider.PLAY_PATH, new Action<MyWebRequest>(((ContentProvider)this).Play), HttpMethod.Get);
            server.AddRouteUrl(ChannelContentProvider.FAVOURITE_ADD, new Action<MyWebRequest>(this.AddFavouriteRequest), HttpMethod.Get);
            server.AddRouteUrl(ChannelContentProvider.FAVOURITE_DEL, new Action<MyWebRequest>(this.DelFavouriteRequest), HttpMethod.Get);
            server.AddRouteUrl(ChannelContentProvider.SCREEN_PATH, new Action<MyWebRequest>(this.ScreenRequest), HttpMethod.Get);
            this._device = device;
        }

        private void ScreenRequest(MyWebRequest myWebRequest)
        {
            if (!myWebRequest.Parameters.ContainsKey("id"))
            {
                this._device.Web.Send404(myWebRequest);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(TranslationScreen.XRun(this._device.Proxy.SessionState.session, int.Parse(myWebRequest.Parameters["id"])).ToString());
                MyWebResponse response = myWebRequest.GetResponse();
                int num1 = 1;
                string str1 = this._device.Web.GetMime(".xml").ToString();
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
        }

        private void DelFavouriteRequest(MyWebRequest req)
        {
            FavouriteDel favouriteDel = FavouriteDel.Run(this._device.Proxy.SessionState.session, int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]));
            MyWebResponse response = req.GetResponse();
            int num1 = 1;
            string str1 = this._device.Web.GetMime(".xml").ToString();
            string charset1 = "utf-8";
            response.AddHeader((HttpHeader)num1, str1, charset1);
            byte[] bytes = Encoding.UTF8.GetBytes(favouriteDel.GetXState());
            int num2 = 0;
            string str2 = bytes.Length.ToString();
            string charset2 = "utf-8";
            response.AddHeader((HttpHeader)num2, str2, charset2);
            int code = 200;
            response.SendHeaders(code);
            response.GetStream().Write(bytes, 0, bytes.Length);
        }

        private void AddFavouriteRequest(MyWebRequest req)
        {
            FavouriteAdd favouriteAdd = FavouriteAdd.Run(this._device.Proxy.SessionState.session, int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]));
            MyWebResponse response = req.GetResponse();
            int num1 = 1;
            string str1 = this._device.Web.GetMime(".xml").ToString();
            string charset1 = "utf-8";
            response.AddHeader((HttpHeader)num1, str1, charset1);
            byte[] bytes = Encoding.UTF8.GetBytes(favouriteAdd.GetXState());
            int num2 = 0;
            string str2 = bytes.Length.ToString();
            string charset2 = "utf-8";
            response.AddHeader((HttpHeader)num2, str2, charset2);
            int code = 200;
            response.SendHeaders(code);
            response.GetStream().Write(bytes, 0, bytes.Length);
        }

        public void UpdateTranslations(Dictionary<string, string> req)
        {
            if (this._lastUpdate + this._maxAge < DateTime.Now)
                this._apires = AllTranslation.Run(this._device.Proxy.SessionState.session, req.ContainsKey("filter") ? req["filter"] : "all");
            if (!this._apires.state.IsSuccess())
            {
                if (this._apires.state.error == ApiError.incorrect || this._apires.state.error == ApiError.noconnect)
                {
                    do
                        ;
                    while (!this._device.Proxy.Login() || this._device.Proxy.SessionState.error == ApiError.noconnect);
                    if (!this._device.Proxy.SessionState.IsSuccess())
                        throw new Exception(this._device.Proxy.SessionState.GetXState());
                    this.UpdateTranslations(req);
                }
                throw new Exception(this._apires.state.GetXState());
            }
        }

        public List<Channel> GetChannels()
        {
            this.UpdateTranslations(new Dictionary<string, string>()
      {
        {
          "filter",
          "all"
        }
      });
            return this._apires.channels;
        }

        public override string GetPlaylist(MyWebRequest req)
        {
            if (!req.Parameters.ContainsKey("type"))
                return AllTranslation.XRun(this._device.Proxy.SessionState.session, req.Parameters.ContainsKey("filter") ? req.Parameters["filter"] : "all").ToString();
            try
            {
                this.UpdateTranslations(req.Parameters);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            Playlist playlist = Playlist.CreatePlaylist(req.Parameters["type"], Playlist.ContentType.Channel);
            List<Channel> list = req.Parameters.ContainsKey("group") ? Enumerable.ToList<Channel>(Enumerable.Where<Channel>((IEnumerable<Channel>)this._apires.channels, (Func<Channel, bool>)(channel => channel.group == req.Parameters["group"]))) : this._apires.channels;
            if (req.Parameters.ContainsKey("sort"))
            {
                string str = req.Parameters["sort"];
                if (!(str == "group"))
                {
                    if (!(str == "-group"))
                    {
                        if (!(str == "title"))
                        {
                            if (!(str == "-title"))
                            {
                                if (!(str == "id"))
                                {
                                    if (str == "-id")
                                        list = Enumerable.ToList<Channel>((IEnumerable<Channel>)Enumerable.OrderByDescending<Channel, string>((IEnumerable<Channel>)list, (Func<Channel, string>)(channel => channel.id)));
                                }
                                else
                                    list = Enumerable.ToList<Channel>((IEnumerable<Channel>)Enumerable.OrderBy<Channel, string>((IEnumerable<Channel>)list, (Func<Channel, string>)(channel => channel.id)));
                            }
                            else
                                list = Enumerable.ToList<Channel>((IEnumerable<Channel>)Enumerable.OrderByDescending<Channel, string>((IEnumerable<Channel>)list, (Func<Channel, string>)(channel => channel.name)));
                        }
                        else
                            list = Enumerable.ToList<Channel>((IEnumerable<Channel>)Enumerable.OrderBy<Channel, string>((IEnumerable<Channel>)list, (Func<Channel, string>)(channel => channel.name)));
                    }
                    else
                        list = Enumerable.ToList<Channel>((IEnumerable<Channel>)Enumerable.OrderByDescending<Channel, string>((IEnumerable<Channel>)list, (Func<Channel, string>)(channel => channel.group)));
                }
                else
                    list = Enumerable.ToList<Channel>((IEnumerable<Channel>)Enumerable.OrderBy<Channel, string>((IEnumerable<Channel>)list, (Func<Channel, string>)(channel => channel.group)));
            }
            foreach (Channel channel in list)
                playlist.AddLine((object)channel, req.Headers["host"], false, req.Parameters.ContainsKey("transcode") ? "&transcode=" + req.Parameters["transcode"] : "");
            return playlist.ToString();
        }

        public override void Play(MyWebRequest req)
        {
            GetStream getStream = GetStream.Run(this._device.Proxy.SessionState.session, int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]));
            if (!getStream.IsSuccess())
            {
                if (getStream.error != ApiError.incorrect)
                    throw new Exception(getStream.error.ToString());
                while (!this._device.Proxy.Login() && this._device.Proxy.SessionState.error == ApiError.noconnect)
                    TtvProxy.Log.WriteInfo("Попыдка авторизации на torrent-tv.ru");
                if (!this._device.Proxy.SessionState.IsSuccess())
                    throw new Exception("No authorized");
                this.Play(req);
            }
            else
            {
                TorrentStream ts = this._device.Proxy.GetTsClient(getStream.source);
                Task<string> task;
                if (ts == null)
                {
                    if (!req.Client.Connected)
                        return;
                    ts = new TorrentStream(req.Client);
                    ts.Connect();
                    task = ts.Play(getStream.source, getStream.type, 0);
                    if (task != null)
                        this._device.Proxy.AddToTsPool(ts);
                }
                else
                {
                    task = ts.GetPlayTask();
                    ts.Owner.Add(req.Client);
                }
                if (task != null && !task.IsCompleted)
                    task.Wait();
                else if (task == null)
                    throw new FileNotFoundException();
                if (string.IsNullOrEmpty(task.Result))
                {
                    this._device.Proxy.RemoveFromTsPoos(ts);
                    req.GetResponse().SendText("AceStream TimeOut");
                }
                else
                {
                    string result = task.Result;
                    string str = string.Empty;
                    try
                    {
                        str = this._device.Proxy.FindBroadcastUrl(result);
                        if (string.IsNullOrEmpty(str))
                            str = this._device.Proxy.StartBroadcastStream(result, (string)null);
                        this._device.Proxy.AddVlcBroadcastClient(str, req.Client);
                        req.GetResponse().SendStream(str, (Action<MyWebRequest>)null);
                        if (req.Client.Connected)
                            req.Client.Close();
                        if (!this._device.Proxy.StopBroadcast(str, result))
                            return;
                        ts.Disconnect(true);
                        this._device.Proxy.RemoveFromTsPoos(ts);
                    }
                    catch (Exception ex)
                    {
                        TtvProxy.Log.WriteError(ex.Message);
                        ts.Disconnect(true);
                        this._device.Proxy.RemoveFromTsPoos(ts);
                        this._device.Proxy.StopBroadcast(str, result);
                    }
                }
            }
        }
    }
}