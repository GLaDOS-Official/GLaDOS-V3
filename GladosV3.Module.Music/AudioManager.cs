using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using GladosV3.Services;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Module.Music
{
    public class AudioService
    {
        public static AudioService service;
        internal static readonly ConcurrentDictionary<ulong, MusicClass> ConnectedChannels = new ConcurrentDictionary<ulong, MusicClass>();
        public int type;
        public bool fail = true;
        public AudioService(IConfigurationRoot config)
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
            if (ConnectedChannels.TryGetValue(guild.Id, out _)) return false;
            if (target?.Guild.Id != guild.Id) return false;
            var audioClient = await target.ConnectAsync();
            return ConnectedChannels.TryAdd(guild.Id, new MusicClass(audioClient,target.Id));
        }
        public async Task LeaveAudioAsync(IGuild guild)
        {
            if (ConnectedChannels.TryRemove(guild.Id, out MusicClass mclass))
            {
                mclass.process?.Kill();
                await mclass.GetClient.StopAsync().ConfigureAwait(false);
                mclass.GetClient.Dispose();
            }
        }

        public async Task SendAudioAsync(string path, ICommandContext context)
        {
            if (!ConnectedChannels.TryGetValue(context.Guild.Id, out MusicClass mclass))
            {
                if (!await JoinAudioAsync(context.Guild, ((IVoiceState)context.User).VoiceChannel))
                { await context.Channel.SendMessageAsync("Please join VC first!"); return; }
                ConnectedChannels.TryGetValue(context.Guild.Id, out mclass);
            }
            if (string.IsNullOrWhiteSpace(path))
            { await context.Channel.SendMessageAsync("We're sorry, something went wrong on our side."); return; }
            if (!path.StartsWith("http")) // i guess we search it on youtube?
                path = $"ytsearch:{Uri.EscapeUriString(path)}";
            mclass.AddToQueue(path);
            if (mclass.GetQueue.Count >= 2)
                return;
            using (var client = mclass.GetClient)
            using (var stream = client.CreatePCMStream(AudioApplication.Music))
            {
                atg:
                if (type == 1 && DateTime.UtcNow.Hour != 3 && DateTime.UtcNow.Hour != 4)
                    using (var output = StreamAPI(mclass))
                    {
                        try
                        { await output.CopyToAsync(stream); await stream.FlushAsync(); }
                        catch (TaskCanceledException)
                        { stream.Close(); await client.StopAsync(); if (!mclass.process.HasExited) mclass.process.Kill(); ConnectedChannels.TryRemove(context.Guild.Id, out mclass); return; } // PANIC
                    }
                else
                    using (var output = CmdYoutube(mclass))
                    {
                        try
                        { await output.CopyToAsync(stream); await stream.FlushAsync(); }
                        catch (TaskCanceledException)
                        { stream.Close(); await client.StopAsync(); if (!mclass.process.HasExited) mclass.process.Kill(); ConnectedChannels.TryRemove(context.Guild.Id, out mclass); return; } // PANIC
                    }
                mclass.GetQueue.Remove(mclass.GetQueue[0]);
                if (mclass.GetQueue.Count >= 1)
                    goto atg;
            }
        }

        public Task<string> QueueAsync(IGuild guild)
        {
            if (!ConnectedChannels.TryGetValue(guild.Id, out MusicClass mclass)) return Task.FromResult("");
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
            webClient.OpenRead("https://otherwise-grams.000webhostapp.com/stream.php")?.CopyToAsync(x.StandardInput.BaseStream).GetAwaiter();
            return x.StandardOutput.BaseStream;
        }
    }

    public class MusicClass
    {
        private List<string> Queue { get; } = new List<string>(15); // limit so memory usage won't go **bam boom**
        private IAudioClient Client { get; }
        public void AddToQueue(string url) => Queue.Add(url);
        public void ClearQueue() => Queue.Clear();
        public List<string> GetQueue => Queue;
        public bool IsPlaying => Queue.Count >= 1;
        public ulong VCID;
        public IAudioClient GetClient => Client;
        public Process process;

        public MusicClass(IAudioClient client, ulong vcid)
        {
            Client = client;
            VCID = vcid;
        }
    }
}