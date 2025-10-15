using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Parsing
{
    public static class Constants
    {
        public static string SongVarName = "song";
        public static string OutputVarName = "out";
        public static string HeadVarName = "head";
        public static string LastVarName = "last";

        // Default values
        public static double BMPDefault = 120.0;

        // Settings
        public static readonly string BMP = "bmp";
        public static readonly string JumpPoints = "jump";

        public static string TimeKey = nameof(AstType.Time).ToLowerInvariant();
        public static string SampleKey = nameof(AstType.Sample).ToLowerInvariant();
        public static string ContainerKey = nameof(AstType.Container).ToLowerInvariant();
    }
}
