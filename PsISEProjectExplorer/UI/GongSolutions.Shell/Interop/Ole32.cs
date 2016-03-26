using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ComTypes = System.Runtime.InteropServices.ComTypes;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public class Ole32
    {
        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr pv);

        [DllImport("ole32.dll")]
        public static extern int DoDragDrop(ComTypes.IDataObject pDataObject,
            IDropSource pDropSource, DragDropEffects dwOKEffect,
            out DragDropEffects pdwEffect);

        [DllImport("ole32.dll")]
        public static extern int RegisterDragDrop(IntPtr hwnd, IDropTarget pDropTarget);

        [DllImport("ole32.dll")]
        public static extern int RevokeDragDrop(IntPtr hwnd);

        public static Guid IID_IDataObject
        {
            get { return new Guid("0000010e-0000-0000-C000-000000000046"); }
        }

        public static Guid IID_IDropTarget
        {
            get { return new Guid("00000122-0000-0000-C000-000000000046"); }
        }
    }
}
