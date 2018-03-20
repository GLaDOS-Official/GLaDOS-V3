using Discord.Commands;
using GladosV3.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.ImageGeneration
{
    public class GeneratorService
    {
        IDisposable typing;
        public bool fail = false;
        public GeneratorService()
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..\\Binaries\\wkhtmltoimage.exe"))) { LoggingService.Log(Discord.LogSeverity.Error, "ImageGenerator", "wkhtmltoimage.exe not found!"); fail = true; };
        }
        public Task<MemoryStream> Shit(string[] items, ICommandContext context)
        {
            typing = context.Channel.EnterTypingState();
            string item = items.Aggregate(string.Empty, (current, type) => current + type + ", ");
            string html = File.ReadAllTextAsync(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..\\images\\shit.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item.Remove(item.Length - 2)).Replace("REPLACECORRECTPLURAL",items.Length > 1 ? "are" : "is");
            var jpgBytes = Exec(html).GetAwaiter().GetResult();
            typing.Dispose();
            return Task.FromResult(new MemoryStream(jpgBytes)); 
        }
        public Task<MemoryStream> Delete(string item, ICommandContext context)
        {
            typing = context.Channel.EnterTypingState();
            string html = File.ReadAllTextAsync(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..\\images\\delete.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item);
            var jpgBytes = Exec(html,50).GetAwaiter().GetResult();
            typing.Dispose();
            return Task.FromResult(new MemoryStream(jpgBytes));
        }

        public async Task<byte[]> Exec(string html,int width = 1024, int height = 0) // Custom wrapper!!!
        {
            var e = Process.Start(new ProcessStartInfo() {Arguments = $"-q --width {width} --height {height} -f jpeg  - -", FileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "..\\Binaries\\wkhtmltoimage.exe"), RedirectStandardOutput = true, RedirectStandardInput = true });
            using (StreamWriter stream = e.StandardInput)
            {
                byte[] htmlcontent = Encoding.UTF8.GetBytes(html);
                await stream.BaseStream.WriteAsync(htmlcontent, 0, htmlcontent.Length).ConfigureAwait(false);
                await stream.WriteLineAsync().ConfigureAwait(false);
                await stream.BaseStream.FlushAsync().ConfigureAwait(false);
                stream.Close();
            }
            using (MemoryStream stream = new MemoryStream()) {
                await e.StandardOutput.BaseStream.CopyToAsync(stream);
                await e.StandardOutput.BaseStream.FlushAsync().ConfigureAwait(false);
                e.StandardOutput.Close();
                return stream.ToArray();
            }
        }
    }
}
