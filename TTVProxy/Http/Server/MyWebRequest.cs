// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Server.MyWebRequest
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace TTVProxy.Http.Server
{
    public class MyWebRequest
    {
        private Dictionary<string, string> headers;
        private Dictionary<string, string> urlParams;
        private HttpMethod method;
        private string version;
        private string url;
        private NetworkStream stream;
        private string[] _soapOutParams;
        private string _soapService;
        private string _soapAction;

        public TcpClient Client { get; private set; }

        public string Url
        {
            get
            {
                return this.url;
            }
        }

        public HttpMethod Method
        {
            get
            {
                return this.method;
            }
        }

        public Dictionary<string, string> Headers
        {
            get
            {
                return this.headers;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                return this.urlParams;
            }
        }

        public string QueryString { get; private set; }

        public string[] SoapOutParams
        {
            get
            {
                return this._soapOutParams;
            }
        }

        public object SoapAction
        {
            get
            {
                return (object)this._soapAction;
            }
        }

        public string SoapService
        {
            get
            {
                return this._soapService;
            }
        }

        public string[] SoapOutParam
        {
            get
            {
                return this._soapOutParams;
            }
        }

        public static MyWebRequest Create(TcpClient client)
        {
            MyWebRequest myWebRequest = new MyWebRequest()
            {
                headers = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase),
                urlParams = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase),
                Client = client,
                stream = client.GetStream(),
                QueryString = string.Empty
            };
            StringBuilder stringBuilder = new StringBuilder(128);
            bool flag1 = false;
            bool flag2 = true;
            int num1;
            while ((num1 = myWebRequest.stream.ReadByte()) >= 0)
            {
                if (num1 == 10 & flag1)
                {
                    string str1 = stringBuilder.ToString(0, stringBuilder.Length - 1);
                    if (!(str1 == string.Empty))
                    {
                        stringBuilder.Clear();
                        if (flag2)
                        {
                            string[] strArray1 = str1.Split(new char[1]
                            {
                ' '
                            }, 2, StringSplitOptions.RemoveEmptyEntries);
                            myWebRequest.method = (HttpMethod)Enum.Parse(typeof(HttpMethod), strArray1[0], true);
                            int length = strArray1[1].LastIndexOf(' ');
                            myWebRequest.version = strArray1[1].Substring(length + 1);
                            string[] strArray2 = HttpUtility.UrlDecode(strArray1[1].Substring(0, length)).Split(new char[1]
                            {
                '?'
                            }, 2, StringSplitOptions.RemoveEmptyEntries);
                            myWebRequest.url = strArray2[0];
                            if (strArray2.Length > 1)
                                myWebRequest.QueryString = strArray2[1];
                            if (strArray2.Length == 2)
                            {
                                string str2 = strArray2[1];
                                char[] separator1 = new char[1]
                                {
                  '&'
                                };
                                int num2 = 1;
                                foreach (string str3 in str2.Split(separator1, (StringSplitOptions)num2))
                                {
                                    char[] separator2 = new char[1]
                                    {
                    '='
                                    };
                                    int count = 2;
                                    int num3 = 1;
                                    string[] strArray3 = str3.Split(separator2, count, (StringSplitOptions)num3);
                                    myWebRequest.urlParams[strArray3[0]] = strArray3.Length == 2 ? strArray3[1] : string.Empty;
                                }
                            }
                            flag2 = false;
                        }
                        else
                        {
                            string[] strArray = str1.Split(new char[1]
                            {
                ':'
                            }, 2, StringSplitOptions.RemoveEmptyEntries);
                            myWebRequest.headers[strArray[0].Trim()] = strArray.Length > 1 ? strArray[1].Trim() : string.Empty;
                        }
                        flag1 = false;
                    }
                    else
                        break;
                }
                else if (num1 == 13)
                {
                    stringBuilder.Append((char)num1);
                    flag1 = true;
                }
                else
                {
                    stringBuilder.Append((char)num1);
                    flag1 = false;
                }
            }
            return myWebRequest;
        }

        public int GetLength()
        {
            return int.Parse(this.headers["Content-Length"]);
        }

        public MemoryStream GetContent()
        {
            byte[] buffer = new byte[this.GetLength()];
            int offset = 0;
            int num;
            while ((num = this.stream.Read(buffer, offset, buffer.Length - offset)) > 0)
            {
                offset += num;
                if (offset >= buffer.Length)
                    break;
            }
            return new MemoryStream(buffer, 0, buffer.Length);
        }

        public void SetSoap(string soapAction, string soapService, string[] soapOutParam)
        {
            this._soapAction = soapAction;
            this._soapService = soapService;
            this._soapOutParams = soapOutParam;
        }

        public MyWebResponse GetResponse()
        {
            return new MyWebResponse(this, this.stream);
        }
    }
}