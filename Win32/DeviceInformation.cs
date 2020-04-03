using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Win32
{
    [Flags]
    public enum DeviceCapabilities : int
    {
        None              = 0x00000000,
        LockSupported     = 0x00000001,
        EjectSupported    = 0x00000002,
        Removable         = 0x00000004,
        DockDevice        = 0x00000008,
        UniqueId          = 0x00000010,
        SilentInstall     = 0x00000020,
        RawDeviceOk       = 0x00000040,
        SurpriseRemovalOk = 0x00000080,
        HarwareDisabled   = 0x00000100,
        NonDynamic        = 0x00000200
    }

    public class DeviceInformation
    {
        public DeviceInformationSet DeviceInformationSet { get; }
        private SP_DEVINFO_DATA deviceInfo;

        public SP_DEVINFO_DATA DeviceInfo { get { return deviceInfo; } }

        public Guid ClassGuid { get; private set; }

        public uint DeviceInstance { get; private set; }

        public DeviceInformation(DeviceInformationSet deviceInfoSet, SP_DEVINFO_DATA deviceInfo)
        {
            DeviceInformationSet = deviceInfoSet;
            this.deviceInfo = deviceInfo;
            ClassGuid = deviceInfo.classGuid;
            DeviceInstance = deviceInfo.devInst;
        }

        public string InstanceId
        {
            get
            {
                int requiredSize = 0;
                var ret = SetupDi.GetDeviceInstanceId(
                    DeviceInformationSet.HDevInfo, deviceInfo, null, 0, out requiredSize);
                
                if (ret || Marshal.GetLastWin32Error() != SetupDi.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Exception("GetDeviceInstanceId failed: " + Marshal.GetLastWin32Error());
                }

                var sb = new StringBuilder(requiredSize + 1);
                ret = SetupDi.GetDeviceInstanceId(
                    DeviceInformationSet.HDevInfo, deviceInfo, sb, requiredSize, out requiredSize);

                if (!ret)
                {
                    throw new Exception("GetDeviceInstanceId(2) failed: " + Marshal.GetLastWin32Error());
                }

                return sb.ToString();
            }
        }

        public IEnumerable<DeviceInterface> GetInterfaces(Guid interfaceClassGuid)
        {
            SP_DEVICE_INTERFACE_DATA interfaceData = new SP_DEVICE_INTERFACE_DATA();
            interfaceData.cbSize = Marshal.SizeOf(interfaceData);
            uint index = 0;

            while (SetupDi.EnumDeviceInterfaces(DeviceInformationSet.HDevInfo, deviceInfo, interfaceClassGuid, index, ref interfaceData))
            {
                yield return new DeviceInterface(DeviceInformationSet, interfaceData);
                index++;
            }
        }

        public DEVPROPKEY[] PropertyKeys
        {
            get
            {
                int requiredCount = 0;
                if (!SetupDi.GetDevicePropertyKeys(
                    DeviceInformationSet.HDevInfo, 
                    deviceInfo, 
                    null, 0, 
                    ref requiredCount, 0) && Marshal.GetLastWin32Error() != SetupDi.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Exception("GetDevicePropertyKeys failed: " + Marshal.GetLastWin32Error());
                }

                var keys = new DEVPROPKEY[requiredCount];
                if (!SetupDi.GetDevicePropertyKeys(
                    DeviceInformationSet.HDevInfo,
                    deviceInfo,
                    keys, keys.Length,
                    ref requiredCount, 0))
                {
                    throw new Exception("GetDevicePropertyKeys failed: " + Marshal.GetLastWin32Error());
                }

                return keys.Take(Math.Min(requiredCount, keys.Length)).ToArray();
            }
        }

        private Microsoft.Win32.RegistryKey KeyFromHandle(HKey handle)
        {
            var name = Nt.QueryKeyNameInformation(handle);

            // First portion should start with a prefix that will be replaced 
            // by the Registry.LocalMachine key
            if (!name.StartsWith(@"\REGISTRY\MACHINE\"))
            {
                throw new Exception("Not a HKLM key");
            }

            name = name.Substring(@"\REGISTRY\MACHINE\".Length);

            return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(name);
        }

        public Microsoft.Win32.RegistryKey GlobalDeviceRegistryKey
        {
            get
            {
                using (var hkey = SetupDi.OpenDevRegKey(
                        DeviceInformationSet.HDevInfo,
                        deviceInfo,
                        DicsFlag.DICS_FLAG_GLOBAL,
                        0,
                        DiKeyType.DREG_DEV,
                        RegSam.KEU_QUERY_VALUE))
                {
                    return KeyFromHandle(hkey);
                }
            }
        }

        public IEnumerable<object> GlobalDeviceProperties
        {
            get
            {
                var key = GlobalDeviceRegistryKey;
                if (key != null)
                {
                    foreach (var name in key.GetValueNames())
                    {
                        yield return key.GetValueKind(name);
                    }
                }
            }
        }
#if false
        // This method doesn't seem possible to implement, as you always 
        // have to specify an interface class guid for 
        // SetupDiEnumDeviceInterfaces...
        public IEnumerable<DeviceInterface> Interfaces
        {
            get
            {
                var infoSet = new DeviceInformationSet(
                    InstanceId,
                    IntPtr.Zero,
                    DiGetClassFlags.DIGCF_DEVICEINTERFACE | DiGetClassFlags.DIGCF_ALLCLASSES);

                return infoSet.GetInterfaces(interfaceClassGuid);
            }
        }
#endif
        private T? GetRegistryProperty<T>(RegistryProperty property, RegistryValueKind expectedValueKind) where T : struct
        {
            int requiredSize = 0;
            RegistryValueKind valueKind;
            var size = Marshal.SizeOf(typeof(T));
            var buffer = Marshal.AllocHGlobal(size);

            try
            {
                var result = SetupDi.GetDeviceRegistryProperty(
                    DeviceInformationSet.HDevInfo,
                    deviceInfo,
                    property,
                    out valueKind,
                    buffer,
                    size,
                    out requiredSize);

                if (!result) return null;

                return (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private byte[] GetRegistryProperty(RegistryProperty property, RegistryValueKind expectedValueKind)
        {
            int requiredSize = 0;
            RegistryValueKind valueKind;
            byte[] buffer;

            var result = SetupDi.GetDeviceRegistryProperty(
                DeviceInformationSet.HDevInfo,
                deviceInfo,
                property,
                out valueKind,
                IntPtr.Zero,
                0,
                out requiredSize);

            if (result || Marshal.GetLastWin32Error() != SetupDi.ERROR_INSUFFICIENT_BUFFER)
            {
                return null;
            }

            buffer = new byte[requiredSize];

            result = SetupDi.GetDeviceRegistryProperty(
                DeviceInformationSet.HDevInfo,
                deviceInfo,
                property,
                out valueKind,
                buffer,
                requiredSize,
                out requiredSize);

            if (!result) return null;

            return buffer;
        }

        private string GetStringProperty(RegistryProperty property)
        {
            var value = GetRegistryProperty(
                    property,
                    RegistryValueKind.String);

            return value == null ? (string)null
                : Marshal.SystemDefaultCharSize == 2 ? Encoding.Unicode.GetString(value, 0, value.Length - 2)
                : Encoding.ASCII.GetString(value, 0, value.Length - 1);
        }

        public uint? Address
        {
            get
            {
                return GetRegistryProperty<UInt32>(
                    RegistryProperty.SPDRP_ADDRESS, 
                    RegistryValueKind.DWord);
            }
        }

        public uint? BusNumber
        {
            get
            {
                return GetRegistryProperty<UInt32>(
                    RegistryProperty.SPDRP_BUSNUMBER,
                    RegistryValueKind.DWord);
            }
        }

        public Guid? BusTypeGuid
        {
            get
            {
                return GetRegistryProperty<Guid>(
                    RegistryProperty.SPDRP_BUSTYPEGUID, 
                    RegistryValueKind.Binary);
            }
        }

        public DeviceCapabilities? Capabilities
        {
            get
            {
                return (DeviceCapabilities)GetRegistryProperty<int>(
                    RegistryProperty.SPDRP_CAPABILITIES,
                    RegistryValueKind.DWord);
            }
        }

        public string Description
        {
            get
            {
                return GetStringProperty(RegistryProperty.SPDRP_DEVICEDESC);
            }
        }

        public string Manufacturer
        {
            get
            {
                return GetStringProperty(RegistryProperty.SPDRP_MFG);
            }
        }

        public string FriendlyName
        {
            get
            {
                return GetStringProperty(RegistryProperty.SPDRP_FRIENDLYNAME);
            }
        }
    }
}
