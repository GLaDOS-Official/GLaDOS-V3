using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace GladosV3.Services
{
    public class ExtensionLoadingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly BotSettingsHelper<string> _config;
        private readonly IServiceProvider _provider;

        public static List<GladosModuleStruct> extensions = new List<GladosModuleStruct>();
        public ExtensionLoadingService(DiscordSocketClient discord = null, CommandService commands = null, BotSettingsHelper<string> config = null, IServiceProvider provider = null)
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.CurrentDomainOnAssemblyResolve;
            this._discord = discord;
            this._commands = commands;
            this._config = config;
            this._provider = provider;
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies"));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Modules")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
            if (discord != null) return;
            foreach (var assemblyName in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
            {
                if (!this.IsValidCLRFile(assemblyName)) continue;
                var asm = Assembly.LoadFile(assemblyName);
                dependencies.Add(asm.FullName, asm);
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
            name = name.Substring(0, name.IndexOf(',')).ToLower();
            char slash = '/';
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                slash = '\\';
            string path =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{slash}.nuget{slash}packages{slash}{name}";
            if (!Directory.Exists(path)) return null;
            path = Directory.GetDirectories(path)[0];
            if (!Directory.Exists(path)) return null;
            path += $"{slash}lib";
            if (!Directory.Exists(path)) return null;
            path += Directory.GetDirectories(path, "netcoreapp*")[0];
            path += Directory.GetFiles(path, "*.dll")[0];
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
        public Task<Type[]> GetServices()
        {
            List<Type> types = new List<Type>();
            {
                foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), "*.dll")) // Bad extension loading
                {
                    try
                    {
                        Assembly asm = this.ValidFile(file);
                        if (asm == null) continue;
                        Type asmType = asm.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
                        ConstructorInfo asmConstructor = asmType.GetConstructor(Type.EmptyTypes);  // get extension's constructor
                        object magicClassObject = asmConstructor.Invoke(Array.Empty<object>()); // create object of class
                        var memberInfo = asmType.GetMethod("get_Services", BindingFlags.Instance | BindingFlags.Public); //get services method
                        if (memberInfo == null) continue;
                        var item = (Type[])memberInfo.Invoke(magicClassObject, Array.Empty<object>());
                        if (item != null && item.Length > 0)
                            types.AddRange(item); // invoke services method
                    }
                    catch (Exception) { /* ignored */ }
                }
            }
            return Task.FromResult(types.ToArray());

        }


        private Assembly ValidFile(string file)
        {
            if (new FileInfo(file).Length == 0) return null; // file is empty!
            try
            {
                if (!this.IsValidCLRFile(file)) return null; // file is not .NET assembly
                return Assembly.LoadFile(file);
            }
            catch (Exception) { return null; }
        }

        public async Task Load()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), "*.dll")) // Bad extension loading
            {
                try
                {
                    //TODO: remove validfile, cause it causes Assembly.Load (makes Unload worthless)
                    /*Assembly asm = ValidFile(file);
                    if (asm == null) continue;*/
                    GladosModuleStruct module = new GladosModuleStruct(file, this._discord, this._commands, this._config, this._provider);
                    module.Initialize();
                    if (module.loadFailed) continue;
                    module.PreLoad(this._discord, this._commands, this._config, this._provider);
                    var count = this._commands.AddModulesAsync(module.appAssembly, module._provider).GetAwaiter().GetResult().Count();
                    module.PostLoad(this._discord, this._commands, this._config, this._provider);

                    extensions.Add(module);
                    var modules = module.appAssembly.GetTypes().Where(type => type.IsClass && !type.IsSpecialName && type.IsPublic)
                        .Aggregate(string.Empty, (current, type) => current + type.Name + ", ");
                    await LoggingService.Log(LogSeverity.Verbose, "Module",
                        $"Loaded modules: {modules.Remove(modules.Length - 2)} from {Path.GetFileNameWithoutExtension(file)}");
                }
                catch (BadImageFormatException)
                { }
                catch (Exception ex)
                {
                    await LoggingService.Log(LogSeverity.Critical, "Module", $"Exception happened when loading \"{Path.GetFileNameWithoutExtension(file)}\"!\nMessage: {ex.Message}\nCallstack:\n{ex.StackTrace}\nTo prevent any errors or crashes, this module has not been loaded!");
                }
            }
        }
        public static Task Unload(string extensionName)
        {
            for (int i = 0; i < extensions.Count; i++)
            {
                if (extensions[i].moduleName != extensionName) continue;
                GladosModuleStruct e = extensions[i];
                extensions.RemoveAt(i);
                e.Unload();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
        private static readonly IDictionary<string, Assembly> dependencies = new Dictionary<string, Assembly>();


        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            LoggingService.Log(LogSeverity.Verbose, "ExtensionLoadingService", "Loading " + args.Name);
            return dependencies.TryGetValue(args.Name, out var res) ? res : TryLoadFromNuget(args);
        }
        private bool IsValidCLRFile(string file) // based on PE headers
        {
            bool? returnBool = null;
            uint[] dataDictionaryRVA = new uint[16];
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
                dataDictionaryRVA[i] = reader.ReadUInt32();
                dataDictionarySize[i] = reader.ReadUInt32();
            }
            if (peHeaderSignature != 0x4550)
            { LoggingService.Log(LogSeverity.Error, "Module", $"{file} has non-valid PE header!"); returnBool = false; }
            if (dataDictionaryRVA[13] == 64 && returnBool == null)
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

    public class GladosModuleStruct : AssemblyLoadContext
    {
        public AssemblyName asmName;
        public Assembly appAssembly;
        public string moduleName;
        public string moduleAuthor;
        public string moduleVersion;
        public bool loadFailed;
        public IServiceProvider _provider;

        private IGladosModule module;
        private readonly AssemblyDependencyResolver _resolver;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly BotSettingsHelper<string> _config;
        private readonly string _path;
        public GladosModuleStruct(string path, DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider) : base(isCollectible: true)
        {
            this._discord = discord;
            this._commands = commands;
            this._config = config;
            this._provider = provider;
            this._resolver = new AssemblyDependencyResolver(path);
            this._path = path;
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            if (this.appAssembly == null) this.appAssembly = Assembly.LoadFile(this._path);
            var asmType = this.appAssembly.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
            MethodInfo getInterface = asmType.GetMethod("GetModule", BindingFlags.Static | BindingFlags.Public);
            this.module = (IGladosModule)getInterface.Invoke(null, null);
            this.moduleName = this.module.Name();
            this.moduleAuthor = this.module.Author();
            this.moduleVersion = this.module.Version();

            if (string.IsNullOrWhiteSpace(this.moduleName)) this.loadFailed = true; // class doesn't have Name string
            if (string.IsNullOrWhiteSpace(this.moduleVersion)) this.loadFailed = true; // class doesn't have Version string
            if (string.IsNullOrWhiteSpace(this.moduleAuthor)) this.loadFailed = true; // class doesn't have Author string
            if (this.loadFailed) { this.Unload(); return; }
            this.asmName = new AssemblyName(this.moduleName) { CodeBase = _path };
        }

        protected override Assembly Load(AssemblyName name)
        {
            string assemblyPath = this._resolver.ResolveAssemblyToPath(name);
            if (assemblyPath != null)
            {
                return this.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
        public void PreLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider) => this.module.PreLoad(discord, commands, config, provider);

        public void PostLoad(DiscordSocketClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider) => this.module.PostLoad(discord, commands, config, provider);

        public List<Type> GetServices() => this.module.Services.ToList();

        ~GladosModuleStruct() => this.module?.Unload(this._discord, this._commands, this._config, this._provider);
    }
}
