using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Zonkey.ObjectModel;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        /// <summary>
        /// Deletes the specified filter expression.
        /// </summary>
        /// <param name="filterExpression">The filter expression.</param>
        /// <returns></returns>
        public int Delete(Expression<Func<T, bool>> filterExpression)
        {
            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling Delete()");

            var parser = new WhereExpressionParser<T>(DataMap, SqlDialect) { UseQuotedIdentifier = CommandBuilder.UseQuotedIdentifier };
            var result = parser.Parse(filterExpression);

            try
            {
                DbCommand command = CommandBuilder.GetDeleteCommand(result.SqlText);
                if (result.Parameters != null)
                    DataManager.AddParamsToCommand(command, SqlDialect, result.Parameters);

                return ExecuteNonQueryInternal(command);
            }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes the specified filters.
        /// </summary>
        /// <param name="filters">A <see cref="Zonkey.SqlFilter"/> array of filters (WHERE clause).</param>
        /// <returns>A value of type <see cref="System.Int32"/> indicating whether the command executed successfully or not.</returns>
        public int Delete(params SqlFilter[] filters)
        {
            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling Delete()");

            if ((filters == null) || (filters.Length == 0))
                throw new ArgumentNullException("filters");

            try
            {
                DbCommand command = CommandBuilder.GetDeleteCommand(filters);
                return ExecuteNonQueryInternal(command);
            }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes the specified filter.
        /// </summary>
        /// <param name="filter">The filter (WHERE clause).</param>
        /// <returns>A value of type <see cref="System.Int32"/> indicating whether the command executed successfully or not.</returns>
        public int Delete(string filter)
        {
            return Delete(filter, null);
        }

        /// <summary>
        /// Deletes the specified filter.
        /// </summary>
        /// <param name="filter">The filter (WHERE clause).</param>
        /// <param name="parameters">The actual parameters to filter.</param>
        /// <returns>A value of type <see cref="System.Int32"/> indicating whether the command executed successfully or not.</returns>
        public int Delete(string filter, params object[] parameters)
        {
            if (string.IsNullOrEmpty(filter))
                throw new ArgumentNullException("filter");

            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling Delete()");

            try
            {
                DbCommand command = CommandBuilder.GetDeleteCommand(filter);
                if (parameters != null)
                    DataManager.AddParamsToCommand(command, SqlDialect, parameters);

                return ExecuteNonQueryInternal(command);
            }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes the item.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns><c>true</c> if delete succeeds; <c>false</c> otherwise.</returns>
        public bool DeleteItem(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling DeleteItem()");

            var keyValues = new List<object>();
            foreach (IDataMapField field in DataMap.KeyFields)
                keyValues.Add(field.Property.GetValue(obj, null));

            DbCommand deleteItemCommand = CommandBuilder.DeleteItemCommand;
            for (int i = 0; i < keyValues.Count; i++)
                deleteItemCommand.Parameters[i].Value = keyValues[i];

            try
            {
                int result = ExecuteNonQueryInternal(deleteItemCommand);
                return (result == 1);
            }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }
    }
}