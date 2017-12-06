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
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    string input = string.Empty;
                    do
                    {
                        var kb = Console.ReadKey();
                        switch (kb.Key)
                        {
                            case ConsoleKey.Backspace:
                                if(input.Length != 0)
                                  input = input.Remove((input.Length - 1), 1);
                                Console.Write(" \b");
                                Console.SetCursorPosition(Console.CursorLeft,Console.CursorTop);
                                Console.ResetColor();
                                break;
                            case ConsoleKey.Enter:
                                if (!string.IsNullOrWhiteSpace(input)) break;
                                foreach (var t in _discord.Guilds)
                                {
                                    t.DefaultChannel.SendMessageAsync($"System message: {input}");
                                }
                                input = string.Empty;
                                Console.WriteLine($"{Environment.NewLine}Sended!");
                                break;
                            default:
                                Regex r = new Regex(@"^[a-zA-Z0-9_.-]+$", RegexOptions.IgnoreCase);
                                if (char.IsLetterOrDigit(kb.KeyChar) && r.IsMatch(kb.ToString()))
                                {
                                    input += kb.KeyChar;
                                }
                                break;
                        }
                    } while (true);

                }
            });
            thread.Start();
        }
    }
}
