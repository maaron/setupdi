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

    [Flags]
    public enum DicsFlag : uint
    {
        DICS_FLAG_GLOBAL = 0x00000001,
        DICS_FLAG_CONFIGSPECIFIC = 0x00000002
    }

    public enum DiKeyType : uint
    {
        DREG_DEV = 1,
        DIREG_DRV = 2,
        DIREG_BOTH = 4
    }

    public enum RegSam : uint
    {
        KEU_QUERY_VALUE = 1
    }

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

        public const uint STATUS_SUCCESS = 0x00000000;
        public const uint STATUS_BUFFER_TOO_SMALL = 0xC0000023;

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

        [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiOpenDevRegKey(
            IntPtr hDeviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            DicsFlag scope,
            int hwProfile,
            DiKeyType parameterRegistryValueKind,
            RegSam samDesired);

        public static IntPtr OpenDevRegKey(
            IntPtr hDeviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            DicsFlag scope,
            int hwProfile,
            DiKeyType parameterRegistryValueKind,
            RegSam samDesired)
        {
            return SetupDiOpenDevRegKey(hDeviceInfoSet, ref deviceInfoData, scope, hwProfile, parameterRegistryValueKind, samDesired);
        }

        [DllImport("ntdll.dll", CharSet = CharSet.Auto)]
        public static extern uint NtQueryKey(
            IntPtr KeyHandle,
            KeyInformationClass keyInformationClass,
            byte[] KeyInformation,
            int Length,
            ref int ResultLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegCloseKey(IntPtr hKey);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceInstanceId(
           IntPtr DeviceInfoSet,
           ref SP_DEVINFO_DATA DeviceInfoData,
           StringBuilder DeviceInstanceId,
           int DeviceInstanceIdSize,
           out int RequiredSize);

        public static bool GetDeviceInstanceId(
           IntPtr DeviceInfoSet,
           SP_DEVINFO_DATA DeviceInfoData,
           StringBuilder DeviceInstanceId,
           int DeviceInstanceIdSize,
           out int RequiredSize)
        {
            return SetupDiGetDeviceInstanceId(DeviceInfoSet, ref DeviceInfoData, DeviceInstanceId, DeviceInstanceIdSize, out RequiredSize);
        }
    }
}
