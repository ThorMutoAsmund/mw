using System.Runtime.InteropServices;
using System.Text;

namespace MW
{
    public static class WindowHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaxWindow, ref CONSOLE_FONT_INFOEX info);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaxWindow, ref CONSOLE_FONT_INFOEX info);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        struct COORD { public short X, Y; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct CONSOLE_FONT_INFOEX
        {
            public uint cbSize;
            public uint nFont;
            public COORD dwFontSize;   // X=width, Y=height (pixels)
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        const int STD_OUTPUT_HANDLE = -11;

        public static void CenterWindow()
        {
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd == IntPtr.Zero) return;

            GetWindowRect(hWnd, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            GetClientRect(GetDesktopWindow(), out RECT screen);
            int screenWidth = screen.Right - screen.Left;
            int screenHeight = screen.Bottom - screen.Top;

            int newX = (screenWidth - width) / 2;
            int newY = (screenHeight - height) / 2;

            MoveWindow(hWnd, newX, newY, width, height, true);
        }

        public static void SetFont(string face, short heightPx)
        {
            var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
            var info = new CONSOLE_FONT_INFOEX { cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>() };

            if (!GetCurrentConsoleFontEx(hOut, false, ref info)) return;

            info.FaceName = face;           // e.g., "Consolas" or "Lucida Console"
            info.dwFontSize.Y = heightPx;   // pixel height
                                            // optional: set width (X) or weight/family if you need

            SetCurrentConsoleFontEx(hOut, false, ref info);
        }
    }
}
