using System.Runtime.InteropServices;
using System.Text;

namespace MW
{
    public static class Startup
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

        struct RECT { public int Left, Top, Right, Bottom; }

        public static void Run()
        {
            // Read project file
            if (Env.IsDebug)
            {
                Project.TryLoadDefault();
            }

            // Center
            CenterWindow();
        }

        private static void CenterWindow()
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
    }
}
