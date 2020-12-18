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
        const int defaultImageSize = 64;

        static void Main(string[] args)
        {
            int imageSize;
            if (!int.TryParse(args.Length > 0 ? args[0] : $"{defaultImageSize}", out imageSize))
            {
                Console.WriteLine("Invalid image size, must be pozitive number!");
                return;
            }

            var source = Path.GetFullPath(args.Length > 1 ? args[1] : ".");
            var target = Path.GetFullPath(args.Length > 2 ? args[2] : source);

            // Prevent overwriting source files
            if (string.Equals(source, target))
            {
                target = Path.Combine(target, "result");
            }

            Console.WriteLine($"Source: {source}");
            Console.WriteLine($"Target: {target}");
            Console.WriteLine($"Image size: {imageSize}px");

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

            SaveIcons(icons, target, new Size(imageSize, imageSize));

            Console.WriteLine($"Converted {icons.Count} SVG files.");
        }

        static void SaveIcons(List<SvgImage> icons, string targetFolder, Size outputImageSize)
        {
            // Save icon files
            icons.ForEach(icon => icon.SaveToFile(Path.Combine(targetFolder, $"{icon.Name}.svg"), outputImageSize));

            // Save embed data files
            icons.ForEach(icon => File.WriteAllText(Path.Combine(targetFolder, $"{icon.Name}.embed.svg"), icon.GetEmbedString(outputImageSize)));

            // Generate embedded icons copy-sheet
            TestCreator.GenerateEmbedSheetHtml(targetFolder,
                icons.Select(icon => new Tuple<string, string>(icon.GetEmbedString(outputImageSize), icon.Name)));
        }
    }
}