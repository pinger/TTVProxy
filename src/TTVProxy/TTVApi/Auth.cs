// Decompiled with JetBrains decompiler
// Type: TTVProxy.TTVApi.Auth
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TTVProxy.TTVApi
{
    [XmlRoot("result")]
    public class Auth : ApiFunction
    {
        public double balance;
        public string session;
        public string UserName;
        private string xml;

        public static Auth Run(string username, string password)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Auth));
            XDocument xdocument = Auth.XRun(username, password);
            XmlReader reader = xdocument.CreateReader();
            Auth auth = (Auth)xmlSerializer.Deserialize(reader);
            xdocument.Element((XName)"result").Element((XName)"session").Remove();
            string str1 = xdocument.ToString();
            auth.xml = str1;
            string str2 = username;
            auth.UserName = str2;
            return auth;
        }

        public static XDocument XRun(string username, string password)
        {
            string str1 = Guid.NewGuid().ToString("N");
            string str2 = string.Format("{0}/v3/auth.php?username={1}&password={2}&application={3}&typeresult=xml&guid={4}",
                ApiFunction.Host, username, password, "tsproxy", str1);
            return XDocument.Load(WebRequest.Create(str2).GetResponse().GetResponseStream() ?? Stream.Null);
        }

        public override string ToString()
        {
            return this.xml;
        }
    }
}