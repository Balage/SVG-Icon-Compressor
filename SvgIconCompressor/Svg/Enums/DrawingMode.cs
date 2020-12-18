namespace SvgIconCompressor.Svg.Enums
{
    public enum DrawingMode
    {
        Default = 0,
        NoFill = 1,
        HasStroke = 2,
        Outline = NoFill | HasStroke,
        StrokeLinecapRound = 4,
        StrokeLinecapSquare = 8,
        StrokeLinejoinRound = 16,
        StrokeLinejoinBevel = 32,
    }
}