using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("8BE2D872-86AA-4d47-B776-32CCA40C7018")]
    public interface IKnownFolderManager
    {
        Guid FolderIdFromCsidl(int nCsidl);

        CSIDL FolderIdToCsidl(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        void GetFolderIds(
            [Out] out IntPtr ppKFId,
            [Out] out uint pCount);

        IKnownFolder GetFolder(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        [PreserveSig]
        HResult GetFolderByName(
            [In] string pszCanonicalName,
            [Out] out IKnownFolder ppkf);

        void RegisterFolder(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            [In] IntPtr pKFD);

        void UnregisterFolder(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        [PreserveSig]
        HResult FindFolderFromPath(
            [In] string pszPath,
            [In] FFFP_MODE mode,
            [Out] out IKnownFolder ppkf);

        [PreserveSig]
        HResult FindFolderFromIDList(
            [In] IntPtr pidl,
            [Out] out IKnownFolder ppkf);

        string Redirect(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            [In] IntPtr hwnd,
            [In] int flags,
            [In] string pszTargetPath,
            [In] uint cFolders,
            [In] Guid[] pExclusion);
    }
}
