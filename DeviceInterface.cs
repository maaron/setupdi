using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Win32
{
    public class DeviceInterface
    {
        public Guid InterfaceClassGuid { get; private set; }
        public int Flags { get; private set; }
        private DeviceInformationSet deviceInfoSet;

        public DeviceInterface(DeviceInformationSet deviceInfoSet, SP_DEVICE_INTERFACE_DATA interfaceData)
        {
            this.deviceInfoSet = deviceInfoSet;
            InterfaceClassGuid = interfaceData.interfaceClassGuid;
            Flags = interfaceData.flags;
        }
    }
}
