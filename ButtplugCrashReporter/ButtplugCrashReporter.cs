using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using NLog;
using NLog.Config;
using SharpRaven;
using SharpRaven.Data;

namespace ButtplugCrashReporter
{
    public class ButtplugCrashReporter
    {
        [NotNull]
        private readonly RavenClient _ravenClient;
        [NotNull]
        private readonly ButtplugCrashReporterLogger _logTarget;
        private bool _sentCrashLog;

        public ButtplugCrashReporter(string aRavenClientUrl)
        {
            _ravenClient = new RavenClient(aRavenClientUrl);
            var c = LogManager.Configuration ?? new LoggingConfiguration();
            _logTarget = new ButtplugCrashReporterLogger();
            c.AddTarget("ButtplugCrashReportLogger", _logTarget);
            c.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _logTarget));
            LogManager.Configuration = c;
            // Cover all of the possible bases for WPF failure
            // http://stackoverflow.com/questions/12024470/unhandled-exception-still-crashes-application-after-being-caught
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            // Null check application, otherwise test bringup for GUI tests will fail
            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            }
        }

        private void SendExceptionToSentry(Exception aEx)
        {
            if (_sentCrashLog)
            {
                return;
            }

            _sentCrashLog = true;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException -= CurrentOnDispatcherUnhandledException;
                if (Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.UnhandledException -= DispatcherOnUnhandledException;
                }
            }

            var result = MessageBox.Show("An error was encountered! Do you want to report this to the developers?", "Error encountered", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            aEx.Data.Add("LogMessages", string.Join("\n", _logTarget.GetLogsAsString()));

            _ravenClient.Capture(new SentryEvent(aEx));
        }

        private void DispatcherOnUnhandledException(object aObj, DispatcherUnhandledExceptionEventArgs aEx)
        {
            SendExceptionToSentry(aEx.Exception);
        }

        private void CurrentDomainOnUnhandledException(object aObj, UnhandledExceptionEventArgs aEx)
        {
            SendExceptionToSentry(aEx.ExceptionObject as Exception);
        }

        private void CurrentOnDispatcherUnhandledException(object aObj, DispatcherUnhandledExceptionEventArgs aEx)
        {
            SendExceptionToSentry(aEx.Exception);
        }
    }
}
