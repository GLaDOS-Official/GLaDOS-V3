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

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public SystemMessage(
            DiscordSocketClient discord)
        {
            _discord = discord;
        }
        public void KeyPress()
        {
            Thread thread = new Thread(() =>
            {
                var stdout = Console.OpenStandardOutput();
                while (true)
                {
                    ConsoleKeyInfo kb;
                    string input = string.Empty;
                    do
                    {
                        kb = Console.ReadKey();
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
                                    //Console.Write(kb.KeyChar);
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
