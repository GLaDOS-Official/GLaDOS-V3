using Discord;
using Discord.Commands;
using GLaDOSV3.Services;
using ImageMagick;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GLaDOSV3.Module.ImageGeneration
{
    public class GeneratorService
    {
        public bool Fail;
        private const string HtmlSplit = " ";
        public GeneratorService()
        {
            if (!OperatingSystem.IsWindows())  {this.Fail = false; return;}
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"Binaries{Path.DirectorySeparatorChar}wkhtmltoimage.exe"))) { LoggingService.Log(LogSeverity.Error, "ImageGenerator", "wkhtmltoimage.exe not found!"); this.Fail = true; };
        }
        public Task<MemoryStream> Shit(string[] items, ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                string item = items.Aggregate(string.Empty, (current, type) => current + type + ", ");
                string html = File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                    $"images{Path.DirectorySeparatorChar}shit.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item.Remove(item.Length - 2)).Replace("REPLACECORRECTPLURAL", items.Length > 1 ? "are" : "is");
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                string html = File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                    $"images{Path.DirectorySeparatorChar}delete.html")).GetAwaiter().GetResult().Replace("REPLACEWITHITEM", item);
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                //TODO: maybe do this locally?
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?text={clyde}&type=clyde&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Relationship(ICommandContext context, IUser user1, IUser user2)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                if (user2 == null) user2 = context.Client.CurrentUser;
                string user1_url = (user1.GetAvatarUrl() ?? user1.GetDefaultAvatarUrl()).Replace(".gif", ".png");
                string user2_url = (user2.GetAvatarUrl() ?? user2.GetDefaultAvatarUrl()).Replace(".gif", ".png");
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?user1={user1_url}&user2={user2_url}&type=ship&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Captcha(ICommandContext context, string url, string username)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?url={url}&username={username}&type=captcha&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> WhoWouldWin(ICommandContext context, IUser user1, IUser user2)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                if (user2 == null) user2 = context.Client.CurrentUser;
                string user1_url = (user1.GetAvatarUrl() ?? user1.GetDefaultAvatarUrl()).Replace(".gif", ".png");
                string user2_url = (user2.GetAvatarUrl() ?? user2.GetDefaultAvatarUrl()).Replace(".gif", ".png");
                using HttpClient hc = new HttpClient();
                byte[] jpgBytes = hc.GetByteArrayAsync($"https://nekobot.xyz/api/imagegen?user1={user1_url}&user2={user2_url}&type=whowouldwin&raw=1").GetAwaiter().GetResult();
                return Task.FromResult(new MemoryStream(jpgBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> ChangeMyMind(ICommandContext context, string cmm)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            const int splitPerChar = 10;
            const int splitPerUpperChar = 8;
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {

                if (text.Length >= 45) text = text.Substring(0, 45);

                string[] split = text.Split(' ');
                for (int i = 0; i < split.Length; i++)
                {
                    int splitNum = char.IsUpper(split[i][^1]) ? splitPerUpperChar : splitPerChar;
                    if (split[i].Length < splitNum) continue;
                    for (int j = splitNum; j < split[i].Length; j += splitNum) split[i] = split[i].Insert(j, HtmlSplit);
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                if (trump.Length > 33) trump = trump.Insert(34, HtmlSplit);
                if (trump.Length >= 72) trump = trump.Substring(0, 72);

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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
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
        public Task<MemoryStream> Beautiful(ICommandContext context, string url)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                using var images = new MagickImageCollection();
                using HttpClient hc = new HttpClient();
                MagickImage image1 = new MagickImage($".{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}beautiful.png");
                MagickImage image2 = new MagickImage(hc.GetByteArrayAsync(url.Replace(".gif", ".png")).GetAwaiter().GetResult());
                MagickImage image3 = new MagickImage(image2);
                image1.Alpha(AlphaOption.Set);

                image2.InterpolativeResize(90, 112 + 7, PixelInterpolateMethod.Bilinear);
                image2.Page = new MagickGeometry("+256+20");
                image3.InterpolativeResize(90, 105 + 7, PixelInterpolateMethod.Bilinear);
                image3.Page = new MagickGeometry("+257+220");
                images.Add(image3);
                images.Add(image2);
                images.Add(image1);
                var result = images.Merge();
                using var stream = new MemoryStream();
                result.Write(stream);
                byte[] bytes;
                bytes = stream.ToArray();
                return Task.FromResult(new MemoryStream(bytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Pat(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/pat").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Kiss(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/kiss").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Tickle(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/tickle").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Poke(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/poke").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Slap(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/slap").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Cuddle(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/cuddle").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public Task<MemoryStream> Hug(ICommandContext context)
        {
            IDisposable typing = context.Channel.EnterTypingState();
            try
            {
                var gifBytes = NekosDevApi("sfw/gif/hug").GetAwaiter().GetResult();
                return gifBytes == null ? null : Task.FromResult(new MemoryStream(gifBytes));
            }
            finally
            {
                typing.Dispose();
            }
        }
        public static async Task<byte[]> NekosDevApi(string path)
        {
            using HttpClient hc = new HttpClient();
            JObject json = JObject.Parse(hc.GetStringAsync($"https://api.nekos.dev/api/v3/images/{path}").GetAwaiter().GetResult());
            if (!(bool)json["data"]["status"]["success"].ToObject(typeof(bool))) return null;
            if (string.IsNullOrWhiteSpace(json["data"]["response"]["url"].ToString())) return null;
            var gifBytes = hc.GetByteArrayAsync(json["data"]["response"]["url"].ToString()).GetAwaiter().GetResult();
            return gifBytes;
        }
        public static async Task<byte[]> Exec(string html, int width = 0, int height = 0) // Custom wrapper!!!
        {
            var e = Process.Start(new ProcessStartInfo
            {
                Arguments = $"-q --width {width} --height {height} -f jpeg  - -",
                FileName = Path.Combine(Directory.GetCurrentDirectory(),
                    $"Binaries{Path.DirectorySeparatorChar}wkhtmltoimage.exe"),
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            });
            await using (StreamWriter stream = e.StandardInput)
            {
                byte[] htmlcontent = Encoding.UTF8.GetBytes(html);
                await stream.BaseStream.WriteAsync(htmlcontent, 0, htmlcontent.Length).ConfigureAwait(false);
                await stream.WriteLineAsync().ConfigureAwait(false);
                await stream.BaseStream.FlushAsync().ConfigureAwait(false);
                stream.Close();
            }
            await using (MemoryStream stream = new MemoryStream())
            {
                await e.StandardOutput.BaseStream.CopyToAsync(stream);
                await e.StandardOutput.BaseStream.FlushAsync().ConfigureAwait(false);
                e.StandardOutput.Close();
                return stream.ToArray();
            }
        }

    }
}