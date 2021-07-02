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
            WriteString(read);
        }
        public static int WriteString(string outString)
        {
            byte[] outBuffer               = streamEncoding.GetBytes(outString);
            int    len                     = outBuffer.Length;
            if (len > ushort.MaxValue) len = (int)ushort.MaxValue;
            byte[] info                    = new[] { (byte) (len / 256), (byte) (len & 255) };
            byte[] newArray                = new byte[info.Length + outBuffer.Length];
            Array.Copy(info, newArray, info.Length);
            Array.Copy(outBuffer, 0, newArray, info.Length, outBuffer.Length);
            pipeClient.Write(newArray, 0, info.Length + outBuffer.Length);
            pipeClient.Flush();
            return info.Length + outBuffer.Length;
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
