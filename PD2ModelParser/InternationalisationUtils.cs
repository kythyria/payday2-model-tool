using System.Globalization;

namespace PD2ModelParser
{
    public static class InternationalisationUtils
    {
        public static float ParseFloat(this string str)
        {
            // Clean up the complete mess that was number formatting, where a modelscript
            // from one locale might not work in another.
            // Here we support ISO-8601 5.3.1.3 where numbers are to be formatted as follows:
            //
            // Numbers are separated at the thousands with spaces, and decimals are separated
            // either by full stops or commas (the latter being preferred). Full stops and
            // commas shall never be used as a thousands separator.
            //
            // We loosely convert these to a valid British formatting (by removing all the spaces
            // and converting the comma to a full stop) and then parse it with the invariant culture.
            //
            // This should put a stop to locale-related troubles.
            str = str.Trim();
            str = str.Replace(',', '.');
            str = str.Replace(" ", "");
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}
