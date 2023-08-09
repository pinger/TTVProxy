// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.Channel
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("channel")]
    [Serializable]
    public class Channel
    {
        private string _icon;
        [XmlAttribute]
        public string epg_id;
        [XmlAttribute]
        public ChannelsType type;
        [XmlAttribute]
        public string source;
        [XmlAttribute]
        public AccessTranslation access_translation;
        [XmlAttribute]
        public int access_user;
        private Category _category;

        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string group { get; set; }

        [XmlIgnore]
        public string group_name
        {
            get
            {
                if (this._category == null)
                    return this.group;
                return this._category.name;
            }
        }

        [XmlAttribute]
        public string logo
        {
            get
            {
                return this._icon;
            }
            set
            {
                this._icon = "http://torrent-tv.ru/uploads/" + value;
            }
        }

        [XmlIgnore]
        public bool Access
        {
            get
            {
                return this.access_user == 1;
            }
        }

        public int GetId()
        {
            return int.Parse(this.id);
        }

        public int GetGroup()
        {
            return int.Parse(this.group);
        }

        public void SetCategory(Category cat)
        {
            this._category = cat;
        }

        public StreamSource GetSource()
        {
            return (StreamSource)Enum.Parse(typeof(StreamSource), this.source, true);
        }

        public int GetEpgId()
        {
            if (!string.IsNullOrEmpty(this.epg_id))
                return int.Parse(this.epg_id);
            return 0;
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}