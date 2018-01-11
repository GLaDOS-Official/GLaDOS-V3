using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }

        [Command("modules")]
        [Remarks("modules")]
        [Summary("Shows information on a module such as any remarks and a count of how many commands")]
        public async Task Modules(string remarks = null)
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(0, 255, 100),
                Title = ($"Here's the information about all modules\n")
            };
            foreach (var module in _service.Modules)
            {
                builder.AddField(e =>
                {
                    e.Name = ($"**{module.Name}**");
                    e.Value = $"{(remarks == "remarks" ? (string.IsNullOrWhiteSpace(module.Remarks) ? $"*No remarks found*\n" : $"Remarks:\n***{module.Remarks}***") : null)}Number of commands in this module: {module.Commands.Count}"; ;
                    e.IsInline = (false);
                });
            }
            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        [Remarks("help [command]")]
        [Summary("How 2 use ...?")]
        public async Task Help([Remainder]string command = null)
        {
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync();
            EmbedBuilder builder;
            string prefix = _config["prefix"];;
            Random rnd = new Random();
            if (command != null)
            {
                var result = _service.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                builder = new EmbedBuilder()
                {
                    Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)),
                    Description = $"Here are some commands like **{command}**"
                };
                
                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;
                    var text = string.Empty;
                    if (cmd.Parameters.Count != 0)
                        text += $"Arguments: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n";
                    else
                        text += "Arguments: None\n";
                    if (!string.IsNullOrWhiteSpace(cmd.Summary) || !string.IsNullOrEmpty(cmd.Summary))
                        text += $"Info: {cmd.Summary}\n";
                    if (!string.IsNullOrWhiteSpace(cmd.Remarks))
                        text += $"Example: {cmd.Remarks}\n";
                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = text;
                        x.IsInline = false;
                    });
                }
            }
            else
            {
                builder = new EmbedBuilder
                {
                    Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)),
                    Description = "These are the commands you can use."
                };
                foreach (var module in _service.Modules)
                {
                    List<string> array = new List<string>();
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (!result.IsSuccess) continue;
                        if(!array.Contains($"{prefix}{cmd.Remarks}\n"))
                            array.Add($"{prefix}{cmd.Remarks}\n"); // tried adding command info, bad dev -_-
                    }
                    string description = array.Aggregate<string, string>(null, (current, s) => current + s);
                    if (string.IsNullOrWhiteSpace(description)) continue;
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }
            await dm.SendMessageAsync("", false, builder.Build());
        }
        public T Reduce<T, U>(Func<U, T, T> func, IEnumerable<U> list, T acc)
        {
            foreach (var i in list)
                acc = func(i, acc);

            return acc;
        }
    }
}
