// Decompiled with JetBrains decompiler
// Type: TTVProxy.VlcConnectError
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System.Net.Sockets;

namespace TTVProxy
{
    public class VlcConnectError : VlcError
    {
        public VlcConnectError() : base(SocketError.ConnectionRefused.ToString())
        {
        }
    }
}