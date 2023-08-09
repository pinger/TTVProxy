// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.Category
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("category", ElementName = "category")]
    public class Category
    {
        [XmlAttribute]
        public int id;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public int position;
        [XmlAttribute]
        public int adult;

        public static string GetApiUrl(string session, string type)
        {
            string res = string.Format("http://api.torrent-tv.ru/v2_alltranslation.php?session={0}&type={1}&typeresult=xml", session, type);
            return res;
        }
    }
}