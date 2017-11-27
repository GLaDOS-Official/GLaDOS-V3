using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using GladosV3.Services;
using Microsoft.Extensions.PlatformAbstractions;

namespace GladosV3.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("info"), Remarks("Displays bot info.")]
        [Summary("info")]
        public async Task Info()
        {
            var uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
            var heapsize = Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
            var guildcount = Context.Client.Guilds.Count;
            var runtimeAttributes = System.Reflection.Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute),true);
            if (runtimeAttributes.Length > 0)
            {
                var targetFrameworkAttribute = (TargetFrameworkAttribute)runtimeAttributes.First();
            }
            IDMChannel DM = await Context.Message.Author.GetOrCreateDMChannelAsync();
            var message =(
                                                   $"{Format.Bold("Info")}\n" +
                                                   $"- Library: Discord.Net ({DiscordConfig.APIVersion})\n" +
                                                   $"- Runtime: {PlatformServices.Default.Application.RuntimeFramework.Identifier.Replace("App",String.Empty)} {PlatformServices.Default.Application.RuntimeFramework.Version} {IntPtr.Size * 8}-bit\n" +
                                                   $"- System: { System.Runtime.InteropServices.RuntimeInformation.OSDescription} {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower()}\n" +
                                                   $"- Uptime: {uptime}\n" +
                                                   $"- Author: <@195225230908588032> \n\n" +


                                                   $"{Format.Bold("Stats")}\n" +
                                                   $"- Heap Size: {heapsize} mb\n" +
                                                   $"- Servers: {guildcount}\n"
            );
            if (Context.Guild != null)
            {
                var channelscount = Context.Guild.Channels.Count;
                var userscount = Context.Guild.Users.Count();
                message += $"- Channels: {channelscount} (in this guild) \n" +
                           $"- Users: {userscount} (in this guild) \n";
            }
            await DM.SendMessageAsync(message);
        }
    }
}
