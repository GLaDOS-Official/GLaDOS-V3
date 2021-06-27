using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GLaDOSV3.Services
{
    public class ExtensionLoadingService
    {
        private static DiscordSocketClient discord;
        private static CommandService commands;
        private static BotSettingsHelper<string> config;
        private static IServiceProvider provider;

        public static List<GladosModuleStruct> Extensions = new List<GladosModuleStruct>();
        public static void Init(DiscordSocketClient discord = null, CommandService commands = null, BotSettingsHelper<string> config = null, IServiceProvider provider = null)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;
            ExtensionLoadingService.discord = discord;
            ExtensionLoadingService.commands = commands;
            ExtensionLoadingService.config = config;
            ExtensionLoadingService.provider = provider;
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies"));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Modules")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
            if (discord != null) return;
            foreach (var assemblyName in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
            {
                if (!ValidFile(assemblyName)) continue;
                var asm = Assembly.LoadFile(assemblyName);
                Dependencies.Add(asm.FullName, asm);
            }
        }

        public static Assembly TryLoadFromNuget(ResolveEventArgs args)
        {
            try
            {
                Assembly load = Assembly.LoadFile(args.Name);
                return load; // no error
            }
            catch { }

            string name = args.Name;
            name = name.Substring(0, name.IndexOf(',', StringComparison.Ordinal)).ToLowerInvariant();
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}.nuget{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}{name}";
            if (!Directory.Exists(path)) return null;
            path = Directory.GetDirectories(path)[0];
            if (!Directory.Exists(path)) return null;
            path += $"{Path.DirectorySeparatorChar}lib";
            if (!Directory.Exists(path)) return null;
            if (Directory.Exists(Path.Combine(path, "netcoreapp31"))) path = Path.Combine(path, "netcoreapp31");
            else if (Directory.Exists(Path.Combine(path, "netstandard20"))) path = Path.Combine(path, "netstandard20");
            path = Directory.GetFiles(path, "*.dll")[0];
            if (!File.Exists(path)) return null;
            try
            {
                Assembly load = Assembly.LoadFile(path);
                return load;
            }
            catch
            { }
            return null;
        }
        public static Task<Type[]> GetServices(DiscordSocketClient client, IServiceCollection provider)
        {
            List<Type> types = new List<Type>();
            foreach (var extension in Extensions) // Bad extension loading
            {
                types.AddRange(extension.GetServices(client, provider));
            }

            return Task.FromResult(types.ToArray());

        }

        private static bool ValidFile(string file)
        {
            if (new FileInfo(file).Length == 0) return false; // file is empty!
            try { return IsValidClrFile(file); }
            catch (Exception) { return false; }
        }

        public static async void LoadExtensions()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), "*.dll")) // Bad extension loading
            {
                try
                {
                    if (!ValidFile(file)) continue;
                    using GladosModuleStruct module = new GladosModuleStruct(file, discord, commands, config, provider);
                    module.Initialize();
                    if (module.LoadFailed) { module.Dispose(); continue; }
                    Extensions.Add(module);
                }
                catch (BadImageFormatException)
                { }
                catch (Exception ex)
                {
                    await LoggingService.Log(LogSeverity.Critical, "Module", $"Exception happened when loading \"{Path.GetFileNameWithoutExtension(file)}\"!\nMessage: {ex.Message}\nCallstack:\n{ex.StackTrace}\nTo prevent any errors or crashes, this module has not been loaded!").ConfigureAwait(false);
                }
            }
        }
        public static async Task Load()
        {
            foreach (var module in Extensions) // Bad extension loading
            {
                module.FixShit(discord, commands, config, provider);
                module.PreLoad();
                var count = commands.AddModulesAsync(module.AppAssembly, module.Provider).GetAwaiter().GetResult().Count();
                module.PostLoad();

                var modules = module.AppAssembly.GetTypes().Where(type => type.IsClass && !type.IsSpecialName && type.IsPublic)
                                    .Aggregate(string.Empty, (current, type) => current + type.Name + ", ");
                await LoggingService.Log(LogSeverity.Verbose, "Module",
                                         $"Loaded modules: {modules.Remove(modules.Length - 2)} from {Path.GetFileNameWithoutExtension(module.AppAssembly.Location)}").ConfigureAwait(false);
            }
        }
        public static Task Unload(string extensionName)
        {
            for (int i = 0; i < Extensions.Count; i++)
            {
                if (Extensions[i].ModuleName != extensionName) continue;
                GladosModuleStruct e = Extensions[i];
                Extensions.RemoveAt(i);
                e.Unload();
                e.Dispose();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
        private static readonly IDictionary<string, Assembly> Dependencies = new Dictionary<string, Assembly>();


        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            LoggingService.Log(LogSeverity.Verbose, "ExtensionLoadingService", "Loading " + args.Name);
            return Dependencies.TryGetValue(args.Name, out var res) ? res : TryLoadFromNuget(args);
        }
        private static bool IsValidClrFile(string file) // based on PE headers
        {
            bool? returnBool = null;
            uint[] dataDictionaryRva = new uint[16];
            uint[] dataDictionarySize = new uint[16];
            Stream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            fs.Position = 0x3C;
            var peHeader = reader.ReadUInt32();
            fs.Position = peHeader;
            var peHeaderSignature = reader.ReadUInt32();
            ushort dataDictionaryStart = Convert.ToUInt16(Convert.ToUInt16(fs.Position) + 0x60);
            fs.Position = dataDictionaryStart;
            for (int i = 0; i < 15; i++)
            {
                dataDictionaryRva[i] = reader.ReadUInt32();
                dataDictionarySize[i] = reader.ReadUInt32();
            }
            if (peHeaderSignature != 0x4550)
            { LoggingService.Log(LogSeverity.Error, "Module", $"{file} has non-valid PE header!"); returnBool = false; }
            if (dataDictionaryRva[13] == 64 && returnBool == null)
            {
                LoggingService.Log(LogSeverity.Error, "Module", $"{file} is NOT a valid CLR file!!");
                returnBool = false;
            }
            else
                returnBool = true;
            reader.Close();
            fs.Close();
            return (bool)returnBool;
        }
    }

    public sealed class GladosModuleStruct : AssemblyLoadContext, IDisposable
    {
        public AssemblyName AsmName;
        public Assembly AppAssembly;
        public string ModuleName;
        public string ModuleAuthor;
        public string ModuleVersion;
        public bool LoadFailed;
        public IServiceProvider Provider;
        private bool disposed;

        private IGladosModule module;
        private AssemblyDependencyResolver resolver;
        private DiscordSocketClient discord;
        private CommandService commands;
        private BotSettingsHelper<string> config;
        private string path;
        public GladosModuleStruct(string path, DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider) : base(isCollectible: true)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
            this.Provider = provider;
            this.resolver = new AssemblyDependencyResolver(path);
            this.path = path;
        }
        public void FixShit(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
            this.Provider = provider;
        }
        public void Initialize()
        {
            if (this.AppAssembly == null) this.AppAssembly = this.LoadFromAssemblyPath(this.path);
            var asmType = this.AppAssembly.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
            MethodInfo getInterface = asmType.GetMethod("GetModule", BindingFlags.Static | BindingFlags.Public);
            Debug.Assert(getInterface != null, nameof(getInterface) + " != null");
            this.module = (IGladosModule)getInterface?.Invoke(null, null);
            this.ModuleName = this.module.Name();
            this.ModuleAuthor = this.module.Author();
            this.ModuleVersion = this.module.Version();

            if (string.IsNullOrWhiteSpace(this.ModuleName))    this.LoadFailed = true; // class doesn't have Name string
            if (string.IsNullOrWhiteSpace(this.ModuleVersion)) this.LoadFailed = true; // class doesn't have Version string
            if (string.IsNullOrWhiteSpace(this.ModuleAuthor))  this.LoadFailed = true; // class doesn't have Author string
            if (this.LoadFailed) { this.Unload(); return; }
            this.AsmName = new AssemblyName(this.ModuleName ?? throw new InvalidOperationException()) { CodeBase = this.path };
        }


        protected override Assembly Load(AssemblyName name)
        {
            string assemblyPath = this.resolver.ResolveAssemblyToPath(name);
            return assemblyPath != null ? this.LoadFromAssemblyPath(assemblyPath) : null;
        }
        public void PreLoad() => this.module.PreLoad(this.discord, this.commands, this.config, this.Provider);

        public void PostLoad() => this.module.PostLoad(this.discord, this.commands, this.config, this.Provider);

        public List<Type> GetServices(DiscordSocketClient client, IServiceCollection provider) => this.module.Services(client, this.commands, this.config, provider).ToList();

        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;
            this.Unload();
            this.module?.Unload(this.discord, this.commands, this.config, this.Provider);
        }

        ~GladosModuleStruct() => this.Dispose();
    }
}
