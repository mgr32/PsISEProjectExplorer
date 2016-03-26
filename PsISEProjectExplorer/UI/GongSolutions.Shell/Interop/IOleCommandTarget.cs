using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OLECMDTEXT
    {
        public uint cmdtextf;
        public uint cwActual;
        public uint cwBuf;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public char rgwz;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OLECMD
    {
        public uint cmdID;
        public uint cmdf;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b722bccb-4e68-101b-a2bc-00aa00404770")]
    public interface IOleCommandTarget
    {
        void QueryStatus(ref Guid pguidCmdGroup, UInt32 cCmds,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] 
            OLECMD[] prgCmds,
            ref OLECMDTEXT CmdText);
        void Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdExecOpt,
                  ref object pvaIn, ref object pvaOut);
    }
}
