using SvgIconCompressor.Svg.Enums;
using SvgIconCompressor.Svg.Extensions;
using SvgIconCompressor.Svg.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using WebSymbolsFontGenerator.Svg.Extensions;
using WebSymbolsFontGenerator.Svg.Models;

namespace WebSymbolsFontGenerator.Svg
{
    // https://css-tricks.com/svg-path-syntax-illustrated-guide/

    public class SvgImage
    {
        public string Name { get; }

        private readonly SizeF _originalSize;
        private readonly List<PathCommand> _pathCommands;


        public SvgImage(string fileName)
        {
            Name = Path.GetFileNameWithoutExtension(fileName);

            var doc = XDocument.Load(fileName);

            // Get viewBox
            var viewBox = doc.Root.Attribute("viewBox").Value.Split(" ");
            var originalOffset = new PointF(
                -SvgUtils.ParseFloat(viewBox[0]),
                -SvgUtils.ParseFloat(viewBox[1])
            );
            _originalSize = new SizeF(
                SvgUtils.ParseFloat(viewBox[2]),
                SvgUtils.ParseFloat(viewBox[3])
            );

            // Get elements
            _pathCommands = new List<PathCommand>();
            GetCommands(_pathCommands, doc.Root);

            // Pre-offset (apply original viewport)
            _pathCommands.ForEach(cmd => cmd.ApplyOffset(originalOffset));
        }

        public void SaveToFile(string fileName, Size canvasSize, int decimalPlaces = 2)
        {
            XNamespace xmlns = "http://www.w3.org/2000/svg";

            var doc = new XDocument(
                new XElement(xmlns + "svg",
                    new XAttribute("version", "1.1"),
                    new XAttribute("xmlns", "http://www.w3.org/2000/svg"),

                    new XAttribute("x", "0px"),
                    new XAttribute("y", "0px"),
                    new XAttribute("width", $"{canvasSize.Width}px"),
                    new XAttribute("height", $"{canvasSize.Height}px"),
                    new XAttribute("viewBox", $"0 0 {canvasSize.Width} {canvasSize.Height}"),

                    GetScaledPaths(canvasSize, decimalPlaces).Select(x => x.ToXmlElement(xmlns))
                )
            );
            var docType = new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);
            doc.AddFirst(docType);
            doc.Save(fileName);
        }

        public string ToHtmlEmbedString(Size canvasSize, int decimalPlaces = 2)
        {
            var svgPath = string.Join("", GetScaledPaths(canvasSize, decimalPlaces).Select(x => x.ToXmlElementString()));
            return $"<svg viewBox=\"0 0 {canvasSize.Width} {canvasSize.Height}\">{svgPath}</svg>";
        }

        public string ToCssEmbedString(Size canvasSize, int decimalPlaces = 2)
        {
            // Thoughts on gzip: https://base64.guru/developers/data-uri/gzip
            // Embeded SVG with IE compatibility: https://codepen.io/tigt/post/optimizing-svgs-in-data-uris

            var path = string.Join("", GetScaledPaths(canvasSize, decimalPlaces).Select(x => x.ToXmlElementString()));
            var svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {canvasSize.Width} {canvasSize.Height}'>{path}</svg>";
            return $"url(\"data:image/svg+xml,{StringToUrlSafe(svg)}\")";
        }

        private static string StringToUrlSafe(string input)
        {
            return input
                .Replace('"', '\'')
                .Replace("<", "%3C")
                .Replace(">", "%3E");
        }

