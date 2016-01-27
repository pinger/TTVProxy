// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.GetEpg
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class GetEpg
    {
        public ApiFunction state;
        [XmlArray("data")]
        [XmlArrayItem(typeof(Epg), ElementName = "telecast")]
        public List<Epg> data;

        public static GetEpg Run(string session, int channel_id)
        {
            return (GetEpg)new XmlSerializer(typeof(GetEpg)).Deserialize(GetEpg.XRun(session, channel_id).CreateReader());
        }

        public static XDocument XRun(string session, int channel_id)
        {
            return XDocument.Load(WebRequest.Create(string.Format("{0}/v3/translation_epg.php?session={1}&epg_id={2}&typeresult=xml", (object)ApiFunction.Host, (object)session, (object)channel_id)).GetResponse().GetResponseStream());
        }
    }
}