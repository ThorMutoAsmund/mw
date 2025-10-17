using System.Globalization;

namespace MW.Helpers
{
    public static class Formatting
    {
        public static string FromSeconds(this double value) =>
            value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
