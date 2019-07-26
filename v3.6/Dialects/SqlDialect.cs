using System;
using System.Data;
using System.Data.Common;

namespace Zonkey.Dialects
{
    /// <summary>
    /// Base class describing properties and methods for specific Database servers
    /// </summary>
    public abstract class SqlDialect
    {
        /// <summary>
        /// Creates the proper SqlDialect form the specified DbConnection.
        /// </summary>
        /// <param name="connection">The DbConnection.</param>
        /// <returns></returns>
        public static SqlDialect Create(DbConnection connection)
        {
            if (connection == null) return null;

            switch (connection.GetType().FullName)
            {
                case "CoreLab.MySql.MySqlConnection":
                case "Devart.Data.MySql.MySqlConnection":
                    return new MySqlDialect();
                case "System.Data.SqlClient.SqlConnection":
                    return new SqlServerDialect();
                case "System.Data.OracleClient.OracleConnection":
                    return new OracleSqlDialect();
                case "IBM.Data.DB2.DB2Connection":
                    return new DB2SqlDialect();
                case "Npgsql.NpgsqlConnection":
                    return new PostgrSqlDialect();
                case "System.Data.OleDb.OleDbConnection":
                    switch (((System.Data.OleDb.OleDbConnection)connection).Provider)
                    {
                        case "Microsoft.Jet.OLEDB.3.5.1":
                        case "Microsoft.Jet.OLEDB.4.0":
                            return new AccessSqlDialect();
                        default:
                            return new GenericSqlDialect();
                    }
                default:
                    return new GenericSqlDialect();
            }
        }

        /// <summary>
        /// Gets a value indicating whether [supports row version].
        /// </summary>
        /// <value><c>true</c> if [supports row version]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsRowVersion
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports schema].
        /// </summary>
        /// <value><c>true</c> if [supports schema]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsSchema
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports limit].
        /// </summary>
        /// <value><c>true</c> if [supports limit]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsLimit
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports S procs].
        /// </summary>
        /// <value><c>true</c> if [supports S procs]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsStoredProcedures
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether [use SQL batches].
        /// </summary>
        /// <value><c>true</c> if [use SQL batches]; otherwise, <c>false</c>.</value>
        public virtual bool UseSqlBatches
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether [use named parameters].
        /// </summary>
        /// <value><c>true</c> if [use named parameters]; otherwise, <c>false</c>.</value>
        public virtual bool UseNamedParameters
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the quoted identifiers mode settings.
        /// </summary>
        /// <value>The quoted identifiers setting.</value>
        public virtual QuotedIdentifiers QuotedIdentifiers
        {
            get { return QuotedIdentifiers.NotSupported; }
        }

        /// <summary>
        /// Gets the last identity var.
        /// </summary>
        /// <value>The last identity var.</value>
        public virtual string FormatAutoIncrementSelect(string sequenceName)
        {
            throw new NotSupportedException("This SQL dialect does not support the LastIdentity feature.");
        }

        /// <summary>
        /// Formats the limit query.
        /// </summary>
        /// <param name="columnString">The column string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereText">The where text.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public virtual string FormatLimitQuery(string columnString, string tableName, string whereText, string orderBy, int start, int length)
        {
            throw new NotSupportedException("This SQL dialect does not support the FormatLimitQuery feature.");
        }

        /// <summary>
        /// Formats the name of the field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual string FormatFieldName(string name)
        {
            return FormatFieldName(name, null);
        }

        /// <summary>
        /// Formats the name of the field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="useQuotedIdentifier">if set to <c>true</c> [use quoted identifier].</param>
        /// <returns></returns>
        public abstract string FormatFieldName(string name, bool? useQuotedIdentifier);

        /// <summary>
        /// Formats the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FormatTable")]
        public virtual string FormatTableName(string tableName, string schemaName)
        {
            return FormatTableName(tableName, schemaName, null);
        }

        /// <summary>
        /// Formats the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="useQuotedIdentifier">if set to <c>true</c> [use quoted identifier].</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "FormatTable")]
        public abstract string FormatTableName(string tableName, string schemaName, bool? useQuotedIdentifier);

        /// <summary>
        /// Formats the name of the paramter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns></returns>
        public abstract string FormatParameterName(string name, CommandType commandType);

        /// <summary>
        /// Formats the name of the paramter.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns></returns>
        public abstract string FormatParameterName(int index, CommandType commandType);
    }
}