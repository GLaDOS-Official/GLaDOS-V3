using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }

        [Command("help")]
        [Summary("help [command]")]
        [Remarks("How 2 use ...?")]
        public async Task HelpAsync(string command = null)
        {
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
                        text += "None";
                    if (!string.IsNullOrWhiteSpace(cmd.Summary) || !string.IsNullOrEmpty(cmd.Summary))
                        text += $"Info: {cmd.Summary}";
                    if (!string.IsNullOrWhiteSpace(cmd.Remarks))
                        text += $"Example: {cmd.Remarks}";
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
                var unwantedModules = new string[] {"HelpModule", "ExampleModule" };
                builder = new EmbedBuilder
                {
                    Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)),
                    Description = "These are the commands you can use"
                };

                foreach (var module in _service.Modules)
                {
                   // if (unwantedModules.Contains(module.Name) || unwantedModules.Contains(module.Name.Replace("Module", String.Empty)) || unwantedModules.Contains(module.Name + "Module")) continue;
                    List<string> array = new List<string>();
                    foreach (var cmd in module.Commands)
                    {
                        
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                            if(!array.Contains($"{prefix}{cmd.Summary}\n"))
                               array.Add($"{prefix}{cmd.Summary}\n");
                    }
                    string description = array.Aggregate<string, string>(null, (current, s) => current + s);
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                            x.IsInline = false;
                        });
                    }
                }
            }
            IDMChannel DM = await Context.Message.Author.GetOrCreateDMChannelAsync();
            await DM.SendMessageAsync("", false, builder.Build());
        }
    }
}
