using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000122-0000-0000-C000-000000000046")]
    public interface IDropTarget
    {
        void DragEnter(IDataObject pDataObj, int grfKeyState,
                       Point pt, ref int pdwEffect);
        void DragOver(int grfKeyState, Point pt, ref int pdwEffect);
        void DragLeave();
        void Drop(IDataObject pDataObj, int grfKeyState,
                 Point pt, ref int pdwEffect);
    }
}
