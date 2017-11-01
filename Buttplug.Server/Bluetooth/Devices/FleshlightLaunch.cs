using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buttplug.Core;
using Buttplug.Core.Messages;
using JetBrains.Annotations;

namespace Buttplug.Server.Bluetooth.Devices
{
    internal class FleshlightLaunchBluetoothInfo : IBluetoothDeviceInfo
    {
        public enum Chrs : uint
        {
            Tx = 0,
            Rx,
            Cmd,
            Battery,
        }

        public string[] Names { get; } = { "Launch" };

        public Guid[] Services { get; } = { new Guid("88f80580-0000-01e6-aace-0002a5d5c51b") };

        public Guid[] Characteristics { get; } =
        {
            // tx
            new Guid("88f80581-0000-01e6-aace-0002a5d5c51b"),

            // rx
            new Guid("88f80582-0000-01e6-aace-0002a5d5c51b"),

            // cmd
            new Guid("88f80583-0000-01e6-aace-0002a5d5c51b"),
        };

        public IButtplugDevice CreateDevice(
            IButtplugLogManager aLogManager,
            IBluetoothDeviceInterface aInterface)
        {
            return new FleshlightLaunch(aLogManager, aInterface, this);
        }
    }

    internal class FleshlightLaunch : ButtplugBluetoothDevice
    {
        private uint _position = 0;
        private uint _speed = 0;

        public FleshlightLaunch([NotNull] IButtplugLogManager aLogManager,
                                [NotNull] IBluetoothDeviceInterface aInterface,
                                [NotNull] IBluetoothDeviceInfo aInfo)
            : base(aLogManager,
                   "Fleshlight Launch",
                   aInterface,
                   aInfo)
        {
            // Setup message function array
            MsgFuncs.Add(typeof(FleshlightLaunchFW12Cmd), new ButtplugDeviceWrapper(HandleLinearCmd));
            MsgFuncs.Add(typeof(LinearCmd), new ButtplugDeviceWrapper(HandleLinearCmd, new Dictionary<string, string>() { { "LinearCount", "1" } }));
            MsgFuncs.Add(typeof(StopDeviceCmd), new ButtplugDeviceWrapper(HandleStopDeviceCmd));
        }

        public override async Task<ButtplugMessage> Initialize()
        {
            BpLogger.Trace($"Initializing {Name}");
            return await Interface.WriteValue(ButtplugConsts.SystemMsgId,
                Info.Characteristics[(uint)FleshlightLaunchBluetoothInfo.Chrs.Cmd],
                new byte[] { 0 },
                true);
        }

        private Task<ButtplugMessage> HandleStopDeviceCmd(ButtplugDeviceMessage aMsg)
        {
            // This probably shouldn't be a nop, but right now we don't have a good way to know
            // if the launch is moving or not, and surprisingly enough, setting speed to 0 does not
            // actually stop movement. It just makes it move really slow.
            // However, since each move it makes is finite (unlike setting vibration on some devices),
            // so we can assume it will be a short move, similar to what we do for the Kiiroo toys.
            BpLogger.Debug("Stopping Device " + Name);
            return Task.FromResult<ButtplugMessage>(new Ok(aMsg.Id));
        }

        private async Task<ButtplugMessage> HandleLinearCmd(ButtplugDeviceMessage aMsg)
        {
            // TODO: Split into Command message and Control message? (Issue #17)
            var cmdMsg1 = aMsg as FleshlightLaunchFW12Cmd;
            var cmdMsg2 = aMsg as LinearCmd;
            if (cmdMsg1 is null && cmdMsg2 is null)
            {
                return BpLogger.LogErrorMsg(aMsg.Id, Error.ErrorClass.ERROR_DEVICE, "Wrong Handler");
            }

            if (cmdMsg1 != null)
            {
                _position = cmdMsg1.Position;
                _speed = cmdMsg1.Speed;
            }
            else
            {
                foreach (var vi in cmdMsg2.Vectors)
                {
                    if (vi.Index == 0)
                    {
                        _position = (uint)(vi.Position * 99);
                        _speed = (uint)(vi.Speed * 99);
                    }
                }
            }

            return await Interface.WriteValue(aMsg.Id,
                Info.Characteristics[(uint)FleshlightLaunchBluetoothInfo.Chrs.Tx],
                new[] { (byte)_position, (byte)_speed });
        }
    }
}
