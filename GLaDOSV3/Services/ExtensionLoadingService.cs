using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using GLaDOSV3.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace GLaDOSV3.Services
{
    internal enum ExtensionErrorCodes : ushort
    {
        VALID,
        UNTRUSTED_UNKNOWN,
        UNTRUSTED_INVALID,
        UNTRUSTED_FAILED
    }
    public class ExtensionLoadingService
    {
        private static DiscordShardedClient _discord;
        private static CommandService _commands;
        private static BotSettingsHelper<string> _config;
        private static IServiceProvider _provider;
        private static ILogger _logger;
        public static List<GladosModuleStruct> Extensions = new List<GladosModuleStruct>();
        public static Dictionary<string, IntPtr> PInvokes = new Dictionary<string, IntPtr>();
        private static readonly string Architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        [RequiresUnreferencedCodeAttribute("Load module's dependencies")]
        public static void Init(DiscordShardedClient discord = null, CommandService commands = null, BotSettingsHelper<string> config = null, IServiceProvider provider = null)
        {
            _logger = Log.ForContext<ExtensionLoadingService>();
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies"));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Modules")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
            if (discord != null) return;
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), PInvokeResolver);
            foreach (var file in GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "PInvoke"), "*.so|*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    if (new DirectoryInfo(file).Parent?.Name.ToLowerInvariant() == "x64" && Architecture != "x64") continue;
                    if (new DirectoryInfo(file).Parent?.Name.ToLowerInvariant() == "x86" && Architecture != "x86") continue;
                    var valid = Path.GetExtension(file) == ".so" && StaticTools.IsLinux() || Path.GetExtension(file) == ".dll" && StaticTools.IsWindows();
                    if (!valid) continue;
                    var filename = Path.GetFileNameWithoutExtension(file);
                    if (!NativeLibrary.TryLoad(file, out IntPtr ptr))
                    { _logger.Error("Failed to load PInvoke \'{filename}\'", filename); continue; }
                    _logger.Verbose("Successfully loaded PInvoke \'{filename}\'", filename);
                    PInvokes.Add(filename, ptr);
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex, ex.Message);
                }
            }
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies"), "*.dll", SearchOption.AllDirectories))
            {
                if (new DirectoryInfo(file).Parent?.Name.ToLowerInvariant() == "x64" && Architecture != "x64") continue;
                if (new DirectoryInfo(file).Parent?.Name.ToLowerInvariant() == "x86" && Architecture != "x86") continue;
                if (!ValidFile(file)) continue;
                Assembly asm = Assembly.LoadFile(file);
                NativeLibrary.SetDllImportResolver(asm, PInvokeResolver);
                Dependencies.Add(asm.FullName ?? throw new InvalidOperationException(), asm);
            }
        }

        public static string GetRuntimePInvoke(string name)
        {
            //Try my crazy hack :tm:
            List<string> dirName = new List<string>();
            if (StaticTools.IsWindows()) { dirName.AddRange(new[] { "win-" + Architecture, "win" }); }
            if (StaticTools.IsLinux()) { dirName.Add("linux-" + Architecture); }
            if (StaticTools.IsMacOS()) { dirName.Add("osx-" + Architecture); }
            if (StaticTools.IsUnix()) { dirName.Add("unix"); }

            foreach (var path in dirName)
            {
                foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "runtimes", path), "*", SearchOption.AllDirectories))
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName == name) return file;
                }
            }

            return "";
        }
        public static IntPtr PInvokeResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var filename = Path.GetFileNameWithoutExtension(libraryName);
            if (PInvokes.ContainsKey(filename)) return PInvokes[filename];
            _logger.Debug("Resolving PInvoke: {name}", libraryName);
            if (NativeLibrary.TryLoad(libraryName, out IntPtr ptr)) { PInvokes.Add(filename, ptr); return ptr; }

            //crazy russian doll hack
            if (NativeLibrary.TryLoad(GetRuntimePInvoke(libraryName), out ptr))
            { PInvokes.Add(filename, ptr); return ptr; }
            _logger.Fatal("Failed to load PInvoke: {name}", libraryName);
            return IntPtr.MaxValue;
        }
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            var searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (var sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }
        [RequiresUnreferencedCodeAttribute("Load's dependency from Nuget folder")]
        public static Assembly TryLoadFromNuget(ResolveEventArgs args)
        {
            try
            {
                Assembly load = Assembly.LoadFile(args.Name);
                return load; // no error
            }
            catch { }

            var name = args.Name;
            name = name[..name.IndexOf(',', StringComparison.Ordinal)].ToLowerInvariant();
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}.nuget{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}{name}";
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
        public static IEnumerable<Type[]> GetServices(DiscordShardedClient client, IServiceCollection provider) => Extensions.Select(extension => extension.GetServices(client, provider));

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
                    using GladosModuleStruct module = new GladosModuleStruct(file, _discord, _commands, _config, _provider);
                    module.Initialize();
                    if (module.LoadFailed) continue;
                    Extensions.Add(module);
                }
                catch (BadImageFormatException)
                { }
                catch (Exception ex)
                {
                    _logger.Fatal(ex.ToString());
                }
            }
        }
        [RequiresUnreferencedCode("Get types of modules's discord modules")]
        public static Task Load()
        {
            foreach (GladosModuleStruct module in Extensions) // Bad extension loading
            {
                Load(module);
            }
            return Task.CompletedTask;
        }

        public static Task Load(GladosModuleStruct module)
        {
            module.FixShit(_discord, _commands, _config, _provider);
            module.PreLoad();
            var count = _commands.AddModulesAsync(module.AppAssembly, module.Provider).GetAwaiter().GetResult().Count();
            module.PostLoad();

            var modules = module.AppAssembly.GetTypes().Where(type => type.IsClass && !type.IsSpecialName && type.IsPublic)
                                .Aggregate(string.Empty, (current, type) => current + type.Name + ", ");
            _logger.Verbose("Loaded modules: {modules} from {filename}", modules.Remove(modules.Length - 2), Path.GetFileNameWithoutExtension(module.ModulePath));
            return Task.CompletedTask;
        }

        public static bool Load(CommandService service, string fileName)
        {
            if (Extensions.Count(p => Path.GetFileNameWithoutExtension(p.ModulePath) == fileName) != 0) return true;
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), "*.dll")) // Bad extension loading
            {
                if (Path.GetFileNameWithoutExtension(file).ToLowerInvariant() != fileName.ToLowerInvariant()) continue;
                try
                {
                    if (!ValidFile(file)) continue;
                    using GladosModuleStruct module = new GladosModuleStruct(file, _discord, _commands, _config, _provider);
                    module.Initialize();
                    if (module.LoadFailed) continue;
                    Extensions.Add(module);
                    Load(module);
                    return true;
                }
                catch (BadImageFormatException)
                { }
                catch (Exception ex)
                {
                    _logger.Fatal(ex.ToString());
                }
            }
            return false;
        }
        public static bool Reload(CommandService service, string extensionName)
        {
            _logger.Verbose("[ExtensionLoadingService] Reloading extension: {extensionName}", extensionName);
            foreach (GladosModuleStruct e in Extensions.Where(t => t.ModuleName == extensionName))
            {
                e.Dispose();
                Extensions.Remove(e);
                using GladosModuleStruct module = new GladosModuleStruct(e.ModulePath, _discord, _commands, _config, _provider);
                module.Initialize();
                if (module.LoadFailed) continue;
                Extensions.Add(module);
                Load(module);
                return true;
            }

            return false;
        }
        public static bool Unload(CommandService service, string extensionName)
        {
            _logger.Verbose("[ExtensionLoadingService] Unloading extension: {extensionName}", extensionName);
            for (var i = 0; i < Extensions.Count; i++)
            {
                if (Extensions[i].ModuleName != extensionName) continue;
                GladosModuleStruct e = Extensions[i];
                Extensions.RemoveAt(i);
                e.Dispose();
                return true;
            }
            return false;
        }
        private static readonly IDictionary<string, Assembly> Dependencies = new Dictionary<string, Assembly>();


        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (Dependencies.TryGetValue(args.Name, out Assembly res))
                return res;
            _logger.Verbose("[ExtensiongLoadingService] Loading {Name}", args.Name);
            return TryLoadFromNuget(args);
        }
        private static bool IsValidClrFile(string file) // based on PE headers
        {
            try
            {
                AssemblyName.GetAssemblyName(file);
            }
            catch (BadImageFormatException)
            {

                _logger.Error("[Module] {file} is NOT a valid CLR file!!", file);
                return false;
            }

            return true;
        }
