using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Win32
{
    [Flags]
    public enum DiGetClassFlags : uint
    {
        DIGCF_DEFAULT = 0x00000001,
        DIGCF_PRESENT = 0x00000002,
        DIGCF_ALLCLASSES = 0x00000004,
        DIGCF_PROFILE = 0x00000008,
        DIGCF_DEVICEINTERFACE = 0x00000010,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVINFO_DATA
    {
        public Int32 cbSize;
        public Guid classGuid;
        public uint devInst;
        public IntPtr reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVICE_INTERFACE_DATA
    {
        public Int32 cbSize;
        public Guid interfaceClassGuid;
        public Int32 flags;
        private UIntPtr reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVPROPKEY
    {
        Guid fmtid;
        Int32 pid;
    };

    public class SetupDi
    {
        public const int ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(IntPtr guid, string enumerator, IntPtr parent, DiGetClassFlags flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid guid, string enumerator, IntPtr parent, DiGetClassFlags flags);

        public static IntPtr GetClassDevs(string enumerator, IntPtr parent, DiGetClassFlags flags)
        {
            return SetupDiGetClassDevs(IntPtr.Zero, enumerator, parent, flags);
        }

        public static IntPtr GetClassDevs(Guid guid, string enumerator, IntPtr parent, DiGetClassFlags flags)
        {
            return SetupDiGetClassDevs(ref guid, enumerator, parent, flags);
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        public static bool EnumDeviceInfo(IntPtr deviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData)
        {
            return SetupDiEnumDeviceInfo(deviceInfoSet, MemberIndex, ref DeviceInfoData);
        }

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(
           IntPtr hDevInfo,
           ref SP_DEVINFO_DATA devInfo,
           ref Guid interfaceClassGuid,
           UInt32 memberIndex,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        public static bool EnumDeviceInterfaces(
            IntPtr hdevInfo, 
            ref SP_DEVINFO_DATA devInfo, 
            ref Guid interfaceClassGuid,
            UInt32 memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            return SetupDiEnumDeviceInterfaces(hdevInfo, ref devInfo, ref interfaceClassGuid, memberIndex, ref deviceInterfaceData);
        }

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(
           IntPtr hDevInfo,
           IntPtr devInfo,
           ref Guid interfaceClassGuid,
           UInt32 memberIndex,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        public static bool EnumDeviceInterfaces(
            IntPtr hdevInfo,
            ref Guid interfaceClassGuid,
            UInt32 memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            return SetupDiEnumDeviceInterfaces(hdevInfo, IntPtr.Zero, ref interfaceClassGuid, memberIndex, ref deviceInterfaceData);
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        public static bool DestroyDeviceInfoList(IntPtr DeviceInfoSet)
        {
            return SetupDiDestroyDeviceInfoList(DeviceInfoSet);
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDevicePropertyKeys(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            [In, Out] DEVPROPKEY[] PropertyKeyArray,
            int PropertyKeyCount,
            ref int RequiredPropertyKeyCount,
            int Flags);

        public static bool GetDevicePropertyKeys(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            DEVPROPKEY[] PropertyKeyArray,
            int PropertyKeyCount,
            ref int RequiredPropertyKeyCount,
            int Flags)
        {
            return SetupDiGetDevicePropertyKeys(DeviceInfoSet, ref DeviceInfoData, PropertyKeyArray, PropertyKeyCount, ref RequiredPropertyKeyCount, Flags);
        }
    }
}
