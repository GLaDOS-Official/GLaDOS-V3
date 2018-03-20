using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GladosV3.Helpers;
using GladosV3.Services;

namespace GladosV3.Module.Music
{
    public class AudioModule : ModuleBase<ICommandContext>
    {
        // Scroll down further for the AudioService.
        // Like, way down
        private readonly AudioService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public AudioModule(AudioService service)
        {
            _service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        [Remarks("join")]
        [Summary("Bot connect to the VC!")]
        [RequireContext(ContextType.Guild)]
        public Task JoinCmd()
        {
            if (!_service.fail)
                return _service.JoinAudioAsync(Context.Guild, ((IVoiceState)Context.User).VoiceChannel);
            else
            { Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask; }
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
            if (!_service.fail)
                return _service.LeaveAudioAsync(Context.Guild);
            else
            { Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask; }
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("play <youtube url>")]
        [Summary("Plays music from youtube!")]
        [RequireContext(ContextType.Guild)]
        public Task PlayCmd([Remainder] string song)
        {
            if (!_service.fail)
                return _service.SendAudioAsync(song,Context);
            else
            { Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask; }
        }
        [Command("queue", RunMode = RunMode.Async)]
        [Remarks("queue")]
        [Summary("Gets the playlist!")]
        [RequireContext(ContextType.Guild)]
        public Task QueueCmd()
        {
            if (_service.fail)
            { Context.Channel.SendMessageAsync("There was an error... Check the logs!").GetAwaiter(); return Task.CompletedTask; }
            var result = _service.QueueAsync(Context.Guild).GetAwaiter().GetResult();
            return ReplyAsync(string.IsNullOrWhiteSpace(result) ? "Queue is empty!" : result);
        }
    }
}