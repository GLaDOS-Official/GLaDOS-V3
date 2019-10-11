﻿using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Runtime.Loader;
using GladosV3.Helpers;

namespace GladosV3.Module.ImageGeneration
{
    public class ModuleInfo : IGladosModule
    {
        public string Name() => "ImageGenerator";

        public string Version() => "0.0.0.1";

        public string UpdateUrl() => null;

        public string Author() => "BlackOfWorld#8125";

        public Type[] Services => new[] { typeof(GeneratorService) };
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

        public void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceProvider provider)
        { }

        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Reload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public void Unload(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        { }

        public static void OnPluginUnloadingRequested(AssemblyLoadContext obj)
        { }
    }
}
