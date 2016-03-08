using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32
{
    public class DeviceInterface
    {
        public Guid InterfaceClassGuid { get; private set; }
        public int Flags { get; private set; }

        public DeviceInterface(DeviceInformationSet deviceInfoSet, SP_DEVICE_INTERFACE_DATA interfaceData)
        {
            InterfaceClassGuid = interfaceData.interfaceClassGuid;
            Flags = interfaceData.flags;
        }
    }
}
