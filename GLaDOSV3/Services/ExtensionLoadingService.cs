using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GladosV3.Services
{
    class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
    {
        public SimpleUnloadableAssemblyLoadContext()
        {
        }

        protected override Assembly Load(AssemblyName assemblyName) => null;
    }

    class ExtensionLoadingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        public ExtensionLoadingService(DiscordSocketClient discord = null, CommandService commands = null, IConfigurationRoot config = null, IServiceProvider provider = null)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies"));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Modules")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
        }

        public Task<Type[]> GetServices()
        {
            List<Type> types = new List<Type>();

            {
                foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), "*.dll")) // Bad extension loading
                {
                    try
                    {
                        Assembly asm = ValidFile(file);
                        if (asm == null) continue;
                        Type asmType = asm.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
                        ConstructorInfo asmConstructor = asmType.GetConstructor(Type.EmptyTypes);  // get extension's constructor
                        object magicClassObject = asmConstructor.Invoke(new object[] { }); // create object of class
                        var memberInfo = asmType.GetMethod("get_Services", BindingFlags.Instance | BindingFlags.Public); //get services method
                        if (memberInfo == null) continue;
                        var item = (Type[])memberInfo.Invoke(magicClassObject, new object[] { });
                        if (item != null && item.Length > 0)
                            types.AddRange(item); // invoke services method
                    }
<<<<<<< HEAD
                    catch (Exception) { /* ignored */ }
=======
                    catch (Exception)
                    {
                    }
>>>>>>> 15c8c1a0bfe431ef18b6d59b1b1b4a5255cbeb05
                }
            }
            return Task.FromResult(types.ToArray());
        }


        private Assembly ValidFile(string file)
        {
            if (new FileInfo(file).Length == 0) return null; // file is empty!
            Assembly asm = null;
            try
            {
                if (!IsValidCLRFile(file)) return null; // file is not .NET assembly
                asm = Assembly.LoadFile(file);
                if (!IsValidExtension(asm))
                { return null; } // every extension must have ModuleInfo class
            }
            catch (Exception) { return null; }
            
            return asm;
        }

        //private List<GladosModuleStruct> extensions = new List<GladosModuleStruct>();
        public async Task Load()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Modules"), "*.dll")) // Bad extension loading
            {
                try
                {
                    Assembly asm = ValidFile(file);
                    //this.extensions.Add(asm);
                    if (asm == null) continue;
                    await LoadExtension(asm).ConfigureAwait(false); // load the extension
                    var modules = asm.GetTypes().Where(type => type.IsClass && !type.IsSpecialName && type.IsPublic)
                        .Aggregate(string.Empty, (current, type) => current + type.Name + ", ");
                    await LoggingService.Log(LogSeverity.Verbose, "Module",
                        $"Loaded modules: {modules.Remove(modules.Length - 2)} from {Path.GetFileNameWithoutExtension(file)}");
                }
                catch (BadImageFormatException)
                {
                }
            }
            foreach (var assemblyName in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Dependencies")))
            {
                if (!IsValidCLRFile(assemblyName)) continue;
                var asm = Assembly.LoadFile(assemblyName);
                dependencies.Add(asm.FullName, asm);
            }
        }
        //public Task Unload(string extensionName)
        //{
        //    for (int i = 0; i < extensions.Count; i++)
        //    {
        //        if (extensions[i].moduleName != extensionName) continue;
        //        extensions[i] = null;
        //        return Task.CompletedTask;
        //    }
        //    return Task.CompletedTask;
        //}
        private static IDictionary<string, Assembly> dependencies = new Dictionary<string, Assembly>();
        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return dependencies.TryGetValue(args.Name, out var res) ? res : Assembly.LoadFile(args.Name);
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
            if (peHeaderSignature != 17744)
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
        public bool IsValidExtension(Assembly asm)
        {
            try
            {
                if (!asm.GetTypes().Any(t => t.Namespace.Contains("GladosV3.Module"))) return false;
                if (!asm.GetTypes().Any(type => (type.IsClass && type.IsPublic && type.Name == "ModuleInfo"))) return false; //extension doesn't have ModuleInfo class
                Type asmType = asm.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
                if (asmType.GetInterfaces().Distinct().FirstOrDefault() != typeof(IGladosModule)) return false; // extension's moduleinfo is not extended
                ConstructorInfo asmConstructor = asmType.GetConstructor(Type.EmptyTypes); // get extension's constructor
                object classO = asmConstructor.Invoke(new object[] { }); // create object of class
                string moduleName = GetModuleInfo(asmType, classO, "Name").ToString();
                string moduleVersion = GetModuleInfo(asmType, classO, "Version").ToString();
                string moduleAuthor = GetModuleInfo(asmType, classO, "Author").ToString();
                if (string.IsNullOrWhiteSpace(moduleName)) return false; // class doesn't have Name string
                if (string.IsNullOrWhiteSpace(moduleVersion)) return false; // class doesn't have Version string
                if (string.IsNullOrWhiteSpace(moduleAuthor)) return false; // class doesn't have Author string
            }
            catch
            { return false; }
            return true;
        }
        public Task LoadExtension(Assembly asm)
        {
            try
            {
                Type asmType = asm.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
                ConstructorInfo asmConstructor = asmType.GetConstructor(Type.EmptyTypes);  // get extension's constructor
                object magicClassObject = asmConstructor.Invoke(new object[] { }); // create object of class
                var memberInfo = asmType.GetMethod("PreLoad", BindingFlags.Instance | BindingFlags.Public); //get PreLoad method
                if (memberInfo != null)  // does the extension have PreLoad?
                    memberInfo.Invoke(magicClassObject, new object[] { _discord, _commands, _config, _provider });// invoke PreLoad method
                _commands.AddModulesAsync(asm, _provider).GetAwaiter(); // add the extension's commands
                memberInfo = asmType.GetMethod("PostLoad", BindingFlags.Instance | BindingFlags.Public); //get PostLoad method
                if (memberInfo != null)  // does the extension have PostLoad?
                    memberInfo.Invoke(magicClassObject, new object[] { _discord, _commands, _config, _provider });// invoke PostLoad method
            }
            catch (Exception ex)
            { return Task.FromException(ex); }
            return Task.CompletedTask;
        }
        public object GetModuleInfo(Type type, object classO, string info)
        {
            var memberInfo = type.GetMethod(info, BindingFlags.Instance | BindingFlags.Public);
            return memberInfo.Invoke(classO, new object[] { });
        }
    }

    //class GladosModuleStruct
    //{
    //    public AppDomain appDomain;
    //    public AssemblyName asmName;
    //    public Assembly appAssembly;
    //    public string moduleName;
    //    public string moduleAuthor;
    //    public string moduleVersion;
    //    public GladosModuleStruct(string path,string moduleName,string moduleAuthor, string moduleVersion)
    //    {
    //        appDomain = AppDomain.CreateDomain(moduleName);
    //        this.moduleName = moduleName;
    //        this.moduleAuthor = moduleAuthor;
    //        this.moduleVersion = moduleVersion;
    //        asmName = new AssemblyName(moduleName) {CodeBase = path};
    //        appAssembly = appDomain.Load(asmName);
    //    }
    //    ~GladosModuleStruct()
    //    {
    //        AppDomain.Unload(appDomain);
    //    }
    //}
}
