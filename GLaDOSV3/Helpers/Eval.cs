using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using GLaDOSV3.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

// ReSharper disable All

namespace GLaDOSV3.Helpers
{
    public static class EvalWorkaround
    {
        public static async Task<object> Eval(string sourceText, List<string> imports, object globals, Type globalsType)
        {
            // The `CSharpScript` API cannot be used when `Assembly.Location` is not supported, see:
            // - https://github.com/dotnet/roslyn/issues/50719
            // - https://www.samprof.com/2018/12/15/compile-csharp-and-blazor-inside-browser-en

            var diagnostics = new List<Diagnostic>();

            // https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/CSharp/CSharpScriptCompiler.cs#L43
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(
                sourceText,
                new CSharpParseOptions(
                    kind: SourceCodeKind.Script,
                    languageVersion: LanguageVersion.Latest));
            diagnostics.AddRange(syntaxTree.GetDiagnostics());

            // https://github.com/dotnet/runtime/issues/36590#issuecomment-689883856
            static MetadataReference GetReference_type(Type type) => GetReference_asm(type.Assembly);
            static MetadataReference GetReference_asm(Assembly asm)
            {
                unsafe
                {
                    return asm.TryGetRawMetadata(out var blob, out var length)
                               ? AssemblyMetadata
                                .Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob, length))
                                .GetReference()
                               : throw new InvalidOperationException($"Could not get raw metadata for assembly {asm}");
                }
            }
            var list = new List<MetadataReference>();
            list.Add(GetReference_type(typeof(object)));
            list.Add(GetReference_asm(Assembly.GetExecutingAssembly()));
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic&& !string.IsNullOrWhiteSpace(asm.Location)))
            {
                foreach(var types in asm.GetTypes()) list.Add(GetReference_type(types));
            }

            foreach (var asm in ExtensionLoadingService.Extensions)
            {
                list.Add(GetReference_asm(asm.AppAssembly));
            }
            if (imports.Count != 0)
            {
                foreach (var import in imports)
                {
                    foreach (var type in ScriptMetadataResolver.Default.ResolveReference(import, null,
                        MetadataReferenceProperties
                           .Assembly))
                        list.Add((MetadataReference) type);
                }
            }

            var references = list.ToArray();

            // In this example, a return type of `string` is expected

            // Note that `ScriptBuilder` would normally generate a unique assembly name
            // https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/Core/ScriptBuilder.cs#L64
            //AssemblyMetadata.
            var compilation = CSharpCompilation.CreateScriptCompilation(
                                                                        assemblyName: "Script",
                                                                        syntaxTree: syntaxTree,
                                                                        references: references,
                                                                        returnType: typeof(object),
                                                                        globalsType: globalsType);
            var submissionFactory = default(Func<object[], Task<object>>);

            await using (var peStream = new MemoryStream())
            await using (var pdbStream = new MemoryStream())
            {
                // https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/Core/ScriptBuilder.cs#L121
                // https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/Core/Utilities/PdbHelpers.cs#L10
                var result = compilation.Emit(
                    peStream,
                    pdbStream,
                    xmlDocumentationStream: null,
                    win32Resources: null,
                    manifestResources: null,
                    new EmitOptions(
                        debugInformationFormat: DebugInformationFormat.PortablePdb,
                        pdbChecksumAlgorithm: default(HashAlgorithmName)));

                diagnostics.AddRange(result.Diagnostics);

                if (result.Success)
                {
                    var scriptAssembly = AppDomain.CurrentDomain.Load(peStream.ToArray(), pdbStream.ToArray());

                    // https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/Core/ScriptBuilder.cs#L188
                    var entryPoint = compilation.GetEntryPoint(CancellationToken.None) ?? throw new InvalidOperationException("Entry point could be determined");

                    var entryPointType = scriptAssembly
                        .GetType(
                            $"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}",
                            throwOnError: true,
                            ignoreCase: false);

                    var entryPointMethod = entryPointType?.GetTypeInfo().GetDeclaredMethod(entryPoint.MetadataName) ?? throw new InvalidOperationException("Entry point method could be determined");

                    submissionFactory = entryPointMethod.CreateDelegate<Func<object[], Task<object>>>();
                }

            }
            if (submissionFactory == null)
            {
                throw new CompilationErrorException("Compilation failed", diagnostics.ToImmutableArray());
            }

            // The first argument is the globals type, the remaining are preceding script states 
            // - https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/Core/ScriptExecutionState.cs#L31
            // - https://github.com/dotnet/roslyn/blob/version-3.2.0/src/Scripting/Core/ScriptExecutionState.cs#L65
            var message = await submissionFactory.Invoke(new object[] { globals, null });
            return message;
        }
    }

    public sealed class Eval
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public class Globals
        {
            public SocketUserMessage Message => Context.Message;
            public ISocketMessageChannel Channel => Context.Message.Channel;
            public SocketGuild Guild => Context.Guild;
            public SocketUser User => Context.Message.Author;
            public BotSettingsHelper<string> Config => new BotSettingsHelper<string>();

            public DiscordSocketClient Client => Context.Client;
            public Tools Tools = new Tools();
            public SocketCommandContext Context { get; }

            public SQLiteConnection SQLConnection = SqLite.Connection;

            public Globals(SocketCommandContext ctx) => Context = ctx;
        }

       
        public static async Task<string> EvalTask(SocketCommandContext ctx, string cScode)
        {
            List<string> imports = new List<string>(16)
            {
                "System",
                "System.Runtime",
                "System.Collections.Generic",
                "System.Reflection",
                "System.Text",
                "System.Threading.Tasks",
                "System.Linq",
                "System.Math",
                "System.IO",
                "System.Diagnostics",
                "GLaDOSV3.Helpers",
                "Discord",
                "Discord.Commands",
                "Discord.WebSocket",
                "Newtonsoft.Json",
                "System.Data.SQLite", "Microsoft.Extensions"
            };
            try
            {
                //ScriptOptions options = ScriptOptions.Default
                //                                     .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                //                                                              .Where(asm => !asm.IsDynamic
                //                                                                   && !string
                //                                                                      .IsNullOrWhiteSpace(asm
                //                                                                          .Location)))
                //                                     .AddImports(imports)
                //                                     .WithEmitDebugInformation(true)
                //                                     .WithAllowUnsafe(true)
                //                                     .WithCheckOverflow(true)
                //                                     .WithWarningLevel(5)
                //                                     ;

                //Script result = CSharpScript.Create(cScode, options, typeof(Globals));
                //var returnVal = result.RunAsync(new Globals(ctx)).GetAwaiter().GetResult().ReturnValue?.ToString();
                var returnVal = (await EvalWorkaround.Eval(cScode, imports, new Globals(ctx), typeof(Globals))).ToString();
                BotSettingsHelper<string> r = new BotSettingsHelper<string>();
                var token = r["tokens_discord"];
                if (!string.IsNullOrWhiteSpace(returnVal) && returnVal.Contains(token, StringComparison.Ordinal)) returnVal = returnVal?.Replace(token, "Nah, no token leak 4 u.", StringComparison.OrdinalIgnoreCase);

                return !string.IsNullOrWhiteSpace(returnVal) ? await Task.FromResult($"**Executed!**{Environment.NewLine}Output: ```{string.Join(Environment.NewLine, returnVal)}```").ConfigureAwait(true) : await Task.FromResult("**Executed!** *No output.*").ConfigureAwait(false);
            }
            catch (CompilationErrorException e)
            {
                return $"**Compiler error**\nOutput: ```{string.Join('\n', e.Diagnostics)}```";
            }
            catch (Exception e)
            {
                return $"**Exception!** {e.Message}\n{e.StackTrace}";
            }
        }
    }
}
