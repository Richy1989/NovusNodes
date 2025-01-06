namespace NovusNodo.Helper
{
    public class Helper
    {
        /// <summary>
        /// Converts a System.Drawing.Color to a CSS color string.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The CSS color string.</returns>
        public static string ConvertColorToCSSColor(System.Drawing.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
