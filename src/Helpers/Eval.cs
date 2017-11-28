using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;

namespace GladosV3.Helpers
{
    class Eval
    {
        public static async Task<string> evalTask(string sCSCode, bool returnval)
        {
            var code = @"
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GladosV3.Helpers;
                class Eval
                { 
                    public void Run() => { %d }
                }".Replace("%d", sCSCode);
            if (returnval)
                return CSharpScript.EvaluateAsync(code,ScriptOptions.Default).GetAwaiter().GetResult().ToString();
            else
               await CSharpScript.RunAsync(code,ScriptOptions.Default);//.ContinueWith();
            return await Task.FromResult("good");
        }

    }
}
