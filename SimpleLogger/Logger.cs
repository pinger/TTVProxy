using System;
using System.IO;
using System.Threading;

namespace SimpleLogger
{
    public class Logger
    {
        private readonly bool _dublConsole;
        private bool _opened;
        private StreamWriter sw;

        public Logger(string path, bool console = false)
        {
            this._dublConsole = console;
            this._opened = this.OpenFile(path);
        }

        private bool OpenFile(string path)
        {
            try
            {
                if (this._opened)
                    this.Close();
                this.sw = new StreamWriter(path, true)
                {
                    AutoFlush = true
                };
                this._opened = true;
            }
            catch (Exception ex)
            {
                Random random = new Random(DateTime.Now.Millisecond);
                path = string.Concat(new object[4]
                {
          (object) Path.GetFileNameWithoutExtension(path),
          (object) "_",
          (object) random.Next((int) ushort.MaxValue),
          (object) Path.GetExtension(path)
                });
                return this.OpenFile(path);
            }
            return true;
        }

        public void Close()
        {
            StreamWriter streamWriter = this.sw;
            bool lockTaken = false;
            try
            {
                Monitor.Enter((object)streamWriter, ref lockTaken);
                this.sw.Close();
                this.sw.Dispose();
                this._opened = false;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit((object)streamWriter);
            }
        }

        public void WriteInfo(string value)
        {
            Write(value, TypeMessage.Info);
        }

        public void WriteWarning(string value)
        {
            Write(value, TypeMessage.Warning);
        }

        public void WriteError(string value)
        {
            Write(value, TypeMessage.Error);
        }

        public void Critical(string value)
        {
            Write(value, TypeMessage.Critical);
        }

        public void Write(string value, TypeMessage type)
        {
            if (this.sw == null || !this._opened)
                return;
            ThreadPool.QueueUserWorkItem((WaitCallback)(state =>
           {
               StreamWriter streamWriter = this.sw;
               bool lockTaken = false;
               try
               {
                   Monitor.Enter((object)streamWriter, ref lockTaken);
                   string str = string.Format("[{0:HH:mm:ss.fff}] {1}: {2}", DateTime.Now, type, value);
                   if (this._dublConsole)
                       Console.WriteLine(str);
                   try
                   {
                       this.sw.WriteLine(str);
                   }
                   catch
                   {
                       this.Close();
                   }
               }
               finally
               {
                   if (lockTaken)
                       Monitor.Exit((object)streamWriter);
               }
           }));
        }
    }
}