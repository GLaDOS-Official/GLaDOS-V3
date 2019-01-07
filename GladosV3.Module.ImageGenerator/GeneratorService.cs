using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GladosV3.Services;

namespace GladosV3.Module.ImageGeneration
{
    public class GeneratorService
    {
        IDisposable typing;
        public bool fail;
        public GeneratorService()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Binaries\\wkhtmltoimage.exe"))) { LoggingService.Log(LogSeverity.Error, "ImageGenerator", "wkhtmltoimage.exe not found!"); fail = true; };
        }
        public Task<MemoryStream> Shit(string[] items, ICommandContext context)
        {
            typing = context.Channel.EnterTypingState();
            string item = items.Aggregate(string.Empty, (current, type) => current + type + ", ");
            string html = File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                "..\\images\\shit.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item.Remove(item.Length - 2)).Replace("REPLACECORRECTPLURAL",items.Length > 1 ? "are" : "is");
            var jpgBytes = Exec(html).GetAwaiter().GetResult();
            typing.Dispose();
            return Task.FromResult(new MemoryStream(jpgBytes)); 
        }
        public Task<MemoryStream> Delete(string item, ICommandContext context)
        {
            typing = context.Channel.EnterTypingState();
            string html = File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                "..\\images\\delete.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item);
            var jpgBytes = Exec(html).GetAwaiter().GetResult();
            typing.Dispose();
            return Task.FromResult(new MemoryStream(jpgBytes));
        }
        public Task<MemoryStream> MinecraftAchivementGet(string text, ICommandContext context)
        {
            typing = context.Channel.EnterTypingState();
            var randoms = new[] {29,20,1,21,13,18,17,9,31,22,23,2,11,19,24,25,14,12,33,34,32,3,35,37,38,7,10,39,4,5,30,8,16,28 };
            HttpClient hc = new HttpClient();
            byte[] jpgBytes = hc.GetByteArrayAsync($"https://www.minecraftskinstealer.com/achievement/a.php?i={randoms[new Random().Next(randoms.Length)]}&h=Achievement+Get%21&t={text}").GetAwaiter().GetResult();
            typing.Dispose();
            return Task.FromResult(new MemoryStream(jpgBytes));
        }
        public async Task<byte[]> Exec(string html, int width = 0, int height = 0) // Custom wrapper!!!
        {
            var e = Process.Start(new ProcessStartInfo
<<<<<<< HEAD
            {
                Arguments = $"-q --width {width} --height {height} -f jpeg  - -",
                FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "..\\Binaries\\wkhtmltoimage.exe"),
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            });
=======
            {Arguments = $"-q --width {width} --height {height} -f jpeg  - -", FileName = Path.Combine(Directory.GetCurrentDirectory(),
                "..\\Binaries\\wkhtmltoimage.exe"), RedirectStandardOutput = true, RedirectStandardInput = true });
>>>>>>> 15c8c1a0bfe431ef18b6d59b1b1b4a5255cbeb05
            using (StreamWriter stream = e.StandardInput)
            {
                byte[] htmlcontent = Encoding.UTF8.GetBytes(html);
                await stream.BaseStream.WriteAsync(htmlcontent, 0, htmlcontent.Length).ConfigureAwait(false);
                await stream.WriteLineAsync().ConfigureAwait(false);
                await stream.BaseStream.FlushAsync().ConfigureAwait(false);
                stream.Close();
            }
            using (MemoryStream stream = new MemoryStream())
            {
                await e.StandardOutput.BaseStream.CopyToAsync(stream);
                await e.StandardOutput.BaseStream.FlushAsync().ConfigureAwait(false);
                e.StandardOutput.Close();
                return stream.ToArray();
            }
        }

    }
}
