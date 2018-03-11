﻿using System;
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
        public AudioService()
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries\\youtube-dl.exe"))) LoggingService.Log(LogSeverity.Error, "AudioService", "youtube-dl.exe not found!");
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries\\ffmpeg.exe"))) LoggingService.Log(LogSeverity.Error, "AudioService", "ffmpeg.exe not found!");
            //LoadLibraryEx.Invoke(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libsodium.dll"), IntPtr.Zero, 0);
            //LoadLibraryEx.Invoke(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "opus.dll"), IntPtr.Zero, 0);

        }

        public async Task JoinAudioAsync(IGuild guild, IVoiceChannel target)
        {
            if (_connectedChannels.TryGetValue(guild.Id, out _)) return;
            if (target.Guild.Id != guild.Id) return;
            var audioClient = await target.ConnectAsync();
            if (_connectedChannels.TryAdd(guild.Id, new MusicClass(audioClient))) return;
        }   
        public async Task LeaveAudioAsync(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out MusicClass mclass))
            {
                mclass.ClearQueue();
                await mclass.GetClient.StopAsync().ConfigureAwait(false);
            }
        }

        public async Task SendAudioAsync(string path, ICommandContext context)
        {
            if (!_connectedChannels.TryGetValue(context.Guild.Id, out MusicClass mclass))
            {
                await JoinAudioAsync(context.Guild, ((IVoiceState)context.User).VoiceChannel);
                _connectedChannels.TryGetValue(context.Guild.Id, out mclass);
            }
            if (string.IsNullOrWhiteSpace(path))
            { await context.Channel.SendMessageAsync("We're sorry, something went wrong on our side."); return; }
            if (!path.StartsWith("http")) // i guess we search it on youtube?
                path = $"ytsearch:{path}";
            if (mclass.GetQueue.Count >= 1) { mclass.AddToQueue(path); return; }
            else
                mclass.AddToQueue(path);
            var client = mclass.GetClient;
            await client.SetSpeakingAsync(false);
            using (var stream = client.CreatePCMStream(AudioApplication.Music, 98304, 200))
            {
                atg:
                using (var output = CmdYoutube(mclass))
                {
                    try
                    { await output.StandardOutput.BaseStream.CopyToAsync(stream); }
                    catch
                    { await stream.FlushAsync(); if (!output.HasExited) output.Kill(); }
                    finally
                    { await stream.FlushAsync(); }
                }
                mclass.GetQueue.Remove(mclass.GetQueue[0]);
                if (mclass.GetQueue.Count >= 1)
                    goto atg;
            }

            await client.SetSpeakingAsync(false);
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
        private Process CmdYoutube(MusicClass mclass) // around 3s-5s delay 
        {
            Process x = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe --no-playlist -q --age-limit 15 --no-warnings --geo-bypass --no-mark-watched -f bestaudio \"{mclass.GetQueue[0]}\" -o - | ffmpeg.exe -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 -filter:a \"volume=1.25\" pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Binaries")
            });
            if (x.HasExited)
                throw new Exception("youtube-dl or ffmpeg has exited immediately!");
            mclass.ProcessID = x.Id;
            x.StandardInput.WriteLine(""); // don't ask me why, without this, it wouldn't work
            x.StandardInput.WriteLine(""); // don't ask me why, without this, it wouldn't work
            return x;
        }
    }

    public class MusicClass
    {
        public int ProcessID;
        private List<string> queue { get; } = new List<string>(15); // limit so memory usage won't go **bam boom**
        private bool playing { get;  } = false;
        private IAudioClient client { get; }
        public void AddToQueue(string url) => queue.Add(url);
        public void ClearQueue() => queue.Clear();
        public List<string> GetQueue => queue;
        public bool IsPlaying => playing;
        public IAudioClient GetClient => client;

        public MusicClass(IAudioClient client)
        {
            this.client = client;
        }
    }
}