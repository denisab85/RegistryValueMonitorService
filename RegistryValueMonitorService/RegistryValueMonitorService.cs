using RegistryValueMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RegistryValueMonitorService
{
    public partial class RegistryValueMonitorService : ServiceBase
    {
        Monitor monitor;

        public RegistryValueMonitorService()
        {
            InitializeComponent();
            try
            {
                eventLog = new EventLog();
                if (!EventLog.SourceExists("RegistryValueMonitor"))
                {
                    EventLog.CreateEventSource("RegistryValueMonitor", "RegistryValueMonitorLog");
                }
                eventLog.Source = "RegistryValueMonitor";
                eventLog.Log = "RegistryValueMonitorLog";
            }
            catch (Exception ignored) { }
        }

        protected override void OnStart(string[] args)
        {
            monitor = new Monitor("RegistryValueMonitor", eventLog);
            monitor.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
