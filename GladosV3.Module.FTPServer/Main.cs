using System.Net;
using System.Threading;
using Zhaobang.FtpServer;

namespace GladosV3.Module.FTPServer
{
    internal class Main
    {
        public static void StartFTP()
        {
            var o = new CancellationTokenSource();
            new FtpServer(new IPEndPoint(IPAddress.Any, 21), "C:\\").RunAsync(o.Token);
        }
    }
}
