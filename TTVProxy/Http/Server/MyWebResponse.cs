// Decompiled with JetBrains decompiler
// Type: TTVProxy.Http.Server.MyWebResponse
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using SimpleLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TTVProxy;

namespace TTVProxy.Http.Server
{
    public class MyWebResponse
    {
        private static Dictionary<int, string> _codes = new Dictionary<int, string>()
    {
      {
        100,
        "Continue"
      },
      {
        101,
        "Switching Protocols"
      },
      {
        200,
        "OK"
      },
      {
        201,
        "Created"
      },
      {
        202,
        "Accepted"
      },
      {
        203,
        "Non-Authoritative Information"
      },
      {
        204,
        "No Content"
      },
      {
        205,
        "Reset Content"
      },
      {
        206,
        "Partial Content"
      },
      {
        300,
        "Multiple Choices"
      },
      {
        301,
        "Moved Permanently"
      },
      {
        302,
        "Found"
      },
      {
        303,
        "See Other"
      },
      {
        304,
        "Not Modified"
      },
      {
        305,
        "Use Proxy"
      },
      {
        306,
        "(Unused)"
      },
      {
        307,
        "Temporary Redirect"
      },
      {
        400,
        "Bad Request"
      },
      {
        401,
        "Unauthorized"
      },
      {
        402,
        "Payment Required"
      },
      {
        403,
        "Forbidden"
      },
      {
        404,
        "Not Found"
      },
      {
        405,
        "Method Not Allowed"
      },
      {
        406,
        "Not Acceptable"
      },
      {
        407,
        "Proxy Authentication Required"
      },
      {
        408,
        "Request Timeout"
      },
      {
        409,
        "Conflict"
      },
      {
        410,
        "Gone"
      },
      {
        411,
        "Length Required"
      },
      {
        412,
        "Precondition Failed"
      },
      {
        413,
        "Request Entity Too Large"
      },
      {
        414,
        "Request-URI Too Long"
      },
      {
        415,
        "Unsupported Media Type"
      },
      {
        416,
        "Requested Range Not Satisfiable"
      },
      {
        417,
        "Expectation Failed"
      },
      {
        500,
        "Internal Server Error"
      },
      {
        501,
        "Not Implemented"
      },
      {
        502,
        "Bad Gateway"
      },
      {
        503,
        "Service Unavailable"
      },
      {
        504,
        "Gateway Timeout"
      },
      {
        505,
        "HTTP Version Not Supported"
      }
    };
        private readonly MyWebRequest _request;
        private readonly NetworkStream _stream;
        private const int _stateCode = 200;
        private readonly Dictionary<string, string> _headers;
        private bool responseSended;

        internal MyWebResponse(MyWebRequest req, NetworkStream stream)
        {
            this._request = req;
            this._stream = stream;
            this._headers = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        }

        public void AddHeader(string key, string value)
        {
            this._headers.Add(key, value);
        }

        public void AddHeader(HttpHeader header, string value, string charset = "utf-8")
        {
            switch (header)
            {
                case HttpHeader.ContentLength:
                    this.AddHeader("Content-Length", value);
                    break;
                case HttpHeader.ContentType:
                    if (value.Contains("charset="))
                    {
                        this.AddHeader("Content-Type", value);
                        break;
                    }
                    this.AddHeader("Content-Type", string.Format("{0};charset={1}", (object)value, (object)charset));
                    break;
                case HttpHeader.Server:
                    this.AddHeader("Server", value);
                    break;
                case HttpHeader.Date:
                    this.AddHeader("Date", value);
                    break;
                case HttpHeader.Connection:
                    this.AddHeader("Connection", value);
                    break;
                case HttpHeader.AcceptRanges:
                    this.AddHeader("Accept-Ranges", value);
                    break;
                case HttpHeader.TransferEncoding:
                    this.AddHeader("Transfer-Encoding", value);
                    break;
            }
        }

