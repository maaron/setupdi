using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Win32.Devices
{
    public enum DeviceType
    {
        Port, Interface
    }

    public class DeviceInfo
    {
        public DeviceType DeviceType;
        public Guid ClassGuid;
        public string Name;

        public DeviceInfo(DeviceType deviceType, Guid classGuid, string name)
        {
            DeviceType = deviceType;
            ClassGuid = classGuid;
            Name = name;
        }
    }

    /// <summary>
    /// This class provides device add/remove event notification based on the device class GUID
    /// </summary>
    public class DeviceListener : NativeWindow, IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            // Name is variable length
            //internal short Name;
        }

        const int WM_DEVICECHANGE = 0x0219;
        const int WM_DESTROY = 0x0002;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        const int DBT_DEVTYPE_PORT = 0x0003;
        const int DBT_DEVTYP_DEVICEINTERFACE = 0x0005;

        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004;

        const int HWND_MESSAGE = -3;

        public event Action<DeviceInfo> DeviceArrived = delegate { };
        public event Action<DeviceInfo> DeviceRemoveCompleted = delegate { };
        private ApplicationContext context = new ApplicationContext();
        private Nullable<DeviceType> deviceType = null;

        /// <summary>
        /// Creates a DeviceListener that listens for devices that implement 
        /// the specified interface class.  Note that this is different than 
        /// the "device class guid" shown in Device Manager, which is 
        /// technically the "device setup class".  The guid that must be 
        /// specified here is an identifier for the "device interface class".
        /// </summary>
        /// <param name="classGuid"></param>
        public DeviceListener(Guid classGuid)
        {
            deviceType = DeviceType.Interface;
            Initialize(classGuid);
        }

        /// <summary>
        /// Creates a DeviceListener that listens for devices of the specified 
        /// type.
        /// </summary>
        /// <param name="deviceType"></param>
        public DeviceListener(DeviceType deviceType)
        {
            this.deviceType = deviceType;
            Initialize(new Guid());
        }

        /// <summary>
        /// Creates a DeviceListener that listens for all ports and device 
        /// interfaces
        /// </summary>
        public DeviceListener()
        {
            Initialize(new Guid());
        }

        private void Initialize(Guid guid)
        {
            // This is a little complicated by the fact that the window must 
            // be created on the same thread that does the processing, but we 
            // want to be able to forward exceptions during initialization to 
            // the caller on the calling thread.
            //
            // This works by signaling an event when initialization is 
            // complete and setting an exception reference if one occurred.
            var initialized = new System.Threading.AutoResetEvent(false);
            var notificationHandle = new IntPtr(0);
            Exception exception = null;
            var notificationThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    CreateHandle(new CreateParams());

                    // Register for additional device notifications 
                    // (WM_DEVICECHANGE messages) beyond what a window 
                    // receives by default.
                    notificationHandle = RegisterForDeviceNotifications(guid);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    initialized.Set();
                }

                try
                {
                    // Run the message loop.  All the WM_DEVICECHANGE 
                    // processing will happen from this context.
                    Application.Run(context);
                }
                finally
                {
                    // Need to always unregister the device notification 
                    // handle and the window handle created during the call 
                    // to CreateHandle
                    if (notificationHandle != IntPtr.Zero)
                        UnregisterDeviceNotification(notificationHandle);
                    ReleaseHandle();
                }
            }));

            // Start the thread and wait for the initialization to occur 
            // before returning.
            notificationThread.Start();
            initialized.WaitOne();

            // If an exception occured while initizing the windo or 
            // notification registration, re-throw it now.
            if (exception != null) throw exception;
        }

        private IntPtr RegisterForDeviceNotifications(Guid guid)
        {
            var filter = new DEV_BROADCAST_DEVICEINTERFACE();
            filter.dbcc_size = Marshal.SizeOf(filter) + 4;
            filter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            filter.dbcc_reserved = 0;
            filter.dbcc_classguid = guid;

            var flags = guid == new Guid() ?
                DEVICE_NOTIFY_WINDOW_HANDLE | DEVICE_NOTIFY_ALL_INTERFACE_CLASSES
                : DEVICE_NOTIFY_WINDOW_HANDLE;

            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(filter) + 4);
            Marshal.StructureToPtr(filter, ptr, false);

            var handle = RegisterDeviceNotification(this.Handle, ptr, 
                guid == new Guid() ?
                DEVICE_NOTIFY_WINDOW_HANDLE | DEVICE_NOTIFY_ALL_INTERFACE_CLASSES :
                DEVICE_NOTIFY_WINDOW_HANDLE);
            
            Marshal.FreeHGlobal(ptr);
            if (handle == IntPtr.Zero)
            {
                throw new Exception("RegisterDeviceNotification failed: " + GetLastError());
            }
            return handle;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.ExitThread();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt64() == DBT_DEVICEARRIVAL)
                {
                    var header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(
                        m.LParam, typeof(DEV_BROADCAST_HDR));

                    if (header.dbch_devicetype == DBT_DEVTYPE_PORT)
                    {
                        if (deviceType == DeviceType.Port || deviceType == null)
                        {
                            var name = Marshal.PtrToStringAuto(new IntPtr(
                                m.LParam.ToInt64() + Marshal.SizeOf(typeof(DEV_BROADCAST_HDR))));

                            DeviceArrived(new DeviceInfo(
                                DeviceType.Port,
                                new Guid(),
                                name));
                        }
                    }
                    else if (header.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        if (deviceType == DeviceType.Interface || deviceType == null)
                        {
                            var di = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(
                                m.LParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

                            var name = Marshal.PtrToStringAuto(new IntPtr(
                                m.LParam.ToInt64() + Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE))));

                            DeviceArrived(new DeviceInfo(
                                DeviceType.Interface,
                                di.dbcc_classguid,
                                name));
                        }
                    }
                }
                else if (m.WParam.ToInt64() == DBT_DEVICEREMOVECOMPLETE)
                {
                    var header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(
                        m.LParam, typeof(DEV_BROADCAST_HDR));

                    if (header.dbch_devicetype == DBT_DEVTYPE_PORT)
                    {
                        if (deviceType == DeviceType.Port || deviceType == null)
                        {
                            var name = Marshal.PtrToStringAuto(new IntPtr(
                                m.LParam.ToInt64() + Marshal.SizeOf(typeof(DEV_BROADCAST_HDR))));

                            DeviceRemoveCompleted(new DeviceInfo(
                                DeviceType.Port,
                                new Guid(),
                                name));
                        }
                    }
                    else if (header.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        if (deviceType == DeviceType.Interface || deviceType == null)
                        {
                            var di = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(
                                m.LParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

                            var name = Marshal.PtrToStringAuto(new IntPtr(
                                m.LParam.ToInt64() + Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE))));

                            DeviceRemoveCompleted(new DeviceInfo(
                                DeviceType.Interface,
                                di.dbcc_classguid,
                                name));
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
