using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Zonkey.ObjectModel
{
    /// <summary>
    /// Provides a thread-safe implementation of a generic <see cref="System.Collections.Generic.Dictionary&lt;T, T&gt;"/>
    /// </summary>
    [Serializable]
    public class FieldValuesDictionary : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValuesDictionary"/> class.
        /// </summary>
        public FieldValuesDictionary() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValuesDictionary"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public FieldValuesDictionary(DbDataReader reader) : base()
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            lock (this)
            {
                for (int i = 0; i < reader.VisibleFieldCount; i++)
                    Add(reader.GetName(i), reader[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValuesDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"></see> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param>
        protected FieldValuesDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    } ;
}