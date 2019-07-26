using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zonkey.Text
{
    /// <summary>
    /// Writes classes as text records to a file or stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TextClassWriter<T> : TextClassRWBase<T>
        where T: class
    {
        protected TextWriter Output;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public TextClassWriter(string path)
        {
            Output = new StreamWriter(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        public TextClassWriter(string path, bool append)
        {
            Output = new StreamWriter(path, append);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        /// <param name="encoding">The encoding.</param>
        public TextClassWriter(string path, bool append, Encoding encoding)
        {
            Output = new StreamWriter(path, append, encoding);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public TextClassWriter(TextWriter output)
        {
            Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public TextClassWriter(Stream stream)
        {
            Output = new StreamWriter(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoding">The encoding.</param>
        public TextClassWriter(Stream stream, Encoding encoding)
        {
            Output = new StreamWriter(stream, encoding);
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="TextClassWriter&lt;T&gt;"/> class.
		/// </summary>
		protected TextClassWriter()
		{
			// for overrides	
		}

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            try
            {
				if (Output != null)
					Output.Dispose();
            }
            catch (ObjectDisposedException) { }
            finally
            {
                // Call Dispose on your base class.
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Flushes the data to the underlying stream or disk.
        /// </summary>
        public void Flush()
        {
            Output.Flush();
        }

        /// <summary>
        /// Writes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void Write(T obj)
        {
            // init if necessary
            if (! Initialized) Initialize();

            // write record
            if (RecordType == TextRecordType.Delimited)
                WriteInternal_Delimited(obj);
            else
                WriteInternal_Fixed(obj);
        }

        /// <summary>
        /// Writes the specified obj.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void Write(IEnumerable<T> collection)
        {
            foreach (var obj in collection)
                Write(obj);
        }

        private void WriteInternal_Delimited(T obj)
        {
            var sb = new StringBuilder();
            foreach (var field in FieldArray)
            {
                if (sb.Length > 0) sb.Append(Delimiter);

                string sValue = GetFieldValue(field, obj);
                if ( (field.Property.PropertyType == typeof(string)) || (field.Property.PropertyType == typeof(char)) )
                {
                    sb.Append(TextQualifier);
                    sb.Append(sValue);
                    sb.Append(TextQualifier);
                }
                else
                    sb.Append(sValue);
            }

            Output.WriteLine(sb.ToString());
        }

        private void WriteInternal_Fixed(T obj)
        {
            var buffer = (new String(' ', RecordLength)).ToCharArray();
            foreach (var field in FieldArray)
            {
                var sValue = GetFieldValue(field, obj).ToCharArray();
                Array.Copy(sValue, 0, buffer, field.Position, Math.Min(sValue.Length, field.Length));
            }

            Output.WriteLine(buffer, 0, RecordLength);
        }

        private static string GetFieldValue(ITextField field, T obj)
        {
            var pi = field.Property;

            object oValue = pi.GetValue(obj, null);
            if (oValue == null) return string.Empty;

            if (oValue is Boolean) return field.FormatBoolean((bool)oValue);

            if ( (! string.IsNullOrEmpty(field.OutputFormat)) && (oValue is IFormattable) )
                return ((IFormattable)oValue).ToString(field.OutputFormat, null);

            return oValue.ToString();
        }

		protected override void PostInitialize()
		{
			if (NewLine != null)
				Output.NewLine = NewLine;

			base.PostInitialize();
		}
    }
}
