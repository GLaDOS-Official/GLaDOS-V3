using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GladosV3.Helpers
{
    class PInvokes
    {
        [DllImport("kernel32.dll")]
        internal static extern bool SetProcessWorkingSetSize(IntPtr hProcess, UIntPtr
            dwMinimumWorkingSetSize, UIntPtr dwMaximumWorkingSetSize);

        [DllImport("psapi")]
        internal static extern bool EmptyWorkingSet(IntPtr hProcess);

    }
}
