using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Module.NomadCI
{
    public class Commands : ModuleBase<ICommandContext>
    {
        [Command("build", RunMode = RunMode.Async)]
        [Remarks("build")]
        [Summary("build")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [NomadOnly]
        public Task JoinCmd()
        {
            BuilderService.Service.BuildNow().GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}