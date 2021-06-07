using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Rest;

namespace GLaDOSV3.Module.Music
{
    public class AudioService
    {
        public static AudioService service;
        private readonly LavaNode lavaNode;
        private readonly DiscordSocketClient socketClient;
        public AudioService(DiscordSocketClient socketClient)
        {
            this.lavaNode = new LavaNode(socketClient, new LavaConfig
            {
                Hostname = "127.0.0.1",
                Port = 9593,
                SelfDeaf = true
            });
            this.socketClient = socketClient;
            lavaNode.OnTrackEnded += this.LavaNodeOnOnTrackEnded;
            socketClient.Disconnected += exception => this.lavaNode.DisconnectAsync();
            socketClient.Ready += () => this.lavaNode.ConnectAsync();
            this.socketClient.UserVoiceStateUpdated += (user, old, _) =>
            {
                if (old.VoiceChannel == null) return Task.CompletedTask;
                var player = this.GetPlayer(old.VoiceChannel.Guild);
                if (player is null) return Task.CompletedTask;
                if (player.VoiceChannel.Id == old.VoiceChannel.Id && old.VoiceChannel.Users.Count <= 1) this.lavaNode.LeaveAsync(old.VoiceChannel);
                return Task.CompletedTask;
            };

        }


        public LavaPlayer GetPlayer(IGuild guild) => this.lavaNode.HasPlayer(guild) ? this.lavaNode.GetPlayer(guild) : null;

        public async Task<string> ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            if (voiceChannel is null) return "You need to connect to a voice channel.";
            await this.lavaNode.JoinAsync(voiceChannel, textChannel);
            return $"Connected to {voiceChannel.Name}! 🔈";
        }

        public async Task<string> LeaveAsync(SocketVoiceChannel voiceChannel)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            if (voiceChannel is null) return "Please join the channel the bot is in to make it leave.";
            await this.lavaNode.LeaveAsync(voiceChannel);
            return $"Leaving {voiceChannel.Name}! 👋";
        }

        private static async Task<string> GetYoutubeId(string url)
        {
            Regex r = new Regex(".*((youtu.be\\/)|(v\\/)|(\\/u\\/\\w\\/)|(embed\\/)|(watch\\?))\\??v?=?([^#\\&\\?]*).*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match match = r.Match(url);
            return (match.Success && match.Groups[7].Length == 11) ? match.Groups[7].Value : url;
        }
        public async Task<string> PlayAsync(string query, IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            var player = this.GetPlayer(guild);
            if (player == null) return "Player error. Please contact the bot owner.";
            if (player.Queue.Count > 35) return "Queue is full!";
            SearchResponse results;
            try
            {
                if (query.StartsWith("https://soundcloud.com")) results = await this.lavaNode.SearchSoundCloudAsync(query.Remove(0, 23));
                else if (query.StartsWith("https://music.youtube.com/watch?v=") || query.StartsWith("https://www.youtube.com/watch?v=") || query.StartsWith("https://youtu.be/")) { results = await this.lavaNode.SearchYouTubeAsync(query); if (results.Tracks.Count == 0) results = await this.lavaNode.SearchYouTubeAsync(await GetYoutubeId(query)); }
                else results = await this.lavaNode.SearchAsync(query);
            }
            catch (HttpRequestException)
            {
                return "Player is offline. Please contact the bot owner.";
            }

            if (results.LoadStatus == LoadStatus.NoMatches || results.LoadStatus == LoadStatus.LoadFailed) results = await this.lavaNode.SearchYouTubeAsync(query);
            if (results.LoadStatus == LoadStatus.NoMatches || results.LoadStatus == LoadStatus.LoadFailed || results.Tracks.Count == 0) return "No song found on YT or SC.";
            var track = results.Tracks.FirstOrDefault();
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.Enqueue(track);
                return $"{track.Title} has been added to the queue.";
            }
            await player.PlayAsync(track);
            return $"Now Playing: `{track.Title}` by `{track.Author}`";
        }

        public async Task<string> StopAsync(IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            var player = this.GetPlayer(guild);
            if (player is null)
                return "Error with player. Please contact the bot owner.";
            await player.StopAsync();
            player.Queue.Clear();
            return "Music playback stopped.";
        }

        public async Task<string> SkipAsync(IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            var player = this.GetPlayer(guild);
            if (player is null || player.Queue.Count() is 0)
                return "There's nothing left in the queue!";

            var oldTrack = player.Track;
            await player.SkipAsync();
            return $"Skipped: {oldTrack.Title}\nNow Playing: {player.Track.Title}";
        }

        public async Task<string> SetVolumeAsync(ushort vol, IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            var player = this.GetPlayer(guild);
            if (player is null)
                return "Player isn't playing.";

            if (vol > 250 || vol <= 10) return "Please use range between 10 - 250";

            await player.UpdateVolumeAsync(vol);
            return $"Volume set to: {vol}";
        }

        public async Task<string> PauseOrResumeAsync(IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            var player = this.GetPlayer(guild);
            if (player is null)
                return "Player isn't playing.";

            if (player.PlayerState != PlayerState.Paused)
            {
                await player.PauseAsync();
                return "Player paused.";
            }
            await player.ResumeAsync();
            return "Playback resumed.";
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return "Player is offline. Please contact the bot owner.";
            var player = this.GetPlayer(guild);
            if (player is null) return "Player isn't playing.";

            if (player.PlayerState == PlayerState.Playing) return "Player is not paused.";
            await player.ResumeAsync();
            return "Playback resumed.";

        }
        public Task<string> QueueCMD(IGuild guild)
        {
            if (!this.lavaNode.IsConnected) return Task.FromResult("Player is offline. Please contact the bot owner.");
            var player = this.GetPlayer(guild);
            if (player is null || player.PlayerState != PlayerState.Playing)
                return Task.FromResult("Player isn't playing.");
            var msg = $"Queue:\n```1. {player.Track.Title} by {player.Track.Author}";
            var i = 2;
            msg = player.Queue.Cast<LavaTrack>().Aggregate(msg, (current, e) => current + $"\n{i++}. {e.Title} by {e.Author}");
            msg += "\n```";
            return Task.FromResult(msg);
        }
        private async Task LavaNodeOnOnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
                return;

            if (!args.Player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await args.Player.TextChannel.SendMessageAsync("There are no more tracks in the queue.");
                return;
            }
            await args.Player.PlayAsync(nextTrack);
            await args.Player.TextChannel.SendMessageAsync($"Now Playing: {args.Player.Track.Title} by {args.Player.Track.Author}");
        }
    }
}