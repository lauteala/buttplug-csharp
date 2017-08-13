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

        private ProcessInfoList _processList = new ProcessInfoList();
        public event EventHandler<int> ProcessAttachRequested;

        public ProcessTab(ButtplugServer aServer)
        {
            InitializeComponent();
            ProcessListBox.ItemsSource = _processList;
            EnumProcesses();
            
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
            ProcessAttachRequested?.Invoke(this, process[0].Id);
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EnumProcesses();
        }
    }
}