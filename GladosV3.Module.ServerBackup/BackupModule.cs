using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using GLaDOSV3.Module.ServerBackup.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Module.ServerBackup
{
    public class BackupModule : ModuleBase<SocketCommandContext>
    {
        private class OrderedPropertiesContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                return props.OrderBy(p => p.PropertyName).ToList();
            }
        }
        private class OrderedExpandoPropertiesConverter : ExpandoObjectConverter
        {
            public override bool CanWrite => true;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var expando = (IDictionary<string, object>)value;
                var orderedDictionary = expando.OrderBy(x => x.Key).ToDictionary(t => t.Key, t => t.Value);
                serializer.Serialize(writer, orderedDictionary);
            }
        }
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new OrderedPropertiesContractResolver(),
                    Converters = { new OrderedExpandoPropertiesConverter() }
                };
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, settings);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                writer?.Close();
            }
        }
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new OrderedPropertiesContractResolver(),
                    Converters = { new OrderedExpandoPropertiesConverter() }
                };
                reader = new StreamReader(filePath);
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), settings);
            }
            finally
            {
                reader?.Close();
            }
        }

        [Command("backup")]
        [Remarks("backup")]
        [Summary("Backup the server with ease!")]
        [Attributes.RequireOwner()]
        [RequireContext(ContextType.Guild)]
        public async Task BackupCmd()
        {
            BackupGuild backup = new BackupGuild(Context);
            WriteToJsonFile("backup.json", backup);
            await this.ReplyAsync("Done!");
        }

        [Command("loadbackup")]
        [Remarks("loadbackup")]
        [Summary("Loads the backup with ease!")]
        [Attributes.RequireOwner()]
        [RequireContext(ContextType.Guild)]
        public async Task LoadCmd()
        {
            BackupGuild b = ReadFromJsonFile<BackupGuild>("backup.json");
            // load bans
            foreach (var ban in await Context.Guild.GetBansAsync()) await Context.Guild.RemoveBanAsync(ban.User.Id);
            foreach (var ban in b.Bans) await Context.Guild.AddBanAsync(ban.Id, 0, ban.Reason);

            // load roles
            foreach (var role in Context.Guild.Roles) { if (!role.IsEveryone && !role.IsManaged) await role.DeleteAsync(); }
            foreach (var role in b.Roles.OrderByDescending(x => x.Position)) await Context.Guild.CreateRoleAsync(role.RoleName, new GuildPermissions(role.GuildPermissions), new Color(role.RawColour), role.Hoisted, role.AllowMention);
            // load everyone role perms
            await Context.Guild.EveryoneRole.ModifyAsync(f => f.Permissions = new GuildPermissions(b.EveryonePerms));

            // load channels
            foreach (var channel in Context.Guild.Channels) await channel.DeleteAsync();
            var backupMessages = new Dictionary<ulong, List<BackupChatMessage>>();
            foreach (var category in b.Categories.OrderBy(x => x.Position))
            {
                var cCat = await Context.Guild.CreateCategoryChannelAsync(category.Name, (f) => f.Position = category.Position);
                category.Permissions.ForEach(f => cCat.AddPermissionOverwriteAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == f.RoleName), new OverwritePermissions(f.AllowPermissions, f.DenyPermissions)));
                foreach (var (c, channel) in from c in category.TextChannels.OrderBy(x => x.Position) let channel = Context.Guild.CreateTextChannelAsync(c.Name, p => { p.CategoryId = cCat.Id; ; p.IsNsfw = c.IsNSFW; p.Topic = c.Topic; p.SlowModeInterval = c.Slowmode; p.Position = c.Position; }).GetAwaiter().GetResult() select (c, channel)) { c.Permissions.Select((f, b) => channel.AddPermissionOverwriteAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == f.RoleName), new OverwritePermissions(f.AllowPermissions, f.DenyPermissions))); if (c.IsHidden) backupMessages.Add(channel.Id, await BackupGuild.GenChannelHiddenMessage()); else backupMessages.Add(channel.Id, c.LastMessages); }
                foreach (var (c, channel) in from c in category.VoiceChannels.OrderBy(x => x.Position) let channel = Context.Guild.CreateVoiceChannelAsync(c.Name, p => { p.CategoryId = cCat.Id; p.UserLimit = c.UserLimit; p.Bitrate = c.Bitrate; p.Position = c.Position; }).GetAwaiter().GetResult() select (c, channel)) c.Permissions.ForEach((f) => channel.AddPermissionOverwriteAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == f.RoleName), new OverwritePermissions(f.AllowPermissions, f.DenyPermissions)));
            }

            foreach (var (c, channel) in from c in b.TextChannels.OrderBy(x => x.Position) let channel = Context.Guild.CreateTextChannelAsync(c.Name, p => { p.IsNsfw = c.IsNSFW; p.Topic = c.Topic; p.SlowModeInterval = c.Slowmode; p.Position = c.Position; }).GetAwaiter().GetResult() select (c, channel)) { c.Permissions.Select((f, b) => channel.AddPermissionOverwriteAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == f.RoleName), new OverwritePermissions(f.AllowPermissions, f.DenyPermissions))); if (c.IsHidden) backupMessages.Add(channel.Id, await BackupGuild.GenChannelHiddenMessage()); else backupMessages.Add(channel.Id, c.LastMessages); }
            foreach (var (c, channel) in from c in b.VoiceChannels.OrderBy(x => x.Position) let channel = Context.Guild.CreateVoiceChannelAsync(c.Name, p => { p.UserLimit = c.UserLimit; p.Bitrate = c.Bitrate; p.Position = c.Position; }).GetAwaiter().GetResult() select (c, channel)) c.Permissions.ForEach((f) => channel.AddPermissionOverwriteAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == f.RoleName), new OverwritePermissions(f.AllowPermissions, f.DenyPermissions)));

            //get system and afk channel
            Optional<ulong?> systemId = null;
            Optional<ulong?> afkId = null;
            if (b.AFKChannelLocalId != -1) afkId = (await BackupGuild.GetDiscordChannelFromLocalIdAsync(Context.Guild, b.AFKChannelLocalId)).Id;
            if (b.SystemChannelLocalId != -1) systemId = (await BackupGuild.GetDiscordChannelFromLocalIdAsync(Context.Guild, b.SystemChannelLocalId)).Id;

            // modify guild settings
            await Context.Guild.ModifyAsync(f => { f.AfkChannelId = afkId; f.SystemChannelId = systemId; f.VerificationLevel = b.VerificationLevel; f.RegionId = b.VoiceRegion; f.DefaultMessageNotifications = b.DefaultNotifications; f.ExplicitContentFilter = b.ContentFilter; f.Icon = b.Icon.Length == 0 ? (Image?)null : new Image(new MemoryStream(b.Icon)); f.Name = b.ServerName; f.Splash = b.SplashImage.Length == 0 ? (Image?)null : new Image(new MemoryStream(b.SplashImage)); f.AfkTimeout = b.AFKTimeout; f.SystemChannelFlags = b.SystemChannelFlags; });

            // load emotes
            foreach (var emoji in Context.Guild.Emotes) await Context.Guild.DeleteEmoteAsync(emoji);
            foreach (var emoji in b.Emojis) await Context.Guild.CreateEmoteAsync(emoji.Name, new Image(new MemoryStream(emoji.Image)));

            // fix channel topics
            foreach (var channel in Context.Guild.TextChannels) await channel.ModifyAsync(async f => f.Topic = (await BackupGuild.FixId(Context.Guild, channel.Topic, true)));

            Parallel.ForEach(Context.Guild.TextChannels, async t => await this.SendMessagesToChannelAsync(backupMessages, t));

            await Context.User.SendMessageAsync("Your server has been fully restored!");
        }
        private async Task SendMessagesToChannelAsync(Dictionary<ulong, List<BackupChatMessage>> backupMessages, SocketTextChannel o)
        {
            var webhook = await o.CreateWebhookAsync("Message restorer");
            var wc = new DiscordWebhookClient(webhook);
            wc.Log += Services.LoggingService.OnLogAsync;
            if (!backupMessages.TryGetValue(o.Id, out var messages)) return;
            foreach (var message in messages)
            {
                var embeds = (from embed in message.Embeds let fields = embed.Fields.Select(f => new EmbedFieldBuilder().WithName(f.Name).WithValue(f.Value).WithIsInline(f.Inline)).ToList() let embedBuilder = new EmbedBuilder { /* Embed property can be set within object initializer*/ Title = embed.Title, Description = (BackupGuild.FixId(Context.Guild, embed.Description, true).GetAwaiter().GetResult()), Author = (embed.Author == null && embed.AuthorIcon == null && embed.AuthorUrl == null) ? null : new EmbedAuthorBuilder().WithName(embed.Author).WithIconUrl(embed.AuthorIcon).WithUrl(embed.AuthorUrl), Color = embed.Color, Fields = fields, Footer = (embed.FooterText == null && embed.FooterIconUrl == null) ? null : new EmbedFooterBuilder().WithText(embed.FooterText).WithIconUrl(embed.FooterIconUrl), ImageUrl = embed.Image, ThumbnailUrl = embed.Thumbnail, Timestamp = embed.Timestamp, Url = embed.Url } let m = embedBuilder.Build() select embedBuilder.Build()).ToList();
                SocketUser user = Context.Client.GetUser(message.AuthorId);
            retry:
                try
                {
                    ulong msgId = 0;
                    if (message.HasAttachments)
                        msgId = await wc.SendFileAsync(new MemoryStream(Array.Empty<byte>(), false), "FILE_LOST", string.IsNullOrWhiteSpace(message.Text) ? null : message.Text, false, embeds, user == null ? message.Author : user.Username, user == null ? message.AuthorPic : (user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));
                    else
                        msgId = await wc.SendMessageAsync(string.IsNullOrWhiteSpace(message.Text) ? null : (await BackupGuild.FixId(Context.Guild, message.Text, true)), false, embeds, user == null ? message.Author : user.Username, user == null ? message.AuthorPic : (user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()));
                    if (!message.IsPinned) continue;
                    await ((RestUserMessage)await o.GetMessageAsync(msgId)).PinAsync();
                    var oof = (await o.GetMessagesAsync(1).FlattenAsync()).First();
                    if (oof.Type == MessageType.ChannelPinnedMessage) await oof.DeleteAsync().ConfigureAwait(false);
                }
                catch (Exception) { await Task.Delay(8500); goto retry; }
            }
            await wc.DeleteWebhookAsync();
        }
    }
}
