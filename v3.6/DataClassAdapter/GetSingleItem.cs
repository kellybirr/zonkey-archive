using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Zonkey.ObjectModel;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        /// <summary>
        /// Gets the single item.
        /// </summary>
        /// <param name="filterExpression">The filter expression.</param>
        /// <returns>A value of type T.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public T GetSingleItem(Expression<Func<T, bool>> filterExpression)
        {
            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling GetSingleItem()");

            var parser = new WhereExpressionParser<T>(_dataMap, SqlDialect) { UseQuotedIdentifier = CommandBuilder.UseQuotedIdentifier };
            var result = parser.Parse(filterExpression);

            DbCommand command = PrepCommandForSelect(result.SqlText, FillMethod.FilterText, result.Parameters);
            return GetSingleItem(command);
        }

        /// <summary>
        /// Gets the single item.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>A value of type T.</returns>
        public T GetSingleItem(params SqlFilter[] filters)
        {
            if ((filters == null) || (filters.Length == 0))
                throw new ArgumentNullException("filters");

            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling GetSingleItem()");

            DbCommand command = PrepCommandForSelect(string.Empty, FillMethod.FilterArray, filters);
            return GetSingleItem(command);
        }

        /// <summary>
        /// Gets the single item.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>A value of type T.</returns>
        public T GetSingleItem(string filter)
        {
            return GetSingleItem(filter, null);
        }

        /// <summary>
        /// Gets the single item.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A value of type T.</returns>
        public T GetSingleItem(string filter, params object[] parameters)
        {
            if (string.IsNullOrEmpty(filter))
                throw new ArgumentNullException("filter");

            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling GetSingleItem()");

            DbCommand command = PrepCommandForSelect(filter, FillMethod.FilterText, parameters);
            return GetSingleItem(command);
        }

        /// <summary>
        /// Gets the single item.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A value of type T.</returns>
        public T GetSingleItem(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            if ((command.Connection == null) || (command.Connection.State != ConnectionState.Open))
            {
                if (Connection != null)
                    command.Connection = Connection;
                else
                    throw new InvalidOperationException("Either the command or the adapter must have an open connection.");
            }

            using (DbDataReader reader = ExecuteReaderInternal(command, CommandBehavior.SingleRow))
            {
                if (reader.Read())
                {
                    var item = new T();
                    var itemCF = item as ICustomFill;
                    if (itemCF != null)
                    {
                        itemCF.FillObject(reader);
                    }
                    else
                    {
                        PopulateSingleObject(item, reader, true);

                        var itemDC = item as ISavable;
                        if (itemDC != null) itemDC.CommitValues();
                    }

                    return item;
                }
                
                return null;
            }
        }
    }
}