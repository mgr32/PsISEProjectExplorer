using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum CDBOSC
    {
        CDBOSC_SETFOCUS,
        CDBOSC_KILLFOCUS,
        CDBOSC_SELCHANGE,
        CDBOSC_RENAME,
        CDBOSC_STATECHANGE,
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F1-0000-0000-C000-000000000046")]
    public interface ICommDlgBrowser
    {
        [PreserveSig]
        HResult OnDefaultCommand(IShellView ppshv);
        [PreserveSig]
        HResult OnStateChange(IShellView ppshv, CDBOSC uChange);
        [PreserveSig]
        HResult IncludeObject(IShellView ppshv, IntPtr pidl);
    }
}
