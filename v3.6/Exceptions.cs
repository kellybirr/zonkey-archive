using System;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;

namespace Zonkey
{
    /// <summary>
    /// Thrown when an update in insert method fails
    /// </summary>
    [Serializable]
    public class SaveFailedException : DataException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFailedException"/> class.
        /// </summary>
        public SaveFailedException() { }

        /// <summary>
        /// Initializes a new instance with specified message
        /// </summary>
        /// <param name="message">The exception message</param>
        public SaveFailedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance with specified message and inner exception
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public SaveFailedException(string message, Exception innerException) : base(message, innerException) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFailedException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected SaveFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }

    /// <summary>
    /// Thrown when an update method fails due to a data conflict
    /// </summary>
    [Serializable]
    public class UpdateConflictException : DataException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateConflictException"/> class.
        /// </summary>
        public UpdateConflictException() { }

        /// <summary>
        /// Initializes a new instance with specified message
        /// </summary>
        /// <param name="message">The exception message</param>
        public UpdateConflictException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance with specified message and inner exception
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public UpdateConflictException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateConflictException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected UpdateConflictException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    
    }

    /// <summary>
    /// Thrown when an update method fails due to a data conflict
    /// </summary>
    [Serializable]
    public class CollectionSaveException<T> : DataException
    {
        private const string DEFAULT_ERROR_MESSAGE = "Errors occurred durring the collection save";
		
		/// <summary>
		/// Gets or sets the results.
		/// </summary>
		/// <value>The results.</value>
        public CollectionSaveResult<T> Results { get; private set; }

        /// <summary>
		/// Initializes a new instance of the <see cref="CollectionSaveException&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        public CollectionSaveException(CollectionSaveResult<T> results) 
            : base(DEFAULT_ERROR_MESSAGE)
        {
            Results = results;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionSaveException&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="innerException">The inner exception.</param>
        public CollectionSaveException(CollectionSaveResult<T> results, Exception innerException)
            : base(DEFAULT_ERROR_MESSAGE, innerException)
        {
            Results = results;
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="CollectionSaveException&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected CollectionSaveException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }

	/// <summary>
	/// Thrown when a record read fails on a porperty
	/// </summary>
	[Serializable]
	public class PropertyReadException : DataException
	{
		private const string DEFAULT_ERROR_MESSAGE = "Error reading property '{0}' of class '{1}'";

		/// <summary>
		/// Gets or sets the name of the property.
		/// </summary>
		/// <value>The name of the property.</value>
		public PropertyInfo Property { get; set; }

		/// <summary>
		/// Gets or sets the field value.
		/// </summary>
		/// <value>The field value.</value>
		public object FieldValue { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyReadException"/> class.
		/// </summary>
		/// <param name="prop">The property being read.</param>
		/// <param name="value">The value being set.</param>
		/// <param name="innerException">The inner exception.</param>
		internal PropertyReadException(PropertyInfo prop, object value, Exception innerException)
			: base(string.Format(DEFAULT_ERROR_MESSAGE, prop.Name, prop.DeclaringType.FullName), innerException)
		{
			Property = prop;
			FieldValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyReadException"/> class.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected PropertyReadException(SerializationInfo info, StreamingContext context) : base(info, context) { }

	}

}
