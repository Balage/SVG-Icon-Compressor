﻿using SvgIconCompressor.Svg.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WebSymbolsFontGenerator.Svg.Extensions;

namespace WebSymbolsFontGenerator.Svg.Models
{
    public class PathCommand
    {
        public char Key { get; }
        public bool IsRelative => 'a' <= Key && Key <= 'z';
        public DrawingMode Drawing { get; }

        public readonly List<IPathParam> Params = new List<IPathParam>();


        public PathCommand(char key, DrawingMode drawingMode, params IPathParam[] paramList)
        {
            Key = key;
            Drawing = drawingMode;
            Params.AddRange(paramList);
        }

        public void ApplyOffset(PointF offset)
        {
            if (IsRelative)
                return;
            
            Params.ForEach(p => p.ApplyOffset(offset));
        }

        public void ApplyScale(PointF scale)
        {
            Params.ForEach(p => p.ApplyScale(scale));
        }

        public string ToString(PointF offset, float scale, bool compress)
        {
            if (Params.Any())
            {
                if (IsRelative)
                    offset = new PointF();

                return string.Concat(Key, compress ? "" : " ",
                    Params.Select(p => p.ToString(offset, scale, compress)).ToArray().NumberJoin(compress));
            }
            else
            {
                return Key.ToString();
            }
        }
    }
}