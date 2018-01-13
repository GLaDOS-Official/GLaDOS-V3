﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using GladosV3.Services;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using GladosV3.Attributes;

namespace GladosV3.Modules
{
    [Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        [Summary("Displays bot info.")]
        [Remarks("info")]
        public async Task Info()
        {
            IDMChannel DM = await Context.Message.Author.GetOrCreateDMChannelAsync();
            var waitMessage = DM.SendMessageAsync("Please wait!").GetAwaiter().GetResult();
            var date = DateTime.Now;
            var uptime = (date - Process.GetCurrentProcess().StartTime).ToString(@"d'd 'hh'h 'mm'm 'ss's'");
            string heapsize = ToFileSize2(GC.GetTotalMemory(true));
            var guildcount = Context.Client.Guilds.Count;
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
                $"- Library: Discord.Net ({DiscordConfig.APIVersion})\n" +
                $"- Runtime: {PlatformServices.Default.Application.RuntimeFramework.Identifier.Replace("App", String.Empty)} {PlatformServices.Default.Application.RuntimeFramework.Version} {IntPtr.Size * 8}-bit\n" +
                $"- System: {System.Runtime.InteropServices.RuntimeInformation.OSDescription} {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower()}\n" +
                $"- Up-time: {uptime}\n" +
                $"- Ping: {Context.Client.Latency} ms\n"+
                $"- Thread running: {((IEnumerable)Process.GetCurrentProcess().Threads).OfType<ProcessThread>().Count(t => t.ThreadState == ThreadState.Running)} out of {Process.GetCurrentProcess().Threads.Count}\n" +
                $"- RAM usage: {ToFileSize2(Process.GetCurrentProcess().PagedMemorySize64)}\n" +
                $"- CPU usage: {GetCpuUsage():N1}%\n" +
                $"- Heap Size: {heapsize}\n" +
                $"- Owner of the bot: <@{IsOwner.GetOwner(Context).GetAwaiter().GetResult()}>\n" +
                $"- Author of the bot: <@195225230908588032> \n\n" +
                 
                $"{Format.Bold("Stats")}\n" +
                $"- Servers: {guildcount}\n"
            );
            if (Context.Guild != null)
            {
                var channelscount = Context.Guild.Channels.Count;
                var userscount = Context.Guild.Users.Count();
                message += $"- Channels: {channelscount} (in this guild) \n" +
                           $"- Users: {userscount} (in this guild) \n";
            }
            message += $"- Channels: {channelCount} (total) \n" +
                       $"- Users: {userCount} (total) \n";
            await waitMessage.ModifyAsync(u => u.Content = message);
        }
        [Command("ping")]
        [Summary("Shows bot's latency.")]
        [Remarks("ping")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! {(Context.Client as DiscordSocketClient).Latency} ms :ping_pong:");
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
                    $"[Click to Invite](https://discordapp.com/oauth2/authorize?client_id="+Context.Client.CurrentUser.Id+"&scope=bot&permissions=2146958591)"
            };
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync("", false, eb);
        }
        [Command("user")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Remarks("user (mention)")]
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
                    Description = $"Created on {userInfo.CreatedAt.ToString().Remove(userInfo.CreatedAt.ToString().Length - 6)}. That is {(int)(DateTime.Now.Subtract(userInfo.CreatedAt.DateTime).TotalDays)} days ago!", //{(int)(DateTime.Now.Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)}
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | {userInfo.Username} ID: {userInfo.Id}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
                var socketUser = userInfo as SocketGuildUser;
                eb.AddField((x) =>
                {
                    string[] array = new []{"Offline", "Online", "Idle", "AFK", "Do not disturb", "Invisible"};
                    x.Name = "Status";
                    x.IsInline = true;
                    x.Value = array[(int)userInfo.Status];
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
                            $"{socketUser?.JoinedAt.ToString().Remove(socketUser.JoinedAt.ToString().Length - 6)}\n({(int) DateTime.Now.ToUniversalTime().Subtract(((DateTimeOffset) socketUser?.JoinedAt).DateTime).TotalDays} days ago)";
                    });
                    string permissions = "";
                    int take = 0;
                    var list = (userInfo as SocketGuildUser)?.GuildPermissions.ToList();
                    list?.ForEach(x =>
                    {
                        if (list.Contains(GuildPermission.Administrator))
                        { permissions = "Administrator"; return;} 
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
                        //string roles = socketUser.Roles.Where(role => role.Name != "@everyone").Aggregate("", (current, role) => current + $"{role.Name}, ");
                        string roles = "";
                        if(socketUser != null && socketUser.Roles.Count > 1) {
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


                /*
                eb.AddField((efb) =>
                {
                    efb.Name = "User Info";
                    efb.IsInline = true;
                    efb.Value = $"**Name + Discriminator:** \t{userInfo.Username}#{userInfo.Discriminator} \n" +
                                $"**ID** \t{userInfo.Id}\n" +
                                $"**Created at:** \t{userInfo.CreatedAt.ToString().Remove(userInfo.CreatedAt.ToString().Length -6)} \n" +
                                $"**Status:** \t{userInfo.Status}\n" +
                                $"**Avatar:** \t[Link]({userInfo.AvatarUrl})";
                });*/

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
                    Title = $"{Context.Guild.Name} info",

                    Description = $"Created on {Context.Guild.CreatedAt.ToString().Remove(Context.Guild.CreatedAt.ToString().Length - 6)}. That's {(int)(DateTime.Now.ToUniversalTime().Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)} days ago!",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | Guild ID: {Context.Guild.Id}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
                var guild = ((SocketGuild)Context.Guild);
                var GuildOwner = Context.Guild.GetUser(Context.Guild.OwnerId);
                int online = 0;
                foreach (var u in guild.Users)
                    if (u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline)
                        online++;

                eb.AddField((x) =>
                {
                    x.Name = "Owner";
                    x.IsInline = true;
                    x.Value = GuildOwner.Username;
                });

                eb.AddField((x) =>
                {
                    x.Name = "Members";
                    x.IsInline = true;
                    x.Value = $"{online} / {(Context.Guild).MemberCount}";
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

                int voice = Context.Guild.VoiceChannels.Count;
                int text = Context.Guild.TextChannels.Count;
                eb.AddField((x) =>
                {
                    x.Name = "Channels";
                    x.IsInline = true;
                    x.Value = $"{text} text, {voice} voice";
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
                    x.Value = $"{Context.Guild.Emotes.Count}";
                });

                eb.AddField((x) =>
                {
                    x.Name = "Avatar Url";
                    x.IsInline = true;
                    x.Value = $"[Click to view]({avatarURL})";
                });
                if (Context.Guild.Emotes.Count != 0)
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
