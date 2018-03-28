using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Module.Music
{
    public class AudioModule : ModuleBase<ICommandContext>
    {
        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        [Remarks("join")]
        [Summary("Bot connect to the VC!")]
        [RequireContext(ContextType.Guild)]
        public Task JoinCmd()
        {
            if (!AudioService.service.fail)
                return AudioService.service.JoinAudioAsync(Context.Guild, ((IVoiceState)Context.User).VoiceChannel);
            Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask;
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        [Remarks("leave")]
        [Summary("Bot disconnects from the VC!")]
        [RequireContext(ContextType.Guild)]
        public Task LeaveCmd()
        {
            if (!AudioService.service.fail)
                return AudioService.service.LeaveAudioAsync(Context.Guild);
            Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask;
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("play <youtube url>")]
        [Summary("Plays music from youtube!")]
        [RequireContext(ContextType.Guild)]
        public Task PlayCmd([Remainder] string song)
        {
            if (!AudioService.service.fail)
                return AudioService.service.SendAudioAsync(song,Context);
            { Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask; }
        }
        [Command("queue", RunMode = RunMode.Async)]
        [Remarks("queue")]
        [Summary("Gets the playlist!")]
        [RequireContext(ContextType.Guild)]
        public Task QueueCmd()
        {
            if (AudioService.service.fail)
            { Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask; }
            var result = AudioService.service.QueueAsync(Context.Guild).GetAwaiter().GetResult();
            return ReplyAsync(string.IsNullOrWhiteSpace(result) ? "Queue is empty!" : result);
        }
    }
}