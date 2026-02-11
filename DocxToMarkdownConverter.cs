using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SA_ToolBelt
{
    public class DocxToMarkdownConverter
    {
        /// <summary>
        /// Converts a .docx file to markdown and writes the output to a .md file.
        /// Returns the output file path on success.
        /// </summary>
        public string Convert(string docxPath)
        {
            if (!File.Exists(docxPath))
                throw new FileNotFoundException("The specified .docx file was not found.", docxPath);

            string mdPath = Path.ChangeExtension(docxPath, ".md");

            using var doc = WordprocessingDocument.Open(docxPath, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                throw new InvalidOperationException("The document has no body content.");

            var sb = new StringBuilder();
            bool previousWasBlank = false;

            foreach (var element in body.Elements())
            {
                if (element is Paragraph para)
                {
                    string line = ConvertParagraph(para);

                    // Collapse multiple blank lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (!previousWasBlank)
                        {
                            sb.AppendLine();
                            previousWasBlank = true;
                        }
                        continue;
                    }

                    previousWasBlank = false;
                    sb.AppendLine(line);
                    sb.AppendLine();
                }
                else if (element is Table table)
                {
                    previousWasBlank = false;
                    sb.AppendLine(ConvertTable(table));
                    sb.AppendLine();
                }
            }

            // Clean up trailing whitespace
            string result = sb.ToString().TrimEnd() + Environment.NewLine;

            File.WriteAllText(mdPath, result, Encoding.UTF8);
            return mdPath;
        }

        private string ConvertParagraph(Paragraph para)
        {
            string styleName = GetStyleName(para);
            string text = GetFormattedText(para);

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Headings
            if (styleName.StartsWith("heading", StringComparison.OrdinalIgnoreCase) ||
                styleName.StartsWith("heading", StringComparison.OrdinalIgnoreCase))
            {
                int level = ExtractHeadingLevel(styleName);
                string prefix = new string('#', Math.Clamp(level, 1, 6));
                return $"{prefix} {text.Trim()}";
            }

            // Title style
            if (styleName.Equals("title", StringComparison.OrdinalIgnoreCase))
                return $"# {text.Trim()}";

            // Subtitle style
            if (styleName.Equals("subtitle", StringComparison.OrdinalIgnoreCase))
                return $"## {text.Trim()}";

            // List items
            var numProps = para.ParagraphProperties?.NumberingProperties;
            if (numProps != null)
            {
                int indentLevel = GetListIndentLevel(numProps);
                string indent = new string(' ', indentLevel * 2);
                bool isOrdered = IsOrderedList(para);
                string bullet = isOrdered ? "1." : "-";
                return $"{indent}{bullet} {text.Trim()}";
            }

            // List styles without numbering properties
            if (styleName.Contains("list", StringComparison.OrdinalIgnoreCase))
                return $"- {text.Trim()}";

            return text;
        }

        private string GetFormattedText(Paragraph para)
        {
            var parts = new List<string>();

            foreach (var element in para.Elements())
            {
                if (element is Run run)
                {
                    string runText = GetRunText(run);
                    if (string.IsNullOrEmpty(runText))
                        continue;

                    var props = run.RunProperties;
                    bool isBold = props?.Bold != null && (props.Bold.Val == null || props.Bold.Val.Value);
                    bool isItalic = props?.Italic != null && (props.Italic.Val == null || props.Italic.Val.Value);
                    bool isStrikethrough = props?.Strike != null && (props.Strike.Val == null || props.Strike.Val.Value);
                    bool isCode = IsCodeStyle(props);

                    if (isCode)
                        runText = $"`{runText}`";
                    else if (isBold && isItalic)
                        runText = $"***{runText}***";
                    else if (isBold)
                        runText = $"**{runText}**";
                    else if (isItalic)
                        runText = $"*{runText}*";

                    if (isStrikethrough)
                        runText = $"~~{runText}~~";

                    parts.Add(runText);
                }
                else if (element is Hyperlink hyperlink)
                {
                    parts.Add(ConvertHyperlink(hyperlink, para));
                }
            }

            return string.Join("", parts);
        }

        private string GetRunText(Run run)
        {
            var sb = new StringBuilder();
            foreach (var child in run.Elements())
            {
                if (child is Text t)
                    sb.Append(t.Text);
                else if (child is Break br)
                    sb.Append(br.Type?.Value == BreakValues.Page ? "\n\n---\n\n" : "\n");
                else if (child is TabChar)
                    sb.Append("    ");
            }
            return sb.ToString();
        }

        private string ConvertHyperlink(Hyperlink hyperlink, Paragraph para)
        {
            string text = string.Join("", hyperlink.Elements<Run>().Select(r => GetRunText(r)));
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Try to get the URL from the relationship
            string? url = null;
            if (hyperlink.Id?.Value != null)
            {
                var doc = para.Ancestors<Document>().FirstOrDefault();
                var mainPart = doc?.MainDocumentPart;
                if (mainPart != null)
                {
                    try
                    {
                        var rel = mainPart.HyperlinkRelationships
                            .FirstOrDefault(r => r.Id == hyperlink.Id.Value);
                        url = rel?.Uri?.ToString();
                    }
                    catch { }
                }
            }

            return url != null ? $"[{text}]({url})" : text;
        }

        private string ConvertTable(Table table)
        {
            var rows = table.Elements<TableRow>().ToList();
            if (rows.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            // Header row
            var headerCells = rows[0].Elements<TableCell>()
                .Select(c => GetCellText(c).Replace("|", "\\|"))
                .ToList();

            sb.AppendLine("| " + string.Join(" | ", headerCells) + " |");
            sb.AppendLine("| " + string.Join(" | ", headerCells.Select(_ => "---")) + " |");

            // Data rows
            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i].Elements<TableCell>()
                    .Select(c => GetCellText(c).Replace("|", "\\|"))
                    .ToList();

                // Pad if fewer cells than header
                while (cells.Count < headerCells.Count)
                    cells.Add("");

                sb.AppendLine("| " + string.Join(" | ", cells) + " |");
            }

            return sb.ToString().TrimEnd();
        }

        private string GetCellText(TableCell cell)
        {
            var parts = cell.Elements<Paragraph>()
                .Select(p => GetFormattedText(p).Trim())
                .Where(t => !string.IsNullOrEmpty(t));
            return string.Join(" ", parts);
        }

        private string GetStyleName(Paragraph para)
        {
            return para.ParagraphProperties?.ParagraphStyleId?.Val?.Value?.ToLower() ?? "normal";
        }

        private int ExtractHeadingLevel(string styleName)
        {
            // Styles like "heading1", "heading2", "Heading 1", etc.
            foreach (char c in styleName)
            {
                if (char.IsDigit(c))
                    return c - '0';
            }
            return 1;
        }

        private int GetListIndentLevel(NumberingProperties numProps)
        {
            var level = numProps.NumberingLevelReference?.Val?.Value;
            return level ?? 0;
        }

        private bool IsOrderedList(Paragraph para)
        {
            // Check numbering definitions for ordered vs unordered
            var numProps = para.ParagraphProperties?.NumberingProperties;
            if (numProps?.NumberingId?.Val == null)
                return false;

            var doc = para.Ancestors<Document>().FirstOrDefault();
            var numPart = doc?.MainDocumentPart?.NumberingDefinitionsPart;
            if (numPart == null)
                return false;

            try
            {
                var numId = numProps.NumberingId.Val.Value;
                var level = numProps.NumberingLevelReference?.Val?.Value ?? 0;

                var numInstance = numPart.Numbering?
                    .Elements<NumberingInstance>()
                    .FirstOrDefault(n => n.NumberID?.Value == numId);

                var abstractNumId = numInstance?.AbstractNumId?.Val?.Value;
                if (abstractNumId == null)
                    return false;

                var abstractNum = numPart.Numbering?
                    .Elements<AbstractNum>()
                    .FirstOrDefault(a => a.AbstractNumberId?.Value == abstractNumId);

                var levelDef = abstractNum?.Elements<Level>()
                    .FirstOrDefault(l => l.LevelIndex?.Value == level);

                var fmt = levelDef?.NumberingFormat?.Val?.Value;
                return fmt == NumberFormatValues.Decimal ||
                       fmt == NumberFormatValues.UpperLetter ||
                       fmt == NumberFormatValues.LowerLetter ||
                       fmt == NumberFormatValues.UpperRoman ||
                       fmt == NumberFormatValues.LowerRoman;
            }
            catch
            {
                return false;
            }
        }

        private bool IsCodeStyle(RunProperties? props)
        {
            if (props == null) return false;

            // Check for monospace font (common code indicators)
            var fontName = props.RunFonts?.Ascii?.Value?.ToLower();
            if (fontName != null && (fontName.Contains("consolas") ||
                                     fontName.Contains("courier") ||
                                     fontName.Contains("mono")))
                return true;

            // Check for code style reference
            var styleId = props.RunStyle?.Val?.Value?.ToLower();
            if (styleId != null && styleId.Contains("code"))
                return true;

            return false;
        }
    }
}
