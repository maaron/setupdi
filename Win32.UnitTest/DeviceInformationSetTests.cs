using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DM = Win32.DeviceManagement;

namespace Win32.UnitTest
{
    [TestClass]
    public class DeviceInformationSetTests
    {
        [TestMethod]
        public void AllClassesDevicesNonEmpty()
        {
            Assert.IsTrue(DM.AllDevices().Any());
        }

        [TestMethod]
        public void ByClassGuidDevicesSameGuid()
        {
            var allDevices = DM.AllDevices().ToList();
            var firstDevice = allDevices.First();

            var classDevices = DM.DevicesBySetupClass(firstDevice.ClassGuid).ToList();

            Assert.IsTrue(classDevices.Count() < allDevices.Count());
            Assert.IsTrue(classDevices.All(d => d.ClassGuid == firstDevice.ClassGuid));
        }

        [TestMethod]
        public void VolumeInterfacesNonEmpty()
        {
            var allDevices = DM.AllDevices().ToList();

            var volumeDevices = DM.DevicesByInterfaceClass(DM.InterfaceClasses.Storage.Volume).ToList();

            Assert.IsTrue(volumeDevices.Any());
            Assert.IsTrue(volumeDevices.Count() < allDevices.Count());
        }
    }
}
