using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        private DbCommand _bulkInsertCommand;
        private PropertyInfo[] _bulkInsertProperties;

        /// <summary>
        /// Bulk inserts an entire collection of objects/records into the database.
        /// Does not preform any select-back or modify the state of the object
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The number of inserted items.</returns>
        public int BulkInsert(ICollection<T> collection)
        {
            // prep for bulk insert operations
            if (_bulkInsertCommand == null)
                CommandBuilder.GetBulkInsertInfo(out _bulkInsertCommand, out _bulkInsertProperties);

            // init counter
            int nRecords = 0;

            // insert objects/records
            foreach (T obj in collection)
            {
                try
                {
                    BulkInsertObjectInternal(obj);
                    nRecords++;
                }
                catch (Exception ex)
                {
                    string sMsg = string.Format("Bulk Insert failed due to exception on record {0}, prevoius objects were inserted properly.", nRecords);
                    throw new DataException(sMsg, ex);
                }
            }

            return nRecords;
        }

        /// <summary>
        /// Bulk inserts a single object/record into the database.
        /// Does not preform any select-back or modify the state of the object
        /// </summary>
        /// <param name="obj">The object.</param>
        public void BulkInsert(T obj)
        {
            // prep for bulk insert operations
            if (_bulkInsertCommand == null)
                CommandBuilder.GetBulkInsertInfo(out _bulkInsertCommand, out _bulkInsertProperties);

            // insert object/record
            try
            {
                BulkInsertObjectInternal(obj);
            }
            catch (Exception ex)
            {
                throw new DataException("Bulk Insert failed due to exception.", ex);
            }
        }

        private void BulkInsertObjectInternal(T obj)
        {
            // set parameter values from obj instance
            for (int i = 0; i < _bulkInsertProperties.Length; i++)
            {
                PropertyInfo pi = _bulkInsertProperties[i];
                object oValue = pi.GetValue(obj, null);

                // fix empty guids
                if ((oValue is Guid) && (Guid.Empty == (Guid)oValue))
                    oValue = DBNull.Value;

                if (pi.PropertyType == typeof (string))
                    _bulkInsertCommand.Parameters[i].Value = (oValue ?? _nullStringDefault);
                else
                    _bulkInsertCommand.Parameters[i].Value = (oValue ?? DBNull.Value);
            }

            // execute insert command
            ExecuteNonQueryInternal(_bulkInsertCommand);
        }
    }
}