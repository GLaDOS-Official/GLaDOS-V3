using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GladosV3.Services
{
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
            
        }

    public Task<Type[]> GetServices()
    {
            List<Type> types = new List<Type>();
            if (Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
    "Modules")))
            {
                foreach (var file in Directory.GetFiles(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Modules"))
                ) // Bad extension loading
                {
                    try
                    {
                        Assembly asm = ValidFile(file);
                        if (asm == null) continue;
                        Type asmType = asm.GetTypes().Where(type => type.IsClass && type.Name == "ModuleInfo").Distinct().First(); //create type
                        ConstructorInfo asmConstructor = asmType.GetConstructor(Type.EmptyTypes);  // get extension's constructor
                        object magicClassObject = asmConstructor.Invoke(new object[] { }); // create object of class
                        var memberInfo = asmType.GetMethod("get_Services", BindingFlags.Instance | BindingFlags.Public); //get services method
                        if (memberInfo != null)  // does the extension have services?
                        {
                            var item = (System.Type[])((MethodInfo)memberInfo).Invoke(magicClassObject, new object[] { });
                            if ( item != null && item.Length > 0)
                              types.AddRange(item); // invoke services method
                        }
                        else
                            continue;
                    }
                    catch (Exception) { continue; }
                }
            }
            return Task.FromResult(types.ToArray());
        }


        private Assembly ValidFile(string file)
        {
            if (Path.GetExtension(file) != ".dll") return null;
            if (new System.IO.FileInfo(file).Length == 0) return null; // file is empty!
            Assembly asm = null;
            try
            {
                if (!IsValidCLRFile(file)) return null; // file is not .NET assembly
                asm = Assembly.LoadFrom(file);
                if (!IsValidExtension(asm)) return null; // every extension must have ModuleInfo class
            }
            catch (Exception) { return null; }
            return asm;
        }
        public async Task Load()
        {
            if (Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Modules")))
            {
                foreach (var file in Directory.GetFiles(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Modules"))
                ) // Bad extension loading
                {
                    try {
                        Assembly asm = ValidFile(file);
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
                foreach (var assemblyName in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dependencies")))
                {
                    if (!IsValidCLRFile(assemblyName)) continue;
                    var asm = Assembly.LoadFile(assemblyName);
                    dependencies.Add(asm.FullName, asm);
                }
            }
        }
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
                if (string.IsNullOrWhiteSpace(GetModuleInfo(asmType, classO, "Name").ToString())) return false; // class doesn't have Name string
                if (string.IsNullOrWhiteSpace(GetModuleInfo(asmType, classO, "Version").ToString())) return false; // class doesn't have Version string
                if (string.IsNullOrWhiteSpace(GetModuleInfo(asmType, classO, "Author").ToString())) return false; // class doesn't have Author string
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
                    ((MethodInfo)memberInfo).Invoke(magicClassObject, new object[] { _discord, _commands, _config, _provider });// invoke PreLoad method
                _commands.AddModulesAsync(asm).GetAwaiter(); // add the extension's commands
                memberInfo = asmType.GetMethod("PostLoad", BindingFlags.Instance | BindingFlags.Public); //get PostLoad method
                if (memberInfo != null)  // does the extension have PostLoad?
                    ((MethodInfo)memberInfo).Invoke(magicClassObject, new object[] { _discord, _commands, _config, _provider });// invoke PostLoad method
            }
            catch (Exception ex)
            { return Task.FromException(ex); }
            return Task.CompletedTask;
        }
        public object GetModuleInfo(Type type, object classO, string info)
        {
            var memberInfo = type.GetMethod(info, BindingFlags.Instance | BindingFlags.Public);
            return ((MethodInfo)memberInfo).Invoke(classO, new object[] { });
        }
    }
}
