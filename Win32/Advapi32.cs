using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static Win32.Advapi32;

namespace Win32
{
    public struct HKey : IDisposable
    {
        public IntPtr IntPtr { get; }

        public HKey(IntPtr intPtr)
        {
            IntPtr = intPtr;
        }

        public void Dispose()
        {
            if (IntPtr != IntPtr.Zero)
            {
                RegCloseKey(this);
            }
        }
    }

    public static class Advapi32
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegCloseKey(HKey hKey);
    }
}
