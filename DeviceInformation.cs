using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Win32.Devices
{
    public class DeviceInformation
    {
        private DeviceInformationSet deviceInfoSet;
        private SP_DEVINFO_DATA deviceInfo;

        public SP_DEVINFO_DATA DeviceInfo { get { return deviceInfo; } }

        public Guid ClassGuid { get; private set; }

        public uint DeviceInstance { get; private set; }

        public DeviceInformation(DeviceInformationSet deviceInfoSet, SP_DEVINFO_DATA deviceInfo)
        {
            this.deviceInfoSet = deviceInfoSet;
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
                    deviceInfoSet.Handle, deviceInfo, null, 0, out requiredSize);
                
                if (ret || Marshal.GetLastWin32Error() != SetupDi.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Exception("GetDeviceInstanceId failed: " + Marshal.GetLastWin32Error());
                }

                var sb = new StringBuilder(requiredSize + 1);
                ret = SetupDi.GetDeviceInstanceId(
                    deviceInfoSet.Handle, deviceInfo, sb, requiredSize, out requiredSize);

                if (!ret)
                {
                    throw new Exception("GetDeviceInstanceId(2) failed: " + Marshal.GetLastWin32Error());
                }

                return sb.ToString();
            }
        }

        public DEVPROPKEY[] PropertyKeys
        {
            get
            {
                int requiredCount = 0;
                if (!SetupDi.GetDevicePropertyKeys(
                    deviceInfoSet.Handle, 
                    deviceInfo, 
                    null, 0, 
                    ref requiredCount, 0) && Marshal.GetLastWin32Error() != SetupDi.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Exception("GetDevicePropertyKeys failed: " + Marshal.GetLastWin32Error());
                }

                var keys = new DEVPROPKEY[requiredCount];
                if (!SetupDi.GetDevicePropertyKeys(
                    deviceInfoSet.Handle,
                    deviceInfo,
                    keys, keys.Length,
                    ref requiredCount, 0))
                {
                    throw new Exception("GetDevicePropertyKeys failed: " + Marshal.GetLastWin32Error());
                }

                return keys.Take(Math.Min(requiredCount, keys.Length)).ToArray();
            }
        }

        private Microsoft.Win32.RegistryKey KeyFromHandle(IntPtr handle)
        {
            int length = 0;
            Console.WriteLine("handle = " + handle.ToInt64());
            var ret = SetupDi.NtQueryKey(handle, KeyInformationClass.KeyNameInformation, (byte[])null, 0, ref length);
            if (ret != SetupDi.STATUS_BUFFER_TOO_SMALL)
            {
                throw new Exception(String.Format(
                    "NtQueryKey({0}, {1}, {2}, {3}, {4}) failed: {5}",
                    handle.ToInt64(), KeyInformationClass.KeyNameInformation, "null", 0, length, ret));
            }

            var keyInformation = new byte[length];
            ret = SetupDi.NtQueryKey(handle, KeyInformationClass.KeyNameInformation, keyInformation, length, ref length);
            if (ret != SetupDi.STATUS_SUCCESS)
            {
                throw new Exception("NtQueryKey(2) failed: " + ret);
            }

            // first four bytes is length, followed by utf-16 string
            var name = Encoding.Unicode.GetString(keyInformation, 4, keyInformation.Length - 4);

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
                var hkey = SetupDi.OpenDevRegKey(
                        deviceInfoSet.Handle,
                        deviceInfo,
                        DicsFlag.DICS_FLAG_GLOBAL,
                        0,
                        DiKeyType.DREG_DEV,
                        RegSam.KEU_QUERY_VALUE);

                if (hkey == IntPtr.Zero) return null;

                try
                {
                    return KeyFromHandle(hkey);
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    SetupDi.RegCloseKey(hkey);
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
        public DeviceInterface GetInterface(Guid interfaceClassGuid)
        {
            var infoSet = new DeviceInformationSet(
                interfaceClassGuid,
                InstanceId,
                IntPtr.Zero,
                DiGetClassFlags.DIGCF_DEVICEINTERFACE);

            return infoSet.GetInterfaces(interfaceClassGuid).FirstOrDefault();
        }
    }
}
