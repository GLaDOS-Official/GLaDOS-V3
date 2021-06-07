using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace GLaDOSV3.Module.Music
{
    public class MusicModule : ModuleBase<ICommandContext>
    {
        private readonly AudioService musicService;

        public MusicModule(AudioService musicService) => this.musicService = musicService;

        [Command("join", RunMode = RunMode.Async)]
        [Remarks("join")]
        [Summary("Bot connect to the VC!")]
        [RequireContext(ContextType.Guild)]
        public async Task JoinCmd() => await this.ReplyAsync(await this.musicService.ConnectAsync(((SocketGuildUser)Context.User).VoiceChannel, Context.Channel as ITextChannel));
        [Command("leave", RunMode = RunMode.Async)]
        [Remarks("leave")]
        [Summary("Bot disconnects from the VC!")]
        [RequireContext(ContextType.Guild)]
        public async Task LeaveCmd() => await this.ReplyAsync(await this.musicService.LeaveAsync(((SocketGuildUser)Context.User).VoiceChannel));

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

            if (this.musicService.GetPlayer(Context.Guild) == null && !(await this.musicService.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel)).Contains("Connected")) return;

            await this.ReplyAsync(await this.musicService.PlayAsync(query, Context.Guild));
        }

        [Command("stop")]
        [Remarks("stop")]
        [RequireContext(ContextType.Guild)]
        public async Task Stop() => await this.ReplyAsync(await this.musicService.StopAsync(Context.Guild));

        [Command("skip")]
        [Remarks("skip")]
        [RequireContext(ContextType.Guild)]
        public async Task Skip() => await this.ReplyAsync(await this.musicService.SkipAsync(Context.Guild));

        [Command("volume")]
        [Remarks("volume")]
        [RequireContext(ContextType.Guild)]
        public async Task Volume(ushort vol)
            => await this.ReplyAsync(await this.musicService.SetVolumeAsync(vol, Context.Guild));

        [Command("pause")]
        [Remarks("pause")]
        [RequireContext(ContextType.Guild)]
        public async Task Pause()
            => await this.ReplyAsync(await this.musicService.PauseOrResumeAsync(Context.Guild));

        [Command("resume")]
        [Remarks("resume")]
        [RequireContext(ContextType.Guild)]
        public async Task Resume()
            => await this.ReplyAsync(await this.musicService.ResumeAsync(Context.Guild));

        [Command("queue")]
        [Remarks("queue")]
        [RequireContext(ContextType.Guild)]
        public async Task Queue() => await this.ReplyAsync(await this.musicService.QueueCMD(Context.Guild));
    }
}