using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using WebSymbolsFontGenerator.Helpers;
using WebSymbolsFontGenerator.Svg;

namespace WebSymbolsFontGenerator
{
    class Program
    {
        const int DefaultImageSize = 512;
        const int DefaultDecimalPlaces = 0;

        static void Main(string[] args)
        {
            if (!int.TryParse(args.Length > 0 ? args[0] : $"{DefaultImageSize}", out int imageSize))
            {
                Console.WriteLine("Invalid image size, must be positive number!");
                return;
            }

            if (!int.TryParse(args.Length > 1 ? args[1] : $"{DefaultDecimalPlaces}", out int decimalPlaces))
            {
                Console.WriteLine("Invalid image size, must be positive number!");
                return;
            }

            var source = Path.GetFullPath(args.Length > 1 ? args[2] : ".");
            var target = Path.GetFullPath(args.Length > 2 ? args[3] : source);

            // Prevent overwriting source files
            if (string.Equals(source, target))
            {
                target = Path.Combine(target, "result");
            }

            Console.WriteLine($"Source: {source}");
            Console.WriteLine($"Target: {target}");
            Console.WriteLine($"Image size: {imageSize}px");
            Console.WriteLine($"Decimal places: {decimalPlaces}");

            // Load images
            var icons = new List<SvgImage>();

            if (File.Exists(source))
            {
                // Single file operation
                icons.Add(new SvgImage(source));
            }
            else if (source == "" || Directory.Exists(source))
            {
                // Batch operation
                var inputFiles = Directory.EnumerateFiles(source, "*.svg");
                icons.AddRange(inputFiles.Select(file => new SvgImage(file)));
            }
            else
            {
                Console.WriteLine("Provided source is invalid.");
                return;
            }

            if (!icons.Any())
            {
                Console.WriteLine("No SVG files found.");
                return;
            }
            
            Directory.CreateDirectory(target);

            SaveIcons(icons, target, new Size(imageSize, imageSize), decimalPlaces);
            //TestResultSize(icons);

            Console.WriteLine($"Converted {icons.Count} SVG files.");
        }

        static void SaveIcons(List<SvgImage> icons, string targetFolder, Size outputImageSize, int decimalPlaces)
        {
            // Save icon files
            icons.ForEach(icon => icon.SaveToFile(Path.Combine(targetFolder, $"{icon.Name}.svg"), outputImageSize, decimalPlaces));

            // Generate embedded icons copy-sheet
            var renderedIcons = icons.Select(icon => new TestCreator.RenderedIcon
            {
                Name = icon.Name,
                EmbededHtml = icon.ToHtmlEmbedString(outputImageSize, decimalPlaces),
                EmbededCss = icon.ToCssEmbedString(outputImageSize, decimalPlaces)
            });
            TestCreator.GenerateEmbedSheetHtml(targetFolder, renderedIcons);
        }

        static void TestResultSize(IList<SvgImage> icons)
        {
            for (int decimalPlaces = 0; decimalPlaces < 3; decimalPlaces++)
            {
                long avg64 = 0;
                long avg128 = 0;
                long avg256 = 0;
                long avg512 = 0;
                long avg1024 = 0;
                long avg8192 = 0;

                foreach (var icon in icons)
                {
                    avg64 += icon.ToHtmlEmbedString(new Size(64, 64), decimalPlaces).Length;
                    avg128 += icon.ToHtmlEmbedString(new Size(128, 128), decimalPlaces).Length;
                    avg256 += icon.ToHtmlEmbedString(new Size(256, 256), decimalPlaces).Length;
                    avg512 += icon.ToHtmlEmbedString(new Size(512, 512), decimalPlaces).Length;
                    avg1024 += icon.ToHtmlEmbedString(new Size(1024, 1024), decimalPlaces).Length;
                    avg8192 += icon.ToHtmlEmbedString(new Size(8192, 8192), decimalPlaces).Length;
                }

                avg64 /= icons.Count;
                avg128 /= icons.Count;
                avg256 /= icons.Count;
                avg512 /= icons.Count;
                avg1024 /= icons.Count;
                avg8192 /= icons.Count;

                Console.WriteLine($"Decimal places: {decimalPlaces}");
                Console.WriteLine($"64:   {avg64} bytes;");
                Console.WriteLine($"128:  {avg128} bytes;");
                Console.WriteLine($"256:  {avg256} bytes;");
                Console.WriteLine($"512:  {avg512} bytes;");
                Console.WriteLine($"1024: {avg1024} bytes;");
                Console.WriteLine($"8192: {avg8192} bytes;");
                Console.WriteLine();
            }
        }
    }
}