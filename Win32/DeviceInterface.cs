using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Win32
{
    public class DeviceInterface
    {
        private DeviceInformationSet DeviceInfoSet;
        private SP_DEVICE_INTERFACE_DATA InterfaceData;

        public DeviceInterface(DeviceInformationSet deviceInfoSet, SP_DEVICE_INTERFACE_DATA interfaceData)
        {
            DeviceInfoSet = deviceInfoSet;
            InterfaceData = interfaceData;
        }

        public Guid InterfaceClassGuid => InterfaceData.interfaceClassGuid;

        public int Flags => InterfaceData.flags;

        public string Path => SetupDi.GetDevicePath(DeviceInfoSet.HDevInfo, InterfaceData);
    }
}
