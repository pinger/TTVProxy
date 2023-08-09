// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.ChannelScreen
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("screen")]
    public struct ChannelScreen
    {
        public int id;
        public int num;
        public string filename;

        public static string GetApiUrl(string session, int epg_id)
        {
            return string.Format("http://api.torrent-tv.ru/v2_translation_screen.php?session={0}&typeresult=xml&channel_id={1}", (object)session, (object)epg_id);
        }
    }
}