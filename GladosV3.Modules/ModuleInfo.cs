using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Modules
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "Default";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";
        public void OnLoad(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {
            //Console.WriteLine($"I AM LOADED!!! HEY!! IT'S ME!!! DEFAULT MODULE!!!");
        }
    }
}
