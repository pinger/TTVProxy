// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.ArcGetRecords
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class ArcGetRecords
    {
        public ApiFunction state;
        [XmlArray("records")]
        [XmlArrayItem(typeof(Archive), ElementName = "channel")]
        public List<Archive> records;

        public static ArcGetRecords Run(string session, DateTime date, int channel_id = 0)
        {
            return (ArcGetRecords)new XmlSerializer(typeof(ArcGetRecords)).Deserialize(ArcGetRecords.XRun(session, date, channel_id).CreateReader());
        }

        public void SetChannel(List<Channel> channels)
        {
            this.records.ForEach((Action<Archive>)(archive => archive.SetChannel(Enumerable.FirstOrDefault<Channel>((IEnumerable<Channel>)channels, (Func<Channel, bool>)(channel => channel.GetEpgId() == archive.epg_id)))));
        }

        public static XDocument XRun(string session, DateTime date, int channel_id = 0)
        {
            return XDocument.Load(WebRequest.Create(string.Format("{0}/v3/arc_records.php?session={1}&epg_id={2}&date={3}&typeresult=xml", (object)ApiFunction.Host, (object)session, (object)(channel_id == 0 ? "all" : channel_id.ToString()), (object)date.ToString("d-M-yyyy"))).GetResponse().GetResponseStream() ?? Stream.Null);
        }
    }
}