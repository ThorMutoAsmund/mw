using System.Runtime.InteropServices;
using System.Text;

namespace MW
{
    public static class WinMMHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WAVEOUTCAPS
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwFormats;
            public ushort wChannels;
            public ushort wReserved1;
            public uint dwSupport;
        }

        static class WinMM
        {
            [DllImport("winmm.dll", CharSet = CharSet.Auto)]
            public static extern int waveOutGetDevCaps(UIntPtr hwo, out WAVEOUTCAPS pwoc, uint cbwoc);
        }

        public static string GetWaveOutName(int deviceIndex)
        {
            var ok = WinMM.waveOutGetDevCaps((UIntPtr)(uint)deviceIndex, out var caps,
                                             (uint)Marshal.SizeOf<WAVEOUTCAPS>()) == 0;
            return ok ? caps.szPname : $"waveOut device {deviceIndex}";
        }
    }
}
