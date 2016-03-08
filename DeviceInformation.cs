using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Win32
{
    public class DeviceInformation
    {
        public DeviceInformationSet DeviceInformationSet { get; private set; }
        private SP_DEVINFO_DATA deviceInfo;

        public Guid ClassGuid { get; private set; }

        public uint DeviceInstance { get; private set; }

        public DeviceInformation(DeviceInformationSet deviceInfoSet, SP_DEVINFO_DATA deviceInfo)
        {
            DeviceInformationSet = deviceInfoSet;
            this.deviceInfo = deviceInfo;
            ClassGuid = deviceInfo.classGuid;
            DeviceInstance = deviceInfo.devInst;
        }

        public DEVPROPKEY[] PropertyKeys
        {
            get
            {
                int requiredCount = 0;
                if (!SetupDi.GetDevicePropertyKeys(
                    DeviceInformationSet.Handle, 
                    ref deviceInfo, 
                    null, 0, 
                    ref requiredCount, 0) && Marshal.GetLastWin32Error() != SetupDi.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Exception("GetDevicePropertyKeys failed: " + Marshal.GetLastWin32Error());
                }

                var keys = new DEVPROPKEY[requiredCount];
                if (!SetupDi.GetDevicePropertyKeys(
                    DeviceInformationSet.Handle,
                    ref deviceInfo,
                    keys, keys.Length,
                    ref requiredCount, 0))
                {
                    throw new Exception("GetDevicePropertyKeys failed: " + Marshal.GetLastWin32Error());
                }

                return keys.Take(Math.Min(requiredCount, keys.Length)).ToArray();
            }
        }
    }
}