        public void SendHeaders(int code = 200)
        {
            if (!this._headers.ContainsKey("Server"))
                this.AddHeader(HttpHeader.Server, string.Format("{2}/{0}.{1} TTVProxy/{3}", (object)Environment.OSVersion.Version.Major, (object)Environment.OSVersion.Version.Minor, (object)Environment.OSVersion.Platform, (object)TtvProxy.Version), "utf-8");
            if (!this._headers.ContainsKey("Date"))
                this.AddHeader(HttpHeader.Date, DateTime.Now.ToString("r"), "utf-8");
            if (!this._headers.ContainsKey("Connection"))
                this.AddHeader(HttpHeader.Connection, "close", "utf-8");
            string str = MyWebResponse._codes.ContainsKey(code) ? MyWebResponse._codes[code] : "Unknown";
            byte[] bytes1 = Encoding.ASCII.GetBytes(string.Format("HTTP/1.1 {0} {1} \r\n", (object)code, (object)str));
            this._stream.Write(bytes1, 0, bytes1.Length);
            foreach (KeyValuePair<string, string> keyValuePair in this._headers)
            {
                byte[] bytes2 = Encoding.ASCII.GetBytes(string.Format("{0}: {1}\r\n", (object)keyValuePair.Key, (object)keyValuePair.Value));
                try
                {
                    this._stream.Write(bytes2, 0, bytes2.Length);
                }
                catch (Exception ex)
                {
                }
            }
            byte[] bytes3 = Encoding.ASCII.GetBytes("\r\n");
            try
            {
                this._stream.Write(bytes3, 0, bytes3.Length);
            }
            catch (Exception ex)
            {
                TtvProxy.Log.WriteError(ex.Message);
            }
        }

        public string GetState()
        {
            return "OK";
        }

        public Stream GetStream()
        {
            return (Stream)this._stream;
        }

        public void SendStream(string url, Action<MyWebRequest> SendHeaders = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                TtvProxy.Log.WriteError("Íå÷åãî ïðîèãðûâàòü");
            }
            else
            {
                Uri uri = new Uri(url, UriKind.Absolute);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                try
                {
                    socket.Connect(uri.Host, uri.Port);
                    socket.Send(Encoding.UTF8.GetBytes(string.Format("GET {0}{1}\r\nHost: {2}:{3}\r\nConnection: Keep-Alive\r\n\r\n", (object)uri.PathAndQuery, (object)" HTTP/1.1", (object)uri.Host, (object)uri.Port)));
                }
                catch (Exception ex)
                {
                    this.SendText("File not found");
                    this.responseSended = true;
                    return;
                }
                byte[] numArray = new byte[464];
                int num = 0;
                if (SendHeaders != null)
                {
                    int count = socket.Receive(numArray, 0, numArray.Length, SocketFlags.None);
                    SendHeaders(this._request);
                    string str = Encoding.ASCII.GetString(numArray, 0, count).Split(new string[1]
                    {
            "\r\n\r\n"
                    }, StringSplitOptions.RemoveEmptyEntries)[0];
                    byte[] buffer = Enumerable.ToArray<byte>(Enumerable.Skip<byte>(Enumerable.Take<byte>((IEnumerable<byte>)numArray, count), str.Length + 4));
                    this._stream.Write(buffer, 0, buffer.Length);
                }
                while (socket.Connected)
                {
                    if (this._stream.CanWrite)
                    {
                        try
                        {
                            int count = socket.Receive(numArray, 0, numArray.Length, SocketFlags.None);
                            if (count == 0)
                            {
                                Thread.Sleep(VlcClient.Cache);
                                if (num < 3)
                                    ++num;
                                else
                                    break;
                            }
                            else
                            {
                                num = 0;
                                this._stream.Write(numArray, 0, count);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                    else
                        break;
                }
                if (socket.Connected)
                    socket.Close();
                this.responseSended = true;
            }
        }

        public void SendText(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            this.AddHeader(HttpHeader.ContentType, "text/plain;charset=utf-8", "utf-8");
            this.AddHeader(HttpHeader.ContentLength, bytes.Length.ToString(), "utf-8");
            this.SendHeaders(200);
            try
            {
                this._stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                TtvProxy.Log.WriteError(ex.Message);
            }
            this.responseSended = true;
        }
    }
}