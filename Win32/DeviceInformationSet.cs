using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Win32
{
    public class DeviceInformationSet : IDisposable
    {
        public HDevInfo HDevInfo { get; }
        public Guid? Guid { get; }
        public string Enumerator { get; }
        public HWnd? Parent { get; }
        public DiGetClassFlags Flags { get; }

        public DeviceInformationSet(Guid? guid, string enumerator, HWnd? parent, DiGetClassFlags flags)
        {
            Guid = guid;
            Enumerator = enumerator;
            Parent = parent;
            Flags = flags;
            HDevInfo = SetupDi.GetClassDevs(guid, enumerator, parent, flags);
        }

        public DeviceInformationSet(Guid guid, DiGetClassFlags flags)
            : this(guid, null, null, flags)
        {
        }

        public DeviceInformationSet(Guid guid)
            : this(guid, null, null, 0)
        {
        }

        public DeviceInformationSet(DiGetClassFlags flags)
            : this(null, null, null, flags)
        {
        }

        public DeviceInformationSet()
            : this(null, null, null, DiGetClassFlags.DIGCF_ALLCLASSES)
        {
        }

        public IEnumerable<DeviceInformation> Devices
        {
            get
            {
                SP_DEVINFO_DATA deviceInfo = new SP_DEVINFO_DATA();
                deviceInfo.cbSize = Marshal.SizeOf(deviceInfo);
                uint index = 0;
                while (SetupDi.EnumDeviceInfo(HDevInfo, index, ref deviceInfo))
                {
                    yield return new DeviceInformation(this, deviceInfo);
                    index++;
                }
            }
        }

        public IEnumerable<DeviceInterface> GetInterfaces(Guid interfaceClassGuid)
        {
            SP_DEVICE_INTERFACE_DATA interfaceData = new SP_DEVICE_INTERFACE_DATA();
            interfaceData.cbSize = Marshal.SizeOf(interfaceData);
            uint index = 0;
            while (SetupDi.EnumDeviceInterfaces(HDevInfo, interfaceClassGuid, index, ref interfaceData))
            {
                yield return new DeviceInterface(this, interfaceData);
                index++;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (HDevInfo.IntPtr == IntPtr.Zero)
                    SetupDi.DestroyDeviceInfoList(HDevInfo);

                disposedValue = true;
            }
        }

        ~DeviceInformationSet()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
