// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.ApiFunction
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using TTVProxy;

namespace TTVProxy.TTVApi
{
    public class ApiFunction
    {
        public static readonly string Host = TtvProxy.MySettings.GetSetting("torrent-tv.ru", "host", "http://1ttvxbmc.top");
        public int success;
        public ApiError error;

        public bool IsSuccess()
        {
            return this.success == 1;
        }

        public string GetXState()
        {
            if (this.success != 1)
                return "<state><success>0</success><error>" + (object)this.error + "</error></state>";
            return "<state><success>1</success></state>";
        }
    }
}