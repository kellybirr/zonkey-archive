﻿using System;
using System.Data.Common;
using System.Collections;
using System.Linq.Expressions;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        /// <summary>
        /// Gets the record count
        /// </summary>
        /// <param name="filterExpression">the filter as a lambda expression</param>
        /// <returns></returns>
        public int GetCount(Expression<Func<T, bool>> filterExpression)
        {
            var parser = new ObjectModel.WhereExpressionParser<T>(_dataMap, SqlDialect) { UseQuotedIdentifier = CommandBuilder.UseQuotedIdentifier };
            var result = parser.Parse(filterExpression);

            return GetCountInternal(result.SqlText, FillMethod.FilterText, result.Parameters);
        }

        /// <summary>
        /// Gets the record count
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>the count</returns>
        public int GetCount(string filter, params object[] parameters)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            return GetCountInternal(filter, FillMethod.FilterText, parameters);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public int GetCount(params SqlFilter[] filters)
        {
            if ((filters == null) || (filters.Length == 0))
                throw new ArgumentNullException("filters");

            return GetCountInternal(string.Empty, FillMethod.FilterArray, filters);
        }

        /// <summary>
        /// Gets the count internal.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private int GetCountInternal(string text, FillMethod method, IList parameters)
        {
            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling GetCount()");

            DbCommand command;            
            switch (method)
            {
                case FillMethod.FilterText:
                    command = CommandBuilder.GetCountCommand(text);
                    DataManager.AddParamsToCommand(command, SqlDialect, parameters, ParameterPrefix);
                    break;
                case FillMethod.FilterArray:
                    command = CommandBuilder.GetCountCommand((SqlFilter[])parameters);
                    break;
                default:
                    throw new NotSupportedException("this fill method is not supported by GetCount");
            }

            return (int)command.ExecuteScalar();
        }
    }
}