#pragma warning disable IDE0022
        internal static Task<ExtensionErrorCodes> GetUsernameFromLink(GladosModuleStruct module)
        {
#if DEBUG
            return Task.FromResult(ExtensionErrorCodes.VALID);
#else
            var url = module.ModuleAuthorLink;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return Task.FromResult(ExtensionErrorCodes.UNTRUSTED_INVALID);
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return Task.FromResult(ExtensionErrorCodes.UNTRUSTED_INVALID);
            if (uri.Scheme != Uri.UriSchemeHttps) return Task.FromResult(ExtensionErrorCodes.UNTRUSTED_INVALID);
            UriBuilder builder = new UriBuilder(uri);
            builder.UserName = builder.Password = "";
            builder.Port = 443;
            builder.Host = "api.github.com";
            builder.Path = $"/users/{url.Split('/').Last()}";
            using HttpClient client = new HttpClient();
            try
            {

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; PPC Mac OS X 10 11_0) AppleWebKit/533.1 (KHTML, like Gecko) Chrome/59.0.811.0 Safari/533.1");
                var str = client.GetStringAsync(builder.Uri).GetAwaiter().GetResult();
                var o = JObject.Parse(str);
                module.ModuleAuthor = o["login"].ToString();
                return Task.FromResult(o["html_url"].ToString() == url
                                           ? ExtensionErrorCodes.VALID
                                           : ExtensionErrorCodes.UNTRUSTED_UNKNOWN);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Failed to verify integrity of {moduleName}", module.ModuleName);
                return Task.FromResult(ExtensionErrorCodes.UNTRUSTED_FAILED);
            }
