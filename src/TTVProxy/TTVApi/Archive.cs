// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.Archive
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("channel")]
    public class Archive
    {
        private string _name;
        [XmlAttribute]
        public long time;
        [XmlAttribute]
        public int screen;

        [XmlAttribute]
        public long record_id { get; set; }

        [XmlAttribute]
        public string name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        [XmlAttribute]
        public int epg_id { get; set; }

        [XmlIgnore]
        public Channel Channel { get; private set; }

        [XmlIgnore]
        public DateTime Time
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double)this.time) + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            }
        }

        [XmlIgnore]
        public bool HasScreen
        {
            get
            {
                return this.screen == 1;
            }
        }

        public void SetChannel(Channel ch)
        {
            this.Channel = ch;
        }
    }
}