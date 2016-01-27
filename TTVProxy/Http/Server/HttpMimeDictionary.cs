// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Server.HttpMimeDictionary
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace TTVProxy.Http.Server
{
    [XmlType("MimeTypes")]
    public class HttpMimeDictionary
    {
        private Dictionary<string, HttpMime> mime = new Dictionary<string, HttpMime>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        private bool useRegistry = true;

        [XmlElement("Type")]
        public HttpMime[] Items
        {
            get
            {
                return Enumerable.ToArray<HttpMime>((IEnumerable<HttpMime>)this.mime.Values);
            }
            set
            {
                HttpMime[] httpMimeArray = value;
                StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
                this.mime = Enumerable.ToDictionary<HttpMime, string>((IEnumerable<HttpMime>)httpMimeArray, (Func<HttpMime, string>)(a => a.Extension), (IEqualityComparer<string>)ordinalIgnoreCase);
            }
        }

        [XmlAttribute("UseRegistry")]
        public bool UseRegistry
        {
            get
            {
                return this.useRegistry;
            }
            set
            {
                this.useRegistry = value;
            }
        }

        public HttpMime this[string key]
        {
            get
            {
                return this.mime[key];
            }
        }

        public void Add(HttpMime mime)
        {
            this.mime.Add(mime.Extension, mime);
        }

        public HttpMime GetValue(string key)
        {
            if (this.mime.ContainsKey(key))
                return this.mime[key];
            if (this.useRegistry)
            {
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(key);
                if (registryKey != null && registryKey.GetValue("Content Type") != null)
                {
                    string[] strArray = registryKey.GetValue("Content Type").ToString().Split(new char[1]
                    {
            '/'
                    }, 2);
                    MediaType result;
                    if (strArray.Length == 2 && Enum.TryParse<MediaType>(strArray[0], true, out result))
                        return new HttpMime(key, strArray[1], result);
                }
            }
            return (HttpMime)null;
        }

        public void SerializeMe(XmlWriter xmlWriter)
        {
            new XmlSerializer(typeof(HttpMimeDictionary)).Serialize(xmlWriter, (object)this);
        }

        public static HttpMimeDictionary GetDefaults()
        {
            HttpMimeDictionary httpMimeDictionary = new HttpMimeDictionary();
            HttpMime mime1 = new HttpMime(".asf", "x-ms-asf", MediaType.Video);
            httpMimeDictionary.Add(mime1);
            HttpMime mime2 = new HttpMime(".avi", "avi", MediaType.Video);
            httpMimeDictionary.Add(mime2);
            HttpMime mime3 = new HttpMime(".flv", "x-flv", MediaType.Video);
            httpMimeDictionary.Add(mime3);
            HttpMime mime4 = new HttpMime(".m1v", "mpeg", MediaType.Video);
            httpMimeDictionary.Add(mime4);
            HttpMime mime5 = new HttpMime(".m2v", "mpeg", MediaType.Video);
            httpMimeDictionary.Add(mime5);
            HttpMime mime6 = new HttpMime(".mkv", "x-matroska", MediaType.Video);
            httpMimeDictionary.Add(mime6);
            HttpMime mime7 = new HttpMime(".mp4", "mp4", MediaType.Video);
            httpMimeDictionary.Add(mime7);
            HttpMime mime8 = new HttpMime(".mpeg", "mpeg", MediaType.Video);
            httpMimeDictionary.Add(mime8);
            HttpMime mime9 = new HttpMime(".mpg", "mpeg", MediaType.Video);
            httpMimeDictionary.Add(mime9);
            HttpMime mime10 = new HttpMime(".webm", "webm", MediaType.Video);
            httpMimeDictionary.Add(mime10);
            HttpMime mime11 = new HttpMime(".wmv", "x-ms-wmv", MediaType.Video);
            httpMimeDictionary.Add(mime11);
            HttpMime mime12 = new HttpMime(".flac", "flac", MediaType.Audio);
            httpMimeDictionary.Add(mime12);
            HttpMime mime13 = new HttpMime(".m2a", "mpeg", MediaType.Audio);
            httpMimeDictionary.Add(mime13);
            HttpMime mime14 = new HttpMime(".mid", "midi", MediaType.Audio);
            httpMimeDictionary.Add(mime14);
            HttpMime mime15 = new HttpMime(".midi", "midi", MediaType.Audio);
            httpMimeDictionary.Add(mime15);
            HttpMime mime16 = new HttpMime(".mp2", "mpeg", MediaType.Audio);
            httpMimeDictionary.Add(mime16);
            HttpMime mime17 = new HttpMime(".mp3", "mpeg", MediaType.Audio);
            httpMimeDictionary.Add(mime17);
            HttpMime mime18 = new HttpMime(".mpa", "mpeg", MediaType.Audio);
            httpMimeDictionary.Add(mime18);
            HttpMime mime19 = new HttpMime(".mpga", "mpeg", MediaType.Audio);
            httpMimeDictionary.Add(mime19);
            HttpMime mime20 = new HttpMime(".wav", "wav", MediaType.Audio);
            httpMimeDictionary.Add(mime20);
            HttpMime mime21 = new HttpMime(".wma", "x-ms-wma", MediaType.Audio);
            httpMimeDictionary.Add(mime21);
            HttpMime mime22 = new HttpMime(".bmp", "bmp", MediaType.Image);
            httpMimeDictionary.Add(mime22);
            HttpMime mime23 = new HttpMime(".gif", "gif", MediaType.Image);
            httpMimeDictionary.Add(mime23);
            HttpMime mime24 = new HttpMime(".jpe", "jpeg", MediaType.Image);
            httpMimeDictionary.Add(mime24);
            HttpMime mime25 = new HttpMime(".jpeg", "jpeg", MediaType.Image);
            httpMimeDictionary.Add(mime25);
            HttpMime mime26 = new HttpMime(".jpg", "jpeg", MediaType.Image);
            httpMimeDictionary.Add(mime26);
            HttpMime mime27 = new HttpMime(".png", "png", MediaType.Image);
            httpMimeDictionary.Add(mime27);
            HttpMime mime28 = new HttpMime(".tif", "tiff", MediaType.Image);
            httpMimeDictionary.Add(mime28);
            HttpMime mime29 = new HttpMime(".tiff", "tiff", MediaType.Image);
            httpMimeDictionary.Add(mime29);
            HttpMime mime30 = new HttpMime(".m3u", "mpegurl", MediaType.Other);
            httpMimeDictionary.Add(mime30);
            HttpMime mime31 = new HttpMime(".xml", "xml", MediaType.Text);
            httpMimeDictionary.Add(mime31);
            HttpMime mime32 = new HttpMime(".html", "html", MediaType.Text);
            httpMimeDictionary.Add(mime32);
            return httpMimeDictionary;
        }
    }
}