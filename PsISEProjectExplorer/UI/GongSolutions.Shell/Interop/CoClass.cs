using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public class CoClass
    {
        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        public class FileOpenDialog
        {
        }

        [ComImport, Guid("4df0c730-df9d-4ae3-9153-aa6b82e9795a")]
        public class KnownFolderManager
        {
        }
    }
}
