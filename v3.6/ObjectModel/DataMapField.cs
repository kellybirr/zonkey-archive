using System;
using System.Data;
using System.Reflection;

namespace Zonkey.ObjectModel
{
    /// <summary>
    /// Provides properties and methods to map a property in a <see cref="Zonkey.ObjectModel.DataClass"/> to a field in database table.
    /// </summary>
    public sealed class DataMapField : IDataMapField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFieldAttribute"/> class.
        /// </summary>
        /// <param name="pi">The PropertyInfo Object for the field</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        public DataMapField(PropertyInfo pi, string fieldName, DbType dataType, bool isNullable)
        {
            Property = pi;
            FieldName = fieldName;
            DataType = dataType;
            IsNullable = isNullable;

            Length = -1;
            AccessType = AccessType.ReadWrite;
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <value>The name of the field.</value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public DbType DataType { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is key field.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is key field; otherwise, <c>false</c>.
        /// </value>
        public bool IsKeyField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is row version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is row version; otherwise, <c>false</c>.
        /// </value>
        public bool IsRowVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is auto increment.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is auto increment; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoIncrement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is nullable; otherwise, <c>false</c>.
        /// </value>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is comparable using (=) in a where clause.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this field is comparable; otherwise, <c>false</c>.
        /// </value>
        public bool IsComparable
        {
            get { return _isComparable ?? ((DataType != DbType.Xml) && ((DataType != DbType.Binary) || (Length < Int32.MaxValue))); }
            set { _isComparable = value; }
        }
        private bool? _isComparable;

        /// <summary>
        /// Gets or sets the type of the access.
        /// </summary>
        /// <value>The type of the access.</value>
        public AccessType AccessType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use quoted identifier].
        /// </summary>
        /// <value><c>true</c> if [use quoted identifier]; otherwise, <c>false</c>.</value>
        public bool? UseQuotedIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the version of the table schema that contains this field
        /// </summary>
        public int SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the sequence for an auto-increment column.
        /// </summary>
        /// <value>The name of the sequence.</value>
        public string SequenceName { get; set; }

        /// <summary>
        /// Gets or sets the property info object.
        /// </summary>
        /// <value>The property info object.</value>
        public PropertyInfo Property { get; set; }
    }
}