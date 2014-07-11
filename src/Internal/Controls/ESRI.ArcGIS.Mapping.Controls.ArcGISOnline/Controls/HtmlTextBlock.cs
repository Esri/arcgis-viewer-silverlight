/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

// Based on sample provided at http://www.sharpgis.net/post/2010/09/15/Displaying-HTML-in-Silverlight.aspx
// Originally based on http://blogs.msdn.com/b/delay/archive/2007/09/10/bringing-a-bit-of-html-to-silverlight-htmltextblock-makes-rich-text-display-easy.aspx

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Collections.Generic;
using System;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
    public class HtmlTextBlock : RichTextBox
    {
        // Constants
        protected const string elementA = "A";
        protected const string elementB = "B";
        protected const string elementBR = "BR";
        protected const string elementEM = "EM";
        protected const string elementI = "I";
        protected const string elementP = "P";
        protected const string elementSTRONG = "STRONG";
        protected const string elementU = "U";
        protected const string elementImage = "IMG";

        protected const string elementTR = "TR";
        protected const string elementTD = "TD";
        protected const string elementTABLE = "TABLE";


        // Variables
        protected string _text;

        public HtmlTextBlock()
        {
            IsReadOnly = true;
            BorderThickness = new Thickness(0);
            Background = null;
        }

        protected void ParseAndSetText(string text)
        {
            // Save the original text string
            _text = text;
            // Try for a valid XHTML representation of text
            var success = false;
            try
            {
                // Try to parse as-is
                ParseAndSetSpecifiedText(text);
                success = true;
            }
            catch (XmlException)
            {
                // Invalid XHTML
            }
            if (!success && UseDomAsParser)
            {
                // Fall back on the browser's parsing engine and some custom code
                // Create some DOM nodes to use
                var document = HtmlPage.Document;
                // An invisible DIV to contain all the custom content
                var wrapper = document.CreateElement("div");
                wrapper.SetStyleAttribute("display", "none");
                // A DIV to contain the input to the code
                var input = document.CreateElement("div");
                input.SetProperty("innerHTML", text);
                // A DIV to contain the output to the code
                var output = document.CreateElement("div");
                // There should be only one BODY element, but this is an easy way to handle 0 or more
                foreach (var bodyObject in document.GetElementsByTagName("body"))
                {
                    var body = bodyObject as HtmlElement;
                    if (null != body)
                    {
                        // Add wrapper element to the DOM
                        body.AppendChild(wrapper);
                        try
                        {
                            // Add input/output elements to the DOM
                            wrapper.AppendChild(input);
                            wrapper.AppendChild(output);
                            // Simple code for browsers where .innerHTML returns ~XHTML (ex: Firefox)
                            var transformationSimple =
                                "(function(){" +
                                    "var input = document.body.lastChild.firstChild;" +
                                    "var output = input.nextSibling;" +
                                    "output.innerHTML = input.innerHTML;" +
                                "})();";
                            // Complex code for browsers where .innerHTML returns content as-is (ex: Internet Explorer)
                            var transformationComplex =
                                "(function(){" +
                                    "function computeInnerXhtml(node, inner) {" +
                                        "if (node.nodeValue) {" +
                                            "return node.nodeValue;" +
                                        "} else if (node.nodeName && (0 < node.nodeName.length)) {" +
                                            "var xhtml = '';" +
                                            "if (node.firstChild) {" +
                                                "if (inner) {" +
                                                    "xhtml += '<' + node.nodeName + '>';" +
                                                "}" +
                                                "var child = node.firstChild;" +
                                                "while (child) {" +
                                                    "xhtml += computeInnerXhtml(child, true);" +
                                                    "child = child.nextSibling;" +
                                                "}" +
                                                "if (inner) {" +
                                                    "xhtml += '</' + node.nodeName + '>';" +
                                                "}" +
                                            "} else {" +
                                                "return ('/' == node.nodeName.charAt(0)) ? ('') : ('<' + node.nodeName + '/>');" +
                                            "}" +
                                            "return xhtml;" +
                                        "}" +
                                    "}" +
                                    "var input = document.body.lastChild.firstChild;" +
                                    "var output = input.nextSibling;" +
                                    "output.innerHTML = computeInnerXhtml(input);" +
                                "})();";
                            // Create a list of code options, ordered simple->complex
                            var transformations = new string[] { transformationSimple, transformationComplex };
                            // Try each code option until one works
                            foreach (var transformation in transformations)
                            {
                                // Create a SCRIPT element to contain the code
                                var script = document.CreateElement("script");
                                script.SetAttribute("type", "text/javascript");
                                script.SetProperty("text", transformation);
                                // Add it to the wrapper element (which runs the code)
                                wrapper.AppendChild(script);
                                // Get the results of the transformation
                                var xhtml = (string)output.GetProperty("innerHTML") ?? "";
                                // Perform some final transformations for the BR element which browsers get wrong
                                xhtml = xhtml.Replace("<br>", "<br/>");  // Firefox
                                xhtml = xhtml.Replace("<BR>", "<BR/>");  // Internet Explorer
                                try
                                {
                                    // Try to parse
                                    ParseAndSetSpecifiedText(xhtml);
                                    success = true;
                                    break;
                                }
                                catch (XmlException)
                                {
                                    // Still invalid XML
                                }
                            }
                        }
                        finally
                        {
                            // Be sure to remove the wrapper we added to the DOM
                            body.RemoveChild(wrapper);
                        }
                        // Processed one BODY; that's enough
                        break;
                    }
                }
            }
            if (!success)
            {
                // Invalid, unfixable XHTML; display the supplied text as-is
                Xaml = null;
            }
        }

        private class DocTree
        {
            private DocTree()
            {
                var children = new System.Collections.ObjectModel.ObservableCollection<DocTree>();
                Children = children;
                children.CollectionChanged += children_CollectionChanged;
            }

            private void children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                    foreach (object o in e.NewItems)
                    {
                        (o as DocTree).Parent = this;
                    }
                if (e.OldItems != null)
                    foreach (object o in e.OldItems)
                    {
                        (o as DocTree).Parent = null;
                    }
            }
            private double fontSize = double.NaN;
            public DocTree(Inline element, int row, int column)
                : this()
            {
                Element = element;
                Row = row; Column = column;
            }
            public double FontSize
            {
                get
                {
                    var dt = this;
                    while (dt != null && double.IsNaN(dt.fontSize))
                        dt = dt.Parent;
                    return dt.fontSize;
                }
                set { fontSize = value; }
            }
            public Inline Element { get; private set; }
            public System.Collections.Generic.IList<DocTree> Children { get; private set; }
            public DocTree Parent { get; private set; }
            public bool HasChildren { get { return Children.Count > 0; } }
            public int Row { get; private set; }
            public int Column { get; private set; }
        }
        private static Section CreateSection()
        {
            return XamlReader.Load("<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" />")
                as Section;
        }
        private void ParseAndSetSpecifiedText(string text)
        {
            // Clear the collection of Inlines
            SelectAll();
            this.Selection.Text = "";
            // Wrap the input in a <div> (so even plain text becomes valid XML)
            using (var stringReader = new StringReader(string.Concat("<div>", text, "</div>")))
            {
                DocTree RootElement = new DocTree(new Span(), 0, 0) { FontSize = 10 };
                DocTree currentElement = RootElement;
                // Read the input
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    // State variables
                    var bold = 0;
                    var italic = 0;
                    var underline = 0;
                    var td = 0;
                    var tr = 0;
                    Stack<double> fontSize = new Stack<double>();
                    fontSize.Push(10);
                    System.Collections.Generic.Stack<TextElement> elementTree = new System.Collections.Generic.Stack<TextElement>();
                    string link = null;
                    var lastElement = elementP;
                    // Read the entire XML DOM...
                    while (xmlReader.Read())
                    {
                        var nameUpper = xmlReader.Name.ToUpper();
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                // Handle the begin element
                                switch (nameUpper)
                                {
                                    case "H1":
                                        fontSize.Push(24);
                                        break;
                                    case "H2":
                                        fontSize.Push(18);
                                        break;
                                    case "H3":
                                        fontSize.Push(14);
                                        break;
                                    case "H4":
                                        fontSize.Push(12);
                                        break;
                                    case elementA:
                                        link = "";
                                        // Look for the HREF attribute (can't use .MoveToAttribute because it's case-sensitive)
                                        if (xmlReader.MoveToFirstAttribute())
                                        {
                                            do
                                            {
                                                if ("HREF" == xmlReader.Name.ToUpper())
                                                {
                                                    // Store the link target
                                                    link = xmlReader.Value;
                                                    break;
                                                }
                                            } while (xmlReader.MoveToNextAttribute());
                                        }
                                        break;
                                    case elementB:
                                    case elementSTRONG: bold++; break;
                                    case elementI:
                                    case elementEM: italic++; break;
                                    case elementU: underline++; break;
                                    case elementBR:
                                        currentElement.Children.Add(new DocTree(new LineBreak(), tr, td));
                                        break;
                                    //Selection.Insert(new LineBreak() ); break;
                                    case elementTR:
                                        tr++; td = 0; break;
                                    case elementTD:
                                        td++; break;
                                    case "TH":
                                        td++;
                                        bold++;
                                        break;
                                    case elementTABLE:
                                        DocTree p = new DocTree(new Table(), tr, td);
                                        currentElement.Children.Add(p);
                                        tr = 0; td = 0;
                                        currentElement = p;
                                        break;
                                    case elementP:
                                        DocTree p2 = new DocTree(new InlineParaGraph(), tr, td);
                                        currentElement.Children.Add(p2);
                                        currentElement = p2;
                                        break;
                                    case elementImage:
                                        var img = new InlineUIContainer()
                                        {
                                            Child = new Image()
                                            {
                                                Source = new System.Windows.Media.Imaging.BitmapImage(
                                                    new System.Uri(xmlReader.GetAttribute("src"), System.UriKind.Absolute)
                                                ),
                                                Stretch = Stretch.None //This should be improved to read image size if set
                                            }
                                        };
                                        currentElement.Children.Add(new DocTree(img, tr, td));
                                        break;
                                }
                                lastElement = nameUpper;
                                break;
                            case XmlNodeType.EndElement:
                                // Handle the end element
                                switch (nameUpper)
                                {
                                    case elementA: link = null; break;
                                    case elementB:
                                    case elementSTRONG: bold--; break;
                                    case elementI:
                                    case elementEM: italic--; break;
                                    case elementU: underline--; break;
                                    case "H1":
                                    case "H2":
                                    case "H3":
                                    case "H4":
                                        fontSize.Pop();
                                        break;
                                    case elementTR:
                                        td = 0; break;
                                    case elementTD:
                                        break;
                                    case "TH":
                                        bold--;
                                        break;
                                    case elementTABLE:
                                        tr = 0; td = 0;
                                        currentElement = currentElement.Parent ?? RootElement;
                                        break;
                                    case elementP:
                                        currentElement = currentElement.Parent ?? RootElement;
                                        //Selection.Insert(elementTree.Pop());
                                        //Selection.Insert(new LineBreak());
                                        //Selection.Insert(new LineBreak());
                                        break;
                                }
                                lastElement = nameUpper;
                                break;
                            case XmlNodeType.Text:
                            case XmlNodeType.Whitespace:
                                // Create a Run for the visible text
                                // Collapse contiguous whitespace per HTML behavior
                                StringBuilder builder = new StringBuilder(xmlReader.Value.Length);
                                var last = '\0';
                                foreach (char c in xmlReader.Value.Replace('\n', ' '))
                                {
                                    if ((' ' != last) || (' ' != c))
                                    {
                                        builder.Append(c);
                                    }
                                    last = c;
                                }
                                // Trim leading whitespace if following a <P> or <BR> element per HTML behavior
                                var builderString = builder.ToString();
                                if ((elementP == lastElement) || (elementBR == lastElement))
                                {
                                    builderString = builderString.TrimStart();
                                }
                                // If any text remains to display...
                                if (0 < builderString.Length)
                                {
                                    // Create a Run to display it
                                    Run run = new Run
                                    {
                                        Text = builderString,
                                        FontSize = currentElement.FontSize
                                    };

                                    // Style the Run appropriately
                                    if (0 < bold) run.FontWeight = FontWeights.Bold;
                                    if (0 < italic) run.FontStyle = FontStyles.Italic;
                                    if (0 < underline) run.TextDecorations = System.Windows.TextDecorations.Underline;
                                    if (fontSize.Count > 0) run.FontSize = fontSize.Peek();
                                    if (null != link)
                                    {
                                        // Links get styled and display their HREF since Run doesn't support MouseLeftButton* events
                                        run.TextDecorations = System.Windows.TextDecorations.Underline;
                                        run.Foreground = HyperlinkColor ?? new SolidColorBrush { Color = Color.FromArgb(255, 177, 211, 250) };
                                        Hyperlink hlink = new Hyperlink()
                                        {
                                            NavigateUri = new System.Uri(link, System.UriKind.RelativeOrAbsolute),
                                            TargetName = "_blank"
                                        };
                                        hlink.Inlines.Add(run);
                                        currentElement.Children.Add(new DocTree(hlink, tr, td));
                                    }
                                    else
                                        // Add the Run to the collection
                                        currentElement.Children.Add(new DocTree(run, tr, td));
                                    lastElement = null;
                                }
                                break;
                        }
                    }

                }
                Span s = new Span();
                DocTreeToTextElement(RootElement, s.Inlines);
                Selection.Insert(s);
            }
        }
        private class InlineParaGraph : Inline { }
        private class Table : Inline { }
        private void AddElementToCollection(Inline elm, InlineCollection coll)
        {
            if (elm is InlineParaGraph)
            {
                //coll.Add(new LineBreak());
                coll.Add(new LineBreak());
            }
            else if (elm is Table)
            {
                coll.Add(new LineBreak());
            }
            else
                coll.Add(elm);
        }
        private Inline CreateGrid(DocTree tree)
        {
            Grid g = new Grid();
            int rowMax = 0; int colMax = 0;
            Dictionary<int, Dictionary<int, RichTextBox>> cells = new Dictionary<int, Dictionary<int, RichTextBox>>();
            foreach (var child in tree.Children)
            {
                Span span = new Span();
                DocTreeToTextElement(child, span.Inlines);

                int row = child.Row - 1;
                int col = child.Column - 1;
                if (!cells.ContainsKey(col))
                    cells.Add(col, new Dictionary<int, RichTextBox>());
                var column = cells[col];
                if (!column.ContainsKey(row))
                {
                    var spanel = new RichTextBox()
                    {
                        BorderThickness = new Thickness(0),
                        IsReadOnly = true
                    };
                    spanel.SetValue(Grid.ColumnProperty, col);
                    spanel.SetValue(Grid.RowProperty, row);
                    column.Add(row, spanel);
                    g.Children.Add(spanel);
                }
                RichTextBox tb = column[row];
                tb.Selection.Insert(span);
                rowMax = (int)Math.Max(rowMax, child.Row);
                colMax = (int)Math.Max(colMax, child.Column);
            }
            for (int i = 0; i < rowMax; i++)
                g.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < colMax; i++)
                g.ColumnDefinitions.Add(new ColumnDefinition());
            InlineUIContainer c = new InlineUIContainer()
            {
                Child = g
            };
            return c;
        }
        private void DocTreeToTextElement(DocTree tree, InlineCollection coll)
        {
            if (tree.Element is Table)
            {
                coll.Add(new LineBreak());
                AddElementToCollection(CreateGrid(tree), coll);
            }
            else
            {
                AddElementToCollection(tree.Element, coll);
                if (!tree.HasChildren)
                    return;
                foreach (var child in tree.Children)
                {
                    DocTreeToTextElement(child, coll);
                }
            }
        }
        private TextElement DocTreeChildrenToTextElement(IList<DocTree> children)
        {
            if (children.Count == 1)
                return children[0].Element;
            else return null;
        }
        // Custom properties


        // Specifies whether the browser DOM can be used to attempt to parse invalid XHTML
        // Note: Deliberately not a DependencyProperty because setting this has security implications
        public bool UseDomAsParser { get; set; }

        // TextBlock properties duplicated so HtmlTextBlock can be used as a TextBlock


        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(string), typeof(HtmlTextBlock),
            new PropertyMetadata(delegate(DependencyObject o, DependencyPropertyChangedEventArgs e) { ((HtmlTextBlock)o).ParseAndSetText((string)(e.NewValue)); }));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        public static DependencyProperty HyperlinkColorProperty = DependencyProperty.Register("HyperlinkColor", typeof(SolidColorBrush), typeof(HtmlTextBlock), null);

        public SolidColorBrush HyperlinkColor
        {
            get { return GetValue(HyperlinkColorProperty) as SolidColorBrush; }
            set { SetValue(HyperlinkColorProperty, value); }
        }

    }
}
