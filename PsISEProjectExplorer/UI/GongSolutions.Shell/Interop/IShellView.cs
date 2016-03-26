using System;
using System.Drawing;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum SVGIO : uint
    {
        SVGIO_BACKGROUND = 0,
        SVGIO_SELECTION = 0x1,
        SVGIO_ALLVIEW = 0x2,
        SVGIO_CHECKED = 0x3,
        SVGIO_TYPE_MASK = 0xf,
        SVGIO_FLAG_VIEWORDER = 0x80000000,
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E3-0000-0000-C000-000000000046")]
    public interface IShellView
    {
        void GetWindow(out IntPtr windowHandle);
        void ContextSensitiveHelp(bool fEnterMode);
        [PreserveSig]
        long TranslateAcceleratorA(IntPtr message);
        void EnableModeless(bool enable);
        void UIActivate(UInt32 activtionState);
        void Refresh();
        void CreateViewWindow(
            IShellView previousShellView,
            ref FOLDERSETTINGS folderSetting,
            IShellBrowser shellBrowser,
            ref Rectangle bounds,
            out IntPtr handleOfCreatedWindow);
        void DestroyViewWindow();
        void GetCurrentInfo(ref FOLDERSETTINGS pfs);
        void AddPropertySheetPages([In, MarshalAs(UnmanagedType.U4)] uint reserved, [In]ref IntPtr functionPointer, [In] IntPtr lparam);
        void SaveViewState();
        void SelectItem(IntPtr pidlItem, [MarshalAs(UnmanagedType.U4)] SVSI flags);

        [PreserveSig]
        int GetItemObject(
            [In] SVGIO AspectOfView,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);
    }
}
