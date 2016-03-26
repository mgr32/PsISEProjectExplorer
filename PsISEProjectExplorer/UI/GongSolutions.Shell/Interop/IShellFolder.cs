using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    public interface IShellFolder
    {
        void ParseDisplayName(
            [In] IntPtr hwnd,
            [In] IntPtr pbc,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            [Out] out uint pchEaten,
            [Out] out IntPtr ppidl,
            [In, Out] ref uint pdwAttributes);

        [PreserveSig]
        HResult EnumObjects(
            [In] IntPtr hwnd,
            [In] SHCONTF grfFlags,
            [Out] out IEnumIDList ppenumIDList);

        void BindToObject(IntPtr pidl, IntPtr pbc,
                          [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
                          out IntPtr ppv);

        void BindToStorage(IntPtr pidl, IntPtr pbc,
                           [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
                           out IntPtr ppv);

        [PreserveSig]
        short CompareIDs(SHCIDS lParam, IntPtr pidl1, IntPtr pidl2);

        IntPtr CreateViewObject(IntPtr hwndOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        void GetAttributesOf(UInt32 cidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
            IntPtr[] apidl,
            ref SFGAO rgfInOut);

        void GetUIObjectOf(IntPtr hwndOwner, UInt32 cidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
            IntPtr[] apidl,
           [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            UInt32 rgfReserved,
            out IntPtr ppv);

        void GetDisplayNameOf(IntPtr pidl, SHGNO uFlags, out STRRET pName);

        void SetNameOf(IntPtr hwnd, IntPtr pidl, string pszName,
                       SHCONTF uFlags, out IntPtr ppidlOut);
    }
}
