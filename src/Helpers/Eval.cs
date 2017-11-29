using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
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
        public static async Task<string> EvalTask(string cScode)
        {
            string[] Imports = new[]
            {
                "System", "System.Collections.Generic", "System.Reflection", "System.Text", "System.Threading.Tasks",
                "System.IO"
            };
            try
            {
                object script = (
                    CSharpScript.EvaluateAsync(cScode, ScriptOptions.Default.WithImports(Imports))
                    .GetAwaiter()
                    .GetResult());
                if (!string.IsNullOrEmpty(script?.ToString()))
                    return await Task.FromResult("Executed! Output: "+Environment.NewLine+Convert.ToString(script));
                else
                    return await Task.FromResult("Executed! No output.");
            }
            catch (CompilationErrorException e)
            {
                return await Task.FromResult<string>(string.Join(Environment.NewLine, e.Diagnostics));
            }
            catch (Exception e)
            {
                return await Task.FromResult<string>(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

    }
}
