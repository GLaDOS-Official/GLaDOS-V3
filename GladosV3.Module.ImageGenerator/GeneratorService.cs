using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GladosV3.Services;

namespace GladosV3.Module.ImageGeneration
{
    public class GeneratorService
    {
        IDisposable typing;
        public bool fail;
        private const string htmlSplit = " ";
        public GeneratorService()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Binaries\\wkhtmltoimage.exe"))) { LoggingService.Log(LogSeverity.Error, "ImageGenerator", "wkhtmltoimage.exe not found!"); fail = true; };
        }
        private static string AppendAtPosition(string baseString, int position, string character)
        {
            var sb = new StringBuilder(baseString);
            for (int i = position; i < sb.Length; i += (position + character.Length))
                sb.Insert(i, character);
            return sb.ToString();
        }
        public Task<MemoryStream> Shit(string[] items, ICommandContext context)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                string item = items.Aggregate(string.Empty, (current, type) => current + type + ", ");
                string html = File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                    "images\\shit.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item.Remove(item.Length - 2)).Replace("REPLACECORRECTPLURAL", items.Length > 1 ? "are" : "is");
                var jpgBytes = Exec(html).GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Delete(string item, ICommandContext context)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                string html = File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                    "images\\delete.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item);
                var jpgBytes = Exec(html).GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> MinecraftAchivementGet(string text, ICommandContext context)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                var randoms = new[] { 29, 20, 1, 21, 13, 18, 17, 9, 31, 22, 23, 2, 11, 19, 24, 25, 14, 12, 33, 34, 32, 3, 35, 37, 38, 7, 10, 39, 4, 5, 30, 8, 16, 28 };
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://www.minecraftskinstealer.com/achievement/a.php?i={randoms[new Random().Next(randoms.Length)]}&h=Achievement+Get%21&t={text}").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Threats(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={(url.Replace(".gif", ".png"))}&type=threats&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Baguette(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={(url.Replace(".gif", ".png"))}&type=baguette&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Clyde(ICommandContext context, string clyde)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?text={clyde}&type=clyde&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Relationship(ICommandContext context, IUser user2)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?user1={(context.Message.Author.GetAvatarUrl().Replace(".gif", ".png"))}&user2={(user2.GetAvatarUrl().Replace(".gif", ".png"))}&type=ship&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Captcha(ICommandContext context, string url, string username)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={url}&username={username}&type=captcha&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> WhoWouldWin(ICommandContext context, IUser user2)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?user1={(context.Message.Author.GetAvatarUrl().Replace(".gif", ".png"))}&user2={(user2.GetAvatarUrl().Replace(".gif", ".png"))}&type=whowouldwin&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> ChangeMyMind(ICommandContext context, string cmm)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                if (cmm.Length >= 100) cmm = cmm.Substring(0, 100);
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?text={cmm}&type=changemymind&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Jpegify(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={(url.Replace(".gif", ".png"))}&type=jpeg&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Lolice(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={(url.Replace(".gif", ".png"))}&type=lolice&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Kannagen(ICommandContext context, string text)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                const int splitPerChar = 10;
                const int splitPerUpperChar = 8;
                if (text.Length >= 45) text = text.Substring(0, 45);

                string[] split = text.Split(' ');
                for (int i = 0; i < split.Length; i++)
                {
                    int splitNum = char.IsUpper(split[i][split[i].Length - 1]) ? splitPerUpperChar : splitPerChar;
                    if (split[i].Length < splitNum) continue;
                    for (int j = splitNum;  j < split[i].Length; j += splitNum) split[i] = split[i].Insert(j, htmlSplit);
                }
                string result = string.Join(' ', split);

                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?text={result}&type=kannagen&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> IPhoneX(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={(url.Replace(".gif", ".png"))}&type=iphonex&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Trap(ICommandContext context, IUser user)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?image={(user.GetAvatarUrl(size: 1024).Replace(".gif", ".png"))}&author={context.User.Username}&name={user.Username}&type=trap&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Trump(ICommandContext context, string trump)
        {
            try
            {
                if (trump.Length > 33) trump = trump.Insert(34, htmlSplit);
                if (trump.Length >= 72) trump = trump.Substring(0, 72);
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?text={trump}&type=trumptweet&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Deepfry(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?image={(url.Replace(".gif", ".png"))}&type=deepfry&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Magik(ICommandContext context, string url)
        {
            try
            {
                typing = context.Channel.EnterTypingState();
                using HttpClient hc = new HttpClient();
                Random rnd = new Random();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?image={(url.Replace(".gif", ".png"))}&type=magik&intensity={rnd.Next(10)}&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public async Task<byte[]> Exec(string html, int width = 0, int height = 0) // Custom wrapper!!!
        {
            var e = Process.Start(new ProcessStartInfo
            {
                Arguments = $"-q --width {width} --height {height} -f jpeg  - -",
                FileName = Path.Combine(Directory.GetCurrentDirectory(),
                    "Binaries\\wkhtmltoimage.exe"),
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            });
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
