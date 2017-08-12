using System.Windows;

namespace Buttplug.Apps.XInputInjector.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            
            ButtplugTab.GetLogControl().MaxLogs = 10000;

            ButtplugTab.SetServerDetails("XInput Hijack Server", 0);
            ButtplugTab.SetApplicationTab("Processes", new ProcessTab(ButtplugTab.GetServer()));

        }

    }
}