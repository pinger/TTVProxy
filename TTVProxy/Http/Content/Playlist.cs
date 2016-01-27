// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Content.Playlist
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.IO;
using System.Reflection;
using System.Text;
using TTVProxy;

namespace TTVProxy.Http.Content
{
    public class Playlist
    {
        private readonly StringBuilder _result = new StringBuilder();
        private ExtPlayList _templ;
        private Playlist.ContentType _type;

        private Playlist(ExtPlayList templ)
        {
            this._templ = templ;
            switch (this._type)
            {
                case Playlist.ContentType.Channel:
                    this._result.AppendLine(this._templ.Channel.Header);
                    break;
                case Playlist.ContentType.Record:
                    this._result.AppendLine(this._templ.Record.Header);
                    break;
                case Playlist.ContentType.Archive:
                    this._result.AppendLine(this._templ.Archive.Header);
                    break;
                case Playlist.ContentType.Plugin:
                    this._result.AppendLine(this._templ.Plugin.Header);
                    break;
            }
        }

        public static Playlist CreatePlaylist(string ext, Playlist.ContentType type)
        {
            string str = TtvProxy.ExeDir + "/pltempl/" + ext + ".xml";
            if (!File.Exists(str))
                str = TtvProxy.ApplicationDataFolder + "/pltempl/" + ext + ".xml";
            ExtPlayList templ = ExtPlayList.LoadPlaylist(str);
            if (templ == null)
                throw new Exception("Playlist not found");
            return new Playlist(templ)
            {
                _templ = templ,
                _type = type
            };
        }

        public void AddLine(object obj, string host, bool sublist = false, string append = "")
        {
            Type type = obj.GetType();
            string str = "";
            switch (this._type)
            {
                case Playlist.ContentType.Channel:
                    str = this._templ.Channel.Line;
                    break;
                case Playlist.ContentType.Record:
                    str = this._templ.Record.Line;
                    break;
                case Playlist.ContentType.Archive:
                    str = this._templ.Archive.Line;
                    break;
                case Playlist.ContentType.Plugin:
                    str = sublist ? this._templ.Plugin.Sublist : this._templ.Plugin.Line;
                    break;
            }
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                object obj1 = propertyInfo.GetValue(obj, (object[])null);
                if (obj1 != null)
                    str = str.Replace("{" + propertyInfo.Name.ToUpper() + "}", obj1.ToString());
            }
            this._result.AppendLine(str.Replace("{HOST}", host)
                .Replace(ExtPlayList.SPECIAL_CHAR_GT, "<").Replace(ExtPlayList.SPECIAL_CHAR_LT, "<") + append);
        }

        public override string ToString()
        {
            switch (this._type)
            {
                case Playlist.ContentType.Channel:
                    return this._result.ToString() + this._templ.Channel.Basemenet;
                case Playlist.ContentType.Record:
                    return this._result.ToString() + this._templ.Record.Basemenet;
                case Playlist.ContentType.Archive:
                    return this._result.ToString() + this._templ.Archive.Basemenet;
                case Playlist.ContentType.Plugin:
                    return this._result.ToString() + this._templ.Plugin.Basemenet;
                default:
                    return this._result.ToString();
            }
        }

        public enum ContentType
        {
            Channel,
            Record,
            Archive,
            Plugin,
        }
    }
}