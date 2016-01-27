// Decompiled with JetBrains decompiler
// Type: TTVProxy.VlcError
// Assembly: TTVProxy, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: D41C2231-1566-4087-9DBC-6F7A5F13C38C
// Assembly location: D:\Tools\ttvproxy_2030\TTVProxy.dll

using System;

namespace TTVProxy
{
    public class VlcError : Exception
    {
        public VlcError(string message) : base(message)
        {
        }
    }
}