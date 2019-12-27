using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GladosV3.Helpers
{
    public class PInvokes // Original source found: https://pastebin.com/7qtiPv3A
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
        public static T GetFunction<T>(IntPtr module) => GetFunction<T>(module, typeof(T).Name);

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

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true)]
        private static extern IntPtr GetModulePtr(string libraryFile);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
        private static extern IntPtr GetFunctionPtr(IntPtr hModule, string procedureName);
    }
    public class PInvokes_DllImport
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(string lpPathName);

    }
}
