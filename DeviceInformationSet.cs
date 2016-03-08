using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Win32
{
    public class DeviceInformationSet : IDisposable
    {
        public IntPtr Handle { get; private set; }
        private Guid classGuid;

        public DeviceInformationSet(string enumerator, IntPtr parent, DiGetClassFlags flags)
        {
            Handle = SetupDi.GetClassDevs(enumerator, parent, flags);
        }

        public DeviceInformationSet(Guid guid, string enumerator, IntPtr parent, DiGetClassFlags flags)
        {
            classGuid = guid;
            Handle = SetupDi.GetClassDevs(guid, enumerator, parent, flags);
        }

        public DeviceInformationSet(DiGetClassFlags flags)
            : this(null, IntPtr.Zero, flags)
        {
        }

        public IEnumerable<DeviceInformation> Devices
        {
            get
            {
                SP_DEVINFO_DATA deviceInfo = new SP_DEVINFO_DATA();
                deviceInfo.cbSize = Marshal.SizeOf(deviceInfo);
                uint index = 0;
                while (SetupDi.EnumDeviceInfo(Handle, index, ref deviceInfo))
                {
                    yield return new DeviceInformation(this, deviceInfo);
                    index++;
                }
            }
        }

        public IEnumerable<DeviceInterface> Interfaces
        {
            get
            {
                SP_DEVICE_INTERFACE_DATA interfaceData = new SP_DEVICE_INTERFACE_DATA();
                interfaceData.cbSize = Marshal.SizeOf(interfaceData);
                uint index = 0;
                while (SetupDi.EnumDeviceInterfaces(Handle, ref classGuid, index, ref interfaceData))
                {
                    yield return new DeviceInterface(this, interfaceData);
                    index++;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Handle == IntPtr.Zero)
                    SetupDi.DestroyDeviceInfoList(Handle);

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
