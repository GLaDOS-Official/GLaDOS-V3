using Discord;
using Discord.Commands;
using GLaDOSV3.Attributes;
using GLaDOSV3.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService service;

        public HelpModule(CommandService service) => this.service = service;

        [Command("modules")]
        [Remarks("modules")]
        [Summary("Shows information on a module such as any remarks and a count of how many commands")]
        public async Task Modules()
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(0, 255, 100),
                Title = "Here's the information about all modules\n"
            };
            foreach (var module in this.service.Modules)
            {
                builder.AddField(e =>
                {
                    e.Name = $"**{module.Name}**";
                    e.Value = $"{module.Commands.Count} commands";
                    e.IsInline = true;
                });
            }
            for (var i = 0; i <= Tools.RoundToDividable<int>(builder.Fields.Count, 3) - builder.Fields.Count; i++)
                builder.AddBlankField(true);
            this.ReplyAsync("", false, builder.Build()).GetAwaiter().GetResult();
            if (Context.Guild != null)
                await Context.Message.AddReactionAsync(new Emoji("👌")).ConfigureAwait(false);
        }

        [Command("help")]
        [Remarks("help [command]")]
        [Summary("How 2 use ...?")]
        public async Task Help([Remainder]string command = null)
        {
            IDMChannel dm = await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(true);
            Random rnd = new Random();
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256))
            };
            var prefix = IsOwner.BotSettingsHelper["prefix"];
            if (command != null)
            {
                var result = this.service.Search(Context, command);
                if (!result.IsSuccess)
                {
                    await this.ReplyAsync($"Sorry, I couldn't find a command like **{await Tools.EscapeMentionsAsync(Context.Guild, Context.Channel, command).ConfigureAwait(true)}**.").ConfigureAwait(false);
                    return;
                }

                //Hide commands that the user doesn't have access to
                List<CommandMatch> list = new List<CommandMatch>();
                foreach (var match in result.Commands)
                {
                    var preconditions = await match.CheckPreconditionsAsync(Context).ConfigureAwait(true);
                    if (!preconditions.IsSuccess) continue;
                    list.Add(match);
                }
                if (!list.Any())
                {
                    await this.ReplyAsync($"Sorry, I couldn't find a command like **{await Tools.EscapeMentionsAsync(Context.Guild, Context.Channel, command).ConfigureAwait(true)}**.").ConfigureAwait(false);
                    return;
                }
                builder.Description = $"Here are some commands like **{await Tools.EscapeMentionsAsync(Context.Guild, Context.Channel, command).ConfigureAwait(true)}**";

                foreach (var match in list)
                {
                    var text = (match.Command.Parameters.Count == 0) ? "Arguments: None\n" : $"Arguments: {string.Join(", ", match.Command.Parameters.Select(p => $"({p.Type.Name}) {p.Name}"))}\n";
                    text = string.Concat(text, !string.IsNullOrWhiteSpace(match.Command.Summary) ? "" : $"Info: {match.Command.Summary}\n");
                    text = string.Concat(text, !string.IsNullOrWhiteSpace(match.Command.Remarks) ? "" : $"Example: {match.Command.Remarks}\n");
                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", match.Command.Aliases);
                        x.Value = text;
                        x.IsInline = false;
                    });
                }
                await dm.SendMessageAsync(embed: builder.Build()).ConfigureAwait(false);
            }
            else
            {
                List<CommandInfo> list = new List<CommandInfo>();
                if (this.service != null)
                {

                    var largeCommand = this.service.Commands.OrderByDescending(x => (x.Remarks?.Length ?? x.Name.Length) + (x.Module.Group?.Length + 1 ?? 0)).FirstOrDefault();
                    Debug.Assert(largeCommand != null, nameof(largeCommand) + " != null");
                    var sorted = largeCommand.Remarks.Length + (largeCommand.Module.Group?.Length + 1 ?? 0);


                    builder.Description = "These are the commands you can use.";
                    foreach (var module in this.service.Modules)
                    {
                        List<string> array = new List<string>();
                        foreach (var cmd in module.Commands)
                        {
                            var result = await cmd.CheckPreconditionsAsync(Context).ConfigureAwait(true);
                            if (!result.IsSuccess) continue;
                            array.Add($"{prefix}{(cmd.Module.Group == null ? "" : cmd.Module.Group.ToLowerInvariant() + " ")}{cmd.Remarks ?? cmd.Name} {" ".PadLeft(sorted - (cmd.Module.Group?.Length + 1 ?? 1) - (cmd.Remarks?.Length + 1 ?? cmd.Name.Length + 1))} :: {cmd.Summary ?? ("None")}\n");
                        }

                        var description = array.Aggregate<string, string>(null, string.Concat);
                        if (string.IsNullOrWhiteSpace(description)) continue;
                        list.Add(new CommandInfo(module.Name, description));

                    }
                    foreach (var msg in Tools.SplitMessage($"{list.Aggregate(string.Empty, (current, cmi) => string.Concat(current, $"\n= {cmi.Module} =\n{cmi.Description}\n"))}", 1985))
                        await dm.SendMessageAsync($"```asciidoc\n{msg}```").ConfigureAwait(false);
                    await dm.CloseAsync().ConfigureAwait(false);

                }
                if (Context.Guild != null) await Context.Message.AddReactionAsync(new Emoji("👌")).ConfigureAwait(false);
            }
        }
    }
    internal struct CommandInfo
    {
        public readonly string Module;
        public readonly string Description;
        internal CommandInfo(string module, string description)
        {
            this.Module = module;
            this.Description = description;
        }
    }
}
