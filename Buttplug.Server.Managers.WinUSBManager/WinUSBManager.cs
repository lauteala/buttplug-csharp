using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buttplug.Core;
using MadWizard.WinUSBNet;

namespace Buttplug.Server.Managers.WinUSBManager
{
    public class WinUSBManager : DeviceSubtypeManager
    {
        public WinUSBManager(IButtplugLogManager aLogManager)
            : base(aLogManager)
        {
            BpLogger.Info("Loading XInput Gamepad Manager");
        }

        public override void StartScanning()
        {
            BpLogger.Info("WinUSBManager start scanning");
            // Only valid for our Trancevibrator install
            var TrancevibratorDeviceID = "{3420AFC8-06B8-4215-97C2-8E59F6C16606}";
            var device = USBDevice.GetSingleDevice(TrancevibratorDeviceID);
            BpLogger.Debug($"Found TranceVibrator Device");
            var tvDevice = new WinUSBDevice(LogManager, device);
            InvokeDeviceAdded(new DeviceAddedEventArgs(tvDevice));
            InvokeScanningFinished();
        }

        public override void StopScanning()
        {
            // noop
            BpLogger.Info("WinUSBManager stop scanning");
        }

        public override bool IsScanning()
        {
            // noop
            return false;
        }
    }
}
