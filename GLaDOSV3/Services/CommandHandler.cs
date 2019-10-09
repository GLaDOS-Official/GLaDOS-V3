using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using GladosV3.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        public static List<ulong> BlacklistedUsers = new List<ulong>();
        public static List<ulong> BlacklistedServers = new List<ulong>();
        public static string MaintenanceMode = "";
        public static bool BotBusy = false;
        public static Dictionary<ulong, string> Prefix = new Dictionary<ulong, string>();

        private readonly string fallbackPrefix = "";
        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IServiceProvider provider,
            BotSettingsHelper<string> botSettingsHelper)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _discord.MessageReceived += OnMessageReceivedAsync;
            MaintenanceMode = botSettingsHelper["maintenance"];
            fallbackPrefix = botSettingsHelper["prefix"];
            using (DataTable dt = SqLite.Connection.GetValuesAsync("BlacklistedUsers").GetAwaiter().GetResult())
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    BlacklistedUsers.Add(Convert.ToUInt64(dt.Rows[i]["UserId"]));
                }
            }

            RefreshPrefix();
        }

        internal static void RefreshPrefix()
        {
            string sql = $"SELECT guildid,prefix FROM servers";
            using (DataTable dt2 = new DataTable())
            {

                using (SQLiteDataAdapter reader = new SQLiteDataAdapter(sql, SqLite.Connection))
                    reader.Fill(dt2);
                dt2.TableName = "servers";
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    string pref = dt2.Rows[i]["prefix"].ToString();
                    if (!string.IsNullOrWhiteSpace(pref))
                        Prefix.Add(Convert.ToUInt64(dt2.Rows[i]["guildid"]), pref);
                }
            }
        }
        private bool IsUserBlackListed(SocketUserMessage msg)
        {
            return BlacklistedUsers.Contains(msg.Author.Id);
        }
        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (BotBusy) return;
            if (!(s is SocketUserMessage msg))
            {
                return; // Ensure the message is from a user/bot
            }

            if (msg.Author.Id == _discord.CurrentUser.Id)
            {
                return;     // Ignore self when checking commands
            }

            if (msg.Author.IsBot)
            {
                return; // Ignore other bots
            }

            if (msg.MentionedUsers.Count > 0)
            {
                await MentionBomb(msg);
            }

            int argPos = 0; // Check if the message has a valid command prefix
            string prefix = fallbackPrefix;
            if ((msg.Channel is IGuildChannel ok) && Prefix.TryGetValue(ok.Guild.Id, out string guildPrefix))
            { prefix = guildPrefix; }
            if (!msg.HasStringPrefix(prefix, ref argPos) && !msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                return; // Ignore messages that aren't meant for the bot
            }

            if (IsUserBlackListed(msg))
            {
                return; // We can't let blacklisted users ruin our bot!
            }

            SocketCommandContext context = new SocketCommandContext(_discord, msg); // Create the command context
            if (!string.IsNullOrWhiteSpace(MaintenanceMode) && (IsOwner.CheckPermission(context).GetAwaiter().GetResult())) { await context.Channel.SendMessageAsync("Bot is in maintenance mode! Reason: " + MaintenanceMode).ConfigureAwait(false); ; return; } // Don't execute commands in maintenance mode 
            IResult result = await _commands.ExecuteAsync(context, argPos, _provider); // Execute the command
            if (!result.IsSuccess && result.ErrorReason != "hidden" && result.ErrorReason != "Unknown command.")  // If not successful, reply with the error.
            {
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        break;
                    case CommandError.Exception:
                        break;
                    case CommandError.UnknownCommand:
                        break;
                    case CommandError.ParseFailed:
                        break;
                    case CommandError.ObjectNotFound:
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
                                    await context.Channel.SendMessageAsync("**Error:** This command must be used in a guild!").ConfigureAwait(false);
                                    break;
                                case "The input text has too few parameters.":
                                    await context.Channel.SendMessageAsync("**Error:** None or few arguments are being used.").ConfigureAwait(false);
                                    break;
                                case "User not found.":
                                    await context.Channel.SendMessageAsync("**Error:** No user mention detected.").ConfigureAwait(false);
                                    break;
                                default:
                                    await context.Channel.SendMessageAsync($@"**Error:** {result.ErrorReason}").ConfigureAwait(false);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private Task MentionBomb(SocketUserMessage msg)
        {
            if (msg.MentionedUsers.Distinct().Count() < 5)
            {
                return Task.CompletedTask;
            }

            msg.DeleteAsync().GetAwaiter();
            msg.Channel.SendMessageAsync($"{msg.Author.Mention} Please don't mention that many users!").GetAwaiter();
            return Task.CompletedTask;
        }
    }
}
