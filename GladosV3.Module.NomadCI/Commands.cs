using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Module.NomadCI
{
    public class Commands : ModuleBase<ICommandContext>
    {
        [Command("build", RunMode = RunMode.Async)]
        [Remarks("build [Start/Now/Stop/Status]")]
        [Summary("Nomad group? 🤔")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [NomadOnly]
        public Task JoinCmd(CommandArgument c)
        {
            switch (c)
            {
                case CommandArgument.Now:
                    BuilderService.Service.BuildNow().GetAwaiter().GetResult();
                    break;
                case CommandArgument.Start:
                    if (!BuilderService._timer.Enabled)
                    { BuilderService._timer.Start(); ReplyAsync("Continuous build is now enabled!"); }
                    else
                        ReplyAsync("Continuous build is already enabled!");

                    break;
                case CommandArgument.Stop:
                    if (BuilderService._timer.Enabled)
                    { BuilderService._timer.Stop(); ReplyAsync("Continuous build is now disabled!"); }
                    else
                        ReplyAsync("Continuous build is already disabled!");
                    break;
                case CommandArgument.Status:
                    string status = BuilderService._timer.Enabled ? "Enabled" : "Disabled";
                    ReplyAsync($"Continuous build: {status}\n" +
                               $"Another build in: {BuilderService.nextBuildTime.Subtract(DateTime.Now):d\'d \'hh\'h \'mm\'m \'ss\'s\'}");
                    break;
            }
            return Task.CompletedTask;
        }
    }
    public enum CommandArgument
    {
        Start,
        Now,
        Stop,
        Status
    }
}