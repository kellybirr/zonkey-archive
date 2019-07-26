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
        private int PopulateCollection(ICollection<T> collection, DbDataReader reader)
        {
            _cancelFill = false;
            int nRecordCount = 0;
            int lastEvent = 0;

            // Get DataClassReader
            using (var classReader = new DataClassReader<T>(reader, _dataMap))
            {
                foreach (var item in classReader)
                {
                    nRecordCount++;
                    lock (collection)
                    {
                        collection.Add(item);
                    }

                    if ((ProgressIncrement > 0) && (FillProgress != null))
                    {
                        if (nRecordCount >= (lastEvent + ProgressIncrement))
                        {
                            var args = new FillStatusEventArgs(nRecordCount, false);
                            FillProgress(this, args);
                            if ((args.Cancel) || (_cancelFill)) return nRecordCount;

                            lastEvent = nRecordCount;
                        }
                    }

                    if (_cancelFill) return nRecordCount;
                }
            }

            if (_cancelFill) return nRecordCount;
            if ((ProgressIncrement > 0) && (FillProgress != null))
                FillProgress(this, new FillStatusEventArgs(nRecordCount, true));

            return nRecordCount;
        }

        private void PopulateSingleObject(T obj, IDataRecord record, bool skipDbNull)
        {
            lock (obj)
            {
                for (int i = 0; i < record.FieldCount; i++)
                {
                    PropertyInfo pi = _dataMap.GetReadableProperty(record.GetName(i));
                    if (pi == null) continue;

                    Type propType = pi.PropertyType;
                    if (propType.IsEnum) propType = Enum.GetUnderlyingType(propType);

                    if (record.IsDBNull(i))
                    {
                        if (skipDbNull) continue;
                        if ((! propType.IsValueType) || (Nullable.GetUnderlyingType(propType) != null))
                            pi.SetValue(obj, null, null);
                    }
                    else
                    {
                        object oValue = record.GetValue(i);
                        try
                        {
                            if (!propType.IsAssignableFrom(oValue.GetType()))
                            {
                                if ((propType == typeof (Guid)) && (oValue is string))
                                    pi.SetValue(obj, new Guid(oValue.ToString()), null);
                                else
                                    pi.SetValue(obj, Convert.ChangeType(oValue, propType), null);
                            }
                            else
                                pi.SetValue(obj, oValue, null);
                        }
                        catch (Exception ex)
                        {
                            throw new PropertyReadException(pi, oValue, ex);
                        }
                    }
                }
            }
        }
    }
}