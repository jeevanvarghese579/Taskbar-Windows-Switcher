using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace TaskbarDesktopSwitcher
{
    /// <summary>
    /// Helper class to detect if the mouse cursor is positioned over the Windows taskbar.
    /// Uses Windows API calls to find the taskbar window and check cursor position.
    /// </summary>
    public static class TaskbarHelper
    {
        // Windows API function declarations
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool PtInRect(ref RECT lprc, POINT pt);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Structures for Windows API
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public bool Contains(POINT point)
            {
                return point.X >= Left && point.X <= Right &&
                       point.Y >= Top && point.Y <= Bottom;
            }
        }

        // Windows API constant for getting screen height
        private const int SM_CYSCREEN = 1;
        private const int TASKBAR_DETECTION_ZONE = 30; // pixels from bottom of screen

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        /// <summary>
        /// Checks if the mouse cursor is within 30 pixels of the screen bottom.
        /// This is a simple and reliable method that works with both Windows 10 and 11.
        /// </summary>
        /// <returns>True if mouse is near screen bottom, false otherwise.</returns>
        public static bool IsMouseOverTaskbar()
        {
            try
            {
                // Get current cursor position
                if (!GetCursorPos(out POINT cursorPos))
                {
                    return false;
                }

                // Get screen height
                int screenHeight = GetSystemMetrics(SM_CYSCREEN);
                
                // Check if cursor is within TASKBAR_DETECTION_ZONE pixels of screen bottom
                // In Windows, Y coordinate increases downward (0 at top, screenHeight at bottom)
                if (cursorPos.Y >= screenHeight - TASKBAR_DETECTION_ZONE)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Silently handle exceptions to prevent crashes
                return false;
            }
        }

    }
}