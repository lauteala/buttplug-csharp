using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buttplug.Core;
using Buttplug.Core.Messages;
using MadWizard.WinUSBNet;

namespace Buttplug.Server.Managers.WinUSBManager
{
    class WinUSBDevice : ButtplugDevice
    {
        private USBDevice _device;

        public WinUSBDevice(IButtplugLogManager aLogManager, USBDevice aDevice)
            : base(aLogManager, "WinUSB Device", "Trancevibrator")
        {
            _device = aDevice;
            MsgFuncs.Add(typeof(SingleMotorVibrateCmd), HandleSingleMotorVibrateCmd);
            MsgFuncs.Add(typeof(StopDeviceCmd), HandleStopDeviceCmd);
        }

        private Task<ButtplugMessage> HandleStopDeviceCmd(ButtplugDeviceMessage aMsg)
        {
            BpLogger.Debug("Stopping Device " + Name);
            return HandleSingleMotorVibrateCmd(new SingleMotorVibrateCmd(aMsg.DeviceIndex, 0, aMsg.Id));
        }

        private Task<ButtplugMessage> HandleSingleMotorVibrateCmd(ButtplugDeviceMessage aMsg)
        {
            var cmdMsg = aMsg as SingleMotorVibrateCmd;
            if (cmdMsg is null)
            {
                return Task.FromResult<ButtplugMessage>(BpLogger.LogErrorMsg(aMsg.Id, Error.ErrorClass.ERROR_DEVICE, "Wrong Handler"));
            }
            byte speed = (byte)Math.Floor(cmdMsg.Speed * 255);
            _device.ControlOut(
                (0x02) << 5 | // Vendor Type
                (0x01) | // Interface Recipient
                (0x00), // Out Enpoint
                1,
                speed,
                0);
            return Task.FromResult<ButtplugMessage>(new Ok(aMsg.Id));
        }

        public override void Disconnect()
        {

        }
    }
}