        private IEnumerable<PathElement> GetScaledPaths(Size canvasSize, int decimalPlaces)
        {
            float scale;
            PointF offset;

            var height = canvasSize.Width / _originalSize.Width * _originalSize.Height;
            if (height <= canvasSize.Height)
            {
                // Subject is wide (vertical offset)
                scale = canvasSize.Width / _originalSize.Width;
                offset = new PointF(0.0f, (canvasSize.Height - _originalSize.Height * scale) / 2.0f);
            }
            else
            {
                // Subject is tall (horizontal offset)
                scale = canvasSize.Height / _originalSize.Height;
                offset = new PointF((canvasSize.Width - _originalSize.Width * scale) / 2.0f, 0.0f);
            }

            var sb = new StringBuilder();
            var previousDrawingMode = _pathCommands.FirstOrDefault()?.Drawing ?? DrawingMode.Default;

            foreach (var pathCommand in _pathCommands)
            {
                var path = pathCommand.ToString(offset, scale, decimalPlaces);

                if (pathCommand.Drawing != previousDrawingMode)
                {
                    yield return new PathElement(sb.ToString().Trim(), previousDrawingMode);
                    sb.Clear();
                }

                sb.Append(path);
                previousDrawingMode = pathCommand.Drawing;
            }

            yield return new PathElement(sb.ToString().Trim(), previousDrawingMode);
        }

        private static void GetCommands(List<PathCommand> commands, XElement root)
        {
            // Get transformation for group. TODO: Should inherit
            var translate = new PointF();
            var scale = new PointF(1.0f, 1.0f);

            ParseGroupTransformAttributes(root, ref translate, ref scale);

            foreach (var el in root.Elements())
            {
                // Is group (recursive)
                if (string.Equals(el.Name.LocalName, "g", StringComparison.OrdinalIgnoreCase))
                {
                    GetCommands(commands, el);
                }

                // Collect commands
                var newCommands = new List<PathCommand>();

                // Get path
                if (string.Equals(el.Name.LocalName, "path", StringComparison.OrdinalIgnoreCase))
                {
                    newCommands.AddRange(ParsePath(
                        el.Attribute("d").Value,
                        GetDrawingMode(el)
                    ));
                }

                if (string.Equals(el.Name.LocalName, "rect", StringComparison.OrdinalIgnoreCase))
                {
                    newCommands.AddRange(SvgUtils.PathConverters.FromRect(
                        SvgUtils.ParseFloat(el.Attribute("x").Value),
                        SvgUtils.ParseFloat(el.Attribute("y").Value),
                        SvgUtils.ParseFloat(el.Attribute("width").Value),
                        SvgUtils.ParseFloat(el.Attribute("height").Value),
                        GetDrawingMode(el)
                    ));
                }

                if (string.Equals(el.Name.LocalName, "circle", StringComparison.OrdinalIgnoreCase))
                {
                    var radius = SvgUtils.ParseFloat(el.Attribute("r").Value);
                    newCommands.AddRange(SvgUtils.PathConverters.FromCircle(
                        SvgUtils.ParseFloat(el.Attribute("cx").Value),
                        SvgUtils.ParseFloat(el.Attribute("cy").Value),
                        radius, radius,
                        GetDrawingMode(el)
                    ));
                }

                if (string.Equals(el.Name.LocalName, "ellipse", StringComparison.OrdinalIgnoreCase))
                {
                    newCommands.AddRange(SvgUtils.PathConverters.FromCircle(
                        SvgUtils.ParseFloat(el.Attribute("cx").Value),
                        SvgUtils.ParseFloat(el.Attribute("cy").Value),
                        SvgUtils.ParseFloat(el.Attribute("rx").Value),
                        SvgUtils.ParseFloat(el.Attribute("ry").Value),
                        GetDrawingMode(el)
                    ));
                }

                // Get polygon (polygon line weight and rounding not supported)
                if (string.Equals(el.Name.LocalName, "polygon", StringComparison.OrdinalIgnoreCase))
                {
                    newCommands.AddRange(SvgUtils.PathConverters.FromPolygon(
                        el.Attribute("points").Value,
                        GetDrawingMode(el)
                    ));
                }

                if (string.Equals(el.Name.LocalName, "polyline", StringComparison.OrdinalIgnoreCase))
                {
                    newCommands.AddRange(SvgUtils.PathConverters.FromPolyline(
                        el.Attribute("points").Value,
                        DrawingMode.Outline
                    ));
                }

                if (string.Equals(el.Name.LocalName, "line", StringComparison.OrdinalIgnoreCase))
                {
                    newCommands.AddRange(SvgUtils.PathConverters.FromLine(
                        SvgUtils.ParseFloat(el.Attribute("x1").Value),
                        SvgUtils.ParseFloat(el.Attribute("y1").Value),
                        SvgUtils.ParseFloat(el.Attribute("x2").Value),
                        SvgUtils.ParseFloat(el.Attribute("y2").Value)
                    ));
                }

                // Apply translation and scale
                foreach (var cmd in newCommands)
                {
                    cmd.ApplyScale(scale);
                    cmd.ApplyOffset(translate);
                }

                commands.AddRange(newCommands);
            }
        }

