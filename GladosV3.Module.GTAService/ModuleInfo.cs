using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace GLaDOSV3.Module.GTAService
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "GTAServices";

        public string Version() => "0.0.0.1";

        public Uri UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        private static volatile ModuleInfo singleton;
        public static IGladosModule GetModule()
        {
            if (singleton != null) return singleton;
            singleton = new ModuleInfo();
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            AssemblyLoadContext currentContext = AssemblyLoadContext.GetLoadContext(currentAssembly);
            currentContext.Unloading += OnPluginUnloadingRequested;
            return singleton;
        }

        public void PreLoad(
            DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceProvider    provider)
        {
        }

        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
        }

        public void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public static void OnPluginUnloadingRequested(AssemblyLoadContext obj)
        { }

        public Type[] Services(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceCollection provider)
        {
            var types = new List<Type>
            {
                new InteractivityService(discord, TimeSpan.FromSeconds(60)).GetType(), typeof(TokenManager)
            };
            return types.ToArray();
        }
    }
}
