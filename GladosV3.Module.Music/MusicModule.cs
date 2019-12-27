using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace GladosV3.Module.Music
{
    public class MusicModule : ModuleBase<ICommandContext>
    {
        private readonly AudioService _musicService;

        public MusicModule(AudioService musicService) => this._musicService = musicService;

        [Command("join", RunMode = RunMode.Async)]
        [Remarks("join")]
        [Summary("Bot connect to the VC!")]
        [RequireContext(ContextType.Guild)]
        public async Task JoinCmd()
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await this.ReplyAsync("You need to connect to a voice channel.");
                return;
            }
            await this._musicService.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);
            await this.ReplyAsync($"Connected to {user.VoiceChannel.Name}! 🔈");
        }
        [Command("leave", RunMode = RunMode.Async)]
        [Remarks("leave")]
        [Summary("Bot disconnects from the VC!")]
        [RequireContext(ContextType.Guild)]
        public async Task LeaveCmd()
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await this.ReplyAsync("Please join the channel the bot is in to make it leave.");
                return;
            }
            await this._musicService.LeaveAsync(user.VoiceChannel);
            await this.ReplyAsync($"Leaving {user.VoiceChannel.Name}! 👋");
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("play <youtube url>")]
        [Summary("Plays music from youtube!")]
        [RequireContext(ContextType.Guild)]
        public async Task Play([Remainder]string query)
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await this.ReplyAsync("You need to connect to a voice channel.");
                return;
            }

            if (this._musicService.GetPlayer(Context.Guild.Id) == null)
            {
                await this._musicService.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);
            }
            await this.ReplyAsync(await this._musicService.PlayAsync(query, Context.Guild.Id));
        }

        [Command("stop")]
        [Remarks("stop")]
        [RequireContext(ContextType.Guild)]
        public async Task Stop()
        {
            await this._musicService.StopAsync(Context.Guild.Id);
            await this.ReplyAsync("Music stopped.");
        }

        [Command("skip")]
        [Remarks("skip")]
        [RequireContext(ContextType.Guild)]
        public async Task Skip() => await this.ReplyAsync(await this._musicService.SkipAsync(Context.Guild.Id));

        [Command("volume")]
        [Remarks("volume")]
        [RequireContext(ContextType.Guild)]
        public async Task Volume(int vol)
            => await this.ReplyAsync(await this._musicService.SetVolumeAsync(vol, Context.Guild.Id));

        [Command("pause")]
        [Remarks("pause")]
        [RequireContext(ContextType.Guild)]
        public async Task Pause()
            => await this.ReplyAsync(await this._musicService.PauseOrResumeAsync(Context.Guild.Id));

        [Command("resume")]
        [Remarks("resume")]
        [RequireContext(ContextType.Guild)]
        public async Task Resume()
            => await this.ReplyAsync(await this._musicService.ResumeAsync(Context.Guild.Id));

        [Command("queue")]
        [Remarks("queue")]
        [RequireContext(ContextType.Guild)]
        public async Task Queue() => await this.ReplyAsync(await this._musicService.QueueCMD(Context.Guild.Id));
    }
}