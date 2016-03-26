using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum HResult
    {
        DRAGDROP_S_CANCEL = 0x00040101,
        DRAGDROP_S_DROP = 0x00040100,
        DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102,
        DATA_S_SAMEFORMATETC = 0x00040130,
        S_OK = 0,
        S_FALSE = 1,
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_NOTIMPL = unchecked((int)0x80004001),
        OLE_E_ADVISENOTSUPPORTED = unchecked((int)80040003),
        MK_E_NOOBJECT = unchecked((int)0x800401E5),
    }
}
