using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace GladosV3.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        private static readonly string DownloadPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MusicTemp");

        public AudioService()
        {
            if (!Directory.Exists(DownloadPath))
                Directory.CreateDirectory(DownloadPath);
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "youtube-dl.exe"))) LoggingService.Log(LogSeverity.Error, "AudioService", "youtube-dl.exe not found!");
        }

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task<string> DownloadYoutube(string url)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            new Thread(() =>
            {
                string file;
                int count = 0;
                do
                {
                    file = Path.Combine(DownloadPath, "botsong" + ++count + ".mp3");
                } while (File.Exists(file));

                //youtube-dl.exe

                //Download Video
                var youtubedl = Process.Start(new ProcessStartInfo()
                {
                    FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "youtube-dl.exe"),
                    Arguments =
                        $"-x --prefer-ffmpeg --audio-format mp3 -o \"{file.Replace(".mp3", ".%(ext)s")}\" {url}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WorkingDirectory = DownloadPath
                });
                //Wait until download is finished
                youtubedl.WaitForExit();
                Thread.Sleep(1000);
                if (File.Exists(file))
                {
                    //Return MP3 Path & Video Title
                    tcs.SetResult(file);
                }
                else
                {
                    //Error downloading
                    tcs.SetResult(null);
                    LoggingService.Log(LogSeverity.Error, "youtube-dl",
                        $"Could not download Song, youtube-dl responded with:\n\r{youtubedl.StandardOutput.ReadToEnd()}");
                }
            }).Start();
            string result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl.exe failed to download!");

            //Remove \n at end of Line
            result = result.Replace("\n", "").Replace(Environment.NewLine, "");

            return result;

        }
        public async Task LeaveAudio(IGuild guild)
        {
            if (ConnectedChannels.TryRemove(guild.Id, out IAudioClient client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            if (ConnectedChannels.TryGetValue(guild.Id, out var client))
            {
                path = await DownloadYoutube(path);
                if (string.IsNullOrWhiteSpace(path))
                { await channel.SendMessageAsync("We're sorry, something went wrong on our side."); return; }
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
                await client.SetSpeakingAsync(true);
                using (var output = CreateStream(path).StandardOutput.BaseStream)
                using (var stream = client.CreatePCMStream(AudioApplication.Music, 98304, 200))
                {
                    try { await output.CopyToAsync(stream); }
                    catch
                    { await stream.FlushAsync(); if (File.Exists(path)) File.Delete(path); }
                    finally { await stream.FlushAsync(); if (File.Exists(path)) File.Delete(path); } //delete the file because we won't need it anymore
                }

                await client.SetSpeakingAsync(false);
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffmpeg.exe"),
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -filter:a \"volume=1.75\" -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = DownloadPath
                });
        }
    }
}