// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Server.HttpMime
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.Xml.Serialization;

namespace TTVProxy.Http.Server
{
    [XmlType]
    public class HttpMime
    {
        private string extension;
        private string typeName;
        private MediaType mediaType;

        [XmlAttribute("Extension")]
        public string Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        [XmlAttribute("Name")]
        public string TypeName
        {
            get
            {
                return this.typeName;
            }
            set
            {
                this.typeName = value;
            }
        }

        [XmlAttribute("MediaType")]
        public MediaType MediaType
        {
            get
            {
                return this.mediaType;
            }
            set
            {
                this.mediaType = value;
            }
        }

        public HttpMime()
        {
        }

        public HttpMime(string extension)
        {
            this.extension = extension;
        }

        public HttpMime(string extension, string typeName, MediaType mediaType)
          : this(extension)
        {
            this.typeName = typeName;
            this.mediaType = mediaType;
        }

        public override string ToString()
        {
            switch (this.mediaType)
            {
                case MediaType.Image:
                    return "image/" + this.typeName;
                case MediaType.Audio:
                    return "audio/" + this.typeName;
                case MediaType.Video:
                    return "video/" + this.typeName;
                case MediaType.Text:
                    return "text/" + this.typeName;
                default:
                    return string.Empty;
            }
        }

        public override bool Equals(object obj)
        {
            HttpMime httpMime = obj as HttpMime;
            if (httpMime == null)
                return false;
            return this.extension == httpMime.extension;
        }

        public override int GetHashCode()
        {
            return this.extension.GetHashCode();
        }
    }
}