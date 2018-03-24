using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using GladosV3.Helpers;
using GladosV3.Services;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace GladosV3.Module.Music
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, MusicClass> _connectedChannels = new ConcurrentDictionary<ulong, MusicClass>();
        public int type = 0;
        public bool fail = true;
        public AudioService(Microsoft.Extensions.Configuration.IConfigurationRoot config)
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries\\youtube-dl.exe"))) { LoggingService.Log(LogSeverity.Error, "AudioService", "youtube-dl.exe not found!"); return; }
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries\\ffmpeg.exe"))) { LoggingService.Log(LogSeverity.Error, "AudioService", "ffmpeg.exe not found!"); return; }
            type = Convert.ToInt32(config["MusicMethod"]);
            fail = false;
            //LoadLibraryEx.Invoke(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libsodium.dll"), IntPtr.Zero, 0);
            //LoadLibraryEx.Invoke(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "opus.dll"), IntPtr.Zero, 0);

        }

        public async Task<bool> JoinAudioAsync(IGuild guild, IVoiceChannel target)
        {
            if (_connectedChannels.TryGetValue(guild.Id, out _)) return false;
            if (target == null) return false;
            if (target.Guild.Id != guild.Id) return false;
            var audioClient = await target.ConnectAsync();
            if (_connectedChannels.TryAdd(guild.Id, new MusicClass(audioClient))) return true;
            return false;
        }
        public async Task LeaveAudioAsync(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out MusicClass mclass))
            {
                await mclass.GetClient.StopAsync().ConfigureAwait(false);
            }
        }

        public async Task SendAudioAsync(string path, ICommandContext context)
        {
            if (!_connectedChannels.TryGetValue(context.Guild.Id, out MusicClass mclass))
            {
                if (!await JoinAudioAsync(context.Guild, ((IVoiceState)context.User).VoiceChannel))
                { await context.Channel.SendMessageAsync("Please join VC first!"); return; }
                _connectedChannels.TryGetValue(context.Guild.Id, out mclass);
            }
            if (string.IsNullOrWhiteSpace(path))
            { await context.Channel.SendMessageAsync("We're sorry, something went wrong on our side."); return; }
            if (!path.StartsWith("http")) // i guess we search it on youtube?
                path = $"ytsearch:{Uri.EscapeUriString(path)}";
            if (mclass.GetQueue.Count >= 1) { mclass.AddToQueue(path); return; }
            else
                mclass.AddToQueue(path);
            using (var client = mclass.GetClient)
            {
                await client.SetSpeakingAsync(true);
                using (var stream = client.CreatePCMStream(AudioApplication.Music, 98304, 200))
                {
                    atg:
                    if (type == 1)
                        using (var output = StreamAPI(mclass))
                        {
                            try
                            { await output.CopyToAsync(stream); await stream.FlushAsync(); }
                            catch (TaskCanceledException)
                            { stream.Close(); await client.StopAsync(); if (!mclass.process.HasExited) mclass.process.Kill(); _connectedChannels.TryRemove(context.Guild.Id, out mclass); return; } // PANIC
                        }
                    else
                        using (var output = CmdYoutube(mclass))
                        {
                            try
                            { await output.CopyToAsync(stream); await stream.FlushAsync(); }
                            catch (TaskCanceledException)
                            { stream.Close(); await client.StopAsync(); if (!mclass.process.HasExited) mclass.process.Kill(); _connectedChannels.TryRemove(context.Guild.Id, out mclass); return; } // PANIC
                        }
                    mclass.GetQueue.Remove(mclass.GetQueue[0]);
                    if (mclass.GetQueue.Count >= 1)
                        goto atg;
                }
                await client.SetSpeakingAsync(false);
            }
        }

        public Task<string> QueueAsync(IGuild guild)
        {
            if (!_connectedChannels.TryGetValue(guild.Id, out MusicClass mclass)) return Task.FromResult("");
            List<string> queue = mclass.GetQueue;
            var output = "";
            for (var index = 0; index < queue.Count; index++)
            {
                output += $"{index}. <{queue[index].Replace("ytsearch:", "https://youtube.com/results?search_query=")}>\n";
                if (index == 0) output = $"{output.Remove(output.Length - 1)} <-- playing\n";
            }
            return Task.FromResult(output);
        }
        private Stream CmdYoutube(MusicClass mclass) // around 3s-5s delay 
        {
            Process x = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --no-playlist -4 -q --age-limit 15 --youtube-skip-dash-manifest --no-warnings --geo-bypass --no-mark-watched -f \"bestaudio[filesize<=30M]/worstaudio\" \"{mclass.GetQueue[0]}\" -o - | ffmpeg.exe -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 -filter:a \"volume=1.25\" pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries")
            });
            if (x.HasExited)
                throw new Exception("youtube-dl or ffmpeg has exited immediately!");
            mclass.process = x;
            x.StandardInput.WriteLine(""); // don't ask me why, without this, it wouldn't work
            return x.StandardOutput.BaseStream;
        }
        private Stream StreamAPI(MusicClass mclass)
        {
            WebClient webClient = new WebClient();
            webClient.QueryString.Add("url_bitch", mclass.GetQueue[0]);
            Process x = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = "-hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 -filter:a \"volume=1.25\" pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries")
            });
            if (x.HasExited)
                throw new Exception("ffmpeg has excited immediately!");
            mclass.process = x;
            webClient.OpenRead("https://otherwise-grams.000webhostapp.com/stream.php").CopyToAsync(x.StandardInput.BaseStream).GetAwaiter();
            return x.StandardOutput.BaseStream;
        }
    }

    public class MusicClass
    {
        private List<string> Queue { get; } = new List<string>(15); // limit so memory usage won't go **bam boom**
        private bool Playing { get; } = false;
        private IAudioClient Client { get; }
        public void AddToQueue(string url) => Queue.Add(url);
        public void ClearQueue() => Queue.Clear();
        public List<string> GetQueue => Queue;
        public bool IsPlaying => Playing;
        public IAudioClient GetClient => Client;
        public Process process;

        public MusicClass(IAudioClient client)
        {
            this.Client = client;
        }
    }
}