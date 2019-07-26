using System;
using System.Collections.Generic;

namespace Zonkey.Linq
{
    public static class SqlFilterExtensions
    {
        public static bool SqlIn<TField, TList>(this TField field, Func<TList, bool> filterExpression) where TList : class
        {
            throw new NotSupportedException();
        }

        public static bool SqlIn<TField, TList>(this TField field, Func<TList, TField> fieldExpression, Func<TList, bool> filterExpression) where TList : class
        {
            throw new NotSupportedException();
        }

        public static bool SqlIn<TField>(this TField field, IEnumerable<TField> options)
        {
            throw new NotSupportedException();
        }

    }
}
