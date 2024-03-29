﻿using System;
using System.Data.Common;
using System.Collections;
using System.Linq.Expressions;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        /// <summary>
        /// Checks if any mathcing records exist
        /// </summary>
        /// <param name="filterExpression">the filter as a lambda expression</param>
        /// <returns></returns>
        public bool Exists(Expression<Func<T, bool>> filterExpression)
        {
            var parser = new ObjectModel.WhereExpressionParser<T>(DataMap, SqlDialect) { UseQuotedIdentifier = CommandBuilder.UseQuotedIdentifier };
            var result = parser.Parse(filterExpression);

            return ExistsInternal(result.SqlText, FillMethod.FilterText, result.Parameters);
        }

        /// <summary>
        /// Checks if any mathcing records exist
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public bool Exists(string filter, params object[] parameters)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            return ExistsInternal(filter, FillMethod.FilterText, parameters);
        }

        /// <summary>
        /// Checks if any mathcing records exist
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public bool Exists(params SqlFilter[] filters)
        {
            if ((filters == null) || (filters.Length == 0))
                throw new ArgumentNullException("filters");

            return ExistsInternal(string.Empty, FillMethod.FilterArray, filters);
        }

        /// <summary>
        /// Checks if any mathcing records exist
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private bool ExistsInternal(string text, FillMethod method, IList parameters)
        {
            if (Connection == null)
                throw new InvalidOperationException("must set connection before calling GetCount()");

            DbCommand command;
            switch (method)
            {
                case FillMethod.FilterText:
                    command = CommandBuilder.GetExistsCommand(text);
                    DataManager.AddParamsToCommand(command, SqlDialect, parameters, ParameterPrefix);
                    break;
                case FillMethod.FilterArray:
                    command = CommandBuilder.GetExistsCommand((SqlFilter[])parameters);
                    break;
                default:
                    throw new NotSupportedException("this fill method is not supported by Exists");
            }

            return (command.ExecuteScalar() as int? == 1);
        }
    }
}
