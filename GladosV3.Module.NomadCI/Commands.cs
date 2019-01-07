using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Module.NomadCI
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [NomadOnly]
    [Summary("Nomad group? 🤔")]
    public class Commands : ModuleBase<ICommandContext>
    {
        [Command("build now", RunMode = RunMode.Async)]
        [Remarks("build now")]
        [Summary("Triggers the build right now.")]
        public Task JoinCmd()
        {
<<<<<<< HEAD
            BuilderService.Service.BuildNow().GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
=======
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
>>>>>>> 15c8c1a0bfe431ef18b6d59b1b1b4a5255cbeb05

        [Command("build start", RunMode = RunMode.Async)]
        [Remarks("build start")]
        [Summary("Starts the timer to build trigger.")]
        public Task StartBuild()
        {
            if (!BuilderService.Timer.Enabled)
            {
                BuilderService.Timer.Start();
                ReplyAsync("Continuous build is now enabled!");
            }
            else
                ReplyAsync("Continuous build is already disabled!");
            return Task.CompletedTask;
        }
        [Command("build stop", RunMode = RunMode.Async)]
        [Remarks("build stop")]
        [Summary("Stops the timer to build trigger.")]
        public Task StopBuild()
        {
            if (BuilderService.Timer.Enabled)
            { BuilderService.Timer.Stop(); ReplyAsync("Continuous build is now disabled!"); }
            else
                ReplyAsync("Continuous build is already disabled!");
            return Task.CompletedTask;
        }
        [Command("build status", RunMode = RunMode.Async)]
        [Remarks("build status")]
        [Summary("Shows the current status of the build.")]
        public Task StatusBuild()
        {
            ReplyAsync($"Continuous build: {(BuilderService.Timer.Enabled ? "Enabled" : "Disabled")}\n" +
                     $"Another build in: {BuilderService.NextBuildTime.Subtract(DateTime.Now):d\'d \'hh\'h \'mm\'m \'ss\'s\'}");
            return Task.CompletedTask;
        }
    }
}