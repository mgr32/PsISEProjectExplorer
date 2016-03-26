using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [Guid("00021500-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IQueryInfo
    {
        [PreserveSig]
        void GetInfoTip(int dwFlags, out IntPtr ppwszTip);

        [PreserveSig]
        void GetInfoFlags(IntPtr pdwFlags);
    }
}
