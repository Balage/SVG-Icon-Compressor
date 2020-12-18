using System;
using System.Xml.Linq;

namespace SvgIconCompressor.Svg.Extensions
{
    public static class XmlExtensions
    {
        public static string TryGetAttribute(this XElement element, string name, string defaultValue = "")
        {
            try
            {
                return element.Attribute(name).Value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}