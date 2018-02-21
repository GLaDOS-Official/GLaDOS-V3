using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace GladosV3.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> _connectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public AudioService()
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Binaries\\youtube-dl.exe"))) LoggingService.Log(LogSeverity.Error, "AudioService", "youtube-dl.exe not found!");
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Binaries\\ffmpeg.exe"))) LoggingService.Log(LogSeverity.Error, "AudioService", "ffmpeg.exe not found!");
        }

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (_connectedChannels.TryGetValue(guild.Id, out _)) return;
            if (target.Guild.Id != guild.Id) return;
            var audioClient = await target.ConnectAsync();
            if (_connectedChannels.TryAdd(guild.Id, audioClient)) return;
        }   
        public async Task LeaveAudio(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out IAudioClient client))
                await client.StopAsync().ConfigureAwait(false);
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {

            if (_connectedChannels.TryGetValue(guild.Id, out var client))
            {
                if (string.IsNullOrWhiteSpace(path))
                 { await channel.SendMessageAsync("We're sorry, something went wrong on our side."); return; }
                await client.SetSpeakingAsync(false);
                 using (var output = CmdYoutube(path))
                 using (var stream = client.CreatePCMStream(AudioApplication.Music, 98304,200)) 
                 {
                     try
                     { await output.StandardOutput.BaseStream.CopyToAsync(stream); }
                     catch
                     { await stream.FlushAsync(); if(!output.HasExited) output.Kill(); }
                     finally
                     { await stream.FlushAsync(); }
                 }
                await client.SetSpeakingAsync(false);
            }
        }
        private Process CmdYoutube(string url) // around 3s-5s delay 
        {
            Process x = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --no-playlist -q --no-warnings --geo-bypass --no-mark-watched -f bestaudio \"{url}\" -o - | ffmpeg.exe -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 -filter:a \"volume=1.25\" pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Binaries")
            });
            if (x != null && x.HasExited)
                throw new Exception("youtube-dl or ffmpeg has exited immediately!");
            x?.StandardInput.WriteLine(""); // don't ask me why, without this, it wouldn't work
            x?.StandardInput.WriteLine(""); // don't ask me why, without this, it wouldn't work
            return x;
        }
    }
}