// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Content.ContentProvider
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using TTVProxy.Http.Server;

namespace TTVProxy.Http.Content
{
    public abstract class ContentProvider
    {
        public void SendResponse(MyWebRequest req)
        {
            string text;
            try
            {
                text = this.GetPlaylist(req);
            }
            catch (Exception ex)
            {
                text = ex.Message;
            }
            req.GetResponse().SendText(text);
        }

        public abstract string GetPlaylist(MyWebRequest req);

        public abstract void Play(MyWebRequest req);
    }
}