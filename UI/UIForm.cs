using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using static WinFormsApp1.ProcessWatcher;

namespace WinFormsApp1
{
    public partial class UIForm : Form
    {
        private static string CONNECTING = "Connecting to CEC port";
        private static string CONNECTED = "Controlling CEC port";
        private static string DISCONNECTED = "Disconnected from CEC port";
        private static string FAILURE = "No CEC adaptor installed";

        private bool interceptClose = true;
        private CecClient client;
        private ProcessWatcher watcher;

        public UIForm()
        {
            InitializeComponent();

            client = new CecClient();
            watcher = new ProcessWatcher(CecActions.PROCESS_CEC_ABLE);

            this.notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            this.FormClosing += Form1_FormClosing;
            this.Resize += Form1_Resize;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Register on load so nothing happens before
            this.watcher.ProcessOpened += Watcher_ProcessOpened;
            this.watcher.ProcessClosed += Watcher_ProcessClosed;

            // Start hidden
            this.Visible = false;
            this.ShowInTaskbar = false;
        }

        #region Connections

        private void Disconnect(string? reason)
        {
            if (!String.IsNullOrEmpty(reason))
            {
                WriteToOutput(reason);
            }

            client.Close();
            lblStatus.Text = DISCONNECTED;
            statusStripMenuItem.Text = DISCONNECTED;
        }

        private void Connect(string mappingName, ReadOnlyDictionary<string, Action> maps)
        {
            Disconnect(null);

            lblStatus.Text = CONNECTING;
            statusStripMenuItem.Text = CONNECTING;
            Refresh();

            Task.Run(() =>
            {
                if (!client.Connect(maps))
                {
                    Invoke(() => {
                        lblStatus.Text = FAILURE;
                        statusStripMenuItem.Text = FAILURE;
                        WriteToOutput("Can not connect.");
                    });
                    return;
                }

                Invoke(() =>
                {
                    lblStatus.Text = CONNECTED;
                    statusStripMenuItem.Text = CONNECTED;
                    WriteToOutput($"Using {mappingName} mapping.");
                });
            });

        }

        #endregion

        #region Helpers

        private void WriteToOutput(string message)
        {
            txtOutput.AppendText(message);
            txtOutput.AppendText(Environment.NewLine);
            txtOutput.ScrollToCaret();
        }

        #endregion

        #region Program Changes

        private void Watcher_ProcessOpened(object? sender, ProcessWatcherEventArgs e)
        {
            Invoke(() =>
            {
                if (e.StartupCheck)
                {
                    WriteToOutput($"{e.ProcessName} is running, already has CEC port.");
                    lblStatus.Text = DISCONNECTED;
                    statusStripMenuItem.Text = DISCONNECTED;
                }
                else
                {
                    Disconnect($"Process {e.ProcessName} has started, so disconnecting.");
                }
            });
        }

        private void Watcher_ProcessClosed(object? sender, ProcessWatcherEventArgs e)
        {
            Invoke(() =>
            {
                var p = ProcessUtils.GetForegroundProcess();

                if (e.StartupCheck)
                {
                    WriteToOutput("Initializing.");
                }
                else
                {
                    WriteToOutput($"Process {e.ProcessName} has closed, so retaking control.");

                }
                
                var m = CecActions.GetKeyMaps(p?.ProcessName.ToLower() ?? String.Empty);
                Connect(m.Item1, m.Item2);
            });
        }

        #endregion

        #region Tray Events

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = interceptClose;
        }

        private void notifyIcon_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            interceptClose = false;
            Application.Exit();
        }

        #endregion
    }
}