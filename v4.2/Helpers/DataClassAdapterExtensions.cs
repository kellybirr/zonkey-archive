using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Zonkey.ObjectModel;

namespace Zonkey.Helpers
{
    public static class DataClassAdapterExtensions
    {
        public static List<T> GetList<T>(this DataClassAdapter<T> adapter, Expression<Func<T, bool>> filterExpression) 
            where T : class 
        {
            using (var reader = adapter.OpenReader(filterExpression))
                return reader.ToList();
        }

        public static List<T> GetList<T>(this DataClassAdapter<T> adapter, string filter, params object[] parameters) 
            where T : class
        {
            using (var reader = adapter.OpenReader(filter, parameters))
                return reader.ToList();
        }

        public static T[] GetArray<T>(this DataClassAdapter<T> adapter, Expression<Func<T, bool>> filterExpression) 
            where T : class
        {
            using (var reader = adapter.OpenReader(filterExpression))
                return reader.ToArray();
        }

        public static T[] GetArray<T>(this DataClassAdapter<T> adapter, string filter, params object[] parameters)
            where T : class
        {
            using (var reader = adapter.OpenReader(filter, parameters))
                return reader.ToArray();
        }
    }

    public static class DCAdapterExtensions
    {
        public static List<T> GetList<T>(this DCAdapterBase<T> adapter, Expression<Func<T, bool>> filterExpression)
            where T : class, new()
        {
            using (var reader = adapter.OpenReader(filterExpression))
                return reader.ToList();
        }

        public static T[] GetArray<T>(this DCAdapterBase<T> adapter, Expression<Func<T, bool>> filterExpression)
            where T : class, new()
        {
            using (var reader = adapter.OpenReader(filterExpression))
                return reader.ToArray();
        }
    }

    public static class DataReaderExtension
    {
        public static IEnumerable<object[]> AsEnumeratedValues(this IDataReader sourceReader)
        {
            if (sourceReader == null)
                throw new ArgumentNullException("sourceReader");

            while (sourceReader.Read())
            {
                var row = new Object[sourceReader.FieldCount];
                sourceReader.GetValues(row);
                yield return row;
            }
        }

        public static IEnumerable<IDataRecord> AsEnumerable(this IDataReader sourceReader)
        {
            if (sourceReader == null)
                throw new ArgumentNullException("sourceReader");

            while (sourceReader.Read())
                yield return sourceReader;
        }
    }
}
