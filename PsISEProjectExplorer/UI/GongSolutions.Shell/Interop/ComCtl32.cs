using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum ILD
    {
        NORMAL = 0x00000000,
        TRANSPARENT = 0x00000001,
        MASK = 0x00000010,
        IMAGE = 0x00000020,
        ROP = 0x00000040,
        BLEND25 = 0x00000002,
        BLEND50 = 0x00000004,
        OVERLAYMASK = 0x00000F00,
        PRESERVEALPHA = 0x00001000,
    }

    public class ComCtl32
    {
        [DllImport("comctl32")]
        public static extern bool ImageList_Draw(IntPtr himl,
            int i, IntPtr hdcDst, int x, int y, uint fStyle);

        [DllImport("comctl32")]
        public static extern bool ImageList_GetIconSize(IntPtr himl, out int cx, out int cy);
    }
}
