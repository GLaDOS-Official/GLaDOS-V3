using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Helpers
{
    class SystemMessage
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public SystemMessage(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
        }
        public void KeyPress()
        {
            if (Boolean.Parse(_config["maintenance"])) return;
            Thread thread = new Thread(SystemMessageThread);
            thread.Start();
        }

        private void SystemMessageThread()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input == string.Empty) continue;
                foreach (var t in _discord.Guilds) t.DefaultChannel.SendMessageAsync($"System message: {input}");

                Console.WriteLine($"[Service]System message: Sent!");
            }
        }
    }
}
