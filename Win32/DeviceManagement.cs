using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Win32
{
    public static class DeviceManagement
    {
        public static IEnumerable<DeviceInformation> AllDevices() => 
            new DeviceInformationSet().Devices;

        public static IEnumerable<DeviceInformation> DevicesBySetupClass(Guid setupClassGuid) => 
            new DeviceInformationSet(setupClassGuid).Devices;

        public static IEnumerable<DeviceInformation> DevicesByInterfaceClass(Guid interfaceClassGuid) =>
            new DeviceInformationSet(interfaceClassGuid, DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_DEVICEINTERFACE).Devices;

        public static class SetupClasses
        {
            public static readonly Guid Battery = new Guid("{72631e54-78a4-11d0-bcf7-00aa00b7b32a}");
            public static readonly Guid Biometric = new Guid("{53D29EF7-377C-4D14-864B-EB3A85769359}");
            public static readonly Guid Bluetooth = new Guid("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}");
            public static readonly Guid Camera = new Guid("{ca3e7ab9-b4c3-4ae6-8251-579ef933890f}");
            public static readonly Guid CdRom = new Guid("{4d36e965-e325-11ce-bfc1-08002be10318}");
            public static readonly Guid Disk = new Guid("{4d36e967-e325-11ce-bfc1-08002be10318}");

            public static readonly Guid DisplayAdapter = new Guid("{4d36e968-e325-11ce-bfc1-08002be10318}");
            public static readonly Guid ExtensionInf = new Guid("{e2f84ce7-8efa-411c-aa69-97454ca4cb57}");
            public static readonly Guid FloppyDiskController = new Guid("{4d36e969-e325-11ce-bfc1-08002be10318}");
            public static readonly Guid FloppyDiskDrive = new Guid("{4d36e980-e325-11ce-bfc1-08002be10318}");
            public static readonly Guid HardDiskController = new Guid("{4d36e96a-e325-11ce-bfc1-08002be10318}");
            public static readonly Guid HumanInterface = new Guid("{745a17a0-74d3-11d0-b6fe-00a0c90f57da}");
            public static readonly Guid Ieee1284dot4 = new Guid("{48721b56-6795-11d2-b1a8-0080c72e74a2}");


        }

        public static class InterfaceClasses
        {
            public static class Storage
            {
                public static readonly Guid Disk = new Guid(0x53f56307, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid CdRom = new Guid(0x53f56308, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid Partition = new Guid(0x53f5630a, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid Tape = new Guid(0x53f5630b, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid WriteOnceDisk = new Guid(0x53f5630c, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid Volume = new Guid(0x53f5630d, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid MediumChanger = new Guid(0x53f56310, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid Floppy = new Guid(0x53f56311, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid CdChanger = new Guid(0x53f56312, 0xb6bf, 0x11d0, 0x94, 0xf2, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid StoragePort = new Guid(0x2accfe60, 0xc130, 0x11d2, 0xb0, 0x82, 0x00, 0xa0, 0xc9, 0x1e, 0xfb, 0x8b);
                public static readonly Guid VMLUN = new Guid(0x6f416619, 0x9f29, 0x42a5, 0xb2, 0x0b, 0x37, 0xe2, 0x19, 0xca, 0x02, 0xb0);
                public static readonly Guid SES = new Guid(0x1790c9ec, 0x47d5, 0x4df3, 0xb5, 0xaf, 0x9a, 0xdf, 0x3c, 0xf2, 0x3e, 0x48);
            }

            public static class USB
            {
                public static readonly Guid Hub = new Guid(0xf18a0e88, 0xc30c, 0x11d0, 0x88, 0x15, 0x00, 0xa0, 0xc9, 0x06, 0xbe, 0xd8);
                public static readonly Guid Device = new Guid(0xA5DCBF10, 0x6530, 0x11D2, 0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED);
                public static readonly Guid HostController = new Guid(0x3abf6f2d, 0x71c4, 0x462a, 0x8a, 0x92, 0x1e, 0x68, 0x61, 0xe6, 0xaf, 0x27);
                public static readonly Guid WmiStdData = new Guid(0x4E623B20, 0xCB14, 0x11D1, 0xB3, 0x31, 0x00, 0xA0, 0xC9, 0x59, 0xBB, 0xD2);
                public static readonly Guid WmiStdNotification = new Guid(0x4E623B20, 0xCB14, 0x11D1, 0xB3, 0x31, 0x00, 0xA0, 0xC9, 0x59, 0xBB, 0xD2);
                public static readonly Guid WmiDevicePerfInfo = new Guid(0x66c1aa3c, 0x499f, 0x49a0, 0xa9, 0xa5, 0x61, 0xe2, 0x35, 0x9f, 0x64, 0x7);
                public static readonly Guid WmiNodeInfo = new Guid(0x9c179357, 0xdc7a, 0x4f41, 0xb6, 0x6b, 0x32, 0x3b, 0x9d, 0xdc, 0xb5, 0xb1);
                public static readonly Guid WmiTracing = new Guid(0x3a61881b, 0xb4e6, 0x4bf9, 0xae, 0xf, 0x3c, 0xd8, 0xf3, 0x94, 0xe5, 0x2f);
                public static readonly Guid TransferTracing = new Guid(0x681eb8aa, 0x403d, 0x452c, 0x9f, 0x8a, 0xf0, 0x61, 0x6f, 0xac, 0x95, 0x40);
                public static readonly Guid PerformanceTracing = new Guid(0xd5de77a6, 0x6ae9, 0x425c, 0xb1, 0xe2, 0xf5, 0x61, 0x5f, 0xd3, 0x48, 0xa9);
                public static readonly Guid WmiSurpriseRemovalNotification = new Guid(0x9bbbf831, 0xa2f2, 0x43b4, 0x96, 0xd1, 0x86, 0x94, 0x4b, 0x59, 0x14, 0xb3);
            }
        }
    }
}
