using Discord;
using Discord.Commands;
using GladosV3.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Module.ImageGeneration
{
    public class ImageGeneratorModule : ModuleBase<ICommandContext>
    {
        private readonly GeneratorService _service;
        private readonly string[] imageFormats = { "jpg", "jpeg", "bmp", "png" };
        public ImageGeneratorModule(GeneratorService service) => this._service = service;

        private bool HasImageExtension(string path) => this.imageFormats.Any(path.EndsWith);

        [Command("delete", RunMode = RunMode.Async)]
        [Remarks("delete")]
        [Summary("delete")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Delete([Remainder]string text)
        {
            if (!this._service.Fail)
                await Context.Channel.SendFileAsync(this._service.Delete(text, Context).GetAwaiter().GetResult(), "delet.jpg");
            else
                await Context.Channel.SendMessageAsync("There was an error... Check the logs!");
        }
        [Command("shit", RunMode = RunMode.Async)]
        [Remarks("shit <who>")]
        [Summary("shit")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Shit([Remainder]string text)
        {
            if (!this._service.Fail)
                await Context.Channel.SendFileAsync(this._service.Shit(text.Split(','), Context).GetAwaiter().GetResult(), "shit.jpg");
            else
                await Context.Channel.SendMessageAsync("There was an error... Check the logs!");
        }
        [Command("mc", RunMode = RunMode.Async)]
        [Remarks("mc <name>")]
        [Summary("Achievement get!")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Minecraft([Remainder]string text) => await Context.Channel.SendFileAsync(this._service.MinecraftAchivementGet(text, Context).GetAwaiter().GetResult(), "minecraft_bullshit.jpg");
        [Command("threats", RunMode = RunMode.Async)]
        [Remarks("threats [mention/userid/file]")]
        [Summary("The 3 biggest threats to society...")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Threats(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Threats(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "dangerous_person.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();

                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.Threats(Context, attach.Url).GetAwaiter().GetResult(), "dangerous_object.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.Threats(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "dangerous_person.jpg");
        }
        [Command("baguette", RunMode = RunMode.Async)]
        [Remarks("baguette [mention/userid/file]")]
        [Summary("Yummy!")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Baguette(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Baguette(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "baguette.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();
                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.Baguette(Context, attach.Url).GetAwaiter().GetResult(), "baguette.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.Baguette(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "baguette.jpg");
        }
        [Command("clyde", RunMode = RunMode.Async)]
        [Remarks("clyde <clyde>")]
        [Summary("Clyde? What the are you saying again?")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Clyde([Remainder]string clyde) => await Context.Channel.SendFileAsync(this._service.Clyde(Context, clyde).GetAwaiter().GetResult(), "clyde.jpg");
        [Command("relationship", RunMode = RunMode.Async)]
        [Remarks("relationship <userid/mention>")]
        [Summary("OwO Who's that?")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Relationship(IUser user2) => await Context.Channel.SendFileAsync(this._service.Relationship(Context, user2).GetAwaiter().GetResult(), "relationship.jpg");
        [Command("captcha", RunMode = RunMode.Async)]
        [Remarks("captcha [mention/userid/file]")]
        [Summary("Please verify to continue...")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Captcha(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Captcha(Context, user.GetAvatarUrl(size: 1024), user.Username).GetAwaiter().GetResult(), "captcha.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();

                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.Captcha(Context, attach.Url, System.IO.Path.GetFileNameWithoutExtension(attach.Filename)).GetAwaiter().GetResult(), "captcha.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.Captcha(Context, Context.User.GetAvatarUrl(size: 1024), Context.User.Username).GetAwaiter().GetResult(), "captcha.jpg");
        }
        [Command("whowouldwin", RunMode = RunMode.Async)]
        [Remarks("whowouldwin <userid/mention>")]
        [Summary("Who would win?")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task WhoWouldWin(IUser user2) => await Context.Channel.SendFileAsync(this._service.WhoWouldWin(Context, user2).GetAwaiter().GetResult(), "WhoWouldWin.jpg");
        [Command("changemymind", RunMode = RunMode.Async)]
        [Remarks("changemymind <text>")]
        [Summary("Change my mind bruh!")]
        [Timeout(5, 30, Measure.Seconds)]
        [Alias("cmm")]
        public async Task ChangeMyMind([Remainder]string cmm) => await Context.Channel.SendFileAsync(this._service.ChangeMyMind(Context, cmm).GetAwaiter().GetResult(), "CMM.jpg");
        [Command("jpeg", RunMode = RunMode.Async)]
        [Remarks("jpeg  [mention/userid/file]")]
        [Summary("Jpegify")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Jpegify(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Jpegify(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "jpeg.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();
                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.Jpegify(Context, attach.Url).GetAwaiter().GetResult(), "jpeg.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.Jpegify(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "jpeg.jpg");
        }
        [Command("lolice", RunMode = RunMode.Async)]
        [Remarks("lolice  [mention/userid/file]")]
        [Summary("Anime loli police chief")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Lolice(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Lolice(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "lolice.jpg");
            else
                await Context.Channel.SendFileAsync(this._service.Lolice(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "lolice.jpg");
        }
        [Command("kannafy", RunMode = RunMode.Async)]
        [Remarks("kannafy <text>")]
        [Summary("Kanna OwO")]
        [Timeout(5, 30, Measure.Seconds)]
        [Alias("kannagen")]
        public async Task Kannagen([Remainder]string cmm) => await Context.Channel.SendFileAsync(this._service.Kannagen(Context, cmm).GetAwaiter().GetResult(), "Kanna.jpg");
        [Command("pat", RunMode = RunMode.Async)]
        [Remarks("pat")]
        [Summary("pat")]
        [Alias("headpat", "pattu")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Pat(IUser user = null)
        {
            var pat = this._service.Pat(Context).GetAwaiter().GetResult();
            await Context.Channel.SendFileAsync(pat, "pat.gif");

        }
        [Command("iphonex", RunMode = RunMode.Async)]
        [Remarks("iphonex [mention/userid/file]")]
        [Summary("Hmm what can we fit into iphonex screen this time?")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task IPhoneX(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.IPhoneX(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "iphone.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();

                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.IPhoneX(Context, attach.Url).GetAwaiter().GetResult(), "iphone.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.IPhoneX(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "iphone.jpg");
        }

        [Command("trap", RunMode = RunMode.Async)]
        [Remarks("trap [mention/userid]")]
        [Summary("Got you! Heheh")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Trap(IUser user) => await Context.Channel.SendFileAsync(this._service.Trap(Context, user).GetAwaiter().GetResult(), "trap.jpg");
        [Command("trump", RunMode = RunMode.Async)]
        [Remarks("trump <text>")]
        [Summary("New notification on twitter from realDonaldTrump!")]
        [Timeout(5, 30, Measure.Seconds)]
        [Alias("trumptweet")]
        public async Task TrumpTweet([Remainder]string cmm) => await Context.Channel.SendFileAsync(this._service.Trump(Context, cmm).GetAwaiter().GetResult(), "realDonaldTrump.jpg");
        [Command("deepfry", RunMode = RunMode.Async)]
        [Remarks("deepfry [mention/userid/file]")]
        [Summary("Did anyone said deepfry?")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Deepfry(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Deepfry(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "Deepfry.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();

                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.Deepfry(Context, attach.Url).GetAwaiter().GetResult(), "Deepfry.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.Deepfry(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "Deepfry.jpg");
        }

        [Command("magik", RunMode = RunMode.Async)]
        [Remarks("magik [mention/userid/file]")]
        [Summary("Did anyone said magik?")]
        [Timeout(5, 30, Measure.Seconds)]
        public async Task Magik(IUser user = null)
        {
            if (user != null)
                await Context.Channel.SendFileAsync(this._service.Magik(Context, user.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "Magik.jpg");
            else if (Context.Message.Attachments.Count > 0)
            {
                IAttachment attach = Context.Message.Attachments.First();

                if (this.HasImageExtension(attach.Url))
                    await Context.Channel.SendFileAsync(this._service.Magik(Context, attach.Url).GetAwaiter().GetResult(), "Magik.jpg");
                else
                    await this.ReplyAsync("The attachment is not an image!");
            }
            else
                await Context.Channel.SendFileAsync(this._service.Magik(Context, Context.User.GetAvatarUrl(size: 1024)).GetAwaiter().GetResult(), "Deepfry.jpg");
        }
    }
}