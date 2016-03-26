using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000121-0000-0000-C000-000000000046")]
    public interface IDropSource
    {
        [PreserveSig]
        HResult QueryContinueDrag(bool fEscapePressed, int grfKeyState);
        [PreserveSig]
        HResult GiveFeedback(int dwEffect);
    }
}
