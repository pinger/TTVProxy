// Decompiled with JetBrains decompiler
// Type: TTVProxy.ExtPlayList
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TTVProxy
{
    public class ExtPlayList
    {
        public static readonly string PARAM_NAME = "{NAME}";
        public static readonly string PARAM_GROUP_NAME = "{GROUP_NAME}";
        public static readonly string PARAM_HOST = "{HOST}";
        public static readonly string PARAM_GROUP_ID = "{GROUP_ID}";
        public static readonly string PARAM_CHANNEL_ID = "{CHANNEL_ID}";
        public static readonly string PARAM_CONTENT_TYPE = "{CONTENT_TYPE}";
        public static readonly string PARAM_CURRENT_EPG = "{CURRENT_EPG}";
        public static readonly string PARAM_ID = "{ID}";
        public static readonly string PARAM_TIME = "{TIME}";
        public static readonly string PARAM_MINUTE = "{MINUTE}";
        public static readonly string PARAM_FILENAME = "{FILE_NAME}";
        public static readonly string PARAM_NUM = "{NUM}";
        public static readonly string PARAM_SCREEN = "{SCREEN}";
        public static readonly string SPECIAL_CHAR_AMP = "&amp;";
        public static readonly string SPECIAL_CHAR_LT = "&lt;";
        public static readonly string SPECIAL_CHAR_GT = "&gt;";
        public static readonly string CONST_NONE = "None";
        public static readonly string PARAM_EPGID = "{EPG_ID}";
        public static readonly string PARAM_RECORD_ID = "{RECORD_ID}";
        public static readonly string PARAM_CHANNEL_LOGO = "{CHANNEL_LOGO}";
        public string Format;
        public ExtPlayList.OutTypes[] Out;
        public string Ext;
        public string[] Icon;

        public PlaylistTempate Channel { get; private set; }

        public PlaylistTempate Archive { get; private set; }

        public PlaylistTempate Record { get; private set; }

        public PlaylistTempate Plugin { get; private set; }

        private ExtPlayList()
        {
        }

        public static ExtPlayList LoadPlaylist(string fname)
        {
            if (!File.Exists(fname))
                return (ExtPlayList)null;
            ExtPlayList extPlayList = new ExtPlayList();
            XElement xelement1 = XDocument.Load(fname).Element((XName)"pltempl");
            if (xelement1 == null)
                return (ExtPlayList)null;
            XElement xelement2 = xelement1.Element((XName)"manifest");
            if (xelement2 == null)
                return (ExtPlayList)null;
            XElement xelement3 = xelement2.Element((XName)"format");
            if (xelement3 == null)
                return (ExtPlayList)null;
            extPlayList.Format = xelement3.Value.Replace(ExtPlayList.SPECIAL_CHAR_AMP, "&").Replace(ExtPlayList.SPECIAL_CHAR_GT, ">").Replace(ExtPlayList.SPECIAL_CHAR_LT, "<");
            XElement xelement4 = xelement2.Element((XName)"out");
            if (xelement4 == null)
                return (ExtPlayList)null;
            string str1 = xelement4.Value.Replace(" ", "");
            if (str1 == string.Empty)
                return (ExtPlayList)null;
            string[] strArray1 = str1.Split(Enumerable.ToArray<char>((IEnumerable<char>)","), StringSplitOptions.RemoveEmptyEntries);
            extPlayList.Out = new ExtPlayList.OutTypes[strArray1.Length];
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (strArray1[index].Length < 3)
                    return (ExtPlayList)null;
                extPlayList.Out[index] = (ExtPlayList.OutTypes)Enum.Parse(typeof(ExtPlayList.OutTypes), strArray1[index], true);
            }
            ExtPlayList.OutTypes[] outTypesArray = extPlayList.Out;
            Func<ExtPlayList.OutTypes, bool> func = (Func<ExtPlayList.OutTypes, bool>)(o => o == ExtPlayList.OutTypes.File);
            //Func<ExtPlayList.OutTypes, bool> predicate;
            //if (Enumerable.Any<ExtPlayList.OutTypes>((IEnumerable<ExtPlayList.OutTypes>) outTypesArray, predicate))
            if (Enumerable.Any<ExtPlayList.OutTypes>((IEnumerable<ExtPlayList.OutTypes>)outTypesArray, func))
            {
                XElement xelement5 = xelement2.Element((XName)"ext");
                if (xelement5 == null)
                    return (ExtPlayList)null;
                extPlayList.Ext = xelement5.Value;
            }
            XElement xelement6 = xelement2.Element((XName)"icon");
            string str2 = string.Empty;
            if (xelement6 != null)
                str2 = xelement6.Value.Replace(" ", "");
            extPlayList.Icon = new string[strArray1.Length];
            for (int index = 0; index < extPlayList.Icon.Length; ++index)
                extPlayList.Icon[index] = ExtPlayList.CONST_NONE;
            string[] strArray2 = str2.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string directoryName = Path.GetDirectoryName(fname);
            for (int index = 0; index < strArray2.Length && index < extPlayList.Out.Length; ++index)
                extPlayList.Icon[index] = strArray2[index] == string.Empty || strArray2[index] == "None" ? ExtPlayList.CONST_NONE : directoryName + "/" + strArray2[index];
            XElement element1 = xelement1.Element((XName)"channels");
            if (element1 != null)
                extPlayList.Channel = ExtPlayList.GetTemplate(element1);
            XElement element2 = xelement1.Element((XName)"archive");
            if (element2 != null)
                extPlayList.Archive = ExtPlayList.GetTemplate(element2);
            XElement element3 = xelement1.Element((XName)"records");
            if (element3 != null)
                extPlayList.Record = ExtPlayList.GetTemplate(element3);
            XElement element4 = xelement1.Element((XName)"plugin");
            if (element4 != null)
                extPlayList.Plugin = ExtPlayList.GetTemplate(element4);
            return extPlayList;
        }

        private static PlaylistTempate GetTemplate(XElement element)
        {
            XElement xelement1 = element.Element((XName)"header");
            XElement xelement2 = element.Element((XName)"lines");
            XElement xelement3 = element.Element((XName)"sublist");
            XElement xelement4 = element.Element((XName)"basement");
            try
            {
                PlaylistTempate playlistTempate = new PlaylistTempate();
                if (xelement1 != null)
                {
                    playlistTempate.Header = xelement1.Value.Replace(ExtPlayList.SPECIAL_CHAR_AMP, "&").Replace(ExtPlayList.SPECIAL_CHAR_GT, ">").Replace(ExtPlayList.SPECIAL_CHAR_LT, "<");
                    playlistTempate.Header = ExtPlayList.ConvertNewLineString(playlistTempate.Header);
                }
                if (xelement2 != null)
                {
                    playlistTempate.Line = xelement2.Value.Replace(ExtPlayList.SPECIAL_CHAR_AMP, "&").Replace(ExtPlayList.SPECIAL_CHAR_GT, ">").Replace(ExtPlayList.SPECIAL_CHAR_LT, "<");
                    playlistTempate.Line = ExtPlayList.ConvertNewLineString(playlistTempate.Line);
                }
                if (xelement3 != null)
                {
                    playlistTempate.Sublist = xelement3.Value.Replace(ExtPlayList.SPECIAL_CHAR_AMP, "&").Replace(ExtPlayList.SPECIAL_CHAR_GT, ">").Replace(ExtPlayList.SPECIAL_CHAR_LT, "<");
                    playlistTempate.Sublist = ExtPlayList.ConvertNewLineString(playlistTempate.Sublist);
                }
                if (xelement4 != null)
                {
                    playlistTempate.Basemenet = xelement4.Value.Replace(ExtPlayList.SPECIAL_CHAR_AMP, "&").Replace(ExtPlayList.SPECIAL_CHAR_GT, ">").Replace(ExtPlayList.SPECIAL_CHAR_LT, "<");
                    playlistTempate.Basemenet = ExtPlayList.ConvertNewLineString(playlistTempate.Basemenet);
                }
                return playlistTempate;
            }
            catch (Exception ex)
            {
                return new PlaylistTempate();
            }
        }

        public static string ConvertNewLineString(string value)
        {
            if (!value.Contains("\r\n") && value.Contains("\n"))
                return value.Replace("\n", Environment.NewLine);
            return value;
        }

        public enum OutTypes
        {
            Web,
            File,
            Auto,
        }
    }
}