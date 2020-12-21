using System.Drawing;
using WebSymbolsFontGenerator.Svg.Extensions;

namespace WebSymbolsFontGenerator.Svg.Models
{
    public interface IPathParam
    {
        void ApplyOffset(PointF offset);
        void ApplyScale(PointF scale);
        string ToString(PointF offset, float scale, int decimalPlaces);
    }

    public class PathParamX : IPathParam
    {
        public float X;

        public PathParamX(float x)
        {
            X = x;
        }

        public void ApplyOffset(PointF offset)
        {
            X += offset.X;
        }

        public void ApplyScale(PointF scale)
        {
            X *= scale.X;
        }

        public string ToString(PointF offset, float scale, int decimalPlaces)
        {
            return SvgUtils.NumberToString(X * scale + offset.X, decimalPlaces);
        }
    }

    public class PathParamY : IPathParam
    {
        public float Y;

        public PathParamY(float y)
        {
            Y = y;
        }

        public void ApplyOffset(PointF offset)
        {
            Y += offset.Y;
        }

        public void ApplyScale(PointF scale)
        {
            Y *= scale.Y;
        }

        public string ToString(PointF offset, float scale, int decimalPlaces)
        {
            return SvgUtils.NumberToString(Y * scale + offset.Y, decimalPlaces);
        }
    }

    public class PathParamPoint : IPathParam
    {
        public float X;
        public float Y;

        public PathParamPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void ApplyOffset(PointF offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        public void ApplyScale(PointF scale)
        {
            X *= scale.X;
            Y *= scale.Y;
        }

        public string ToString(PointF offset, float scale, int decimalPlaces)
        {
            return new string[] {
                SvgUtils.NumberToString(X * scale + offset.X, decimalPlaces),
                SvgUtils.NumberToString(Y * scale + offset.Y, decimalPlaces)
            }.NumberJoin();
        }
    }

    public class PathParamArc : IPathParam
    {
        public float RadiusX;
        public float RadiusY;
        public float Rotation;
        public float Arc;
        public float Sweep;
        public float EndX;
        public float EndY;

        public PathParamArc(float radiusX, float radiusY, float rotation, float arc, float sweep, float endX, float endY)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation;
            Arc = arc;
            Sweep = sweep;
            EndX = endX;
            EndY = endY;
        }

        public void ApplyOffset(PointF offset)
        {
            EndX += offset.X;
            EndY += offset.Y;
        }

        public void ApplyScale(PointF scale)
        {
            RadiusX *= scale.X;
            RadiusY *= scale.Y;
            EndX *= scale.X;
            EndY *= scale.Y;
        }

        public string ToString(PointF offset, float scale, int decimalPlaces)
        {
            return new string[] {
                SvgUtils.NumberToString(RadiusX * scale, decimalPlaces),
                SvgUtils.NumberToString(RadiusY * scale, decimalPlaces),
                SvgUtils.NumberToString(Rotation, decimalPlaces),
                SvgUtils.NumberToString(Arc, decimalPlaces),
                SvgUtils.NumberToString(Sweep, decimalPlaces),
                SvgUtils.NumberToString(EndX * scale + offset.X, decimalPlaces),
                SvgUtils.NumberToString(EndY * scale + offset.Y, decimalPlaces)
            }.NumberJoin();
        }
    }
}