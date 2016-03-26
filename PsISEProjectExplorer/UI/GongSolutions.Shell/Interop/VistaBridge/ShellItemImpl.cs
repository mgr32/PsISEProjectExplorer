using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GongSolutions.Shell.Interop.VistaBridge
{
    class ShellItemImpl : IDisposable, IShellItem
    {
        public ShellItemImpl(IntPtr pidl, bool owner)
        {
            if (owner)
            {
                m_Pidl = pidl;
            }
            else
            {
                m_Pidl = Shell32.ILClone(pidl);
            }
        }

        ~ShellItemImpl()
        {
            Dispose(false);
        }

        public IntPtr BindToHandler(IntPtr pbc, Guid bhid, Guid riid)
        {
            if (riid == typeof(IShellFolder).GUID)
            {
                return Marshal.GetIUnknownForObject(GetIShellFolder());
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public HResult GetParent(out IShellItem ppsi)
        {
            IntPtr pidl = Shell32.ILClone(m_Pidl);
            if (Shell32.ILRemoveLastID(pidl))
            {
                ppsi = new ShellItemImpl(pidl, true);
                return HResult.S_OK;
            }
            else
            {
                ppsi = null;
                return HResult.MK_E_NOOBJECT;
            }
        }

        public IntPtr GetDisplayName(SIGDN sigdnName)
        {
            if (sigdnName == SIGDN.FILESYSPATH)
            {
                StringBuilder result = new StringBuilder(512);
                if (!Shell32.SHGetPathFromIDList(m_Pidl, result))
                    throw new ArgumentException();
                return Marshal.StringToHGlobalUni(result.ToString());
            }
            else
            {
                IShellFolder parentFolder = GetParent().GetIShellFolder();
                IntPtr childPidl = Shell32.ILFindLastID(m_Pidl);
                StringBuilder builder = new StringBuilder(512);
                STRRET strret = new STRRET();

                parentFolder.GetDisplayNameOf(childPidl,
                    (SHGNO)((int)sigdnName & 0xffff), out strret);
                ShlWapi.StrRetToBuf(ref strret, childPidl, builder,
                    (uint)builder.Capacity);
                return Marshal.StringToHGlobalUni(builder.ToString());
            }
        }

        public SFGAO GetAttributes(SFGAO sfgaoMask)
        {
            IShellFolder parentFolder = GetParent().GetIShellFolder();
            SFGAO result = sfgaoMask;

            parentFolder.GetAttributesOf(1,
                new IntPtr[] { Shell32.ILFindLastID(m_Pidl) },
                ref result);
            return result & sfgaoMask;
        }

        public int Compare(IShellItem psi, SICHINT hint)
        {
            ShellItemImpl other = (ShellItemImpl)psi;
            ShellItemImpl myParent = GetParent();
            ShellItemImpl theirParent = other.GetParent();

            if (Shell32.ILIsEqual(myParent.m_Pidl, theirParent.m_Pidl))
            {
                return myParent.GetIShellFolder().CompareIDs((SHCIDS)hint,
                    Shell32.ILFindLastID(m_Pidl),
                    Shell32.ILFindLastID(other.m_Pidl));
            }
            else
            {
                return 1;
            }
        }

        public IntPtr Pidl
        {
            get { return m_Pidl; }
        }

        protected void Dispose(bool dispose)
        {
            Shell32.ILFree(m_Pidl);
        }

        ShellItemImpl GetParent()
        {
            IntPtr pidl = Shell32.ILClone(m_Pidl);

            if (Shell32.ILRemoveLastID(pidl))
            {
                return new ShellItemImpl(pidl, true);
            }
            else
            {
                return this;
            }
        }

        IShellFolder GetIShellFolder()
        {
            IShellFolder desktop = Shell32.SHGetDesktopFolder();
            IntPtr desktopPidl;

            Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL.DESKTOP,
                out desktopPidl); ;

            if (Shell32.ILIsEqual(m_Pidl, desktopPidl))
            {
                return desktop;
            }
            else
            {
                IntPtr result;
                desktop.BindToObject(m_Pidl, IntPtr.Zero,
                    typeof(IShellFolder).GUID, out result);
                return (IShellFolder)Marshal.GetTypedObjectForIUnknown(result,
                    typeof(IShellFolder));
            }
        }

        IntPtr m_Pidl;
    }
}
