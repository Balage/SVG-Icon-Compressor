using System.Text;

namespace WebSymbolsFontGenerator.Svg.Extensions
{
    public static class StringExtensions
    {
        public static string NumberJoin(this string[] list, bool compress)
        {
            if (list.Length == 0)
                return "";

            if (compress)
            {
                var sb = new StringBuilder();
                sb.Append(list[0]);

                for (int i = 1; i < list.Length; i++)
                {
                    if (!(compress && list[i].Length != 0 && list[i][0] == '-'))
                        sb.Append(' ');

                    sb.Append(list[i]);
                }

                return sb.ToString();
            }
            else
            {
                return string.Join(" ", list);
            }
        }

        public static string NormalizePath(this string path)
        {
            path = path.Replace("\n", " ");
            path = path.Replace("\r", " ");
            path = path.Replace("\t", " ");
            return path;
        }
    }
}