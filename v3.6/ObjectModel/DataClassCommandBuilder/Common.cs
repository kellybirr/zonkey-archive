using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Zonkey.ObjectModel
{
    /// <summary>
    /// Provides methods to create SQL commands from a <see cref="Zonkey.ObjectModel.DataClass"/>.
    /// </summary>
    public partial class DataClassCommandBuilder
    {
        private readonly Type _dataObjectType;
        private readonly DataMap _dataMap;

        private readonly DbConnection _connection;
        private readonly Dialects.SqlDialect _dialect;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassCommandBuilder"/> class.
        /// </summary>
        /// <param name="type">The type of object.</param>
        /// <param name="map">The data map</param>
        /// <param name="connection">The connection.</param>
        public DataClassCommandBuilder(Type type, DataMap map, DbConnection connection) : this(type, map, connection, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassCommandBuilder"/> class.
        /// </summary>
        /// <param name="type">The type of object.</param>
        /// <param name="map">The data map</param>
        /// <param name="connection">The connection.</param>
        /// <param name="dialect">The sql dialect.</param>
        public DataClassCommandBuilder(Type type, DataMap map, DbConnection connection, Dialects.SqlDialect dialect)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (connection == null) throw new ArgumentNullException("connection");
            if (map == null) throw new ArgumentNullException("map");

            if (map.DataItem == null)
                throw new ArgumentException("Invalid Data Map: Missing DataItem");

            _connection = connection;
            _dialect = (dialect ?? Dialects.SqlDialect.Create(connection));

            _dataObjectType = type;
            _dataMap = map;
        }

        /// <summary>
        /// Creates the set param.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private DbParameter CreateSetParam(DbCommand command, IDataMapField field)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = _dialect.FormatParameterName(field.FieldName, command.CommandType);
            param.SourceColumn = field.FieldName;

            if ((param is SqlParameter) && (field.DataType == DbType.Time))   // hack M$ issue
                ((SqlParameter)param).SqlDbType = SqlDbType.Time;
            else
                param.DbType = field.DataType;
            
            if (field.Length >= 0) param.Size = field.Length;

            return param;
        }

        /// <summary>
        /// Creates the where param.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private DbParameter CreateWhereParam(DbCommand command, IDataMapField field)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = _dialect.FormatParameterName("old_" + field.FieldName, command.CommandType);
            param.SourceColumn = field.FieldName;

            if ((param is SqlParameter) && (field.DataType == DbType.Time))   // hack M$ issue
                ((SqlParameter)param).SqlDbType = SqlDbType.Time;
            else
                param.DbType = field.DataType;

            if (field.Length >= 0) param.Size = field.Length;

            return param;
        }

        /// <summary>
        /// Determines whether the field value has changed.
        /// </summary>
        /// <param name="pi">The property to check.</param>
        /// <param name="obj">The object instance.</param>
        /// <returns>
        /// 	<c>true</c> if field value has changed; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasFieldChanged(PropertyInfo pi, ISavable obj)
        {
            object originalValue;
            if (! obj.OriginalValues.TryGetValue(pi.Name, out originalValue)) 
                return false;
            
            object currentValue = pi.GetValue(obj, null);
            if (currentValue == null) return (originalValue != null);
            
            return (! currentValue.Equals(originalValue));
        }

        /// <summary>
        /// Gets the text command.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private DbCommand GetTextCommand(string text)
        {
            DbCommand command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = text;
            command.Connection = _connection;
            return command;
        }

        private string BuildWhereClauseFromKeys(DbCommand command)
        {
            var whereString = new StringBuilder();
            foreach (IDataMapField field in _dataMap.KeyFields)
            {
                DbParameter whereParam = CreateWhereParam(command, field);

                if (whereString.Length > 0) whereString.Append(" AND ");
                whereString.Append(_dialect.FormatFieldName(field.FieldName, (field.UseQuotedIdentifier ?? UseQuotedIdentifier)));
                whereString.Append(" = ");
                whereString.Append(whereParam.ParameterName);

                command.Parameters.Add(whereParam);
            }
            return whereString.ToString();
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                    _tableName = _dialect.FormatTableName(_dataMap.DataItem.TableName, _dataMap.DataItem.SchemaName,
                                        (_dataMap.DataItem.UseQuotedIdentifier ?? UseQuotedIdentifier) );
                return _tableName;
            }
            set { _tableName = value; }
        }
        private string _tableName;

        /// <summary>
        /// Gets the name of the table to perform the save operation on.
        /// </summary>
        /// <value>The save to table.</value>
        public string SaveToTable
        {
            get
            {
                if (string.IsNullOrEmpty(_saveToTable))
                    _saveToTable = _dialect.FormatTableName(_dataMap.DataItem.SaveToTable, _dataMap.DataItem.SchemaName, 
                                        (_dataMap.DataItem.UseQuotedIdentifier ?? UseQuotedIdentifier) );

                return _saveToTable;
            }
            set { _saveToTable = value; }
        }
        private string _saveToTable;

        /// <summary>
        /// Gets or sets the default value used when inserting null reference strings.
        /// </summary>
        /// <value>The null string default.</value>
        public object NullStringDefault
        {
            get { return _nullStringDefault; }
            set { _nullStringDefault = value; }
        }
        private object _nullStringDefault = DBNull.Value;

        /// <summary>
        /// Gets or sets the use quoted identifiers.
        /// </summary>
        /// <value>The use quoted identifiers.</value>
        public bool? UseQuotedIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the use table with field names.
        /// </summary>
        /// <value>The use table with field names.</value>
        public bool UseTableWithFieldNames
        {
            get { return _useTableWithFieldNames; }
            set
            {
                _useTableWithFieldNames = value;
                _builtColumnsStr = null;
            }
        }
        private bool _useTableWithFieldNames;

    }
}