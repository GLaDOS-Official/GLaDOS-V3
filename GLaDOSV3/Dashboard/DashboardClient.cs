using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;

namespace GLaDOSV3.Dashboard
{
    internal class DashboardClient
    {
        private static readonly NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "GLaDOS_Dashboard", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
        private static readonly UTF8Encoding streamEncoding = new UTF8Encoding();
        public static void Connect()
        {
            pipeClient.Connect();
            var read = ReadString();
            WriteString(read);
        }
        public static int WriteString(string outString)
        {
            var outBuffer = streamEncoding.GetBytes(outString);
            var len = outBuffer.Length;
            if (len > ushort.MaxValue) len = (int)ushort.MaxValue;
            var info = new[] { (byte)(len / 256), (byte)(len & 255) };
            var newArray = new byte[info.Length + outBuffer.Length];
            Array.Copy(info, newArray, info.Length);
            Array.Copy(outBuffer, 0, newArray, info.Length, outBuffer.Length);
            pipeClient.Write(newArray, 0, info.Length + outBuffer.Length);
            pipeClient.Flush();
            return info.Length + outBuffer.Length;
        }
        public static string ReadString()
        {
            int len;
            len = pipeClient.ReadByte() * 256;
            len += pipeClient.ReadByte();
            var inBuffer = new byte[len];
            pipeClient.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }
    }
}
