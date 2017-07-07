using System.Collections.Generic;
using JetBrains.Annotations;
using NLog;
using NLog.Targets;

namespace ButtplugCrashReporter
{
    [Target("ButtplugCrashReporterLogger")]
    public sealed class ButtplugCrashReporterLogger : TargetWithLayoutHeaderAndFooter
    {
        [NotNull]
        private readonly List<string> _logs = new List<string>();

        protected override void Write(LogEventInfo aLogEvent)
        {
            _logs.Add(Layout.Render(aLogEvent));
        }

        public string GetLogsAsString()
        {
            return string.Join("\n", _logs);
        }
    }
}
