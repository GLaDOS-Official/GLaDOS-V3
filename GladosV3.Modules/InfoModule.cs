using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using Microsoft.Extensions.PlatformAbstractions;

namespace GladosV3.Module.Default
{
    [Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        [Summary("Displays bot info.")]
        [Remarks("info")]
        [Timeout(1, 1, Measure.Minutes)]
        public async Task Info()
        {
            IDMChannel DM = await Context.Message.Author.GetOrCreateDMChannelAsync();
            var waitMessage = DM.SendMessageAsync("Please wait!").GetAwaiter().GetResult();
            string ToFileSize2(Double size)
            {
                int scale = 1024;
                var kb = 1 * scale;
                var mb = kb * scale;
                long gb = mb * scale;
                long tb = gb * scale;

                if (size < kb)
                    return size + " Bytes";
                else if (size < mb)
                    return ((Double)size / kb).ToString("0.## KB");
                else if (size < gb)
                    return ((Double)size / mb).ToString("0.## MB");
                else if (size < tb)
                    return ((Double)size / gb).ToString("0.## GB");
                else
                    return ((Double)size / tb).ToString("0.## TB");
            }
            float GetCpuUsage()
            {
                var cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName); //, Process.GetCurrentProcess().ProcessName,true
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(1000);
                return cpuCounter.NextValue();
            }
            var channelCount = 0;
            var userCount = 0;

            foreach (var g in Context.Client.Guilds)
            {
                channelCount += g.Channels.Count;
                userCount += g.MemberCount;
            }

            var message = (
                $"{Format.Bold("Info")}\n" +
                $"- Library: Discord.Net ({DiscordConfig.APIVersion.ToString()})\n" +
                $"- Runtime: {PlatformServices.Default.Application.RuntimeFramework.Identifier.Replace("App", String.Empty)} {PlatformServices.Default.Application.RuntimeFramework.Version} {(IntPtr.Size * 8).ToString()}-bit\n" +
                $"- System: {System.Runtime.InteropServices.RuntimeInformation.OSDescription} {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower()}\n" +
                $"- Up-time: {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"d'd 'hh'h 'mm'm 'ss's'")}\n" +
                $"- Ping: {Context.Client.Latency.ToString()} ms\n" +
                $"- Thread running: {Process.GetCurrentProcess().Threads.OfType<ProcessThread>().Count(t => t.ThreadState == ThreadState.Running).ToString()} out of {Process.GetCurrentProcess().Threads.Count.ToString()}\n" +
                $"- RAM usage: {ToFileSize2(Process.GetCurrentProcess().PagedMemorySize64)}\n" +
                $"- CPU usage: {GetCpuUsage():N1}%\n" +
                $"- Heap Size: {ToFileSize2(GC.GetTotalMemory(true))}\n" +
                $"- Owner of the bot: <@{IsOwner.GetOwner(Context).GetAwaiter().GetResult().ToString()}>\n" +
                $"- Version: {FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion}  (Compiled on {GladosV3.DateCompiled.ToString}) \n" +
                "- Author of the bot: <@195225230908588032> \n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Servers: {Context.Client.Guilds.Count.ToString()}\n"
            );

            if (Context.Guild != null)
            {
                var channelscount = Context.Guild.Channels.Count;
                var userscount = Context.Guild.Users.Count;
                message += $"- Channels: {channelscount.ToString()} (in {Context.Guild.Name}) \n" +
                           $"- Users: {userscount.ToString()} (in {Context.Guild.Name}) \n";
            }
            message += $"- Channels: {channelCount.ToString()} (total) \n" +
                       $"- Users: {userCount.ToString()} (total) \n";
            await waitMessage.ModifyAsync(u => u.Content = message);
        }

        [Command("ping")]
        [Summary("Shows bot's latency.")]
        [Remarks("ping")]
        public async Task Ping()
        {
            var sw = Stopwatch.StartNew();
            var message = await ReplyAsync("Ping!").ConfigureAwait(false);
            sw.Stop();
            var usual = (sw.ElapsedMilliseconds > 10000) ? "there could be something wrong." : "there's nothing wrong";
            await message.ModifyAsync(delegate (MessageProperties properties)
            {
                properties.Content = $":ping_pong: Pong! {sw.ElapsedMilliseconds.ToString()}ms. It means that {usual}.";
            });
        }

