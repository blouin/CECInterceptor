using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace WinFormsApp1
{
    internal sealed class ProcessWatcher
    {
        internal event EventHandler<ProcessWatcherEventArgs>? ProcessClosed;
        internal event EventHandler<ProcessWatcherEventArgs>? ProcessOpened;

        private System.Threading.Timer timer;
        private string[] processes;
        private bool? controllerHasControl;
        private string lastProcessName;
        private object lockTimer;

        internal ProcessWatcher(string[] processes)
        {
            this.processes = processes;
            this.timer = new System.Threading.Timer(ProcessTimer, null, 1000, 2000);
            this.lockTimer = new object();
            this.lastProcessName = String.Empty;
        }

        private void ProcessTimer(object? o)
        {
            lock (lockTimer)
            {
                var p = Process.GetProcesses().FirstOrDefault(i => processes.Contains(i.ProcessName.ToLower()));
                if (p != null)
                {
                    // One process has CEC control
                    if (!controllerHasControl.HasValue || controllerHasControl.Value)
                    {
                        var first = !controllerHasControl.HasValue;
                        lastProcessName = p.ProcessName.ToLower();
                        controllerHasControl = false;

                        if (ProcessOpened != null)
                        {
                            ProcessOpened(this, new ProcessWatcherEventArgs { ProcessName = lastProcessName, StartupCheck = first });
                        }
                    }
                }
                else
                {
                    // No process has CEC control
                    if (!controllerHasControl.HasValue || !controllerHasControl.Value)
                    {
                        var first = !controllerHasControl.HasValue;
                        controllerHasControl = true;
                        if (ProcessClosed != null)
                        {
                            ProcessClosed(this, new ProcessWatcherEventArgs { ProcessName = lastProcessName, StartupCheck = first });
                        }
                    }
                }
            }
        }
        internal class ProcessWatcherEventArgs : EventArgs
        {
            internal string ProcessName { get; set; } = String.Empty;
            internal bool StartupCheck { get; set; } = false;
        }
    }
}
