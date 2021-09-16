using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace GLaDOSV3.Models
{
    public abstract class GladosModule
    {
        public abstract string   Name             { get; }
        public abstract string   Version          { get; }
        public virtual  Uri      UpdateUrl        => null;
        public abstract string   AuthorLink           { get; }
        public static   DateTime GetCompileTime() => Builtin.CompileTime;

        protected GladosModule()
        {
            Instance = this;
            AssemblyLoadContext currentContext  = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            Debug.Assert(currentContext != null, nameof(currentContext) + " != null");
            currentContext.Unloading -= this.OnPluginUnloadingRequested;
            currentContext.Unloading += this.OnPluginUnloadingRequested;
        }

        public static    GladosModule Instance;

        private protected virtual void OnPluginUnloadingRequested(AssemblyLoadContext obj)
        { }

        public virtual Type[] Services(
            DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceCollection   provider) =>
            Type.EmptyTypes;

        public virtual void PreLoad(
            DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceProvider provider)
        { }

        public virtual void PostLoad(
            DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceProvider provider)
        { }

        public virtual void Unload(
            DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config,
            IServiceProvider provider)
        { }
    }
}