using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GLaDOSV3.Services
{
    public class StartupService
    {
        private readonly ILogger logger;
        private readonly DiscordShardedClient discord;
        private readonly CommandService commands;
        private readonly IServiceProvider provider;
        private readonly BotSettingsHelper<string> botSettingsHelper;
        // IServiceProvider, DiscordShardedClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            DiscordShardedClient discord,
            CommandService commands,
            IServiceProvider provider,
            BotSettingsHelper<string> botSettingsHelper,
            ILogger<StartupService> logger)
        {
            this.discord = discord;
            this.commands = commands;
            this.provider = provider;
            this.botSettingsHelper = botSettingsHelper;
            this.logger = logger;
        }

        private Task<string> AskNotNull(string question)
        {
            Console.Write(question);
            var input = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Enter something. This can't be empty.");
                Console.Write(question);
                input = Console.ReadLine();
            }
            return Task.FromResult(input);
        }
        public async Task FirstStartup(bool reset)
        {
            using (DataTable dt = await SqLite.Connection.GetValuesAsync("BotSettings", "WHERE value IS NOT NULL").ConfigureAwait(true))
                if (dt.Rows.Count == 8 && !reset) return;
            await SqLite.Connection.ExecuteSqlAsync("DROP TABLE IF EXISTS BotSettings").ConfigureAwait(false);
            await SqLite.Connection.CreateTableAsync("BotSettings", "`ID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `name` TEXT, `value` TEXT").ConfigureAwait(false);
            Console.WriteLine("Hello user! Looks like your starting this bot for the first time! You'll need to enter some values to start this bot.");
            Console.Write("Please enter your default bot prefix: ");
            var input = await this.AskNotNull("Please enter your default bot prefix: ").ConfigureAwait(true);
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "prefix", input }).ConfigureAwait(false);
            input = await this.AskNotNull("Perfect. Now please enter the name of the bot: ").ConfigureAwait(false);
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "name", input }).ConfigureAwait(false);
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "maintenance", "" }).ConfigureAwait(false);
            input = await this.AskNotNull("Very good. Now add your user ID: ").ConfigureAwait(true);
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "ownerID", input }).ConfigureAwait(false);
            Console.WriteLine("Now you'll can enter co-owners user IDs, this is totally optional (Press enter to skip).");
            Console.WriteLine("If you decide to add any, put them in format \"userID1,userID2\" without quotation marks.");
            Console.WriteLine("Now enter co-owners ID: ");
            input = Console.ReadLine();
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "co-owners", input }).ConfigureAwait(false);
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "discord_game", "" }).ConfigureAwait(false);
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "discord_status", "Online" }).ConfigureAwait(false);
            do
            {
                input = await this.AskNotNull("Ok! Now the final thing! Enter your bot token: ").ConfigureAwait(true);
            } while (!Regex.IsMatch(input, "[MN][A-Za-z\\d]{23}\\.[\\w-]{6}\\.[\\w-]{27}"));
            await SqLite.Connection.AddRecordAsync("BotSettings", "name,value", new[] { "tokens_discord", input }).ConfigureAwait(false); ;
        }
        public async Task StartAsync(string[] args)
        {
            SqLite.Start();

            await this.FirstStartup(args.Contains("--resetdb")).ConfigureAwait(false);
            Console.Title = this.botSettingsHelper["name"];
            Console.Clear();
            //Console.SetWindowSize(150, 35);
            Console.WriteLine("This bot is using a database to store it's settings. Add --resetdb to reset the configuration (token, owners, etc..).");
            var discordToken = this.botSettingsHelper["tokens_discord"];     // Get the discord token from the config file
            /*Log.Error("TOKEN BELOW");
            Log.Error("Discord token: {0}",discordToken);
            Log.Error("TOKEN ABOVE");*/
            try
            {
                await this.discord.LoginAsync(TokenType.Bot, discordToken).ConfigureAwait(false); // Login to discord
                await this.discord.StartAsync().ConfigureAwait(false); // Connect to the websocket
            }
            catch (Exception ex) { this.logger.LogCritical(ex, ex.Message); Environment.Exit(1); }
            await this.commands.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider).ConfigureAwait(false);     // Load commands and modules into the command service
            ExtensionLoadingService.Init(this.discord, this.commands, this.botSettingsHelper, this.provider);
            await ExtensionLoadingService.Load().ConfigureAwait(false);
        }
    }
}
