using Buttplug.Apps.XInputInjector.Interface;
using Buttplug.Apps.XInputInjector.Payload;
using EasyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using Buttplug.Core.Messages;
using Buttplug.Server;
using NLog;

namespace Buttplug.Apps.XInputInjector.GUI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>

    public partial class ProcessTab
    {
        public class ProcessInfo
        {
            public String FileName;
            public Int32 Id;

            public override string ToString()
            {
                var f = System.IO.Path.GetFileNameWithoutExtension(FileName);
                return $"{f} ({Id})";
            }
        }

        public class ProcessInfoList : ObservableCollection<ProcessInfo>
        {
        }

        private ButtplugServer _bpServer;
        private IpcServerChannel _xinputHookServer;
        private string _channelName;
        private ProcessInfoList _processList = new ProcessInfoList();
        private Logger _log;

        public ProcessTab(ButtplugServer aServer)
        {
            InitializeComponent();
            ProcessListBox.ItemsSource = _processList;
            EnumProcesses();
            _bpServer = aServer;
            _log = LogManager.GetCurrentClassLogger();
            ButtplugXInputInjectorInterface.VibrationCommandReceived += onVibrationCommand;
            ButtplugXInputInjectorInterface.VibrationPingMessageReceived += onVibrationPingMessage;
            ButtplugXInputInjectorInterface.VibrationExceptionReceived += onVibrationException;
            _bpServer.SendMessage(new RequestServerInfo("Buttplug XInput Injector"));
            _bpServer.SendMessage(new StartScanning());
        }

        private void onVibrationException(object aObj, Exception aEx)
        {
            _log.Error(aEx);   
        }

        private void onVibrationPingMessage(object aObj, string aMsg)
        {
            _log.Info($"Remote Ping Message: {aMsg}");
        }

        private void onVibrationCommand(object aObj, Vibration aVibration)
        {
            _bpServer.SendMessage(new SingleMotorVibrateCmd(1,
                ((aVibration.LeftMotorSpeed + aVibration.RightMotorSpeed) / 2.0) / 65535.0));
        }

        private void EnumProcesses()
        {
            _processList.Clear();
            foreach (var Proc in from proc in Process.GetProcesses() orderby proc.ProcessName select proc)
            {
                try
                {
                    _processList.Add(new ProcessInfo
                    {
                        FileName = Proc.MainModule.FileName,
                        Id = Proc.Id
                    });
                }
                catch
                {
                }
            }
        }

        private void AttachButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var process = ProcessListBox.SelectedItems.Cast<ProcessInfo>().ToList();
            if (process.Count != 1)
            {
                return;
            }

            _xinputHookServer = RemoteHooking.IpcCreateServer<ButtplugXInputInjectorInterface>(
                ref _channelName,
                WellKnownObjectMode.Singleton);
            Debug.WriteLine("Injecting!");
            RemoteHooking.Inject(
                process[0].Id,
                InjectionOptions.Default,
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ButtplugXInputInjectorPayload).Assembly.Location), "ButtplugXInputInjectorPayload.dll"),
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ButtplugXInputInjectorPayload).Assembly.Location), "ButtplugXInputInjectorPayload.dll"),
                // the optional parameter list...
                _channelName);
            Debug.WriteLine("Finished injecting!");
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _bpServer.SendMessage(new StopScanning());
            EnumProcesses();
        }
    }
}