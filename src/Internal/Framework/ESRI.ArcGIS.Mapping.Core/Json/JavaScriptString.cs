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
using System.Globalization;

namespace ESRI.ArcGIS.Client.Utils
{
    internal class JavaScriptString
    {
        // Fields
        private int _index;
        private string _s;

        // Methods
        internal JavaScriptString(string s)
        {
            this._s = s;
        }

        private static void AppendCharAsUnicode(StringBuilder builder, char c)
        {
            builder.Append(@"\u");
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:x4}", new object[] { (int)c });
        }

        internal string GetDebugString(string message)
        {
            return string.Concat(new object[] { message, " (", this._index, "): ", this._s });
        }

        internal char? GetNextNonEmptyChar()
        {
            while (this._s.Length > this._index)
            {
                char c = this._s[this._index++];
                if (!char.IsWhiteSpace(c))
                {
                    return new char?(c);
                }
            }
            return null;
        }

        internal char? MoveNext()
        {
            if (this._s.Length > this._index)
            {
                return new char?(this._s[this._index++]);
            }
            return null;
        }

        internal string MoveNext(int count)
        {
            if (this._s.Length >= (this._index + count))
            {
                string str = this._s.Substring(this._index, count);
                this._index += count;
                return str;
            }
            return null;
        }

        internal void MovePrev()
        {
            if (this._index > 0)
            {
                this._index--;
            }
        }

        internal void MovePrev(int count)
        {
            while ((this._index > 0) && (count > 0))
            {
                this._index--;
                count--;
            }
        }

        internal static string QuoteString(string value)
        {
            StringBuilder builder = null;
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((((c == '\r') || (c == '\t')) || ((c == '"') || (c == '\''))) || ((((c == '<') || (c == '>')) || ((c == '\\') || (c == '\n'))) || (((c == '\b') || (c == '\f')) || (c < ' '))))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(value.Length + 5);
                    }
                    if (count > 0)
                    {
                        builder.Append(value, startIndex, count);
                    }
                    startIndex = i + 1;
                    count = 0;
                }
                switch (c)
                {
                    case '<':
                    case '>':
                    case '\'':
                        {
                            AppendCharAsUnicode(builder, c);
                            continue;
                        }
                    case '\\':
                        {
                            builder.Append(@"\\");
                            continue;
                        }
                    case '\b':
                        {
                            builder.Append(@"\b");
                            continue;
                        }
                    case '\t':
                        {
                            builder.Append(@"\t");
                            continue;
                        }
                    case '\n':
                        {
                            builder.Append(@"\n");
                            continue;
                        }
                    case '\f':
                        {
                            builder.Append(@"\f");
                            continue;
                        }
                    case '\r':
                        {
                            builder.Append(@"\r");
                            continue;
                        }
                    case '"':
                        {
                            builder.Append("\\\"");
                            continue;
                        }
                }
                if (c < ' ')
                {
                    AppendCharAsUnicode(builder, c);
                }
                else
                {
                    count++;
                }
            }
            if (builder == null)
            {
                return value;
            }
            if (count > 0)
            {
                builder.Append(value, startIndex, count);
            }
            return builder.ToString();
        }

        internal static string QuoteString(string value, bool addQuotes)
        {
            string str = QuoteString(value);
            if (addQuotes)
            {
                str = "\"" + str + "\"";
            }
            return str;
        }

        public override string ToString()
        {
            if (this._s.Length > this._index)
            {
                return this._s.Substring(this._index);
            }
            return string.Empty;
        }
    }
}