        [Command("invite")]
        [Remarks("invite")]
        [Summary("Gives an invite link to invite me to your own guild!")]
        public async Task InviteAsync()
        {
            var eb = new EmbedBuilder()
            {
                Title = $"Invite {Context.Client.CurrentUser.Username}",
                Color = new Color(4, 97, 247),
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                    IconUrl = (Context.User.GetAvatarUrl())
                },
                Description =
                    $"Just uncheck some of the permissions you don't like, this might break {Context.Client.CurrentUser.Username} though. At least give me these permissions:\n" +
                    "Read/Send Messages, Embed Links, Attach Files, Send Embeds, Add Reactions, Read Message History\n" +
                    "For Mod usage higher perms are needed!\n" +
                    $"[Click to Invite](https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions=2146958591)"
            };
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync("", false, eb);
        }

        [Command("user")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Remarks("user [mention]")]
        [Alias("userinfo", "whois")]
        public async Task UserInfo([Summary("The (optional) user to get info for")] SocketUser user = null)
        {
            try
            {
                var userInfo = user ?? Context.User;
                var avatarUrl = userInfo.GetAvatarUrl() ??
                                "http://ravegames.net/ow_userfiles/themes/theme_image_22.jpg";
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = (avatarUrl),
                    Title = $"{userInfo.Username}#{userInfo.Discriminator}",
                    Description = $"Created on {userInfo.CreatedAt.ToString().Remove(userInfo.CreatedAt.ToString().Length - 6)}. That is {(int)(DateTime.Now.ToUniversalTime().Subtract(userInfo.CreatedAt.DateTime).TotalDays)} days ago!", //{(int)(DateTime.Now.Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)}
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | {userInfo.Username} ID: {userInfo.Id}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
                var socketUser = userInfo as SocketGuildUser;
                eb.AddField((x) =>
                {
                    x.Name = "Status";
                    x.IsInline = true;
                    x.Value = new[] { "Offline", "Online", "Idle", "AFK", "Do not disturb", "Invisible" }[(int)userInfo.Status];
                });
                if (userInfo.Game.HasValue)
                {
                    eb.AddField((x) =>
                    {
                        x.Name = "Game";
                        x.IsInline = true;
                        x.Value = $"{(userInfo.Game.Value.Name)}";
                    });
                }

                eb.AddField((x) =>
                {
                    x.Name = "ID";
                    x.IsInline = true;
                    x.Value = userInfo.Id.ToString();
                });
                if (Context.Guild != null)
                {
                    if (socketUser?.Nickname != null)
                    {
                        eb.AddField((x) =>
                        {
                            x.Name = "Nickname";
                            x.IsInline = true;
                            x.Value = $"{(socketUser?.Nickname == null ? "*none*" : $"{socketUser.Nickname}")}";
                        });
                    }
                }

                eb.AddField((x) =>
                {
                    x.Name = "Discriminator";
                    x.IsInline = true;
                    x.Value = $"#{userInfo.Discriminator}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar";
                    x.IsInline = true;
                    x.Value = $"[Click to View]({avatarUrl})";
                });
                if (Context.Guild != null)
                {
                    eb.AddField((x) =>
                    {
                        x.Name = "Joined Guild";
                        x.IsInline = true;
                        x.Value =
                            $"{socketUser?.JoinedAt.ToString().Remove(socketUser.JoinedAt.ToString().Length - 6)}\n({(int)DateTime.Now.ToUniversalTime().Subtract(((DateTimeOffset)socketUser?.JoinedAt).DateTime).TotalDays} days ago)";
                    });
                    string permissions = "";
                    int take = 0;
                    var list = (userInfo as SocketGuildUser)?.GuildPermissions.ToList();
                    list?.ForEach(x =>
                    {
                        if (list.Contains(GuildPermission.Administrator))
                        { permissions = "Administrator"; return; }
                        permissions += x.ToString() + ", ";
                        take++;
                        if (list.Last() == x)
                            permissions = permissions.Remove(permissions.Length - 2);
                    });
                    eb.AddField((x) =>
                    {
                        x.Name = "Guild Permissions";
                        x.IsInline = true;
                        x.Value = $"{(String.IsNullOrWhiteSpace(permissions) ? "*none*" : permissions)}";
                    });
                    eb.AddField((x) =>
                    {
                        string roles = "";

                        if (socketUser != null && socketUser.Roles.Count > 1)
                        {
                            foreach (var role in socketUser.Roles)
                            {
                                if (role.Name != "@everyone" && socketUser.Roles.ToList().Last() != role)
                                    roles += $"{role.Name}, ";
                                else if (socketUser.Roles.ToList().Last() == role)
                                    roles = roles.Remove(roles.Length - 2);
                            }
                        }
                        x.Name = "Roles";
                        x.IsInline = true;
                        x.Value = $"{(String.IsNullOrWhiteSpace(roles) ? "*none*" : $"{roles}")}";
                    });
                }

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Command("serverinfo")]
        [Summary("Returns info about the current Guild")]
        [Remarks("serverinfo")]
        [Alias("server", "guild")]
        [RequireContext(ContextType.Guild)]
        public async Task ServerInfo()
        {
            try
            {
                var avatarURL = Context.Guild.IconUrl ?? "http://ravegames.net/ow_userfiles/themes/theme_image_22.jpg";
                var eb = new EmbedBuilder()
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = (avatarURL),
                    Title = $"{Context.Guild.Name} ({Context.Guild.Id})",
                    Description = $"Created on {Context.Guild.CreatedAt.ToString().Remove(Context.Guild.CreatedAt.ToString().Length - 6)}. That's {Math.Round(DateTime.Now.ToUniversalTime().Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)} days ago!",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | Guild ID: {Context.Guild.Id}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
                eb.AddField((x) =>
                {
                    x.Name = "Owner";
                    x.IsInline = true;
                    x.Value = Context.Guild.GetUser(Context.Guild.OwnerId).Username;
                });

                eb.AddField((x) =>
                {
                    x.Name = "Members";
                    x.IsInline = true;
                    x.Value = $"{(((SocketGuild)Context.Guild).Users.Count(u => u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline)).ToString()} / {(Context.Guild).MemberCount}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Region";
                    x.IsInline = true;
                    x.Value = Context.Guild.VoiceRegionId.ToUpper();
                });

                eb.AddField((x) =>
                {
                    x.Name = "Roles";
                    x.IsInline = true;
                    x.Value = "" + Context.Guild.Roles.Count;
                });

                eb.AddField((x) =>
                {
                    x.Name = "Channels";
                    x.IsInline = true;
                    x.Value = $"{Context.Guild.TextChannels.Count} text, {Context.Guild.VoiceChannels.Count} voice";
                });

                eb.AddField((x) =>
                {
                    x.Name = "AFK Channel";
                    x.IsInline = true;
                    x.Value = $"{(Context.Guild.AFKChannel == null ? $"No AFK Channel" : $"{Context.Guild.AFKChannel.Name}\n*in {(int)(Context.Guild.AFKTimeout / 60)} Min*")}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Total Emojis";
                    x.IsInline = true;
                    x.Value = $"{(Context.Guild.Emotes.Count == 0 ? "Anti-emoji" : Context.Guild.Emotes.Count.ToString())}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar Url";
                    x.IsInline = true;
                    x.Value = $"[Click to view]({avatarURL})";
                });
                if (Context.Guild.Emotes.Count > 0)
                {
                    eb.AddField((x) =>
                    {
                        x.Name = "Emojis";
                        x.IsInline = false;

                        string val = "";

                        foreach (var e in Context.Guild.Emotes)
                        {
                            if (val.Length < 950)
                                val += $"<:{e.Name}:{e.Id}> ";
                        }

                        x.Value = (string.IsNullOrWhiteSpace(val) ? "*none*" : val);
                    });
                }
                await Context.Channel.SendMessageAsync("", false, eb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}