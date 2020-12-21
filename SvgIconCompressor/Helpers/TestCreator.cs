using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WebSymbolsFontGenerator.Helpers
{
	public static class TestCreator
	{
		public class RenderedIcon
        {
			public string Name { get; set; }
			public string EmbededHtml { get; set; }
			public string EmbededCss { get; set; }
		}

		public static void GenerateEmbedSheetHtml(string targetFolder, IEnumerable<RenderedIcon> icons)
		{
			File.WriteAllText(Path.Combine(targetFolder, "embedded-icons-sheet.html"), $@"<html>
<head>
	<style type='text/css'>
		* {{
			box-sizing: border-box;
			font-family: Arial, Helvetica, sans-serif;
			font-size: 16px;
		}}
		
		body {{
			background: #ccc;
		}}

		.holder {{
			max-width: 90%;
			margin: 0 auto;
		}}

		div {{
			overflow: hidden;
		}}
		
		.row {{
			display: flex;
			position: relative;
			float: left;
			width: 100%;
			margin: 6px 0;
			padding: 12px;
			border: 1px solid #999;
			background: #eee;
			box-shadow: 2px 2px 2px #bbb;
			border-radius: 3px;
		}}
		
		.col-preview {{
			flex: 0 0 128px;
		}}
		
		.col-preview svg {{
			display: block;
			width: 128px;
			height: 128px;
			float: left;
		}}
		
		.col-preview span {{
			width: 100%;
			margin-top: 12px;
			float: left;
			font-size: 14px;
			font-weight: bold;
			white-space: pre-wrap;
			white-space: -moz-pre-wrap;
			white-space: -pre-wrap;
			white-space: -o-pre-wrap;
			word-break: break-all;
			text-align: center;
		}}
		
		.col-data {{
			flex: 1 1 0px;
			margin: 0 0 0 12px;
		}}
		
		.col-data .vertical {{
			display: flex;
			flex-direction: column;
			height: 100%;
		}}
		
		.col-data .vertical pre {{
			flex: 1 0 auto;
			width: 100%;
			margin: 0 0 12px 0;
			padding: 8px;
			background: white;
			border: 1px solid #999;
			border-radius: 3px;
			font-family: Consolas, 'Courier New';
			white-space: pre-wrap;
			white-space: -moz-pre-wrap;
			white-space: -pre-wrap;
			white-space: -o-pre-wrap;
			word-break: break-all;
			cursor: pointer;
		}}
		
		.col-data .vertical span {{
			flex: 0 0 auto;
			font-size: 14px;
		}}
	</style>
	<script type='text/javascript'>
		function clip(el) {{
			var range = document.createRange();
			range.selectNodeContents(el);
			var sel = window.getSelection();
			sel.removeAllRanges();
			sel.addRange(range);
		}};
	</script>
</head>
<body>
	<div class=""holder"">{string.Join("", icons.Select(icon => $"\n\t\t{RenderRow(icon)}"))}
	</div>
</body>
</html>");
		}

		private static string RenderRow(RenderedIcon icon)
        {
			return string.Concat(
				@$"<div class=""row"">",
					@$"<div class=""col-preview"">{icon.EmbededHtml}<span>{icon.Name}</span></div>",
					@$"<div class=""col-data""><div class=""vertical""><pre onclick=""clip(this);"">{EscapeHtml(icon.EmbededHtml)}</pre><span>Embedded HTML ({icon.EmbededHtml.Length.ToString("N0", CultureInfo.InvariantCulture)} bytes)</span></div></div>",
					@$"<div class=""col-data""><div class=""vertical""><pre onclick=""clip(this);"">{EscapeHtml(icon.EmbededCss)}</pre><span>Embedded CSS ({icon.EmbededCss.Length.ToString("N0", CultureInfo.InvariantCulture)} bytes)</span></div></div>",
				"</div>"
			);
        }

		private static string EscapeHtml(string input)
		{
			input = input.Replace("<", "&lt;");
			input = input.Replace(">", "&gt;");
			return input;
		}
	}
}