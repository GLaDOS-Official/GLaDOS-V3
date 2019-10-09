using Discord;
using Discord.Commands;
using GladosV3.Attributes;
using GladosV3.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GladosV3.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("modules")]
        [Remarks("modules")]
        [Summary("Shows information on a module such as any remarks and a count of how many commands")]
        public Task Modules(string remarks = null)
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
            ReplyAsync("", false, builder.Build()).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        [Command("help")]
        [Remarks("help [command]")]
        [Summary("How 2 use ...?")]
        public async Task Help([Remainder]string command = null)
        {
            // ((ITextChannel)Context.Channel).CreateWebhookAsync("GLaDOS V3");

            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync();
            Random rnd = new Random();
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256))
            };
            string prefix = IsOwner.botSettingsHelper["prefix"];
            if (command != null)
            {
                var result = _service.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                builder.Description = $"Here are some commands like **{command}**";

                foreach (var match in result.Commands)
                {
                    string text = (match.Command.Parameters.Count != 0) ? "Arguments: None\n" : $"Arguments: {string.Join(", ", match.Command.Parameters.Select(p => p.Name))}\n";
                    text = string.Concat(text, !string.IsNullOrWhiteSpace(match.Command.Summary) ? "" : $"Info: {match.Command.Summary}\n");
                    text += string.Concat(text, !string.IsNullOrWhiteSpace(match.Command.Remarks) ? "" : $"Example: {match.Command.Remarks}\n");
                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", match.Command.Aliases);
                        x.Value = text;
                        x.IsInline = false;
                    });
                }
                await dm.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                List<CommandInfo> list = new List<CommandInfo>();
                if (_service != null)
                {

                    var sorted = _service.Commands.OrderByDescending(x => x.Remarks.Length).FirstOrDefault().Remarks.Length;


                    builder.Description = "These are the commands you can use.";
                    foreach (var module in _service.Modules)
                    {
                        List<string> array = new List<string>();
                        foreach (var cmd in module.Commands)
                        {
                            var result = await cmd.CheckPreconditionsAsync(Context);
                            if (!result.IsSuccess) continue;
                            if (!array.Contains($"{prefix}{cmd.Remarks}\n"))
                                array.Add($"{prefix}{cmd.Remarks} {" ".PadLeft(sorted - cmd.Remarks.Length + 1)} :: {cmd.Summary}\n");
                        }

                        var description = array.Aggregate<string, string>(null, string.Concat);
                        if (string.IsNullOrWhiteSpace(description)) continue;
                        list.Add(new CommandInfo(module.Name, description));

                    }
                }
                foreach (var msg in Tools.SplitMessage($"{list.Aggregate(string.Empty, (current, cmi) => string.Concat(current, $"\n= {cmi.GetModName()} =\n{cmi.GetDec()}\n"))}", 1985))
                    await dm.SendMessageAsync($"```asciidoc\n{msg}```");
                await dm.CloseAsync();
                if (Context.Guild != null)
                    await Context.Channel.SendMessageAsync("Check your DMs!");
            }
        }


    }

    internal class CommandInfo
    {
        private readonly string _module;
        private readonly string _description;

        internal CommandInfo(string module, string description)
        {
            _module = module;
            _description = description;
        }
        internal string GetModName() => _module;
        internal string GetDec() => _description;
    }
}
