using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;

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

    public enum RegistryProperty : uint
    {
        SPDRP_DEVICEDESC = 0x00000000, // DeviceDesc (R/W)
        SPDRP_HARDWAREID = 0x00000001, // HardwareID (R/W)
        SPDRP_COMPATIBLEIDS = 0x00000002, // CompatibleIDs (R/W)
        SPDRP_UNUSED0 = 0x00000003, // unused
        SPDRP_SERVICE = 0x00000004, // Service (R/W)
        SPDRP_UNUSED1 = 0x00000005, // unused
        SPDRP_UNUSED2 = 0x00000006, // unused
        SPDRP_CLASS = 0x00000007, // Class (R--tied to ClassGUID)
        SPDRP_CLASSGUID = 0x00000008, // ClassGUID (R/W)
        SPDRP_DRIVER = 0x00000009, // Driver (R/W)
        SPDRP_CONFIGFLAGS = 0x0000000A, // ConfigFlags (R/W)
        SPDRP_MFG = 0x0000000B, // Mfg (R/W)
        SPDRP_FRIENDLYNAME = 0x0000000C, // FriendlyName (R/W)
        SPDRP_LOCATION_INFORMATION = 0x0000000D, // LocationInformation (R/W)
        SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E, // PhysicalDeviceObjectName (R)
        SPDRP_CAPABILITIES = 0x0000000F, // Capabilities (R)
        SPDRP_UI_NUMBER = 0x00000010, // UiNumber (R)
        SPDRP_UPPERFILTERS = 0x00000011, // UpperFilters (R/W)
        SPDRP_LOWERFILTERS = 0x00000012, // LowerFilters (R/W)
        SPDRP_BUSTYPEGUID = 0x00000013, // BusTypeGUID (R)
        SPDRP_LEGACYBUSTYPE = 0x00000014, // LegacyBusType (R)
        SPDRP_BUSNUMBER = 0x00000015, // BusNumber (R)
        SPDRP_ENUMERATOR_NAME = 0x00000016, // Enumerator Name (R)
        SPDRP_SECURITY = 0x00000017, // Security (R/W, binary form)
        SPDRP_SECURITY_SDS = 0x00000018, // Security (W, SDS form)
        SPDRP_DEVTYPE = 0x00000019, // Device Type (R/W)
        SPDRP_EXCLUSIVE = 0x0000001A, // Device is exclusive-access (R/W)
        SPDRP_CHARACTERISTICS = 0x0000001B, // Device Characteristics (R/W)
        SPDRP_ADDRESS = 0x0000001C, // Device Address (R)
        SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D, // UiNumberDescFormat (R/W)
        SPDRP_DEVICE_POWER_DATA = 0x0000001E, // Device Power Data (R)
        SPDRP_REMOVAL_POLICY = 0x0000001F, // Removal Policy (R)
        SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020, // Hardware Removal Policy (R)
        SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021, // Removal Policy Override (RW)
        SPDRP_INSTALL_STATE = 0x00000022, // Device Install State (R)
        SPDRP_LOCATION_PATHS = 0x00000023, // Device Location Paths (R)
        SPDRP_BASE_CONTAINERID = 0x00000024  // Base ContainerID (R)
    }

    public class SetupDi
    {
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int ERROR_INVALID_DATA = 13;

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

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            RegistryProperty property,
            out UInt32 propertyRegDataType,
            IntPtr propertyBuffer,
            int propertyBufferSize,
            out int requiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            RegistryProperty property,
            out UInt32 propertyRegDataType,
            byte[] propertyBuffer,
            int propertyBufferSize,
            out int requiredSize);

        private static Microsoft.Win32.RegistryValueKind MapValueKind(uint valueKind)
        {
            switch (valueKind)
            {
                case 1: return Microsoft.Win32.RegistryValueKind.String;
                case 2: return Microsoft.Win32.RegistryValueKind.ExpandString;
                case 3: return Microsoft.Win32.RegistryValueKind.Binary;
                case 4: return Microsoft.Win32.RegistryValueKind.DWord;
                case 5: return Microsoft.Win32.RegistryValueKind.MultiString;
                case 6: return Microsoft.Win32.RegistryValueKind.QWord;
                default: return Microsoft.Win32.RegistryValueKind.Unknown;
            }
        }

        public static bool GetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            SP_DEVINFO_DATA deviceInfoData,
            RegistryProperty property,
            out Microsoft.Win32.RegistryValueKind propertyRegDataType,
            IntPtr propertyBuffer,
            int propertyBufferSize,
            out int requiredSize)
        {
            uint valueType = 0;

            var result = SetupDiGetDeviceRegistryProperty(
                deviceInfoSet, 
                ref deviceInfoData, 
                property, 
                out valueType, 
                propertyBuffer, 
                propertyBufferSize, 
                out requiredSize);

            propertyRegDataType = MapValueKind(valueType);

            return result;
        }

        public static bool GetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            SP_DEVINFO_DATA deviceInfoData,
            RegistryProperty property,
            out Microsoft.Win32.RegistryValueKind propertyRegDataType,
            byte[] propertyBuffer,
            int propertyBufferSize,
            out int requiredSize)
        {
            uint valueType = 0;

            var result = SetupDiGetDeviceRegistryProperty(
                deviceInfoSet,
                ref deviceInfoData,
                property,
                out valueType,
                propertyBuffer,
                propertyBufferSize,
                out requiredSize);

            propertyRegDataType = MapValueKind(valueType);

            return result;
        }
    }
}
