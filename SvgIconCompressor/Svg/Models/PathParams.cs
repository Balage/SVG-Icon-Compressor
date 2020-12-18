using System.Drawing;
using WebSymbolsFontGenerator.Svg.Extensions;

namespace WebSymbolsFontGenerator.Svg.Models
{
    public interface IPathParam
    {
        void ApplyOffset(PointF offset);
        void ApplyScale(PointF scale);
        string ToString(PointF offset, float scale, bool compress);
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

        public string ToString(PointF offset, float scale, bool compress)
        {
            return SvgUtils.NumberToString(X * scale + offset.X, compress);
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

        public string ToString(PointF offset, float scale, bool compress)
        {
            return SvgUtils.NumberToString(Y * scale + offset.Y, compress);
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

        public string ToString(PointF offset, float scale, bool compress)
        {
            return new string[] {
                SvgUtils.NumberToString(X * scale + offset.X, compress),
                SvgUtils.NumberToString(Y * scale + offset.Y, compress)
            }.NumberJoin(compress);
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

        public string ToString(PointF offset, float scale, bool compress)
        {
            return new string[] {
                SvgUtils.NumberToString(RadiusX * scale, compress),
                SvgUtils.NumberToString(RadiusY * scale, compress),
                SvgUtils.NumberToString(Rotation, compress),
                SvgUtils.NumberToString(Arc, compress),
                SvgUtils.NumberToString(Sweep, compress),
                SvgUtils.NumberToString(EndX * scale + offset.X, compress),
                SvgUtils.NumberToString(EndY * scale + offset.Y, compress)
            }.NumberJoin(compress);
        }
    }
}