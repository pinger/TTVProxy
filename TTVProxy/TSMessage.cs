// Decompiled with JetBrains decompiler
// Type: TTVProxy.TSMessage
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace TTVProxy
{
    public struct TSMessage
    {
        public string Type;
        public string Text;
        public Dictionary<string, string> Parameters;

        public static TSMessage Construct(string msg)
        {
            TSMessage tsMessage = new TSMessage()
            {
                Text = msg
            };
            string[] strArray1 = msg.Split(Enumerable.ToArray<char>((IEnumerable<char>)" "), StringSplitOptions.RemoveEmptyEntries);
            tsMessage.Type = strArray1[0];
            if (tsMessage.Type == TorrentStream.MSG_HELLOTS)
            {
                if (Enumerable.Count<string>((IEnumerable<string>)strArray1) == 2)
                {
                    tsMessage.Parameters = new Dictionary<string, string>();
                    string[] strArray2 = strArray1[1].Split("=".ToCharArray(), StringSplitOptions.None);
                    tsMessage.Parameters.Add(strArray2[0], strArray2[1]);
                }
            }
            else if (tsMessage.Type == TorrentStream.MSG_START)
            {
                tsMessage.Parameters = new Dictionary<string, string>()
        {
          {
            "url",
            strArray1[1].Split("=".ToCharArray())[1].Replace("%3A", ":")
          }
        };
                for (int index = 2; index < strArray1.Length; ++index)
                {
                    string[] strArray2 = strArray1[index].Split("=".ToCharArray());
                    tsMessage.Parameters.Add(strArray2[0], strArray2[1]);
                }
            }
            return tsMessage;
        }
    }
}