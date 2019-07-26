namespace Zonkey
{
    /// <summary>
    /// Provides a class for describing conflicts when executing commands on a database
    /// </summary>
    public class Conflict
    {
        /// <summary>
        /// Default constructor - initializes a new instance of the <see cref="Conflict"/> class.
        /// </summary>
        public Conflict()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Conflict"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="originalValue">The original value.</param>
        /// <param name="currentDBValue">The current DB value.</param>
        /// <param name="attemptedValue">The attempted value.</param>
        public Conflict(string propertyName, object originalValue, object currentDBValue, object attemptedValue)
        {
            _propertyName = propertyName;
            _originalValue = originalValue;
            _currentDBValue = currentDBValue;
            _attemptedValue = attemptedValue;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        private string _propertyName;

        /// <summary>
        /// Gets or sets the original value.
        /// </summary>
        /// <value>The original value.</value>
        public object OriginalValue
        {
            get { return _originalValue; }
            set { _originalValue = value; }
        }

        private object _originalValue;

        /// <summary>
        /// Gets or sets the current DB value.
        /// </summary>
        /// <value>The current DB value.</value>
        public object CurrentDBValue
        {
            get { return _currentDBValue; }
            set { _currentDBValue = value; }
        }

        private object _currentDBValue;

        /// <summary>
        /// Gets or sets the attempted value.
        /// </summary>
        /// <value>The attempted value.</value>
        public object AttemptedValue
        {
            get { return _attemptedValue; }
            set { _attemptedValue = value; }
        }

        private object _attemptedValue;

        /// <summary>
        /// Gets or sets the set value to.
        /// </summary>
        /// <value>The set value to.</value>
        public object SetValueTo
        {
            get { return _setValueTo; }
            set { _setValueTo = value; }
        }

        private object _setValueTo;
    }
}