using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;

namespace GLaDOSV3.Dashboard
{
    class DashboardClient
    {
        private static NamedPipeClientStream pipeClient;
        private static UTF8Encoding streamEncoding = new UTF8Encoding();
        public static void Connect()
        {
            pipeClient = new NamedPipeClientStream(".", "GLaDOS_Dashboard", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            pipeClient.Connect();
            var read = ReadString();
        }
        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int    len       = outBuffer.Length;
            if (len > ushort.MaxValue) len = (int)ushort.MaxValue;
            pipeClient.WriteByte((byte)(len / 256));
            pipeClient.WriteByte((byte)(len & 255));
            pipeClient.Write(outBuffer, 0, len);
            pipeClient.Flush();

            return outBuffer.Length + 2;
        }
        public static string ReadString()
        {
            int len;
            len =  pipeClient.ReadByte() * 256;
            len += pipeClient.ReadByte();
            var inBuffer = new byte[len];
            pipeClient.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }
    }
}
