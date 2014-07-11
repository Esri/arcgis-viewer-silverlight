/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public static class StringExtensions
    {
        /// <summary>
        /// Inserts spaces where the string contains camel-casing or underscores
        /// </summary>
        /// <param name="s">The string to process</param>
        /// <returns>The string with spaces inserted</returns>
        public static string InsertSpaces(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            StringBuilder sb = new StringBuilder();
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                if (char.IsUpper(c))
                {
                    // For a capital letter, we add a space before
                    sb.Insert(0, new char[] { c });
                    sb.Insert(0, new char[] { ' ' });
                }
                else if (c == '_')
                {
                    // replace underscores with a space
                    sb.Insert(0, new char[] { ' ' });
                }
                else
                {
                    sb.Insert(0, new char[] { c });
                }
            }
            return sb.ToString().Trim();
        }

        // Based on technique demonstrated by Richie Carmichael in blog post found at
        // http://kiwigis.blogspot.com/2012/06/how-to-programmatically-add-hyperlinks.html
        /// <summary>
        /// Converts the specified text in a <see cref="RichTextBlock"/> to a hyperlink that
        /// navigates to the specified URL
        /// </summary>
        public static void Hyperlink(this RichTextBlock rtb, string text, string url)
        {
            // Argument Checking
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("url");

            // Loop through text to find specified text
            foreach (Block block in rtb.Blocks)
            {
                // Only examine the contents of paragraphs
                Paragraph paragraph = block as Paragraph;
                if (paragraph == null) { continue; }

                int count = paragraph.Inlines.Count;
                for (int i = 0; i < count; i++)
                {
                    // Only examine runs
                    Inline inline = paragraph.Inlines[i];
                    Run run = inline as Run;
                    if (run == null) { continue; }

                    // Check whether text is present
                    int index = run.Text.IndexOf(text);
                    if (index == -1) { continue; }

                    // Get text before match text
                    string startText = run.Text.Substring(0, index);

                    // Get text after match text
                    int midIndex = index + text.Length;
                    string endText = midIndex < run.Text.Length ? 
                        run.Text.Substring(midIndex, run.Text.Length - 1) : "";

                    // Update current run's text with only text before match string
                    run.Text = startText;

                    // Create hyperlink to embed in text
                    Hyperlink hyperlink = new Hyperlink()
                    {
                        NavigateUri = new Uri(url),
                        TargetName = "_blank",
                        Foreground = rtb.Foreground,
                        MouseOverForeground = rtb.Foreground,
                        TextDecorations = null,
                        MouseOverTextDecorations = TextDecorations.Underline
                    };

                    // Add the hyperlink to the paragraph
                    hyperlink.Inlines.Add(text);
                    if (i + 1 == paragraph.Inlines.Count)
                        paragraph.Inlines.Add(hyperlink);
                    else
                        paragraph.Inlines.Insert(i + 1, hyperlink);

                    // increment the count since an inline has been added to the paragraph
                    count++;

                    // if there was any text after the match string, create a run out of it and
                    // add it to the paragraph
                    if (!string.IsNullOrEmpty(endText))
                    {
                        Run newRun = new Run() { Text = endText };

                        if (i + 2 == paragraph.Inlines.Count)
                            paragraph.Inlines.Add(run);
                        else
                            paragraph.Inlines.Insert(i + 2, run);

                        // increment the count since an inline has been added to the paragraph
                        count++;
                    }
                }
            }
        }
    }
}
