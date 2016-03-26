using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public class ShlWapi
    {
        [DllImport("shlwapi.dll")]
        public static extern Int32 StrRetToBuf(ref STRRET pstr, IntPtr pidl,
                                               StringBuilder pszBuf,
                                               UInt32 cchBuf);
    }
}
