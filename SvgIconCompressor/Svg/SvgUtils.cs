using SvgIconCompressor.Svg.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebSymbolsFontGenerator.Svg.Extensions;
using WebSymbolsFontGenerator.Svg.Models;

namespace WebSymbolsFontGenerator.Svg
{
    public static class SvgUtils
    {
        // Parse float locale independenty
        public static float ParseFloat(string input, float defaultValue = 0.0f)
        {
            return float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float value)
                ? value
                : defaultValue;
        }

        // Print float locale independently
        public static string NumberToString(float number, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0)
                throw new ArgumentException("Decimal places must be at least 0.");

            var temp = number.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);

            if (decimalPlaces > 0)
            {
                // Remove decimal zeros
                temp = temp.TrimEnd('0').TrimEnd('.');

                // Can omit starting 0
                if (temp.StartsWith("0."))
                {
                    temp = temp[1..];
                }
            }

            return temp;
        }

        enum ExplodeScanMode
        {
            None,
            Number,
            Letter
        }

        public static string[] ExplodePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new string[0];

            path = path.Trim();

            var elements = new List<string>();

            int startIndex = 0;
            int index = 0;
            var mode = ExplodeScanMode.None;

            while (index < path.Length)
            {
                char current = path[index];

                var isWhiteSpace = current == ' ' || current == ',';
                var isNumber = ('0' <= current && current <= '9') || current == '.';
                var isMinus = current == '-';

                if (mode == ExplodeScanMode.None)
                {
                    if (!isWhiteSpace)
                    {
                        mode = isNumber || isMinus
                            ? ExplodeScanMode.Number
                            : ExplodeScanMode.Letter;

                        startIndex = index;
                    }
                }
                else if (mode == ExplodeScanMode.Number)
                {
                    if (current == 'e') // this is also part of the number
                    {
                        // "#e-#"
                        // Ended by: comma, space or another letter

                        index++;
                        if (path[index] != '-')
                            throw new Exception("Invalid number format.");

                        // ...continue scanning for the rest of the numbers. Exit criteria is the same.
                    }
                    else
                    {
                        // Ended by: white-space, comma, letter, another minus
                        if (isWhiteSpace)
                        {
                            mode = ExplodeScanMode.None;
                            elements.Add(path.Substring(startIndex, index - startIndex));
                        }
                        else if (isMinus) // Start new number
                        {
                            elements.Add(path.Substring(startIndex, index - startIndex));
                            startIndex = index;
                        }
                        else if (!isNumber) // Probably a letter, add previous and restart scan
                        {
                            mode = ExplodeScanMode.Letter;
                            elements.Add(path.Substring(startIndex, index - startIndex));
                            startIndex = index;
                        }
                    }

                }
                else if (mode == ExplodeScanMode.Letter)
                {
                    // Ended by: white-space, number, minus, another letter
                    if (isWhiteSpace)
                    {
                        mode = ExplodeScanMode.None;
                        elements.Add(path.Substring(startIndex, index - startIndex));
                    }
                    else if (isNumber || isMinus)
                    {
                        mode = ExplodeScanMode.Number;
                        elements.Add(path.Substring(startIndex, index - startIndex));
                        startIndex = index;
                    }
                    else // Another letter
                    {
                        elements.Add(path.Substring(startIndex, index - startIndex));
                        startIndex = index;
                    }
                }

                index++;
            }

            // Add final item
            elements.Add(path.Substring(startIndex));

            return elements.ToArray();
        }

        public static class PathConverters
        {
            public static IEnumerable<PathCommand> FromRect(float cx, float cy, float width, float height, DrawingMode drawingMode)
            {
                return new List<PathCommand>
                {
                    new PathCommand('M', drawingMode, new PathParamPoint(cx, cy)),
                    new PathCommand('L', drawingMode,
                        new PathParamPoint(cx + width, cy),
                        new PathParamPoint(cx + width, cy + height),
                        new PathParamPoint(cx, cy + height)
                    ),
                    new PathCommand('z', drawingMode)
                };
            }

            public static IEnumerable<PathCommand> FromCircle(float cx, float cy, float rx, float ry, DrawingMode drawingMode)
            {
                return new List<PathCommand>
                {
                    new PathCommand('M', drawingMode, new PathParamPoint(cx - rx, cy)),
                    new PathCommand('a', drawingMode, new PathParamArc(rx, ry, 0.0f, 1.0f, 1.0f, 2.0f * rx, 0.0f)),
                    new PathCommand('a', drawingMode, new PathParamArc(rx, ry, 0.0f, 1.0f, 1.0f, -2.0f * rx, 0.0f)),
                };
            }

            public static IEnumerable<PathCommand> FromLine(float x1, float y1, float x2, float y2)
            {
                return new List<PathCommand>
                {
                    new PathCommand('M', DrawingMode.Outline, new PathParamPoint(x1, y1)),
                    new PathCommand('L', DrawingMode.Outline, new PathParamPoint(x2, y2)),
                };
            }

            public static IEnumerable<PathCommand> FromPolyline(string points, DrawingMode drawingMode = DrawingMode.Default)
            {
                var values = ExplodePath(points.NormalizePath());
                if (values.Length < 4)
                    throw new Exception("Insufficient amount of polygon data.");

                if (values.Length % 2 == 1)
                    throw new Exception("Polygon data count cannot be odd.");

                var cmd1 = new PathCommand('M', drawingMode, new PathParamPoint(
                    ParseFloat(values[0]),
                    ParseFloat(values[1])
                ));

                var cmd2 = new PathCommand('L', drawingMode);
                for (int i = 2; i < values.Length; i += 2)
                {
                    cmd2.Params.Add(new PathParamPoint(
                        ParseFloat(values[i]),
                        ParseFloat(values[i + 1])
                    ));
                }

                return new List<PathCommand> { cmd1, cmd2 };
            }

            public static IEnumerable<PathCommand> FromPolygon(string points, DrawingMode drawingMode)
            {
                var output = new List<PathCommand>();
                output.AddRange(FromPolyline(points, drawingMode));
                output.Add(new PathCommand('z', drawingMode));
                return output;
            }
        }
    }
}