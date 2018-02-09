using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace GladosV3.Helpers
{
    class PInvokes // Found this on pastebin. Autor unknown
    {
        /// <summary>
        /// Map a native function to a delegate
        /// </summary>
        public static T GetFunction<T>(IntPtr module, string name)
        {
            var func = GetFunctionPtr(module, name);
            return (T)(Marshal.GetDelegateForFunctionPointer(func, typeof(T)) as object);
        }

        /// <summary>
        /// Map a native function to a delegate, function name must match delegate name
        /// </summary>
        public static T GetFunction<T>(IntPtr module)
        {
            return GetFunction<T>(module, typeof(T).Name);
        }
        /// <summary>
        /// Get the pointer to a module
        /// </summary>
        public static IntPtr GetModule(string name)
        {
            foreach (var module in typeof(Assembly).Assembly.GetModules(true))
                if (module.Name.ToLowerInvariant() == name.ToLowerInvariant())
                    return Marshal.GetHINSTANCE(module);
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
                if (module.ModuleName.ToLowerInvariant() == name.ToLowerInvariant())
                    return module.BaseAddress;
            return GetModulePtr(name);
        }
        public static Delegate GetDelegate<T>(IntPtr module, string name)
        {
            var ptr = GetFunctionPtr(module, name);
            if (ptr == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
        }

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA", SetLastError =  true)]
        private static extern IntPtr GetModulePtr(string libraryFile);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress",SetLastError = true)]
        private static extern IntPtr GetFunctionPtr(IntPtr hModule, string procedureName);
    }
}
