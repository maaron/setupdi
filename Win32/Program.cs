﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Win32;

namespace Win32.Devices
{
    class Program
    {
        static readonly Guid GUID_DEVINTERFACE_MODEM = new Guid("{2C7089AA-2E0E-11D1-B114-00C04FC2AAE4}");
        static readonly Guid GUID_DEVINTERFACE_COMPORT = new Guid("{86E0D1E0-8089-11D0-9CE4-08003E301F73}");
        static readonly Guid EmeraldGuid = new Guid("85B71DDD-D042-4442-952e-6fcc794dc096");

        static void Main(string[] args)
        {
            var emeraldDis = new DeviceInformationSet(EmeraldGuid, DiGetClassFlags.DIGCF_DEVICEINTERFACE);
            var interfaces = emeraldDis.GetInterfaces(EmeraldGuid).ToList();
            var paths = interfaces.Select(i => i.Path).ToList();

            var deviceInterfaces = emeraldDis.Devices.First().GetInterfaces(EmeraldGuid).ToList();

            var set1 = new DeviceInformationSet(DiGetClassFlags.DIGCF_ALLCLASSES);
            var set2 = new DeviceInformationSet(GUID_DEVINTERFACE_MODEM, null, null, DiGetClassFlags.DIGCF_DEVICEINTERFACE | DiGetClassFlags.DIGCF_PRESENT);
            var comports = new DeviceInformationSet(GUID_DEVINTERFACE_COMPORT, DiGetClassFlags.DIGCF_DEVICEINTERFACE | DiGetClassFlags.DIGCF_PRESENT).Devices;
            var modems = new DeviceInformationSet(GUID_DEVINTERFACE_MODEM, DiGetClassFlags.DIGCF_DEVICEINTERFACE | DiGetClassFlags.DIGCF_PRESENT).Devices;
            var modem_comports = comports.Where(port => port.GetInterfaces(GUID_DEVINTERFACE_MODEM).Any());
        }
#if false
        static void PinvokeSample()
        {
            string DevEnum = REGSTR_KEY_USB;

            // Use the "enumerator form" of the SetupDiGetClassDevs API 
            // to generate a list of all USB devices
            IntPtr h = SetupDi.SetupDiGetClassDevs(0, DevEnum, IntPtr.Zero, SetupDi.DIGCF_PRESENT | SetupDi.DIGCF_ALLCLASSES);
            if (h.ToInt32() != 0)
            {
                IntPtr ptrBuf = Marshal.AllocHGlobal(BUFFER_SIZE);
                string KeyName;

                bool Success = true;
                int i = 0;
                while (Success)
                {
                    // create a Device Interface Data structure
                    SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                    da.cbSize = Marshal.SizeOf(da);

                    // start the enumeration 
                    Success = SetupDiEnumDeviceInfo(h, i, ref da);
                    if (Success)
                    {
                        int RequiredSize = 0;
                        int RegType = REG_SZ;
                        KeyName = "";

                        if (SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DRIVER, ref RegType, ptrBuf, BUFFER_SIZE, ref RequiredSize))
                        {
                            KeyName = Marshal.PtrToStringAuto(ptrBuf);
                        }

                        // is it a match?
                        if (KeyName == DriverKeyName)
                        {
                            if (SetupDiGetDeviceRegistryProperty(h, ref da, SPDRP_DEVICEDESC, ref RegType, ptrBuf, BUFFER_SIZE, ref RequiredSize))
                            {
                                ans = Marshal.PtrToStringAuto(ptrBuf);
                            }
                            break;
                        }
                    }
                    i++;
                }
                Marshal.FreeHGlobal(ptrBuf);
                SetupDiDestroyDeviceInfoList(h);
            }
        }
#endif
    }
}
