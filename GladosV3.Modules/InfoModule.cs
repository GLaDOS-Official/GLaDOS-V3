using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using GladosV3.Helpers;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GladosV3.Module.Default
{
    [Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private static string _infoMessage;
        private static DiscordSocketClient _client;
        private readonly Thread t = new Thread(new ThreadStart(RefreshMessage));
        private static BotSettingsHelper<string> _botSettingsHelper;

        private static void RefreshMessage()
        {
            while (true)
            {
                static string ToFileSize2(double size)
                {
                    var scale = 1024;
                    var kb = 1 * scale;
                    var mb = kb * scale;
                    var gb = mb * scale;
                    var tb = gb * scale;

                    if (size < kb)
                        return size + " Bytes";
                    if (size < mb)
                        return (size / kb).ToString("0.## KB", CultureInfo.InvariantCulture);
                    if (size < gb)
                        return (size / mb).ToString("0.## MB", CultureInfo.InvariantCulture);
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (size < tb)
                        return (size / gb).ToString("0.## GB", CultureInfo.InvariantCulture);

                    return (size / tb).ToString("0.## TB", CultureInfo.InvariantCulture);
                }

                static float GetCpuUsage()
                {
                    using var cpuCounter = new PerformanceCounter("Process", "% Processor Time",
                        Process.GetCurrentProcess().ProcessName); //, Process.GetCurrentProcess().ProcessName,true
                    cpuCounter.NextValue();
                    Thread.Sleep(1000);
                    return cpuCounter.NextValue();
                }
                _infoMessage = (
                    $"{Format.Bold("Info")}\n" +
                    $"- Library: Discord.Net ({DiscordConfig.APIVersion.ToString(CultureInfo.InvariantCulture)})\n" +
                    $"- Runtime: {PlatformServices.Default.Application.RuntimeFramework.Identifier.Replace("App", string.Empty, StringComparison.OrdinalIgnoreCase)} {PlatformServices.Default.Application.RuntimeFramework.Version} {(IntPtr.Size * 8).ToString(CultureInfo.InvariantCulture)}-bit\n" +
                    $"- System: {RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture.ToString().ToUpperInvariant()}\n" +
                    $"- Up-time: {(DateTime.Now - Process.GetCurrentProcess().StartTime):d\'d \'hh\'h \'mm\'m \'ss\'s\'}\n" +
                    $"- Heartbeat: {_client.Latency.ToString(CultureInfo.InvariantCulture)} ms\n" +
                    $"- Thread running: {Process.GetCurrentProcess().Threads.OfType<ProcessThread>().Count(t => t.ThreadState == System.Diagnostics.ThreadState.Running).ToString(CultureInfo.InvariantCulture)} out of {Process.GetCurrentProcess().Threads.Count.ToString(CultureInfo.InvariantCulture)}\n" +
                    $"- RAM usage: {ToFileSize2(Process.GetCurrentProcess().PagedMemorySize64)}\n" +
                    $"- CPU usage: {(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{GetCpuUsage():N1}%" : "Can not determine CPU usage 😦")}\n" +
                    $"- Heap Size: {ToFileSize2(GC.GetTotalMemory(true))}\n" +
                    $"- Owner of the bot: <@{ulong.Parse(_botSettingsHelper["ownerID"], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)}>\n" +
                    $"- Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\n" +
                    "- Author of the bot: <@419568355771416577>\n\n" +

                    $"{Format.Bold("Stats")}\n" +
                    $"- Servers: {_client.Guilds.Count.ToString(CultureInfo.InvariantCulture)}\n"
                );
                Thread.Sleep(60000);
            }
        }
        public InfoModule(DiscordSocketClient socketClient, BotSettingsHelper<string> botSettingsHelper)
        {
            if (_client != null && InfoModule._botSettingsHelper != null) return;
            InfoModule._botSettingsHelper = botSettingsHelper;
            _client = socketClient;
            this.t.Start();
        }
        [Command("info")]
        [Summary("Displays bot info.")]
        [Remarks("info")]
        public async Task Info()
        {
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(true);
            if (Context.Guild != null)
            {
                _infoMessage += $"- Channels: {Context.Guild.Channels.Count} (in {Context.Guild.Name})\n" +
                           $"- Users: {Context.Guild.Users.Count} (in {Context.Guild.Name})\n";
            }
            _infoMessage += $"- Channels: {Context.Client.Guilds.Sum(guild => guild.Channels.Count)} (total)\n" +
                           $"- Users: {Context.Client.Guilds.Sum(guild => guild.Users.Count)} (total)\n";
            await dm.SendMessageAsync(_infoMessage).ConfigureAwait(false);
            await dm.CloseAsync().ConfigureAwait(false);
        }

        [Command("ping")]
        [Summary("Shows bot's latency.")]
        [Remarks("ping")]
        [Timeout(5, 1, Measure.Minutes)]
        public async Task Ping()
        {
            var sw = Stopwatch.StartNew();
            var message = await this.ReplyAsync("Ping!").ConfigureAwait(false);
            sw.Stop();
            var usual = (sw.ElapsedMilliseconds > 2500 || Context.Client.Latency > 5000) ? "there could be something wrong." : "there should be nothing wrong.";
            await message.ModifyAsync(delegate (MessageProperties properties)
            {
                properties.Content = $"🏓 Pong!\n👂 Response time: {sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)}ms.\n❤ Websocket heartbeat: {Context.Client.Latency}ms.\n This means that {usual}";
            }).ConfigureAwait(false);
        }

        [Command("invite")]
        [Remarks("invite")]
        [Summary("Gives an invite link to invite me to your own guild!")]
        public async Task InviteAsync()
        {
            var eb = new EmbedBuilder
            {
                Title = $"Invite {Context.Client.CurrentUser.Username}",
                Color = new Color(4, 97, 247),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | ID: {Context.User.Id}",
                    IconUrl = (Context.User.GetAvatarUrl())
                },
                Description =
                    $"Just uncheck some of the permissions you don't like, this might break {Context.Client.CurrentUser.Username} though. At least give me these permissions:\n" +
                    "Read/Send Messages, Embed Links, Attach Files, Send Embeds, Add Reactions, Read Message History\n" +
                    "For Mod usage higher perms are needed!\n" +
                    $"[Click to Invite](https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions={GuildPermissions.All.RawValue})"
            };
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(true);
            await dm.SendMessageAsync("", false, eb.Build()).ConfigureAwait(false);
            await dm.CloseAsync().ConfigureAwait(false);
        }

        [Command("user")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Remarks("user [mention]")]
        [Alias("userinfo", "whois")]
        [Timeout(3, 1, Measure.Minutes)]
        public async Task UserInfo([Summary("The (optional) user to get info for")] SocketUser user = null)
        {
            try
            {
                var userInfo = user ?? Context.User;
                var avatarUrl = userInfo.GetAvatarUrl() ??
                                userInfo.GetDefaultAvatarUrl();
                //var oof = userInfo.ActiveClients.Any(r => r == ClientType.Mobile);
                //TODO: make this simpler for mobile
                var eb = new EmbedBuilder
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = (avatarUrl),
                    Title = $"{userInfo.Username}#{userInfo.Discriminator}",
                    Description = $"Created on {userInfo.CreatedAt.ToString(CultureInfo.InvariantCulture).Remove(userInfo.CreatedAt.ToString(CultureInfo.InvariantCulture).Length - 6)}. That is {(int)(DateTime.Now.ToUniversalTime().Subtract(userInfo.CreatedAt.DateTime).TotalDays)} days ago!", //{(int)(DateTime.Now.Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)}
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | ID: {Context.User.Id}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
                var socketUser = userInfo as SocketGuildUser;
                eb.AddField(x =>
                {
                    x.Name = "Status";
                    x.IsInline = true;
                    x.Value = new[] { "Offline", "Online", "Idle", "AFK", "Do not disturb", "Invisible" }[(int)userInfo.Status];
                });
                if (!string.IsNullOrWhiteSpace(userInfo.Activity?.Name))
                {
                    eb.AddField(x =>
                    {
                        x.Name = "Game";
                        x.IsInline = true;
                        x.Value = $"{(userInfo.Activity.Name)}";
                    });
                }

                eb.AddField(x =>
                {
                    x.Name = "ID";
                    x.IsInline = true;
                    x.Value = userInfo.Id.ToString(CultureInfo.InvariantCulture);
                });
                if (Context.Guild != null)
                {
                    if (socketUser?.Nickname != null)
                    {
                        eb.AddField(x =>
                        {
                            x.Name = "Nickname";
                            x.IsInline = true;
                            x.Value = $"{(socketUser?.Nickname == null ? "*none*" : $"{socketUser.Nickname}")}";
                        });
                    }
                }

                eb.AddField(x =>
                {
                    x.Name = "Discriminator";
                    x.IsInline = true;
                    x.Value = $"#{userInfo.Discriminator}";
                });

                eb.AddField(x =>
                {
                    x.Name = "Avatar";
                    x.IsInline = true;
                    x.Value = $"[Click to View]({avatarUrl})";
                });
                if (Context.Guild != null)
                {
                    eb.AddField(x =>
                    {
                        x.Name = "Joined Guild";
                        x.IsInline = true;
                        if (socketUser?.JoinedAt != null)
                            x.Value =
                                $"{socketUser?.JoinedAt.ToString().Remove(socketUser.JoinedAt.ToString().Length - 6)}\n({(int)DateTime.Now.ToUniversalTime().Subtract(((DateTimeOffset)socketUser?.JoinedAt).DateTime).TotalDays} days ago)";
                    });
                    var permissions = string.Empty;
                    var list = (userInfo as SocketGuildUser)?.GuildPermissions.ToList();
                    list?.ForEach(x =>
                    {
                        if (list.Contains(GuildPermission.Administrator))
                        { permissions = "Administrator"; return; }
                        permissions += x + ", ";
                        if (list.Last() == x)
                            permissions = permissions.Remove(permissions.Length - 2);
                    });
                    eb.AddField(x =>
                    {
                        x.Name = "Guild Permissions";
                        x.IsInline = true;
                        x.Value = $"{(string.IsNullOrWhiteSpace(permissions) ? "*none*" : permissions)}";
                    });
                    eb.AddField(x =>
                    {
                        var roles = string.Empty;

                        if (socketUser != null && socketUser.Roles.Count > 1)
                        {
                            if (socketUser.Roles.Count == 2)
                                roles = socketUser.Roles.ToArray()[1].Name;
                            else
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
                        x.Value = $"{(string.IsNullOrWhiteSpace(roles) ? "*none*" : $"{roles}")}";
                    });
                }
                for (var i = 0; i <= Tools.RoundToDividable<int>(eb.Fields.Count, 3) - eb.Fields.Count; i++)
                    eb.AddBlankField(true);

                await Context.Channel.SendMessageAsync("", false, eb.Build()).ConfigureAwait(false);
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
        [Timeout(3, 1, Measure.Minutes)]
        public async Task ServerInfo()
        {
            Context.Guild.GetUser(Context.Client.CurrentUser.Id).GuildPermissions.ToList();
            try
            {
                var avatarUrl = Context.Guild.IconUrl ?? "http://ravegames.net/ow_userfiles/themes/theme_image_22.jpg";
                var eb = new EmbedBuilder
                {
                    Color = new Color(4, 97, 247),
                    ThumbnailUrl = (avatarUrl),
                    Title = $"{Context.Guild.Name} ({Context.Guild.Id})",
                    Description = $"Created on {Context.Guild.CreatedAt.ToString(CultureInfo.InvariantCulture).Remove(Context.Guild.CreatedAt.ToString(CultureInfo.InvariantCulture).Length - 6)}. That's {Math.Round(DateTime.Now.ToUniversalTime().Subtract(Context.Guild.CreatedAt.DateTime).TotalDays)} days ago!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator} | Guild ID: {Context.Guild.Id}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
                eb.AddField(x =>
                {
                    x.Name = "Owner";
                    x.IsInline = true;
                    x.Value = Context.Guild.GetUser(Context.Guild.OwnerId).Username;
                });

                eb.AddField(x =>
                {
                    x.Name = "Members";
                    x.IsInline = true;
                    x.Value = $"{(Context.Guild.Users.Count(u => u.Status != UserStatus.Invisible && u.Status != UserStatus.Offline)).ToString(CultureInfo.InvariantCulture)} / {(Context.Guild).MemberCount}";
                });

                eb.AddField(x =>
                {
                    x.Name = "Region";
                    x.IsInline = true;
                    x.Value = Context.Guild.VoiceRegionId.ToUpper(CultureInfo.InvariantCulture);
                });

                eb.AddField(x =>
                {
                    x.Name = "Roles";
                    x.IsInline = true;
                    x.Value = string.Empty + Context.Guild.Roles.Count;
                });

                eb.AddField(x =>
                {
                    x.Name = "Channels";
                    x.IsInline = true;
                    x.Value = $"{Context.Guild.TextChannels.Count} text, {Context.Guild.VoiceChannels.Count} voice";
                });

                eb.AddField(x =>
                {
                    x.Name = "AFK Channel";
                    x.IsInline = true;
                    x.Value = $"{(Context.Guild.AFKChannel == null ? $"No AFK Channel" : $"{Context.Guild.AFKChannel.Name}\n*in {Context.Guild.AFKTimeout / 60} Min*")}";
                });

                eb.AddField(x =>
                {
                    x.Name = "Total Emojis";
                    x.IsInline = true;
                    x.Value = $"{(Context.Guild.Emotes.Count == 0 ? "Anti-emoji" : Context.Guild.Emotes.Count.ToString(CultureInfo.InvariantCulture))}";
                });

                eb.AddField(x =>
                {
                    x.Name = "Avatar Url";
                    x.IsInline = true;
                    x.Value = $"[Click to view]({avatarUrl})";
                });
                if (Context.Guild.Emotes.Count > 0)
                {
                    eb.AddField(x =>
                    {
                        x.Name = "Emojis";
                        x.IsInline = false;

                        var val = string.Empty;

                        foreach (var e in Context.Guild.Emotes)
                        {
                            if (val.Length < 950)
                                val += $"<:{e.Name}:{e.Id}> ";
                        }
                        x.Value = (string.IsNullOrWhiteSpace(val) ? "*none*" : val);
                    });
                }
                await Context.Channel.SendMessageAsync(embed: eb.Build()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}