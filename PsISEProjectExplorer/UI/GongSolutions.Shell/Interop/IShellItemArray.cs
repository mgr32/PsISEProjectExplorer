using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
    public interface IShellItemArray
    {
        void BindToHandler(
            [In] IntPtr pbc,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);

        void GetPropertyStore(
            [In] int flags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);

        void GetPropertyDescriptionList(
            [In] int keyType,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);

        void GetAttributes(
            [In] int dwAttribFlags,
            [In] int sfgaoMask,
            [Out] out int psfgaoAttribs);

        void GetCount(
            [Out] out ushort pdwNumItems);

        void GetItemAt(
            [In] ushort dwIndex,
            [Out, MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

        void EnumItems(
            [Out] out IntPtr ppenumShellItems);
    }
}