#endif
        }
#pragma warning restore IDE0022
    }

    public sealed class GladosModuleStruct : AssemblyLoadContext, IDisposable
    {
        public AssemblyName AsmName;
        public Assembly AppAssembly;
        public string ModuleName;
        public string ModuleAuthor;
        public string ModuleAuthorLink;
        public string ModuleVersion;
        public bool LoadFailed;
        public IServiceProvider Provider;
        public string ModulePath;
        public List<Type> Commands;

        private bool disposed;
        private GladosModule module;
        private DiscordShardedClient discord;
        private CommandService commands;
        private BotSettingsHelper<string> config;
        public GladosModuleStruct(string modulePath, DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider) : base(isCollectible: true)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
            this.Provider = provider;
            this.ModulePath = modulePath;
            this.Commands = new List<Type>();
        }
        public void FixShit(DiscordShardedClient discord, CommandService commands, BotSettingsHelper<string> config, IServiceProvider provider)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
            this.Provider = provider;
        }
        [RequiresUnreferencedCodeAttribute("Loads module from ModulePath, get's modules from extension")]
        public void Initialize(bool reload = false)
        {
            if (this.AppAssembly == null) this.AppAssembly = this.LoadFromAssemblyPath(this.ModulePath);
            if (!reload) NativeLibrary.SetDllImportResolver(this.AppAssembly, ExtensionLoadingService.PInvokeResolver);
            Type asmType = this.AppAssembly.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type



            this.module = (GladosModule)Activator.CreateInstance(asmType);
            this.ModuleName = this.module?.Name;
            this.ModuleAuthorLink = this.module?.AuthorLink;
            this.ModuleVersion = this.module?.Version;

            if (string.IsNullOrWhiteSpace(this.ModuleName)) this.LoadFailed = true; // class doesn't have Name string
            if (string.IsNullOrWhiteSpace(this.ModuleVersion)) this.LoadFailed = true; // class doesn't have Version string
            if (string.IsNullOrWhiteSpace(this.ModuleAuthorLink)) this.LoadFailed = true; // class doesn't have AuthorLink string
            if (ExtensionLoadingService.GetUsernameFromLink(this).GetAwaiter().GetResult() != ExtensionErrorCodes.VALID)
                this.LoadFailed = true; // Failed to verify authenticity of the user!
            if (this.LoadFailed) { this.Dispose(); return; }

            IEnumerable<Type> c = this.AppAssembly.GetTypes().Where((t) => t.IsPublic && t.IsClass && !t.IsAbstract && (t.BaseType == typeof(ModuleBase<ShardedCommandContext>) || t.BaseType == typeof(ModuleBase<SocketCommandContext>)));
            this.Commands.AddRange(c);
            //this.AsmName = new AssemblyName(this.ModuleName ?? throw new InvalidOperationException()) { CodeBase = this.ModulePath };
        }

        public void PreLoad() => this.module.PreLoad(this.discord, this.commands, this.config, this.Provider);

        public void PostLoad() => this.module.PostLoad(this.discord, this.commands, this.config, this.Provider);

        public Type[] GetServices(DiscordShardedClient client, IServiceCollection provider) => this.module.Services(client, this.commands, this.config, provider).ToArray();

        public void Dispose()
        {
            //fuck you c# ↓
            if (this.commands == null) return;
            if (this.disposed) return;
            this.disposed = true;
            this.module?.Unload(this.discord, this.commands, this.config, this.Provider);
            this.Unload();
            foreach (Type cmd in this.Commands)
            {
                this.commands.RemoveModuleAsync(cmd);
            }
        }
    }
}
