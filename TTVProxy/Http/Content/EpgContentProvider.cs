// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Content.EpgContentProvider
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using TTVProxy;
using TTVProxy.Http.Server;
using TTVProxy.TTVApi;

namespace TTVProxy.Http.Content
{
    public class EpgContentProvider : ContentProvider
    {
        private readonly TTVProxyDevice _device;

        public EpgContentProvider(TTVProxyDevice device)
        {
            this._device = device;
            this._device.Web.AddRouteUrl("/channels/epg", new Action<MyWebRequest>(((ContentProvider)this).SendResponse), HttpMethod.Get);
        }

        public override string GetPlaylist(MyWebRequest req)
        {
            int channel_id = 0;
            if (req.Parameters.ContainsKey("id"))
                channel_id = int.Parse(req.Parameters["id"]);
            return GetEpg.XRun(this._device.Proxy.SessionState.session, channel_id).ToString();
        }

        public override void Play(MyWebRequest req)
        {
            throw new NotImplementedException();
        }
    }
}