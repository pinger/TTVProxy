// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.Epg
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("telecast")]
    public class Epg
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public int channel_id;
        [XmlAttribute]
        public int btime;
        [XmlAttribute]
        public int etime;

        public DateTime StartTime
        {
            get
            {
                return Epg.UTSToDateTime((double)this.btime);
            }
            set
            {
                this.btime = Epg.DateTimeToUts(value);
            }
        }

        public DateTime EndTime
        {
            get
            {
                return Epg.UTSToDateTime((double)this.etime);
            }
            set
            {
                this.etime = Epg.DateTimeToUts(value);
            }
        }

        private static int DateTimeToUts(DateTime date)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)(date - dateTime).TotalSeconds;
        }

        private static DateTime UTSToDateTime(double unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}