using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLaDOSV3.Module.ServerBackup.Models
{
    internal class BackupGuild
    {
        internal class UserC
        {
            public ulong UserID { get; set; }
            public string Nickname { get; set; }
            public UserC(IGuildUser u)
            {
                if (u == null) return;
                UserID = u.Id;
                Nickname = u.Nickname;
            }
        }
        public string ServerName { get; set; }
        public List<BackupCategory> Categories { get; set; }
        public List<BackupTextChannel> TextChannels { get; set; }
        public List<BackupAudioChannel> VoiceChannels { get; set; }
        public List<BackupRole> Roles { get; set; }
        public List<BackupBan> Bans { get; set; }
        public ulong EveryonePerms { get; set; }
        public List<UserC> Users { get; set; }
        public DateTime LastSave { get; } = DateTime.Now;
        public VerificationLevel VerificationLevel { get; set; }
        public byte[] Icon { get; set; }
        public byte[] SplashImage { get; set; }
        public string VoiceRegion { get; set; }
        public List<BackupEmoji> Emojis { get; set; }
        public DefaultMessageNotifications DefaultNotifications { get; set; }
        public ExplicitContentFilterLevel ContentFilter { get; set; }
        public SystemChannelMessageDeny SystemChannelFlags { get; set; }
        public int SystemChannelLocalId { get; set; }
        public int AFKTimeout { get; set; }
        public int AFKChannelLocalId { get; set; }
        public BackupGuild() => this.SaveGuild(null).GetAwaiter();
        public BackupGuild(SocketCommandContext ctx) => this.SaveGuild(ctx).GetAwaiter().GetResult();
        public static Task<List<BackupChatMessage>> GenChannelHiddenMessage() => Task.FromResult(
            new List<BackupChatMessage>(1) { new BackupChatMessage(null)
            {
                Author    = "Server backup",
                AuthorId  = 1,
                AuthorPic = "https://images.discordapp.net/avatars/536953835361665024/ea7664a628a7a4b772dd06ed81637334.png?size=512",
                Text      = "This channel was unavailable or hidden during backup!",
                Embeds    = Array.Empty<BackupEmbed>()
            }});

        public static async Task<SocketChannel> GetDiscordChannelFromLocalIdAsync(SocketGuild guild, int id)
        {
            var localId = -1;
            foreach (var cat in guild.CategoryChannels.OrderBy(c => c.Id))
            {
                if (++localId == id) return cat;
                foreach (var text in cat.Channels.Where((f, c) => f is SocketTextChannel).OrderBy(c => c.Id).Where(text => ++localId == id).Select(text => text)) return text;
                foreach (var voice in cat.Channels.Where((f, c) => f is SocketVoiceChannel).OrderBy(c => c.Id).Where(voice => ++localId == id).Select(voice => voice)) return voice;
            }

            foreach (var text in guild.TextChannels.Where(c => c.Category == null).OrderBy(c => c.Id).Where(text => ++localId == id).Select(text => text)) return text;
            foreach (var voice in guild.VoiceChannels.Where(c => c.Category == null).OrderBy(c => c.Id).Where(voice => ++localId == id).Select(voice => voice)) return voice;
            return null;
        }
        public async Task<BackupChannel> GetChannelFromLocalIdAsync(int id) => await this.ForEachChannelCheck(f => f.LocalChannelId == id);
        public static async Task<int> DiscordChannelToLocalIdAsync(SocketGuild guild, SocketChannel channel)
        {
            if (channel == null) return -1;
            var localId = -1;
            foreach (var cat in guild.CategoryChannels.OrderBy(c => c.Id))
            {
                localId++;
                if (cat.Id == channel.Id) return localId;
                foreach (var text in cat.Channels.Where((f, c) => f is SocketTextChannel).OrderBy(c => c.Id))
                {
                    localId++;
                    if (text.Id == channel.Id) return localId;
                }
                foreach (var voice in cat.Channels.Where((f, c) => f is SocketVoiceChannel).OrderBy(c => c.Id))
                {
                    localId++;
                    if (voice.Id == channel.Id) return localId;
                }
            }

            foreach (var text in guild.TextChannels.Where(c => c.Category == null).OrderBy(c => c.Id))
            {
                localId++;
                if (text.Id == channel.Id) return localId;
            }
            foreach (var voice in guild.VoiceChannels.Where(c => c.Category == null).OrderBy(c => c.Id))
            {
                localId++;
                if (voice.Id == channel.Id) return localId;
            }
            return -1;
        }
        private async Task<BackupChannel> ForEachChannelCheck(Func<BackupChannel, bool> check)
        {
            foreach (var category in Categories)
            {
                if (check(category)) return category;
                foreach (var text in category.TextChannels.Where(text => check(text))) return text;
                foreach (var voice in category.VoiceChannels.Where(voice => check(voice))) return voice;
            }
            foreach (var text in TextChannels.Where(text => check(text))) return text;
            return VoiceChannels.FirstOrDefault(voice => check(voice));
        }
        private static async Task RegexIdFix(string pattern, string input, Func<string, Task<string>> callback)
        {
            Regex r = new Regex(pattern, RegexOptions.Compiled);
            foreach (Match m in r.Matches(input))
            {
                if (!m.Success) continue;
                if (m.Captures.Count == 0) continue;
                var id = m.Groups[1].Value;
                input = await callback(id);
            }
        }
        public static Task<int> GetRoleId(SocketRole r)
        {
            var id = -1;
            foreach (var role in r.Guild.Roles.OrderBy(c => c.Position))
            {
                if (role.IsEveryone || role.IsManaged) continue;
                id++;
                if (r.Id == role.Id) return Task.FromResult(id);
            }
            return Task.FromResult(-1);
        }
        public static Task<int> GetEmojiId(SocketGuild g, GuildEmote emote)
        {
            var id = -1;
            foreach (var e in g.Emotes.OrderBy(c => c.Name))
            {
                id++;
                if (e.Id == emote.Id) return Task.FromResult(id);
            }
            return Task.FromResult(-1);
        }
        public static async Task<string> FixId(SocketGuild guild, string s, bool revert = false)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            await RegexIdFix("<#!?(\\d+)>", s, async f =>
            {
                if (!ulong.TryParse(f, out var cId)) return s;
                if (revert)
                {
                    var channel = await GetDiscordChannelFromLocalIdAsync(guild, (int)cId);
                    if (channel == null) return s;
                    s = s.Replace(f, $"{channel.Id}");
                }
                else
                {
                    var channel = guild.GetChannel(cId);
                    if (channel == null) return s;
                    s = s.Replace(f, $"{await DiscordChannelToLocalIdAsync(guild, channel)}");
                }
                return s;
            });
            await RegexIdFix("<@&!?(\\d+)>", s, async f =>
            {
                if (!ulong.TryParse(f, out var cId)) return s;
                if (revert)
                {
                    var id = -1;
                    SocketRole role = null;
                    foreach (var r in guild.Roles.Where(r => r.IsEveryone && !r.IsManaged).Where(r => ++id == (int)cId).Select(r => r)) { role = r; break; }
                    if (role == null) return s;
                    s = s.Replace(f, $"{role.Id}");
                }
                else
                {
                    var role = guild.GetRole(cId);
                    if (role == null) return s;
                    s = s.Replace(f, $"{await GetRoleId(role)}");
                }
                return s;
            });
            await RegexIdFix("<a?:.+?(?=:):(\\d+)>", s, async f =>
            {
                if (!ulong.TryParse(f, out var cId)) return s;
                if (revert)
                {
                    var id = -1;
                    GuildEmote emote = guild.Emotes.OrderBy(c => c.Name).Where(e => ++id == (int)cId).Select(e => e).FirstOrDefault();
                    if (emote == null) return s;
                    s = s.Replace(f, $"{emote.Id}");
                }
                else
                {
                    var emote = guild.Emotes.FirstOrDefault(f => f.Id == cId);
                    if (emote == null) return s;
                    s = s.Replace(f, $"{await GetEmojiId(guild, emote)}");
                }
                return s;
            });
            return s;
        }
        private async Task SaveGuild(SocketCommandContext ctx)
        {
            if (ctx == null) return;
            ServerName = ctx.Guild.Name;
            EveryonePerms = ctx.Guild.EveryoneRole.Permissions.RawValue;
            var channelId = -1;
            Categories = ctx.Guild.CategoryChannels.OrderBy(c => c.Id).Select(c => new BackupCategory(c, ref channelId)).ToList();
            TextChannels = ctx.Guild.TextChannels.Where(c => c.Category == null).OrderBy(c => c.Id).Select(c => new BackupTextChannel(c, ref channelId)).ToList();
            VoiceChannels = ctx.Guild.VoiceChannels.Where(c => c.Category == null).OrderBy(c => c.Id).Select(c => new BackupAudioChannel(c, ref channelId)).ToList();
            AFKChannelLocalId = await DiscordChannelToLocalIdAsync(ctx.Guild, ctx.Guild.AFKChannel);
            SystemChannelLocalId = await DiscordChannelToLocalIdAsync(ctx.Guild, ctx.Guild.SystemChannel);
            Roles = ctx.Guild.Roles.Where(r => !r.IsEveryone && !r.IsManaged).OrderBy(c => c.Position).Select(r => new BackupRole(r)).ToList();
            Bans = ctx.Guild.GetUser(ctx.Client.CurrentUser.Id).GuildPermissions.Has(GuildPermission.BanMembers) ? (await ctx.Guild.GetBansAsync()).Select(b => new BackupBan(b)).ToList() : new List<BackupBan>(0);
            Users = ctx.Guild.Users.Select(u => new UserC(u)).ToList();
            VerificationLevel = ctx.Guild.VerificationLevel;
            VoiceRegion = ctx.Guild.VoiceRegionId;
            DefaultNotifications = ctx.Guild.DefaultMessageNotifications;
            ContentFilter = ctx.Guild.ExplicitContentFilter;
            AFKTimeout = ctx.Guild.AFKTimeout;
            SystemChannelFlags = ctx.Guild.SystemChannelFlags;
            using var wc = new WebClient();
            Emojis = ctx.Guild.Emotes.OrderBy(c => c.Name).Select(e => new BackupEmoji(e)).ToList();
            SplashImage = string.IsNullOrWhiteSpace(ctx.Guild.SplashUrl) ? Array.Empty<byte>() : wc.DownloadData(ctx.Guild.SplashUrl);
            Icon = string.IsNullOrWhiteSpace(ctx.Guild.IconUrl) ? Array.Empty<byte>() : wc.DownloadData(ctx.Guild.IconUrl);
        }
    }
}