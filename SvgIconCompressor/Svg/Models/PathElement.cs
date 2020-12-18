using SvgIconCompressor.Svg.Enums;
using System.Text;
using System.Xml.Linq;

namespace SvgIconCompressor.Svg.Models
{
    public class PathElement
    {
        public string Path { get; }
        public DrawingMode Drawing { get; }

        public PathElement(string path, DrawingMode drawingMode)
        {
            if (!drawingMode.HasFlag(DrawingMode.HasStroke))
            {
                drawingMode = drawingMode & ~(
                    DrawingMode.StrokeLinecapRound |
                    DrawingMode.StrokeLinecapSquare |
                    DrawingMode.StrokeLinejoinRound |
                    DrawingMode.StrokeLinejoinBevel);
            }

            Path = path;
            Drawing = drawingMode;
        }

        public XElement ToXmlElement(XNamespace xmlns)
        {
            var el = new XElement(xmlns + "path");

            if (Drawing.HasFlag(DrawingMode.NoFill)) el.Add(new XAttribute("fill", "none"));
            if (Drawing.HasFlag(DrawingMode.HasStroke))
            {
                el.Add(new XAttribute("stroke", "black"));

                if (Drawing.HasFlag(DrawingMode.StrokeLinecapRound)) el.Add(new XAttribute("stroke-linecap", "round"));
                if (Drawing.HasFlag(DrawingMode.StrokeLinecapSquare)) el.Add(new XAttribute("stroke-linecap", "square"));

                if (Drawing.HasFlag(DrawingMode.StrokeLinejoinRound)) el.Add(new XAttribute("stroke-linejoin", "round"));
                if (Drawing.HasFlag(DrawingMode.StrokeLinejoinBevel)) el.Add(new XAttribute("stroke-linejoin", "bevel"));
            }

            el.Add(new XAttribute("d", Path));
            return el;
        }

        public string ToXmlElementString()
        {
            var sb = new StringBuilder();
            sb.Append("<path ");

            if (Drawing.HasFlag(DrawingMode.NoFill)) sb.Append("fill=\"none\" ");
            if (Drawing.HasFlag(DrawingMode.HasStroke))
            {
                sb.Append("stroke=\"black\" ");

                if (Drawing.HasFlag(DrawingMode.StrokeLinecapRound)) sb.Append("stroke-linecap=\"round\" ");
                if (Drawing.HasFlag(DrawingMode.StrokeLinecapSquare)) sb.Append("stroke-linecap=\"square\" ");

                if (Drawing.HasFlag(DrawingMode.StrokeLinejoinRound)) sb.Append("stroke-linejoin=\"round\" ");
                if (Drawing.HasFlag(DrawingMode.StrokeLinejoinBevel)) sb.Append("stroke-linejoin=\"bevel\" ");
            }

            sb.Append($"d=\"{Path}\"/>");
            return sb.ToString();
        }
    }
}