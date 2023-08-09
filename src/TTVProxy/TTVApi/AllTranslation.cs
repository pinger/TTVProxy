// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.AllTranslation
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class AllTranslation
    {
        public ApiFunction state;
        [XmlArray("categories")]
        [XmlArrayItem(typeof(Category), ElementName = "category")]
        public List<Category> categories;
        [XmlArray("channels")]
        [XmlArrayItem(typeof(Channel), ElementName = "channel")]
        public List<Channel> channels;

        public static AllTranslation Run(string session, string type)
        {
            AllTranslation res = (AllTranslation)new XmlSerializer(typeof(AllTranslation)).Deserialize(AllTranslation.XRun(session, type).CreateReader());
            res.channels.RemoveAll((Predicate<Channel>)(channel =>
           {
               if (!string.IsNullOrEmpty(channel.id))
                   return !channel.Access;
               return true;
           }));
            res.channels.ForEach((Action<Channel>)(channel =>
           {
               if (string.IsNullOrEmpty(channel.epg_id))
                   channel.epg_id = "0";
               channel.SetCategory(Enumerable.FirstOrDefault<Category>((IEnumerable<Category>)res.categories, (Func<Category, bool>)(cat => cat.id == channel.GetGroup())));
           }));
            return res;
        }

        public static XDocument XRun(string session, string type)
        {
            return XDocument.Load(WebRequest.Create(string.Format("{0}/v3/translation_list.php?session={1}&type={2}&typeresult=xml", (object)ApiFunction.Host, (object)session, (object)type)).GetResponse().GetResponseStream());
        }
    }
}