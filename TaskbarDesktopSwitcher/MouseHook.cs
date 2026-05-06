using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TaskbarDesktopSwitcher
{
    public class MouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEWHEEL = 0x020A;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        private LowLevelMouseProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private DateTime _lastWheelEvent = DateTime.MinValue;
        private const int DebounceMilliseconds = 250;

        public event EventHandler<WheelEventArgs>? WheelScrolled;

        public MouseHook()
        {
            _proc = new LowLevelMouseProc(HookCallback);
        }

        public void Start()
        {
            if (_hookID == IntPtr.Zero)
            {
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                {
                    var curModule = curProcess.MainModule;
                    if (curModule != null)
                    {
                        _hookID = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
                    }
                }
            }
        }

        public void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEWHEEL)
            {
                var now = DateTime.Now;
                if ((now - _lastWheelEvent).TotalMilliseconds < DebounceMilliseconds)
                {
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }
                _lastWheelEvent = now;

                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int delta = (short)(hookStruct.mouseData >> 16);

                var args = new WheelEventArgs(delta);
                WheelScrolled?.Invoke(this, args);

                if (args.Handled)
                {
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        ~MouseHook()
        {
            Stop();
        }
    }

    public class WheelEventArgs : EventArgs
    {
        public int Delta { get; }
        public bool Handled { get; set; }

        public WheelEventArgs(int delta)
        {
            Delta = delta;
            Handled = false;
        }
    }
}