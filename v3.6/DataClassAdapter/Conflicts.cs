using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Zonkey.ObjectModel;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        /// <summary>
        /// Gets the conflicts.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>A <see cref="Zonkey.Conflict"/> array.</returns>
        public Conflict[] GetConflicts(T obj)
        {
            ISavable objSV = obj as ISavable;
            if (objSV == null) throw new ArgumentException("GetConflicts() is only supported on classes that implement Zonkey.ObjectModel.ISavable", "obj");

            FieldValuesDictionary dbValues;
            DbCommand command = CommandBuilder.GetRequeryCommand(objSV);
            using (DbDataReader reader = ExecuteReaderInternal(command, CommandBehavior.SingleRow))
            {
                if (reader.Read())
                    dbValues = new FieldValuesDictionary(reader);
                else
                    throw new DataException("DataReader returned could not read a record to sync with.");
            }

            List<Conflict> conflicts = new List<Conflict>();
            foreach (KeyValuePair<string, object> original in objSV.OriginalValues)
            {
                PropertyInfo pi = _objectType.GetProperty(original.Key);
                if (pi == null) continue;

                IDataMapField field = _dataMap.GetFieldForProperty(pi);
                if ((field == null) || (field.AccessType == AccessType.ReadOnly)) continue;

                bool valueMatch = false;
                object oDbValue;
                if (dbValues.TryGetValue(field.FieldName, out oDbValue))
                {
                    if (IsNullOrDbNull(original.Value) || IsNullOrDbNull(oDbValue))
                        valueMatch = (IsNullOrDbNull(original.Value) && IsNullOrDbNull(oDbValue));
                    else
                    {
                        Type propType = original.Value.GetType();
                        if (propType.IsGenericType)
                        {
                            // special case to handle nullable types
                            Type baseType = Nullable.GetUnderlyingType(propType);
                            if (baseType != null) propType = baseType;
                        }

                        if (propType.IsEnum)
                            propType = Enum.GetUnderlyingType(propType);

                        if (!propType.IsAssignableFrom(oDbValue.GetType()))
                        {
                            if ((propType == typeof (Guid)) && (oDbValue is string))
                            {
                                if (!string.IsNullOrEmpty(oDbValue.ToString()))
                                    valueMatch = original.Value.Equals(new Guid(oDbValue.ToString()));
                            }
                            else
                                valueMatch = original.Value.Equals(Convert.ChangeType(oDbValue, propType));
                        }
                        else
                            valueMatch = original.Value.Equals(oDbValue);
                    }
                }
                else
                    throw new DataException(string.Format("Missing field '{0}' from db sync query", field.FieldName));

                if (!valueMatch)
                    conflicts.Add(new Conflict(field.Property.Name, original.Value, oDbValue, field.Property.GetValue(objSV, null)));
            }

            return conflicts.ToArray();
        }

        /// <summary>
        /// Determines whether the value is null or dbnull.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if null or db null; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNullOrDbNull(object value)
        {
            return ((value == null) || (Convert.IsDBNull(value)));
        }
    }
}