        private static DrawingMode GetDrawingMode(XElement element)
        {
            var result = DrawingMode.Default;

            if (string.Equals(element.TryGetAttribute("fill"), "none", StringComparison.OrdinalIgnoreCase)) result |= DrawingMode.NoFill;
            if (!string.Equals(element.TryGetAttribute("stroke", "none"), "none", StringComparison.OrdinalIgnoreCase)) result |= DrawingMode.HasStroke;

            var strokeLinecap = element.TryGetAttribute("stroke-linecap", null);
            if (strokeLinecap != null)
            {
                if (string.Equals(strokeLinecap, "round", StringComparison.OrdinalIgnoreCase)) result |= DrawingMode.StrokeLinecapRound;
                if (string.Equals(strokeLinecap, "square", StringComparison.OrdinalIgnoreCase)) result |= DrawingMode.StrokeLinecapSquare;
            }

            var strokeLinejoin = element.TryGetAttribute("stroke-linejoin", null);
            if (strokeLinejoin != null)
            {
                if (string.Equals(strokeLinejoin, "round", StringComparison.OrdinalIgnoreCase)) result |= DrawingMode.StrokeLinejoinRound;
                if (string.Equals(strokeLinejoin, "bevel", StringComparison.OrdinalIgnoreCase)) result |= DrawingMode.StrokeLinejoinBevel;
            }

            return result;
        }

        // Returns translation and scale paramteres for the current group.
        private static void ParseGroupTransformAttributes(XElement groupElement, ref PointF translate, ref PointF scale)
        {
            var atTransform = groupElement.Attribute("transform");
            if (atTransform?.Value == null)
                return;

            var elements = atTransform.Value.Split(' ');
            foreach (var el in elements)
            {
                var pieces = el.Split('(');
                var name = pieces[0];
                var pars = pieces[1].TrimEnd(')').Split(',').Select(x => SvgUtils.ParseFloat(x)).ToArray();

                if (string.Equals(name, "translate", StringComparison.OrdinalIgnoreCase))
                {
                    translate = new PointF(pars[0], pars[1]);
                }

                if (string.Equals(name, "scale", StringComparison.OrdinalIgnoreCase))
                {
                    scale = new PointF(pars[0], pars[1]);
                }
            }
        }

