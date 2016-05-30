/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// General purpose class for writing out JSON.  Can optionally format the output
    /// in a "pretty" format with carriage returns, white space, and indenting for ease of reading.
    /// </summary>
    public class JsonWriter : IDisposable
    {
        private int objectCount;
        private int arrayCount;
        private TextWriter writer;
        private bool pretty;
        private readonly string indent = "  ";
        private string curIndent = "";

        /// <summary>
        /// Initializes a new instance of the JsonWriter class that renders 
		/// "pretty" JSON.
        /// </summary>
        /// <param name="writer">The TextWriter instance to use to write out JSON.</param>
        /// <param name="indent">The number of spaces to use for indenting.</param>
        public JsonWriter(TextWriter writer, int indent)
        {
            this.writer = writer;
            this.pretty = true;

            StringBuilder sbIndent = new StringBuilder();
            for (int i = 0; i < indent; ++i)
                sbIndent.Append(" ");

            this.indent = sbIndent.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the JsonWriter class that writes to a 
		/// single compact line.
        /// </summary>
        /// <param name="writer">The TextWriter instance to use to write out 
		/// JSON.</param>
        public JsonWriter(TextWriter writer)
        {
            this.writer = writer;
            this.pretty = false;
        }

        /// <summary>
        /// The TextWriter instance used for writing out JSON.
        /// </summary>
        public TextWriter Writer
        {
            get { return writer; }
        }

        /// <summary>
        /// If true, line returns and indenting are added when writing out JSON for readability.
        /// If false, the JSON is written out in a single contiguous line.
        /// </summary>
        public bool Pretty
        {
            get { return pretty; }
        }

        /// <summary>
        /// Writes a left brace to start a new object.
        /// </summary>
        public void StartObject()
        {
            ++objectCount;
            writer.Write("{");

            if (pretty)
                IncreaseIndent();
        }

        /// <summary>
        /// Writes a left brace to start a new object.
        /// If Pretty=true the brace will written on a new line.
        /// </summary>
        public void StartObjectIndented()
        {
            WriteIndentedLine();
            StartObject();
        }

        /// <summary>
        /// Writes a comma followed by a left brace to start a new object.
        /// If Pretty=true the brace will written on a new line.
        /// </summary>
        public void StartAppendObjectIndented()
        {
            AddSeparatorIndented();
            StartObject();
        }

        /// <summary>
        /// Writes a right brace to end the current object.
        /// </summary>
        public void EndObject()
        {
            if (objectCount < 1)
                throw new Exception(Resources.Strings.ExceptionEndObjectCalledWithNoMatchingStartObject);

            --objectCount;

            if (pretty)
                DecreaseIndent();

            writer.Write("}");
        }

        /// <summary>
        /// Writes the closing right brace to the current object.
        /// If Pretty=true the brace will written on a new line.
        /// </summary>
        public void EndObjectIndented()
        {
            if (objectCount < 1)
                throw new Exception(Resources.Strings.ExceptionEndObjectIndentedCalledWithNoMatchingStartObject);

            --objectCount;

            if (pretty)
                DecreaseIndent();

            WriteIndentedLine();
            writer.Write("}");
        }

        /// <summary>
        /// Writes a left bracket to start a new array.
        /// </summary>
        public void StartArray()
        {
            ++arrayCount;
            writer.Write("[");

            if (pretty)
                IncreaseIndent();
        }

        /// <summary>
        /// Writes a left bracket to start a new array.
        /// If Pretty=true the bracket will written on a new line.
        /// </summary>
        public void StartArrayIndented()
        {
            StartArray();
            WriteIndentedLine();
        }

        /// <summary>
        /// Writes a right bracket to end the current array.
        /// </summary>
        public void EndArray()
        {
            if (arrayCount < 1)
                throw new Exception(Resources.Strings.ExceptionEndArrayCalledwithNoMatchingStartArray);

            --arrayCount;

            if (pretty)
                DecreaseIndent();

            writer.Write("]");
        }

        /// <summary>
        /// Writes the closing right bracket to the current array.
        /// If Pretty=true the bracket will written on a new line.
        /// </summary>
        public void EndArrayIndented()
        {
            if (arrayCount < 1)
                throw new Exception(Resources.Strings.ExceptionEndArrayIndentedCalledwithNoMatchingStartArray);

            --arrayCount;

            if (pretty)
                DecreaseIndent();

            WriteIndentedLine();
            writer.Write("]");
        }

        /// <summary>
        /// Writes the property name followed by a colon.
        /// </summary>
        public void StartProperty(string name)
        {
            if (objectCount < 1)
                throw new Exception(Resources.Strings.ExceptionStartPropertycalledOutsideOfAnObject);

            if (pretty)
                writer.Write(string.Format("\"{0}\" : ", name));
            else
                writer.Write(string.Format("\"{0}\":", name));
        }

        /// <summary>
        /// Writes the property name followed by a colon.
        /// If Pretty=true the property will be started on a new line.
        /// </summary>
        public void StartPropertyIndented(string name)
        {
            WriteIndentedLine();
            StartProperty(name);
        }

        /// <summary>
        /// Writes a comma followed by the property name followed by a colon.
        /// </summary>
        public void StartAppendProperty(string name)
        {
            AddSeparator();
            StartProperty(name);
        }

        /// <summary>
        /// Writes a comma followed by the property name followed by a colon.
        /// If Pretty=true the property will be started on a new line.
        /// </summary>
        public void StartAppendPropertyIndented(string name)
        {
            AddSeparatorIndented();
            StartProperty(name);
        }

        #region WriteProperty
        public void WriteProperty(string name, object value)
        {
            WriteProperty(name, value, true);
        }
		
		public void WriteProperty(string name, object value, bool useEmptyStringForNullValue)
		{
			if (objectCount < 1)
				throw new Exception(Resources.Strings.ExceptionWritePropertyCalledOutsideOfAnObject);
			StartProperty(name);
			WriteValue(value, useEmptyStringForNullValue);
		}
		
		public void WritePropertyIndented(string name, object value)
		{
			WriteIndentedLine();
			WriteProperty(name, value);
		}

		private void WriteValue(object o, bool useEmptyStringForNullValue)
		{
			if (o == null)
			{
				if (useEmptyStringForNullValue)
					Writer.Write("\"\"");
				else
					Writer.Write("null");
			}
			else if (o is int || o is double || o is float ||
				o is short || o is long || o is byte)
			{
				Writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", o));
			}
			else if (o is bool)
			{
				if ((bool)o)
					Writer.Write("true");
				else
					Writer.Write("false");
			}
			else if (o is DateTime)
			{
                DateTime time = (DateTime)o;
				Writer.Write(time.Ticks);
			}
			else if (o is Array && (o as Array).Rank == 2) //two dimensional arary
			{
				Array values = (Array)o;
				StartArray();
				for (int i = 0; i < values.GetLength(0); i++)
				{
					if (i > 0) AddSeparator();
					StartArray();
					for (int j = 0; j < values.GetLength(1); j++)
					{
						if (j > 0) AddSeparator();
						WriteValue(values.GetValue(i, j), false);
					}
					EndArray();
				}
				EndArray();
			}
			else if (!(o is string) && o is IEnumerable)
			{
				WriteArrayValue(o as IEnumerable);
			}
			else
			{
				Writer.Write(string.Format("\"{0}\"", JsonWriter.EncodeString(
					string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", o))));
			}
		}
		public void WriteArrayValue(IEnumerable array)
		{
			StartArray();
			IEnumerator enm = array.GetEnumerator();
			if (enm.MoveNext())
			{
				WriteValue(enm.Current, false);
				while(enm.MoveNext())
				{
					AddSeparator();
					WriteValue(enm.Current, false);
				}
			}
			EndArray();
		}

		#endregion

        #region AppendProperty
       
		public void AppendProperty(string name, object value)
		{
			if (objectCount < 1)
				throw new Exception(Resources.Strings.ExceptionAppendPropertyCalledOutsideOfAnObject);

			AddSeparator();
			WriteProperty(name, value);
		}

		public void AppendPropertyIndented(string name, object value)
		{
			if (objectCount < 1)
				throw new Exception(Resources.Strings.ExceptionAppendPropertyCalledOutsideOfAnObject);

			AddSeparatorIndented();
			WriteProperty(name, value);
		}
       
        #endregion

        protected void AddSeparator()
        {
            writer.Write(",");

            if (pretty)
                writer.Write(" ");
        }

        protected void AddSeparatorIndented()
        {
            AddSeparator();
            WriteIndentedLine();
        }

        protected void IncreaseIndent()
        {
            curIndent += indent;
        }

        protected void DecreaseIndent()
        {
            curIndent = curIndent.Substring(0, curIndent.Length - indent.Length);
        }

        public void WriteIndentedLine()
        {
            if (pretty)
            {
                writer.WriteLine();
                writer.Write(curIndent);
            }
        }
		
        #region IDisposable Members

        public void Dispose()
        {
            writer.Flush();
            writer.Close();
        }

        #endregion

        #region JSON Formatting

        // From MS Ajax
        private static string EncodeString(string value)
        {
            System.Text.StringBuilder b = null;
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '\r' || c == '\t' || c == '\"' || c == '\'' || c == '<' || c == '>' ||
                       c == '\\' || c == '\n' || c == '\b' || c == '\f' || c < ' ')
                {
                    if (b == null)
                    {
                        b = new System.Text.StringBuilder(value.Length + 5);
                    }
                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }
                    startIndex = i + 1;
                    count = 0;
                }
                switch (c)
                {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\b':
                        b.Append("\\b");
                        break;
                    case '\f':
                        b.Append("\\f");
                        break;
                    case '\'':
                    case '>':
                    case '<':
                        AppendCharAsUnicode(b, c);
                        break;
                    default:
                        if (c < ' ')
                        {
                            AppendCharAsUnicode(b, c);
                        }
                        else
                        {
                            count++;
                        }
                        break;
                }
            }

            if (b == null)
            {
                return value;
            }

            if (count > 0)
            {
                b.Append(value, startIndex, count);
            }

            return b.ToString();
        }

        private static void AppendCharAsUnicode(System.Text.StringBuilder builder, char c)
        {
            builder.Append("\\u");
            builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:x4}", (int)c);
        }

        #endregion
    }
}
