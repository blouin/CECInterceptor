using CecSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal class CecClient : CecCallbackMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private ReadOnlyDictionary<string, Action> keyMaps;
        private LibCecSharp lib;

        public CecClient()
        {
            var config = new LibCECConfiguration();
            config.DeviceTypes.Types[0] = CecDeviceType.RecordingDevice;
            config.DeviceName = "CEC Interceptor";
            config.ClientVersion = LibCECConfiguration.CurrentVersion;

            lib = new LibCecSharp(this, config);
            lib.InitVideoStandalone();

            keyMaps = new ReadOnlyDictionary<string, Action>(new Dictionary<string, Action>());

            Console.WriteLine("CEC Client created - libCEC version " + lib.VersionToString(config.ServerVersion));
        }

        public bool Connect(ReadOnlyDictionary<string, Action> keyMaps)
        {
            this.keyMaps = keyMaps;

            CecAdapter[] adapters = lib.FindAdapters(string.Empty);
            if (adapters.Length > 0)
            {
                var c = lib.Open(adapters[0].ComPort, 10000);
                Console.WriteLine("CEC Client " + (c ? "connected" : "can not connect"));
                return c;
            }
            else
            {
                Console.WriteLine("Did not find any CEC adapters");
                return false;
            }
        }

        public void Close()
        {
            lib.Close();
            Console.WriteLine("CEC Client closed");
        }

        public override int ReceiveCommand(CecCommand command)
        {
            return 1;
        }

        public override int ReceiveKeypress(CecKeypress key)
        {
            return 1;
        }

        public override int ReceiveLogMessage(CecLogMessage message)
        {
            var m = keyMaps.Select(i =>i.Key).FirstOrDefault(i => message.Message.Contains(i));
            if (m != null)
            {
                keyMaps[m]();
            }

            // Debugging
            if (message.Message.Contains("key released:"))
            {
                Debug.WriteLine(message.Message);
            }

            return 1;
        }

    }
}
