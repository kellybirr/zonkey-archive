using System.Data;

namespace Zonkey.Dialects
{
    /// <summary>
    /// Provides properties and methods specific to Microsoft Access.
    /// </summary>
    public class AccessSqlDialect : SqlDialect
    {
        /// <summary>
        /// Gets the server-specific command to obtain the last inserted identity.
        /// </summary>
        public override string FormatAutoIncrementSelect(string sequenceName)
        {
            return "@@IDENTITY";
        }

        /// <summary>
        /// Gets the quoted identifiers mode settings.
        /// </summary>
        /// <value>The quoted identifiers setting.</value>
        public override QuotedIdentifiers QuotedIdentifiers
        {
            get { return QuotedIdentifiers.Preferred; }
        }

        /// <summary>
        /// Formats the name of the field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="useQuotedIdentifier">if set to <c>true</c> then formats the field name with quoted identifiers.</param>
        /// <returns>The formatted field name.</returns>
        public override string FormatFieldName(string name, bool? useQuotedIdentifier)
        {
            return (useQuotedIdentifier != false) ? string.Concat("[", name, "]") : name;
        }

        /// <summary>
        /// Formats the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="useQuotedIdentifier">if set to <c>true</c> then formats the table name with quoted identifiers.</param>
        /// <returns>The formatted table name.</returns>
        public override string FormatTableName(string tableName, string schemaName, bool? useQuotedIdentifier)
        {
            return (useQuotedIdentifier != false) ? string.Concat("[", tableName, "]") : tableName;
        }

        /// <summary>
        /// Formats the name of the paramter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="commandType">An instance of a <see cref="System.Data.CommandType"/>.</param>
        /// <returns>The formatted parameter name.</returns>
        public override string FormatParameterName(string name, CommandType commandType)
        {
            return "?";
        }

        /// <summary>
        /// Formats the name of the paramter.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="commandType">An instance of a <see cref="System.Data.CommandType"/>.</param>
        /// <returns>The formatted parameter name.</returns>
        public override string FormatParameterName(int index, CommandType commandType)
        {
            return "?";
        }
    }
}