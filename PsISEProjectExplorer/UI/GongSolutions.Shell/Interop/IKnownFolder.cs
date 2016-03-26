using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3AA7AF7E-9B36-420c-A8E3-F77D4674A488")]
    public interface IKnownFolder
    {
        Guid GetId();

        int GetCategory();

        IShellItem GetShellItem(
            [In] ushort dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        string GetPath(ushort dwFlags);

        void SetPath(ushort dwFlags, string pszPath);

        IntPtr GetIDList(ushort dwFlags);

        Guid GetFolderType();

        uint GetRedirectionCapabilities();

        KNOWNFOLDER_DEFINITION GetFolderDefinition();
    }
}
