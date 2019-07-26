using System.Data.Common;

namespace Zonkey
{
    /// <summary>
    /// Provides the inherited structure that facilitates the interaction of a class and a database.
    /// </summary>
    public abstract class AdapterBase
    {
        /// <summary>
        /// Database Connection to be used by the adapter
        /// </summary>
        /// <value>A <see cref="System.Data.Common.DbConnection"/> object containing the SQL-specific connection.</value>
        public DbConnection Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;

                if (_connection != null)
                    SqlDialect = Dialects.SqlDialect.Create(_connection);
                else
                    SqlDialect = null;

                OnConnectionChanged();
            }
        }

        private DbConnection _connection;

        /// <summary>
        /// Called when Connection Changes.
        /// </summary>
        protected virtual void OnConnectionChanged()
        {
        }

        /// <summary>
        /// Gets or sets the SQL dialect.
        /// </summary>
        /// <value>The SQL dialect.</value>
        public Dialects.SqlDialect SqlDialect
        {
            get { return _dialect; }
            set
            {
                _dialect = value;

                OnDialectChanged();
            }
        }

        private Dialects.SqlDialect _dialect;

        /// <summary>
        /// Called when SqlDialect changes.
        /// </summary>
        protected virtual void OnDialectChanged()
        {
        }

        /// <summary>
        /// Prefix used for command parameters
        /// Format: FilterValueA = $0 AND FilterValueB = $1
        /// where '$' is the default ParemeterPrefix
        /// </summary>
        /// <value>The parameter prefix.</value>
        public virtual char ParameterPrefix
        {
            get { return _parameterPrefix; }
            set { _parameterPrefix = value; }
        }

        private char _parameterPrefix = '$';
    }
}