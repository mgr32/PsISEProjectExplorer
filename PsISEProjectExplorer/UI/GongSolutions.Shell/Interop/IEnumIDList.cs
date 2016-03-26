using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F2-0000-0000-C000-000000000046")]
    public interface IEnumIDList
    {
        [PreserveSig]
        HResult Next(uint celt,
            out IntPtr rgelt,
            out uint pceltFetched);

        [PreserveSig]
        HResult Skip(uint celt);

        [PreserveSig]
        HResult Reset();

        IEnumIDList Clone();
    }
}
