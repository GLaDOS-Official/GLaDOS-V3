using Discord;
using Discord.Commands;
using GladosV3.Attributes;
using GladosV3.Helpers;
using GladosV3.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Module.Default
{
    [Name("General")]
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Name("Guild settings")]
        public class Guild : ModuleBase<SocketCommandContext>
        {
            [Command("guild farewell message")]
            [Summary("Set the current message of Guild Join module")]
            [Remarks("guild join message <message>")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task FarewellMessage(string value)
            {
                await SqLite.Connection.SetValueAsync("servers", "leave_msg", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                await ReplyAsync("Done!");
            }
            [Command("guild farewell channel")]
            [Summary("Set the current channel ID of Guild Join module")]
            [Remarks("guild join channel <channelId>")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task FarewellChannel(string value)
            {
                if (Context.Guild.GetChannel(Convert.ToUInt64(value)) != null)
                    await SqLite.Connection.SetValueAsync("servers", "joinleave_cid", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                else
                    throw new Exception("Channel ID is invalid!");

                await ReplyAsync("Done!");
            }
            [Command("guild farewell status")]
            [Summary("Set the current status of Guild Join module")]
            [Remarks("guild join status <status>")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task FarewellStatus(string value)
            {
                if (value == "1" || value == "0")
                    await SqLite.Connection.SetValueAsync("servers", "leave_toggle", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                else
                    throw new Exception("Only 0 or 1 is accepted!");
                await ReplyAsync("Done!");
            }
            [Command("guild join message")]
            [Summary("Set the current message of Guild Join module")]
            [Remarks("guild join message <message>")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task JoinMessage(string value)
            {
                await SqLite.Connection.SetValueAsync("servers", "join_msg", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                await ReplyAsync("Done!");
            }
            [Command("guild join channel")]
            [Summary("Set the current channel ID of Guild Join module")]
            [Remarks("guild join channel <channelId>")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task JoinChannel(string value)
            {
                if (Context.Guild.GetChannel(Convert.ToUInt64(value)) != null)
                    await SqLite.Connection.SetValueAsync("servers", "joinleave_cid", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                else
                    throw new Exception("Channel ID is invalid!");

                await ReplyAsync("Done!");
            }
            [Command("guild join status")]
            [Summary("Set the current status of Guild Join module")]
            [Remarks("guild join status <status>")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task JoinStatus(string value)
            {
                if (value == "1" || value == "0")
                    await SqLite.Connection.SetValueAsync("servers", "join_toggle", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                else
                    throw new Exception("Only 0 or 1 is accepted!");
                await ReplyAsync("Done!");
            }
            [Command("guild prefix")]
            [Summary("Set the guild prefix of this bot")]
            [Remarks("guild prefix")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GuildPrefix(string value = null)
            {
                await SqLite.Connection.SetValueAsync("servers", "prefix", value, $"WHERE guildid={Context.Guild.Id.ToString()}").ConfigureAwait(false);
                if (CommandHandler.Prefix.ContainsKey(Context.Guild.Id))
                    CommandHandler.Prefix.Remove(Context.Guild.Id);
                if (!string.IsNullOrWhiteSpace(value))
                    CommandHandler.Prefix.Add(Context.Guild.Id, value);
                await ReplyAsync($"Done! Changed the prefix to: {(string.IsNullOrWhiteSpace(value) ? IsOwner.botSettingsHelper["prefix"] : value)}");
            }
            [Command("guild configuration")]
            [Summary("Lists the current settings of the Guild module")]
            [Remarks("guild configuration")]
            [Alias("guild config")]
            [Attributes.RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task GuildConfig()
            {
                var msg = await ReplyAsync("Please wait...");
                string finalMsg = "";
                DataTable dt = await SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{Context.Guild.Id.ToString()}'");
                var row = dt.Rows[0];
                Random rnd = new Random();
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                        IconUrl = (Context.User.GetAvatarUrl())
                    }
                };
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                finalMsg = $"NSFW module status: {(row?[1] == "0" ? "Enabled ✅" : "Disabled ❌")}\n";
                finalMsg += $"Join and leave announcement channel: {(string.IsNullOrWhiteSpace(row?[2].ToString()) ? "Not set ❌" : $"<#{row?[2]}> ✅")}\n";
                finalMsg += $"Join message: {row?[3]}\n";
                finalMsg += $"Join announcement status: {(row?[4] == "0" ? "Enabled ✅" : "Disabled ❌")}\n";
                finalMsg += $"Leave message: {row?[5]}\n";
                finalMsg += $"Leave announcement status: {(row?[6] == "0" ? "Enabled ✅" : "Disabled ❌")}\n";
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                finalMsg += $"Guild prefix: {(string.IsNullOrWhiteSpace(row?[7].ToString()) ? IsOwner.botSettingsHelper["prefix"] : row?[7])}";
                builder.AddField("Guild settings", finalMsg);
                await msg.ModifyAsync((a) => { a.Content = ""; a.Embed = builder.Build(); });
            }
        }
        [Command("choose")]
        [Summary("Returns a random item that you supplied (splitting by comma character)")]
        [Remarks("choose <items>")]
        [Alias("random")]
        public Task Choose([Remainder]string text)
        {
            string[] array = text.Split(',');
            Random rnd = new Random();
            ReplyAsync($"I have chosen: {array[rnd.Next(array.Length - 1)]}").GetAwaiter();
            return Task.CompletedTask;
        }
        [Command("emojisay")]
        [Summary("Get's the emoji from a server (nitro is gay)")]
        [Remarks("emojisay <serverid> <emoji name> [--notext] [--s]")]
        public async Task EmojiSay(ulong serverid, [Remainder]string emojiname)
        {
            string emojiName = emojiname;
            bool noText = false;
            if (emojiName.Contains(" --notext"))
            {
                emojiName = emojiName.Replace(" --notext", "");
                noText = true;
            }
            if (emojiName.Contains(" --s"))
            {
                emojiName = emojiName.Replace(" --s", "");
                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                    await Context.Message.DeleteAsync();
            }
            var guild = Context.Client.GetGuild(serverid);
            if (guild == null)
            {
                await ReplyAsync("❌I'm not in that server!");
                return;
            }
            var emoteArray = guild.Emotes.ToArray();
            string emojiString = "";
            foreach (var t in emoteArray)
            {
                if (t.Name != emojiName) continue;
                emojiString = $"<{(t.Animated ? "a" : "")}:{t.Name}:{t.Id}>";
                break;
            }

            if (emojiString == "")
                await ReplyAsync("❌Emoji not found on that server!");
            else
                await ReplyAsync($"{(noText ? "" : "Here's your emoji: ")}{emojiString}");
        }
    }
}
