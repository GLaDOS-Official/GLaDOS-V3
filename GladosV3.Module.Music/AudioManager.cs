using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GladosV3.Helpers;
using GladosV3.Services;
using Victoria;
using Discord.WebSocket;
using Victoria.Entities;
using System.Linq;

namespace GladosV3.Module.Music
{
    public class AudioService
    {
        public static AudioService service;
        private readonly LavaSocketClient _lavaClient;
        private readonly DiscordSocketClient _socketClient;
        private readonly LavaRestClient _lavaRestClient;
        public AudioService(LavaRestClient lavaRestClient,LavaSocketClient lavaSocketClient, DiscordSocketClient socketClient)
        {
            _lavaClient = lavaSocketClient;
            _socketClient = socketClient;
            _lavaRestClient = lavaRestClient;
            lavaSocketClient.OnTrackFinished += LavaSocketClient_OnTrackFinished;
            socketClient.Ready += SocketClient_Ready;
        }
        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
            => await _lavaClient.ConnectAsync(voiceChannel, textChannel);

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
            => await _lavaClient.DisconnectAsync(voiceChannel);
        public async Task<string> PlayAsync(string query, ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            var results = await _lavaRestClient.SearchYouTubeAsync(query);
            if (results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed)
            {
                return "No video found on YT.";
            }

            var track = results.Tracks.FirstOrDefault();
            if (_player.Queue.Count > 35) return "Queue is full!";
            if (_player.IsPlaying)
            {
                _player.Queue.Enqueue(track);
                return $"{track.Title} has been added to the queue.";
            }
            else
            {
                await _player.PlayAsync(track);
                return $"Now Playing: {track.Title}";
            }
        }

        public async Task<string> StopAsync(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null)
                return "Error with player. Please contact the bot owner.";
            await _player.StopAsync();
            return "Music Playback Stopped.";
        }

        public async Task<string> SkipAsync(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null || _player.Queue.Items.Count() is 0)
                return "Queue is empty!";

            var oldTrack = _player.CurrentTrack;
            await _player.SkipAsync();
            return $"Skiped: {oldTrack.Title} \nNow Playing: {_player.CurrentTrack.Title}";
        }

        public async Task<string> SetVolumeAsync(int vol, ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null)
                return "Player isn't playing.";

            if (vol > 150 || vol <= 2)
            {
                return "Please use a number between 2 - 150";
            }

            await _player.SetVolumeAsync(vol);
            return $"Volume set to: {vol}";
        }

        public async Task<string> PauseOrResumeAsync(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null)
                return "Player isn't playing.";

            if (!_player.IsPaused)
            {
                await _player.PauseAsync();
                return "Player paused.";
            }
            else
            {
                await _player.ResumeAsync();
                return "Playback resumed.";
            }
        }

        public async Task<string> ResumeAsync(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null)
                return "Player isn't playing.";

            if (_player.IsPaused)
            {
                await _player.ResumeAsync();
                return "Playback resumed.";
            }

            return "Player is not paused.";
        }
        private async Task LavaSocketClient_OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext())
                return;

            if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await player.TextChannel.SendMessageAsync("There are no more tracks in the queue.");
                return;
            }

            await player.PlayAsync(nextTrack);
        }

        private async Task SocketClient_Ready()
        {
            await _lavaClient.StartAsync(_socketClient);
        }

        private async Task TrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext())
                return;

            if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await player.TextChannel.SendMessageAsync("There are no more tracks in the queue.");
                return;
            }

            await player.PlayAsync(nextTrack);
        }
    }
}