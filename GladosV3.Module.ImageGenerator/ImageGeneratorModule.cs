using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GladosV3.Helpers;
using GladosV3.Services;

namespace GladosV3.Module.ImageGeneration
{
    public class ImageGeneratorModule : ModuleBase<ICommandContext>
    {
        private readonly GeneratorService _service;

        public ImageGeneratorModule(GeneratorService service)
        {
            _service = service;
        }
        [Command("delete", RunMode = RunMode.Async)]
        [Remarks("delete")]
        [Summary("delete")]
        public async Task test([Remainder]string text)
        {
            await Context.Channel.SendFileAsync(_service.Delete(text,Context).GetAwaiter().GetResult(),"delet.jpg");
        }
        [Command("shit", RunMode = RunMode.Async)]
        [Remarks("shit")]
        [Summary("shit")]
        public async Task shit([Remainder]string text)
        {
            await Context.Channel.SendFileAsync(_service.Shit(text.Split(','),Context).GetAwaiter().GetResult(), "shit.jpg");
        }
        
    }
}