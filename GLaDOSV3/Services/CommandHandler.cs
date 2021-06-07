using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Attributes;
using GLaDOSV3.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private readonly IServiceProvider provider;
        public static List<ulong> BlacklistedUsers = new List<ulong>();
        public static List<ulong> BlacklistedServers = new List<ulong>();
        public static string MaintenanceMode = string.Empty;
        public static bool BotBusy = false;
        public static Dictionary<ulong, string> Prefix = new Dictionary<ulong, string>();

        private readonly string fallbackPrefix;
        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IServiceProvider provider,
            BotSettingsHelper<string> botSettingsHelper)
        {
            if (discord == null || commands == null || provider == null || botSettingsHelper == null) return;
            this.discord = discord;
            this.commands = commands;
            this.provider = provider;
            discord.MessageReceived += this.OnMessageReceivedAsync;
            MaintenanceMode = botSettingsHelper["maintenance"];
            this.fallbackPrefix = botSettingsHelper["prefix"];
            using DataTable dt = SqLite.Connection.GetValuesAsync("BlacklistedUsers").GetAwaiter().GetResult();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                BlacklistedUsers.Add(Convert.ToUInt64(dt.Rows[i]["UserId"], CultureInfo.InvariantCulture));
            }
            RefreshPrefix();
        }

        public static void RefreshPrefix()
        {
            string sql = $"SELECT guildid,prefix FROM servers";
            using DataTable dt2 = new DataTable();
            using (SQLiteDataAdapter reader = new SQLiteDataAdapter(sql, SqLite.Connection))
                reader.Fill(dt2);
            dt2.TableName = "servers";
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                string pref = dt2.Rows[i]["prefix"].ToString();
                if (!string.IsNullOrWhiteSpace(pref))
                    Prefix.Add(Convert.ToUInt64(dt2.Rows[i]["guildid"], CultureInfo.InvariantCulture), pref);
            }
        }
        private static bool IsUserBlackListed(SocketUserMessage msg) => BlacklistedUsers.Contains(msg.Author.Id);

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (BotBusy) return;
            if (!(s is SocketUserMessage msg))
            {
                return; // Ensure the message is from a user/bot
            }
            if (msg.Author.IsBot)
            {
                return; // Ignore other bots
            }
            if (msg.Author.Id == this.discord.CurrentUser.Id)
            {
                return;     // Ignore self when checking commands
            }

            int argPos = 0; // Check if the message has a valid command prefix
            string prefix = this.fallbackPrefix;
            if ((msg.Channel is IGuildChannel ok) && Prefix.TryGetValue(ok.Guild.Id, out string guildPrefix))
            { prefix = guildPrefix; }
            if (!msg.HasStringPrefix(prefix, ref argPos) && !msg.HasMentionPrefix(this.discord.CurrentUser, ref argPos))
            {
                if (msg.MentionedUsers.Count > 0)
                {
                    await this.MentionBomb(msg).ConfigureAwait(false);
                }
                return; // Ignore messages that aren't meant for the bot
            }

            if (IsUserBlackListed(msg))
            {
                return; // We can't let blacklisted users ruin our bot!
            }

            SocketCommandContext context = new SocketCommandContext(this.discord, msg); // Create the command context
            if (!string.IsNullOrWhiteSpace(MaintenanceMode) && (IsOwner.CheckPermission(context).GetAwaiter().GetResult())) { await context.Channel.SendMessageAsync("Bot is in maintenance mode! Reason: " + MaintenanceMode).ConfigureAwait(false); ; return; } // Don't execute commands in maintenance mode 
            var result = this.commands.ExecuteAsync(context, argPos, this.provider); // Execute the command
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            result.ContinueWith(task =>
            {
                while (!task.IsCompleted) { System.Threading.Thread.Sleep(4); }
                var result = task.Result;
                if (result.IsSuccess || result.ErrorReason == "hidden") return;
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        break;
                    case CommandError.Exception:
                        Console.WriteLine("Exception :(");
                        break;
                    case CommandError.UnknownCommand:
                        break;
                    case CommandError.ParseFailed:
                        break;
                    case CommandError.MultipleMatches:
                        break;
                    case CommandError.UnmetPrecondition:
                        break;
                    case CommandError.Unsuccessful:
                        break;
                    case null:
                        break;
                    default:
                        {
                            switch (result.ErrorReason) // "Custom" error
                            {
                                case "Invalid context for command; accepted contexts: Guild":
                                    context.Channel.SendMessageAsync("**Error:** This command must be used in a guild!").ConfigureAwait(false);
                                    break;
                                case "The input text has too few parameters.":
                                    context.Channel.SendMessageAsync("**Error:** None or few arguments are being used.").ConfigureAwait(false);
                                    break;
                                case "User not found.":
                                    context.Channel.SendMessageAsync("**Error:** No user parameter detected.").ConfigureAwait(false);
                                    break;
                                default:
                                    context.Channel.SendMessageAsync($@"**Error:** {result.ErrorReason}").ConfigureAwait(false);
                                    break;
                            }
                        }
                        break;
                }
            }, TaskScheduler.Default);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private Task MentionBomb(SocketUserMessage msg)
        {
            return Task.CompletedTask;
            if (msg.Channel is not SocketGuildChannel channel) return Task.CompletedTask;
            if (channel.GetUser(msg.Author.Id).GuildPermissions.Has(GuildPermission.ManageGuild) || channel.GetUser(msg.Author.Id).GuildPermissions.Has(GuildPermission.Administrator)) return Task.CompletedTask;
            if (msg.MentionedUsers.Distinct().Count() < 5) return Task.CompletedTask;
            msg.DeleteAsync().GetAwaiter();
            msg.Channel.SendMessageAsync($"{msg.Author.Mention} Please don't mention that many users!").GetAwaiter();
            return Task.CompletedTask;
        }
    }
}
