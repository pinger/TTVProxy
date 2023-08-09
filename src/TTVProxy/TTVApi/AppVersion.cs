// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.AppVersion
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class AppVersion : ApiFunction
    {
        public int support;

        public bool IsSupport
        {
            get
            {
                return this.support == 1;
            }
        }

        public static AppVersion Run(string version)
        {
            return (AppVersion)new XmlSerializer(typeof(AppVersion)).Deserialize(WebRequest.Create(string.Format("{0}/v3/version.php?application=tsproxy&version={1}", (object)ApiFunction.Host, (object)version)).GetResponse().GetResponseStream() ?? Stream.Null);
        }
    }
}