﻿// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.ArcGetStreem
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class ArcGetStreem : GetStream
    {
        public new static GetStream Run(string session, int channel_id)
        {
            return (GetStream)new XmlSerializer(typeof(GetStream)).Deserialize(WebRequest.Create(string.Format("{0}/v3/arc_stream.php?session={1}&record_id={2}&typeresult=xml", (object)ApiFunction.Host, (object)session, (object)channel_id)).GetResponse().GetResponseStream() ?? Stream.Null);
        }
    }
}