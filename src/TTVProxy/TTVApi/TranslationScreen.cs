// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.TranslationScreen
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class TranslationScreen
    {
        public ApiFunction state;
        [XmlArray("screens")]
        [XmlArrayItem(typeof(ChannelScreen), ElementName = "screen")]
        public List<ChannelScreen> screens;

        public static TranslationScreen Run(string session, int channel_id)
        {
            return (TranslationScreen)new XmlSerializer(typeof(TranslationScreen)).Deserialize(TranslationScreen.XRun(session, channel_id).CreateReader());
        }

        public static XDocument XRun(string session, int channel_id)
        {
            return XDocument.Load(WebRequest.Create(string.Format("{0}/v3/translation_screen.php?session={1}&channel_id={2}&typeresult=xml&count=1", (object)ApiFunction.Host, (object)session, (object)channel_id)).GetResponse().GetResponseStream() ?? Stream.Null);
        }
    }
}