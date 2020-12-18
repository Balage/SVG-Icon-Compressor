using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace WebSymbolsFontGenerator.Helpers
{
	public static class TestCreator
	{
		public static void GenerateEmbedSheetHtml(string targetFolder, IEnumerable<Tuple<string, string>> embeddedStrings)
		{
			File.WriteAllText(Path.Combine(targetFolder, "embedded-icons-sheet.html"), $@"<html>
<head>
	<style type='text/css'>
		body {{
			background: #ccc;
		}}

		.table {{
			max-width: 990px;
			margin: 0 auto;
		}}

		div {{
			overflow: hidden;
		}}
		
		.row {{
			position: relative;
			display: block;
			float: left;
			margin: 6px;
			padding: 4px 4px 4px 104px;
			min-height: 92px;
			border: 1px solid #999;
			font-size: 18px;
			background: #eee;
			cursor: pointer;
			box-shadow: 2px 2px 2px #bbb;
			border-radius: 3px;
		}}

		.row svg {{
			position: absolute;
			display: block;
			width: 64px;
			height: 64px;
			left: 20px;
			top: 8px;
		}}

		.row pre {{
			margin: 0;
			font-size: 14px;
			white-space: pre-wrap;
			white-space: -moz-pre-wrap;
			white-space: -pre-wrap;
			white-space: -o-pre-wrap;
			word-break: break-all;
			border: 1px solid #999;
			background: white;
			border-radius: 3px;
			padding: 8px;
		}}

		.row .caption {{
			position: absolute;
			left: 4px;
			top: 78px;
			width: 96px;
			font-size: 14px;
			white-space: pre-wrap;
			white-space: -moz-pre-wrap;
			white-space: -pre-wrap;
			white-space: -o-pre-wrap;
			word-break: break-all;
			text-align: center;
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
	<div class='table'>{string.Join("", embeddedStrings.Select(embed => $"\n\t\t<div class='row'>{embed.Item1}<div class='caption'>{embed.Item2}</div><pre onclick='clip(this);'>{EscapeHtml(embed.Item1)}</pre></div>"))}
	</div>
</body>
</html>");
		}

		private static string EscapeHtml(string input)
		{
			input = input.Replace("<", "&lt;");
			input = input.Replace(">", "&gt;");
			return input;
		}
	}
}