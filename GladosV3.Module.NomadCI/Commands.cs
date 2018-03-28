using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Module.NomadCI
{
    public class Commands : ModuleBase<ICommandContext>
    {
        private readonly BuilderService _service;
        public Commands(BuilderService service)
        {
            _service = service;
        }
        [Command("build", RunMode = RunMode.Async)]
        [Remarks("build")]
        [Summary("build")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [NomadOnly]
        public Task JoinCmd()
        {
            _service.BuildNow().GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}