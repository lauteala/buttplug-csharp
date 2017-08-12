using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Buttplug.Apps.XInputInjector.Interface
{
    [Serializable]
    public struct Vibration
    {
        public ushort LeftMotorSpeed;
        public ushort RightMotorSpeed;
    }


    public class ButtplugXInputInjectorInterface : MarshalByRefObject
    {
        // This will be used as a singleton in the IPC Server, and we should only ever have one process hooked 
        // with this interface. Just make the EventHandler static so we can attach as needed from anywhere.
        public static event EventHandler<Vibration> VibrationCommandReceived;
        public static event EventHandler<Exception> VibrationExceptionReceived;
        public static event EventHandler<string> VibrationPingMessageReceived;

        public void Report(Int32 aPid, Queue<Vibration> aCommands)
        {
            foreach (var command in aCommands)
            {
                VibrationCommandReceived?.Invoke(this, command);
            }
        }

        public void ReportError(Int32 aPid, Exception aEx)
        {
            VibrationExceptionReceived?.Invoke(this, aEx);
        }

        public bool Ping(Int32 aPid, string aMsg)
        {
            if (aMsg.Length > 0)
            {
                VibrationPingMessageReceived?.Invoke(this, aMsg);
            }
            return true;
        }
    }
}