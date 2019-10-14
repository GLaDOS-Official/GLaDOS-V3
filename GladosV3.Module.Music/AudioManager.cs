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
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SearchResult = Victoria.Entities.SearchResult;

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
            _socketClient.UserVoiceStateUpdated += (user, old, _new) =>
            {
                if (old.VoiceChannel == null)
                    return Task.CompletedTask;
                var player = _lavaClient.GetPlayer(old.VoiceChannel.Guild.Id);
                if (player is null)
                    return Task.CompletedTask;
                if (player.VoiceChannel.Id == old.VoiceChannel.Id && old.VoiceChannel.Users.Count <= 1)
                    _lavaClient.DisconnectAsync(old.VoiceChannel);
                return Task.CompletedTask;
            };

        }

        public LavaPlayer GetPlayer(ulong guildId)
        {
            return _lavaClient.GetPlayer(guildId);
        }
        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
            => await _lavaClient.ConnectAsync(voiceChannel, textChannel);

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
            => await _lavaClient.DisconnectAsync(voiceChannel);
        public async Task<string> PlayAsync(string query, ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player == null) return "Player error. Please contact the bot owner.";
            SearchResult results;
            try
            {
                results = await _lavaRestClient.SearchSoundcloudAsync(query);
            }
            catch (JsonReaderException)
            {
                await _lavaClient.DisconnectAsync(_player.VoiceChannel);
                return "Player is offline. Please contact the bot owner.";
            }

            if (results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed) results = await _lavaRestClient.SearchSoundcloudAsync(query); 
            if (results.LoadType == LoadType.NoMatches || results.LoadType == LoadType.LoadFailed) return "No song found on YT or SC.";
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
            return "Music playback Stopped.";
        }

        public async Task<string> SkipAsync(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null || _player.Queue.Items.Count() is 0)
                return "There's nothing left in the queue!'";

            var oldTrack = _player.CurrentTrack;
            await _player.SkipAsync();
            return $"Skiped: {oldTrack.Title}\nNow Playing: {_player.CurrentTrack.Title}";
        }

        public async Task<string> SetVolumeAsync(int vol, ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null)
                return "Player isn't playing.";

            if (vol > 150 || vol <= 2)
            {
                return "Please use range between 2 - 150";
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
            await _player.ResumeAsync();
            return "Playback resumed.";
        }

        public async Task<string> ResumeAsync(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null)
                return "Player isn't playing.";

            if (!_player.IsPaused) return "Player is not paused.";
            await _player.ResumeAsync();
            return "Playback resumed.";

        }
        public async Task<string> QueueCMD(ulong guildId)
        {
            var _player = _lavaClient.GetPlayer(guildId);
            if (_player is null || !_player.IsPlaying)
                return "Player isn't playing.";
            string msg = $"Queue:\n```1. {_player.CurrentTrack.Title} by {_player.CurrentTrack.Author} on {_player.CurrentTrack.Provider}";
            var r = _player.Queue.Items;
            int i = 2;
            msg = r.Cast<LavaTrack>().Aggregate(msg, (current, e) => current + $"\n{i++}. {e.Title} by {e.Author} on {e.Provider}");
            msg += "\n```";
            return msg;
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
            await player.TextChannel.SendMessageAsync($"Now Playing: {player.CurrentTrack.Title} by {player.CurrentTrack.Author}");
        }

        private async Task SocketClient_Ready()
        {
            await _lavaClient.StartAsync(_socketClient);
        }
    }
}