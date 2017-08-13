using Buttplug.Apps.XInputInjector.Interface;
using Buttplug.Apps.XInputInjector.Payload;
using Buttplug.Components.Controls;
using Buttplug.Core.Messages;
using Buttplug.Server;
using EasyHook;
using JetBrains.Annotations;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading.Tasks;

namespace Buttplug.Apps.XInputInjector.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        [NotNull]
        private readonly ButtplugServer _bpServer;

        [NotNull]
        private readonly Logger _log;

        private IpcServerChannel _xinputHookServer;
        private string _channelName;
        private List<ButtplugDeviceInfo> _devices;

        public MainWindow()
        {
            InitializeComponent();

            ButtplugTab.GetLogControl().MaxLogs = 10000;

            ButtplugTab.SetServerDetails("XInput Hijack Server", 0);
            _bpServer = ButtplugTab.GetServer();
            _log = LogManager.GetCurrentClassLogger();
            ButtplugXInputInjectorInterface.VibrationCommandReceived += OnVibrationCommand;
            ButtplugXInputInjectorInterface.VibrationPingMessageReceived += OnVibrationPingMessage;
            ButtplugXInputInjectorInterface.VibrationExceptionReceived += OnVibrationException;
            Task.FromResult(_bpServer.SendMessage(new RequestServerInfo("Buttplug XInput Injector")));
            var processTab = new ProcessTab(_bpServer);
            processTab.ProcessAttachRequested += OnAttachRequested;
            ButtplugTab.SetApplicationTab("Processes", processTab);
            ButtplugTab.AddDevicePanel(_bpServer);
            ButtplugTab.SelectedDevicesChanged += OnSelectedDevicesChanged;
        }

        private void OnSelectedDevicesChanged(object aObj, List<ButtplugDeviceInfo> aDevices)
        {
            _devices = aDevices;
        }

        private void OnVibrationException(object aObj, Exception aEx)
        {
            _log.Error($"Remote Exception: {aEx}");
        }

        private void OnVibrationPingMessage(object aObj, string aMsg)
        {
            _log.Info($"Remote Ping Message: {aMsg}");
        }

        private async void OnVibrationCommand(object aObj, Vibration aVibration)
        {
            foreach (var device in _devices)
            {
                // For now, we only handle devices that can take vibration messages.
                if (!device.Messages.Contains(typeof(SingleMotorVibrateCmd).Name))
                {
                    continue;
                }
                await _bpServer.SendMessage(new SingleMotorVibrateCmd(device.Index,
                    ((aVibration.LeftMotorSpeed + aVibration.RightMotorSpeed) / 2.0) / 65535.0));
            }
        }

        private void OnAttachRequested(object aObject, int aProcessId)
        {
            _xinputHookServer = RemoteHooking.IpcCreateServer<ButtplugXInputInjectorInterface>(
                ref _channelName,
                WellKnownObjectMode.Singleton);
            _log.Info($"Beginning process injection on {aProcessId}...");
            RemoteHooking.Inject(
                aProcessId,
                InjectionOptions.Default,
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ButtplugXInputInjectorPayload).Assembly.Location), "ButtplugXInputInjectorPayload.dll"),
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ButtplugXInputInjectorPayload).Assembly.Location), "ButtplugXInputInjectorPayload.dll"),
                // the optional parameter list...
                _channelName);
            _log.Info($"Finished process injection on {aProcessId}...");
        }
    }
}