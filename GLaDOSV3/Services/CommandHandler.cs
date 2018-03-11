using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Attributes;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
            _discord.MessageReceived += OnMessageReceivedAsync;
        }
        
        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return; // Ensure the message is from a user/bot
            if (msg.Author.Id == _discord.CurrentUser.Id) return;     // Ignore self when checking commands
            if (msg.Author.IsBot) return; // Ignore other bots
            if(msg.MentionedUsers.Count > 0)
              await MentionBomb(msg);
            int argPos = 0; // Check if the message has a valid command prefix
            if (!msg.HasStringPrefix(_config["prefix"], ref argPos) && !msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))  return; // Ignore messages that aren't meant for the bot
            var context = new SocketCommandContext(_discord, msg); // Create the command context
            if (Boolean.Parse(_config["maintenance"]) && !(IsOwner.CheckPermission(context).GetAwaiter().GetResult())) { await context.Channel.SendMessageAsync("This bot is in maintenance mode! Please refrain from using it.").ConfigureAwait(false); ; return; } // Don't execute commands in maintenance mode 
            var result = await _commands.ExecuteAsync(context, argPos, _provider); // Execute the command
            if (!result.IsSuccess && result.ErrorReason != "hidden" && result.ErrorReason != "Unknown command.")  // If not successful, reply with the error.
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

        private Task MentionBomb(SocketUserMessage msg)
        {
            if (msg.MentionedUsers.Distinct().Count() < 5) return Task.CompletedTask;
            msg.DeleteAsync().GetAwaiter();
            msg.Channel.SendMessageAsync($"{msg.Author.Mention} Please don't mention bomb!").GetAwaiter();
            return Task.CompletedTask;
        }
    }
}