        private static IEnumerable<PathCommand> ParsePath(string path, DrawingMode drawingMode)
        {
            var pathSegments = SvgUtils.ExplodePath(path.NormalizePath());

            PathCommand lastCommand = null;

            for (int i = 0; i < pathSegments.Length; i++)
            {
                char firstChar = pathSegments[i][0];

                bool isNumber = firstChar == '-' || firstChar == '.' || ('0' <= firstChar && firstChar <= '9');

                // Append previous command
                if (isNumber && lastCommand != null)
                {
                    switch (char.ToUpper(lastCommand.Key))
                    {
                        // One point
                        case 'M': // Move
                        case 'L': // Line
                        case 'T': // Reflection of Q
                            lastCommand.Params.Add(new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i]),
                                SvgUtils.ParseFloat(pathSegments[i + 1])
                            ));
                            i++;
                            break;

                        // Two points
                        case 'S':
                            lastCommand.Params.Add(new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i]),
                                SvgUtils.ParseFloat(pathSegments[i + 1])
                            ));
                            lastCommand.Params.Add(new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i + 2]),
                                SvgUtils.ParseFloat(pathSegments[i + 3])
                            ));
                            i += 3;
                            break;

                        // Three points
                        case 'C':
                            lastCommand.Params.Add(new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i]),
                                SvgUtils.ParseFloat(pathSegments[i + 1])
                            ));
                            lastCommand.Params.Add(new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i + 2]),
                                SvgUtils.ParseFloat(pathSegments[i + 3])
                            ));
                            lastCommand.Params.Add(new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i + 4]),
                                SvgUtils.ParseFloat(pathSegments[i + 5])
                            ));
                            i += 5;
                            break;
                    }
                }

                // Start new command
                if (!isNumber)
                {
                    if (pathSegments[i].Length != 1)
                        throw new Exception($"Invalid key '{pathSegments[i]}'.");

                    lastCommand = null;

                    switch (char.ToUpper(firstChar))
                    {
                        // No params
                        case 'Z': // Line to the start of the path (no params)
                            yield return new PathCommand(firstChar, drawingMode);
                            break;

                        // Single param
                        case 'H': // Horizontal line
                            yield return new PathCommand(firstChar, drawingMode, new PathParamX(SvgUtils.ParseFloat(pathSegments[i + 1])));
                            i++;
                            break;

                        case 'V': // Vertical line
                            yield return new PathCommand(firstChar, drawingMode, new PathParamY(SvgUtils.ParseFloat(pathSegments[i + 1])));
                            i++;
                            break;

                        // Single point, [continuous]
                        case 'M': // Move
                        case 'L': // Straight line
                        case 'T': // Reflection of Q
                            lastCommand = new PathCommand(firstChar, drawingMode, new PathParamPoint(
                                SvgUtils.ParseFloat(pathSegments[i + 1]),
                                SvgUtils.ParseFloat(pathSegments[i + 2])
                            ));
                            yield return lastCommand;
                            i += 2;
                            break;

                        // Double point
                        case 'S': // Reflect previous bezier curve.
                        case 'Q': // Bezier curve based a single bezier control point and end at specified coordinates.
                            yield return new PathCommand(firstChar, drawingMode,
                                new PathParamPoint(
                                    SvgUtils.ParseFloat(pathSegments[i + 1]),
                                    SvgUtils.ParseFloat(pathSegments[i + 2])
                                ),
                                new PathParamPoint(
                                    SvgUtils.ParseFloat(pathSegments[i + 3]),
                                    SvgUtils.ParseFloat(pathSegments[i + 4])
                                )
                            );
                            i += 4;
                            break;

                        // Triple point, [continuous]
                        case 'C': // Bezier curve
                            lastCommand = new PathCommand(firstChar, drawingMode,
                                new PathParamPoint(
                                    SvgUtils.ParseFloat(pathSegments[i + 1]),
                                    SvgUtils.ParseFloat(pathSegments[i + 2])
                                ),
                                new PathParamPoint(
                                    SvgUtils.ParseFloat(pathSegments[i + 3]),
                                    SvgUtils.ParseFloat(pathSegments[i + 4])
                                ),
                                new PathParamPoint(
                                    SvgUtils.ParseFloat(pathSegments[i + 5]),
                                    SvgUtils.ParseFloat(pathSegments[i + 6])
                                )
                            );
                            yield return lastCommand;
                            i += 6;
                            break;

                        // Other
                        case 'A': // Arc (rX,rY rotation, arc, sweep, eX,eY)
                            yield return new PathCommand(firstChar, drawingMode, new PathParamArc(
                                SvgUtils.ParseFloat(pathSegments[i + 1]),
                                SvgUtils.ParseFloat(pathSegments[i + 2]),
                                SvgUtils.ParseFloat(pathSegments[i + 3]),
                                SvgUtils.ParseFloat(pathSegments[i + 4]),
                                SvgUtils.ParseFloat(pathSegments[i + 5]),
                                SvgUtils.ParseFloat(pathSegments[i + 6]),
                                SvgUtils.ParseFloat(pathSegments[i + 7])
                            ));
                            i += 7;
                            break;

                        default:
                            throw new Exception($"Unexpected command key '{firstChar}'.");
                    }
                }
            }
        }
    }
}