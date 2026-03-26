using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WinFormsApp1
{
    internal static class CecActions
    {
        internal static string[] PROCESS_CEC_ABLE = new string[] { "kodi" };

        private static string DEFAULT_MAP = "batocera";
        private static string VLC_MAP = "vlc";

        private static Dictionary<string, Dictionary<string, Action>> Maps = new Dictionary<string, Dictionary<string, Action>>
        {
            { DEFAULT_MAP,
              new Dictionary<string, Action>
                {
                    { "key released: up", () => SendKey("{UP}") },
                    { "key released: down", () => SendKey("{DOWN}") },
                    { "key released: left",  () => SendKey("{LEFT}") },
                    { "key released: right",  () => SendKey("{RIGHT}") },
                    { "key released: select",  () => SendMouse(MouseButtons.Left) },
                    { "key released: exit", () => SendMouse(MouseButtons.Right) },
                    { "key released: F1 (blue)", () => Process.Start(@"C:\Program Files\Kodi\kodi.exe")  },
                    { "key released: F2 (red)", () => SetForeground("emulationstation") },
                    { "key released: F3 (green)", () =>  SendKey("ENTER") }, // Enter key does not work, but mapped anyways
                    { "key released: F4 (yellow)", () => Process.Start(@"C:\Program Files (x86)\Steam\Steam.exe", "-start steam://open/bigpicture")  },
                }
            },

            { VLC_MAP,
              new Dictionary<string, Action>
                {
                    { "key released: left",  () => SendKey("{LEFT}") },
                    { "key released: right",  () => SendKey("{RIGHT}") },
                    { "key released: select",  () => SendKey(" ") },
                }
            }
        };

        public static (string, ReadOnlyDictionary<string, Action>) GetKeyMaps(string process)
        {
            process = String.IsNullOrEmpty(process) ? DEFAULT_MAP : process.ToLower();
            if (Maps.ContainsKey(process))
            {
                return (process, new ReadOnlyDictionary<string, Action>(Maps[process]));
            }

            return (DEFAULT_MAP, new ReadOnlyDictionary<string, Action>(Maps[DEFAULT_MAP]));
        }

        #region Helpers

        private static void SendKey(string key)
        {
            SendKeys.SendWait(key);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private static void SendMouse(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                uint X = (uint)Cursor.Position.X;
                uint Y = (uint)Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
            }
            else if (button == MouseButtons.Right)
            {
                uint X = (uint)Cursor.Position.X;
                uint Y = (uint)Cursor.Position.Y;
                mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        private static void SetForeground(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes != null && processes.Length > 0)
            {
                SetForegroundWindow(processes[0].MainWindowHandle);
            }
        }

        #endregion
    }
}
