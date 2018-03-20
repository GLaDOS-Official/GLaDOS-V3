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
        [Attributes.Timeout(1, 30, Attributes.Measure.Seconds)]
        public async Task Delete([Remainder]string text)
        {
            if (!_service.fail)
                await Context.Channel.SendFileAsync(_service.Delete(text, Context).GetAwaiter().GetResult(), "delet.jpg");
            else
                await Context.Channel.SendMessageAsync("There was an error... Check the logs!");
        }
        [Command("shit", RunMode = RunMode.Async)]
        [Remarks("shit")]
        [Summary("shit")]
        [Attributes.Timeout(1,30, Attributes.Measure.Seconds)]
        public async Task Shit([Remainder]string text)
        {
            if (!_service.fail)
                await Context.Channel.SendFileAsync(_service.Shit(text.Split(','), Context).GetAwaiter().GetResult(), "shit.jpg");
            else
                await Context.Channel.SendMessageAsync("There was an error... Check the logs!");
        }
        
    }
}