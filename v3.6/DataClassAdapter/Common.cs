using System;
using System.Data;
using System.Data.Common;
using Zonkey.ObjectModel;

namespace Zonkey
{
    public partial class DataClassAdapter<T> : AdapterBase
        where T : class, new()
    {
        private readonly Type _objectType;
        private readonly DataMap _dataMap;
        private bool _cancelFill;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        public DataClassAdapter(DbConnection connection)
        {
            _objectType = typeof (T);
            _dataMap = DataMap.GenerateCached(_objectType, null, null, DataClassAdapter.DefaultSchemaVersion ?? 0);
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        /// <param name="schemaVersion">Version of table schema to use</param>
        public DataClassAdapter(DbConnection connection, int schemaVersion)
        {
            _objectType = typeof(T);
            _dataMap = DataMap.GenerateCached(_objectType, null, null, schemaVersion);
            Connection = connection;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        /// <param name="map">the data map</param>
        public DataClassAdapter(DbConnection connection, DataMap map)
        {
            _objectType = typeof (T);
            _dataMap = map;
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        /// <param name="tableName">Name of the table to use for the data mapping.</param>
        public DataClassAdapter(DbConnection connection, string tableName)
        {
            _objectType = typeof(T);
            _dataMap = DataMap.GenerateCached(_objectType, tableName, null, DataClassAdapter.DefaultSchemaVersion ?? 0);
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        /// <param name="tableName">Name of the table to use for the data mapping.</param>
        /// <param name="schemaVersion">Version of table schema to use</param>
        public DataClassAdapter(DbConnection connection, string tableName, int schemaVersion)
        {
            _objectType = typeof(T);
            _dataMap = DataMap.GenerateCached(_objectType, tableName, null, schemaVersion);
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        /// <param name="tableName">Name of the table to use for the data mapping.</param>
        /// <param name="keyFields">Names of the key fields in the table/object</param>
        public DataClassAdapter(DbConnection connection, string tableName, string[] keyFields)
        {
            _objectType = typeof(T);
            _dataMap = DataMap.GenerateCached(_objectType, tableName, keyFields, DataClassAdapter.DefaultSchemaVersion ?? 0);
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">Database Connection to be used by the adapter</param>
        /// <param name="tableName">Name of the table to use for the data mapping.</param>
        /// <param name="keyFields">Names of the key fields in the table/object</param>
        /// <param name="schemaVersion">Version of table schema to use</param>
        public DataClassAdapter(DbConnection connection, string tableName, string[] keyFields, int schemaVersion)
        {
            _objectType = typeof(T);
            _dataMap = DataMap.GenerateCached(_objectType, tableName, keyFields, schemaVersion);
            Connection = connection;
        }

        /// <summary>
        /// Default constructor - must assign connection before use
        /// </summary>
        public DataClassAdapter()
        {
            _objectType = typeof(T);
            _dataMap = DataMap.GenerateCached(_objectType, null, null, 0);
        }

        /// <summary>
        /// The command builder object
        /// </summary>
        /// <value>The command builder.</value>
        protected internal DataClassCommandBuilder CommandBuilder
        {
            get
            {
                if (_commandBuilder == null)
                    _commandBuilder = new DataClassCommandBuilder(_objectType, _dataMap, Connection, SqlDialect)
                                          {
                                              UseQuotedIdentifier = DataClassAdapter.DefaultQuotedIdentifier
                                          };

                return _commandBuilder;
            }            
        }
        private DataClassCommandBuilder _commandBuilder;

        /// <summary>
        /// Gets or Sets a value that controls the record sorting for Fill operations.
        /// </summary>
        /// <value>The SQL ORDER BY clause.</value>
        public string OrderBy
        {
            get { return _sortStr; }
            set { _sortStr = value; }
        }
        private string _sortStr;

        /// <summary>
        /// Gets or sets the default value used when inserting null reference strings.
        /// </summary>
        /// <value>The null string default.</value>
        public object NullStringDefault
        {
            get { return _nullStringDefault; }
            set
            {
                _nullStringDefault = value;
                CommandBuilder.NullStringDefault = value;
            }
        }
        private object _nullStringDefault = DBNull.Value;

        /// <summary>
        /// The FillProgress Event will fire every time this number of
        /// items are added to the collection.
        /// </summary>
        /// <value>The progress increment.</value>
        public int ProgressIncrement
        {
            get { return _progressIncrement; }
            set { _progressIncrement = value; }
        }
        private int _progressIncrement = -1;

        /// <summary>
        /// Fires during any Fill operation for every (n) items based on ProgressIncrement.
        /// </summary>
        public event EventHandler<FillStatusEventArgs> FillProgress;

        /// <summary>
        /// Fires at the end of a Fill*Async Operation.
        /// </summary>
        public event EventHandler<FillStatusEventArgs> FillAsyncComplete;

        /// <summary>
        /// Fire before a command is executed against the database
        /// </summary>
        public event EventHandler<CommandExecuteEventArgs> BeforeExecuteCommand;

        /// <summary>
        /// Gets or Sets the CommandTimeout for all DbCommands used internally by the DataClassAdapter
        /// </summary>
        public int? CommandTimeout { get; set; }

        /// <summary>
        /// Called when SqlDialect changes.
        /// </summary>
        protected override void OnDialectChanged()
        {
            _commandBuilder = null;
        }

        /// <summary>
        /// Sets a non-public property for advanced scenarios.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to assign.</param>
        public void SetProperty(AdapterProperty propertyName, object value)
        {
            switch (propertyName)
            {
                case AdapterProperty.TableName:
                    CommandBuilder.TableName = (string)value;
                    return;
                case AdapterProperty.SaveToTable:
                    CommandBuilder.SaveToTable = (string)value;
                    return;
                case AdapterProperty.UseQuotedIdentifiers:
                    CommandBuilder.UseQuotedIdentifier = (bool?)value;
                    return;
            }
        }

        private void SetCommandTimeout(IDbCommand command)
        {
            if (CommandTimeout.HasValue)
                command.CommandTimeout = CommandTimeout.Value;
            else if (DataClassAdapter.DefaultCommandTimeout.HasValue)
                command.CommandTimeout = DataClassAdapter.DefaultCommandTimeout.Value;
        }

        private DbDataReader ExecuteReaderInternal(DbCommand command, CommandBehavior behavior)
        {
            SetCommandTimeout(command);
			DbTransactionRegistry.SetCommandTransaction(command);

            if (BeforeExecuteCommand != null)
            {
                var args = new CommandExecuteEventArgs(command);
                BeforeExecuteCommand(this, args);

                if (args.Cancel)
                    throw new OperationCanceledException("Command Execution Canceled");
            }

            return command.ExecuteReader(behavior);
        }

        private int ExecuteNonQueryInternal(DbCommand command)
        {
            SetCommandTimeout(command);
			DbTransactionRegistry.SetCommandTransaction(command);

            if (BeforeExecuteCommand != null)
            {
                var args = new CommandExecuteEventArgs(command);
                BeforeExecuteCommand(this, args);

                if (args.Cancel)
                    throw new OperationCanceledException("Command Execution Canceled");
            }

            return command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Static class to support static methods on the data class adapter
    /// </summary>
    public static class DataClassAdapter
    {
        /// <summary>
        /// Gets or Sets the CommandTimeout for all DbCommands used internally by the any DataClassAdapter 
        /// that does not set it's CommandTimeout
        /// </summary>
        public static int? DefaultCommandTimeout { get; set; }
        
        /// <summary>
        /// Gets or Sets the SchemaVersion for all DataClassAdapters in the current process
        /// </summary>
        public static int? DefaultSchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the default quoted identifiers value.
        /// </summary>
        /// <value>The default quoted identifiers.</value>
        public static bool? DefaultQuotedIdentifier { get; set; }
    }
}