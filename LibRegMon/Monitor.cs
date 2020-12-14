using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Microsoft.Win32;

namespace RegistryValueMonitor
{
    public class Monitor
    {

        private readonly EventLog eventLog;

        private readonly Dictionary<string, object> watchList;

        private int interval = 1000;

        private int eventId = 1;

        public Monitor(String serviceName, EventLog eventLog)
        {
            this.eventLog = eventLog;
            this.watchList = new Dictionary<string, object>();

            Log("RegistryValueMonitor started", EventLogEntryType.Information);
            ReadSettings(serviceName);
        }

        public void ReadSettings(string regKey)
        {
            RegistryKey key = FromFullPath(@"HKLM\SYSTEM\CurrentControlSet\Services\" + regKey + @"\WatchList");
            foreach (string watchItemKey in key.GetValueNames())
            {
                object watchItemValue = key.GetValue(watchItemKey);
                Log("Adding key to monitor: " + watchItemKey + " -> " + watchItemValue, EventLogEntryType.Information);
                watchList.Add(watchItemKey, watchItemValue);
            }
            key.Close();

            key = FromFullPath(@"HKLM\SYSTEM\CurrentControlSet\Services\RegistryValueMonitor");
            if (key.GetValueNames().Contains("Interval") && key.GetValueKind("Interval").Equals(RegistryValueKind.DWord))
            {
                interval = (int)key.GetValue("Interval");
            }
        }

        public void Start()
        {
            Timer timer = new Timer();
            timer.Interval = interval;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Enabled = true;
            timer.Start();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            foreach (var watchItem in watchList)
            {
                string[] split = watchItem.Key.Split('\\');
                string name = split.Last();
                string keyPath = watchItem.Key.Substring(0, watchItem.Key.Length - name.Length);
                RegistryKey key = FromFullPath(keyPath);
                object oldValue = key.GetValue(name);
                if (!watchItem.Value.Equals(oldValue))
                {
                    Log(string.Format("Value change detected: {0} was {1}, but must be {2}. Corrected.", watchItem.Key, watchItem.Value, oldValue), EventLogEntryType.Information);
                    key.SetValue(name, watchItem.Value);
                }

            }
            Log("Monitoring the System", EventLogEntryType.Information);
        }

        private static RegistryKey FromFullPath(string path)
        {
            RegistryKey root = null;

            string[] split = path.Split(new char[] { '\\' }, 2);
            string rootName = split[0];
            string relativePath = split[1];

            if (rootName.Equals("HKCR") || rootName.Equals("HKEY_CLASSES_ROOT"))
            {
                root = Registry.ClassesRoot;
            }
            else if (rootName.Equals("HKCU") || rootName.Equals("HKEY_CURRENT_USER"))
            {
                root = Registry.CurrentUser;
            }
            else if (rootName.Equals("HKLM") || rootName.Equals("HKEY_LOCAL_MACHINE"))
            {
                root = Registry.LocalMachine;
            }
            else if (rootName.Equals("HKU") || rootName.Equals("HKEY_USERS"))
            {
                root = Registry.CurrentUser;
            }
            else if (rootName.Equals("HKCC") || rootName.Equals("HKEY_CURRENT_CONFIG"))
            {
                root = Registry.CurrentUser;
            }

            return root == null ? null : root.CreateSubKey(relativePath);
        }

        private void Log(string msg, EventLogEntryType entryType)
        {
            if (eventLog != null)
            {
                try
                {
                    eventLog.WriteEntry(msg, entryType, eventId++);
                }
                catch (Exception ignored) { }
            }
        }
    }

}
