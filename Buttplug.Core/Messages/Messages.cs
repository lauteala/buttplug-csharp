﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Buttplug.Core.Messages
{
    public class Ok : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        public Ok(uint aId)
            : base(aId)
        {
        }
    }

    // Clients may instantiate Ping message, and it is used in pattern matching.
    // Resharper doesn't seem to be able to deduce that though.
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Ping : ButtplugMessage
    {
        public Ping(uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
        }
    }

    public class Test : ButtplugMessage
    {
        private string _testStringImpl;

        [JsonProperty(Required = Required.Always)]
        public string TestString
        {
            get => _testStringImpl;
            set
            {
                if (value == "Error")
                {
                    throw new ArgumentException("Got an Error Message");
                }

                _testStringImpl = value;
            }
        }

        public Test(string aString, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
            TestString = aString;
        }
    }

    public class Error : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        public enum ErrorClass
        {
            ERROR_UNKNOWN,
            ERROR_INIT,
            ERROR_PING,
            ERROR_MSG,
            ERROR_DEVICE,
        }

        [JsonProperty(Required = Required.Always)]
        public ErrorClass ErrorCode;

        [JsonProperty(Required = Required.Always)]
        public string ErrorMessage;

        public Error(string aErrorMessage, ErrorClass aErrorCode, uint aId)
            : base(aId)
        {
            ErrorMessage = aErrorMessage;
            ErrorCode = aErrorCode;
        }
    }

    public class DeviceMessageInfo
    {
        [JsonProperty(Required = Required.Always)]
        public string DeviceName;

        [JsonProperty(Required = Required.Always)]
        public uint DeviceIndex;

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, string>> DeviceMessages =
            new Dictionary<string, Dictionary<string, string>>();

        public DeviceMessageInfo(uint aIndex, string aName,
            Dictionary<string, Dictionary<string, string>> aMessages)
        {
            DeviceName = aName;
            DeviceIndex = aIndex;
            DeviceMessages = aMessages;
        }
    }

    public class DeviceMessageInfo0
    {
        [JsonProperty(Required = Required.Always)]
        public string DeviceName;

        [JsonProperty(Required = Required.Always)]
        public uint DeviceIndex;

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string[] DeviceMessages = new string[0];

        public DeviceMessageInfo0(uint aIndex, string aName, string[] aMessages)
        {
            DeviceName = aName;
            DeviceIndex = aIndex;
            DeviceMessages = aMessages;
        }
    }

    public class DeviceList : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public readonly DeviceMessageInfo[] Devices = new DeviceMessageInfo[0];

        public DeviceList(DeviceMessageInfo[] aDeviceList, uint aId)
             : base(aId)
        {
            Devices = aDeviceList;

            MessageVersioningVersion = 1;
            MessageVersioningPrevious = new DeviceList0().GetType();
        }
    }

    public class DeviceList0 : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public readonly DeviceMessageInfo0[] Devices = new DeviceMessageInfo0[0];

        public DeviceList0(DeviceMessageInfo0[] aDeviceList, uint aId)
             : base(aId)
        {
            Devices = aDeviceList;
        }

        public DeviceList0()
            : base(0)
        {
        }

        public DeviceList0(DeviceList aMsg)
             : base(aMsg.Id)
        {
            var tmp = new List<DeviceMessageInfo0>();
            foreach (var dev in aMsg.Devices)
            {
                var tmp2 = new List<string>();
                foreach (var k in dev.DeviceMessages.Keys)
                {
                    tmp2.Add(k);
                }

                tmp.Add(new DeviceMessageInfo0(dev.DeviceIndex, dev.DeviceName, tmp2.ToArray()));
            }

            Devices = tmp.ToArray();
        }
    }

    public class DeviceAdded : ButtplugDeviceMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always)]
        public string DeviceName;

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, string>> DeviceMessages =
            new Dictionary<string, Dictionary<string, string>>();

        public DeviceAdded(uint aIndex, string aName,
            Dictionary<string, Dictionary<string, string>> aMessages)
            : base(ButtplugConsts.SystemMsgId, aIndex)
        {
            DeviceName = aName;
            DeviceMessages = aMessages;

            MessageVersioningVersion = 1;
            MessageVersioningPrevious = new DeviceAdded0().GetType();
        }
    }

    public class DeviceAdded0 : ButtplugDeviceMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always)]
        public string DeviceName;

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string[] DeviceMessages = new string[0];

        public DeviceAdded0(uint aIndex, string aName, string[] aMessages)
            : base(ButtplugConsts.SystemMsgId, aIndex)
        {
            DeviceName = aName;
            DeviceMessages = aMessages;
        }

        public DeviceAdded0()
            : base(0, 0)
        {
        }

        public DeviceAdded0(DeviceAdded aMsg)
            : base(aMsg.Id, aMsg.DeviceIndex)
        {
            DeviceName = aMsg.DeviceName;
            var tmp = new List<string>();
            foreach (var k in aMsg.DeviceMessages.Keys)
            {
                tmp.Add(k);
            }

            DeviceMessages = tmp.ToArray();
        }
    }

    public class DeviceRemoved : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always)]
        public uint DeviceIndex;

        public DeviceRemoved(uint aIndex)
            : base(ButtplugConsts.SystemMsgId)
        {
            DeviceIndex = aIndex;
        }
    }

    public class RequestDeviceList : ButtplugMessage
    {
        public RequestDeviceList(uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
        }
    }

    public class StartScanning : ButtplugMessage
    {
        public StartScanning(uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
        }
    }

    public class StopScanning : ButtplugMessage
    {
        public StopScanning(uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
        }
    }

    public class ScanningFinished : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        public ScanningFinished()
            : base(ButtplugConsts.SystemMsgId)
        {
        }
    }

    public class RequestLog : ButtplugMessage
    {
        [JsonProperty(Required = Required.Always)]
        public ButtplugLogLevel LogLevel;

        // JSON.Net gets angry if it doesn't have a default initializer.
        public RequestLog()
            : base(ButtplugConsts.DefaultMsgId)
        {
        }

        public RequestLog(uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId) => LogLevel = ButtplugLogLevel.Off;

        public RequestLog(string aLogLevel, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
            ButtplugLogLevel level;
            if (!Enum.TryParse(aLogLevel, out level))
            {
                throw new ArgumentException("Invalid log level");
            }

            LogLevel = level;
        }
    }

    public class Log : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always)]
        public ButtplugLogLevel LogLevel;

        [JsonProperty(Required = Required.Always)]
        public string LogMessage;

        public Log(ButtplugLogLevel aLogLevel, string aLogMessage)
            : base(ButtplugConsts.SystemMsgId)
        {
            LogLevel = aLogLevel;
            LogMessage = aLogMessage;
        }
    }

    public class RequestServerInfo : ButtplugMessage
    {
        [JsonProperty(Required = Required.Always)]
        public string ClientName;

        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public uint MessageVersion = 0;

        public RequestServerInfo(string aClientName, uint aId = ButtplugConsts.DefaultMsgId, uint aMessageVersion = CurrentMessageVersion)
            : base(aId)
        {
            ClientName = aClientName;
            MessageVersion = aMessageVersion;
        }
    }

    public class ServerInfo : ButtplugMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always)]
        public int MajorVersion;

        [JsonProperty(Required = Required.Always)]
        public int MinorVersion;

        [JsonProperty(Required = Required.Always)]
        public int BuildVersion;

        // Disable can be private here, as this field is used for serialization,
        // and may be needed by clients
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [JsonProperty(Required = Required.Always)]
        public uint MessageVersion;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [JsonProperty(Required = Required.Always)]
        public uint MaxPingTime;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [JsonProperty(Required = Required.Always)]
        public string ServerName;

        public ServerInfo(string aServerName, uint aMessageVersion, uint aMaxPingTime, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
            ServerName = aServerName;
            MessageVersion = aMessageVersion;
            MaxPingTime = aMaxPingTime;
            MajorVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.Major;
            MinorVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.Minor;
            BuildVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.Build;
        }
    }

    public class FleshlightLaunchFW12Cmd : ButtplugDeviceMessage
    {
        private uint _speedImpl;

        [JsonProperty(Required = Required.Always)]
        public uint Speed
        {
            get => _speedImpl;
            set
            {
                if (value > 99)
                {
                    throw new ArgumentException("FleshlightLaunchRawMessage cannot have a speed higher than 99!");
                }

                _speedImpl = value;
            }
        }

        private uint _positionImpl;

        [JsonProperty(Required = Required.Always)]
        public uint Position
        {
            get => _positionImpl;
            set
            {
                if (value > 99)
                {
                    throw new ArgumentException("FleshlightLaunchRawMessage cannot have a position higher than 99!");
                }

                _positionImpl = value;
            }
        }

        public FleshlightLaunchFW12Cmd(uint aDeviceIndex, uint aSpeed, uint aPosition, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Speed = aSpeed;
            Position = aPosition;
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class LovenseCmd : ButtplugDeviceMessage
    {
        public LovenseCmd(uint aDeviceIndex, string aDeviceCmd, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
        }
    }

    public class KiirooCmd : ButtplugDeviceMessage
    {
        [JsonProperty(Required = Required.Always)]
        public string Command;

        [JsonIgnore]
        public uint Position
        {
            get
            {
                if (uint.TryParse(Command, out uint pos) && pos <= 4)
                {
                    return pos;
                }

                return 0;
            }

            set
            {
                if (value > 4)
                {
                    throw new ArgumentException("KiirooRawCmd Position cannot be greater than 4");
                }

                Command = value.ToString();
            }
        }

        public KiirooCmd(uint aDeviceIndex, uint aPosition, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Position = aPosition;
        }
    }

    public class VorzeA10CycloneCmd : ButtplugDeviceMessage
    {
        private uint _speedImpl;

        [JsonProperty(Required = Required.Always)]
        public uint Speed
        {
            get => _speedImpl;
            set
            {
                if (value > 99)
                {
                    throw new ArgumentException("VorzeA10CycloneCmd cannot have a speed higher than 99!");
                }

                _speedImpl = value;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public bool Clockwise;

        public VorzeA10CycloneCmd(uint aDeviceIndex, uint aSpeed, bool aClockwise, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Speed = aSpeed;
            Clockwise = aClockwise;
        }
    }

    public class SingleMotorVibrateCmd : ButtplugDeviceMessage
    {
        private double _speedImpl;

        [JsonProperty(Required = Required.Always)]
        public double Speed
        {
            get => _speedImpl;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("SingleMotorVibrateMessage Speed cannot be less than 0!");
                }

                if (value > 1)
                {
                    throw new ArgumentException("SingleMotorVibrateMessage Speed cannot be greater than 1!");
                }

                _speedImpl = value;
            }
        }

        public SingleMotorVibrateCmd(uint aDeviceIndex, double aSpeed, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Speed = aSpeed;
        }
    }

    public class VibrateCmd : ButtplugDeviceMessage
    {
        public class VibrateIndex
        {
            private double _speedImpl = 0;

            [JsonProperty(Required = Required.Always)]
            public uint Index = 0;

            [JsonProperty(Required = Required.Always)]
            public double Speed
            {
                get => _speedImpl;
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be less than 0!");
                    }

                    if (value > 1)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be greater than 1!");
                    }

                    _speedImpl = value;
                }
            }

            public VibrateIndex(uint aIndex, double aSpeed)
            {
                Index = aIndex;
                Speed = aSpeed;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public List<VibrateIndex> Speeds;

        public VibrateCmd(uint aDeviceIndex, List<VibrateIndex> aSpeeds, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Speeds = aSpeeds;
            MessageVersioningVersion = 1;
        }
    }

    public class RotateCmd : ButtplugDeviceMessage
    {
        public class RotateIndex
        {
            private double _speedImpl = 0;

            [JsonProperty(Required = Required.Always)]
            public uint Index = 0;

            [JsonProperty(Required = Required.Always)]
            public double Speed
            {
                get => _speedImpl;
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be less than 0!");
                    }

                    if (value > 1)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be greater than 1!");
                    }

                    _speedImpl = value;
                }
            }

            [JsonProperty(Required = Required.Always)]
            public bool Clockwise = true;

            public RotateIndex(uint aIndex, double aSpeed, bool aClockwise)
            {
                Index = aIndex;
                Speed = aSpeed;
                Clockwise = aClockwise;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public List<RotateIndex> Speeds;

        public RotateCmd(uint aDeviceIndex, List<RotateIndex> aSpeeds, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Speeds = aSpeeds;
            MessageVersioningVersion = 1;
        }
    }

    public class LinearCmd : ButtplugDeviceMessage
    {
        public class VectoreIndex
        {
            private double _speedImpl = 0;

            private double _positionImpl = 0;

            [JsonProperty(Required = Required.Always)]
            public uint Index = 0;

            [JsonProperty(Required = Required.Always)]
            public double Speed
            {
                get => _speedImpl;
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be less than 0!");
                    }

                    if (value > 1)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be greater than 1!");
                    }

                    _speedImpl = value;
                }
            }

            [JsonProperty(Required = Required.Always)]
            public double Position
            {
                get => _positionImpl;
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be less than 0!");
                    }

                    if (value > 1)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be greater than 1!");
                    }

                    _positionImpl = value;
                }
            }

            public VectoreIndex(uint aIndex, double aSpeed, double aPosition)
            {
                Index = aIndex;
                Speed = aSpeed;
                Position = aPosition;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public List<VectoreIndex> Vectors;

        public LinearCmd(uint aDeviceIndex, List<VectoreIndex> aVectors, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Vectors = aVectors;
            MessageVersioningVersion = 1;
        }
    }

    public class ConstrictCmd : ButtplugDeviceMessage
    {
        public class ConstrictIndex
        {
            private double _levelImpl = 0;

            [JsonProperty(Required = Required.Always)]
            public uint Index = 0;

            [JsonProperty(Required = Required.Always)]
            public double Level
            {
                get => _levelImpl;
                set
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be less than 0!");
                    }

                    if (value > 1)
                    {
                        throw new ArgumentException("VibrateCmd Speed cannot be greater than 1!");
                    }

                    _levelImpl = value;
                }
            }

            public ConstrictIndex(uint aIndex, double aLevel)
            {
                Index = aIndex;
                Level = aLevel;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public List<ConstrictIndex> Levels;

        public ConstrictCmd(uint aDeviceIndex, List<ConstrictIndex> aLevels, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            Levels = aLevels;
            MessageVersioningVersion = 1;
        }
    }

    public class StopDeviceCmd : ButtplugDeviceMessage
    {
        public StopDeviceCmd(uint aDeviceIndex, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
        }
    }

    public class StopAllDevices : ButtplugMessage
    {
        public StopAllDevices(uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId)
        {
        }
    }

    public class StartAccelerometerCmd : ButtplugDeviceMessage
    {
        public StartAccelerometerCmd(uint aDeviceIndex, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            MessageVersioningVersion = 1;
        }
    }

    public class StopAccelerometerCmd : ButtplugDeviceMessage
    {
        public StopAccelerometerCmd(uint aDeviceIndex, uint aId = ButtplugConsts.DefaultMsgId)
            : base(aId, aDeviceIndex)
        {
            MessageVersioningVersion = 1;
        }
    }

    public class AccelerometerData : ButtplugDeviceMessage, IButtplugMessageOutgoingOnly
    {
        [JsonProperty(Required = Required.Always)]
        public int AccelerometerX = 0;

        [JsonProperty(Required = Required.Always)]
        public int AccelerometerY = 0;

        [JsonProperty(Required = Required.Always)]
        public int AccelerometerZ = 0;

        public AccelerometerData(int aX, int aY, int aZ, uint aDeviceIndex, uint aId = ButtplugConsts.SystemMsgId)
             : base(aId, aDeviceIndex)
        {
            AccelerometerX = aX;
            AccelerometerY = aY;
            AccelerometerZ = aZ;
            MessageVersioningVersion = 1;
        }
    }
}
