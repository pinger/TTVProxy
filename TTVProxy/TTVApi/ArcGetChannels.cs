// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.ArcGetChannels
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class ArcGetChannels
    {
        public ApiFunction state;
        [XmlArray("channels")]
        [XmlArrayItem(typeof(Channel), ElementName = "channel")]
        public List<Channel> channels;

        public static ArcGetChannels Run(string session)
        {
            ArcGetChannels arcGetChannels = (ArcGetChannels)new XmlSerializer(typeof(ArcGetChannels)).Deserialize(ArcGetChannels.XRun(session).CreateReader());
            arcGetChannels.channels.RemoveAll((Predicate<Channel>)(channel => string.IsNullOrEmpty(channel.id)));
            return arcGetChannels;
        }

        public static XDocument XRun(string session)
        {
            return XDocument.Load(WebRequest.Create(string.Format("{0}/v3/arc_list.php?session={1}&typeresult=xml", (object)ApiFunction.Host, (object)session)).GetResponse().GetResponseStream());
        }
    }
}