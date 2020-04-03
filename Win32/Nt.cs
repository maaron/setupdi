using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Win32
{
    public enum KeyInformationClass : int
    {
        KeyBasicInformation = 0,
        KeyNodeInformation = 1,
        KeyFullInformation = 2,
        KeyNameInformation = 3,
        KeyCachedInformation = 4,
        KeyFlagsInformation = 5,
        KeyVirtualizationInformation = 6,
        KeyHandleTagsInformation = 7,
        MaxKeyInfoClass = 8
    }

    public static class Nt
    {
        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint NtQueryKey(
            IntPtr KeyHandle,
            KeyInformationClass keyInformationClass,
            byte[] KeyInformation,
            int Length,
            ref int ResultLength);

        public static byte[] QueryKey(HKey keyHandle, KeyInformationClass keyInformationClass)
        {
            int length = 0;

            var ret = NtQueryKey(keyHandle.IntPtr, KeyInformationClass.KeyNameInformation, null, 0, ref length);
            if (ret != SetupDi.STATUS_BUFFER_TOO_SMALL)
            {
                throw new Win32Exception();
            }

            var keyInformation = new byte[length];
            ret = Nt.NtQueryKey(keyHandle.IntPtr, KeyInformationClass.KeyNameInformation, keyInformation, length, ref length);
            if (ret != SetupDi.STATUS_SUCCESS)
            {
                throw new Win32Exception();
            }

            return keyInformation;
        }

        public static string QueryKeyNameInformation(HKey keyHandle)
        {
            var info = QueryKey(keyHandle, KeyInformationClass.KeyNameInformation);
            
            // first four bytes is length, followed by utf-16 string
            return Encoding.Unicode.GetString(info, 4, info.Length - 4);
        }
    }
}
