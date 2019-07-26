using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Zonkey.ObjectModel;

namespace Zonkey
{
	public partial class DataClassAdapter<T>
	{
		public DataClassReader<T> OpenReader(Expression<Func<T, bool>> filterExpression)
		{
			var parser = new WhereExpressionParser<T>(DataMap, SqlDialect) { UseQuotedIdentifier = CommandBuilder.UseQuotedIdentifier };
			var result = parser.Parse(filterExpression);

			return OpenReaderInternal(result.SqlText, FillMethod.FilterText, result.Parameters);
		}

		public DataClassReader<T> OpenReader(params SqlFilter[] filters)
		{
			if ((filters == null) || (filters.Length == 0))
				throw new ArgumentNullException("filters");

			return OpenReaderInternal(string.Empty, FillMethod.FilterArray, filters);
		}

		public DataClassReader<T> OpenReader(string filter)
		{
			if (string.IsNullOrEmpty(filter))
				throw new ArgumentNullException("filter");

			return OpenReaderInternal(filter, FillMethod.FilterText, null);
		}

		public DataClassReader<T> OpenReader(string filter, params object[] parameters)
		{
			if (string.IsNullOrEmpty(filter))
				throw new ArgumentNullException("filter");

			return OpenReaderInternal(filter, FillMethod.FilterText, parameters);
		}

		public DataClassReader<T> OpenReader(DbCommand command)
		{
			if (command == null) throw new ArgumentNullException("command");

			if ((command.Connection == null) || (command.Connection.State != ConnectionState.Open))
			{
				if (Connection != null)
					command.Connection = Connection;
				else
					throw new InvalidOperationException("Either the command or the adapter must have an open connection.");
			}

			DbDataReader reader = ExecuteReaderInternal(command, CommandBehavior.SingleResult);

			return new DataClassReader<T>(reader, DataMap) { ObjectFactory = ObjectFactory };
		}

		public DataClassReader<T> OpenReader(DbDataReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			if (reader.IsClosed) throw new ArgumentException("supplied reader was previously closed");

			return new DataClassReader<T>(reader, DataMap) { ObjectFactory = ObjectFactory };
		}

		private DataClassReader<T> OpenReaderInternal(string text, FillMethod method, IList parameters)
		{
			if (Connection == null)
				throw new InvalidOperationException("must set connection before calling OpenReader()");

			DbCommand command = PrepCommandForSelect(text, method, parameters);
			DbDataReader reader = ExecuteReaderInternal(command, CommandBehavior.SingleResult);
			
			return new DataClassReader<T>(reader, DataMap) { ObjectFactory = ObjectFactory };
		}
	}
}