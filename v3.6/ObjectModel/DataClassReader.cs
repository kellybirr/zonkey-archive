using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Zonkey.ObjectModel
{
    /// <summary>
    /// A class that reads DCs from a DataReader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataClassReader<T> : IEnumerable<T>, IDisposable
        where T : class, new()
    {
        private readonly DataMap _dataMap;
        private readonly DbDataReader _reader;
        private QuickFillInfo[] _fillInfo;
    	//private Func<IDataRecord, T> _builder;
        private bool _disposed;
        
        private bool _isCustomFill;
        private bool _isSavable;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public DataClassReader(DbDataReader reader)
        {
            _dataMap = DataMap.GenerateCached(typeof(T));
            _reader = reader;

            DisposeBaseReader = true;
            TestInterfaces();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="disposeReader">if set to <c>true</c> [dispose reader].</param>
        public DataClassReader(DbDataReader reader, bool disposeReader)
        {
            _dataMap = DataMap.GenerateCached(typeof(T));
            _reader = reader;

            DisposeBaseReader = disposeReader;
            TestInterfaces();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="map">The map.</param>
        public DataClassReader(DbDataReader reader, DataMap map)
        {
            _dataMap = map;
            _reader = reader;

            DisposeBaseReader = true;
            TestInterfaces();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClassReader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="map">The map.</param>
        /// <param name="disposeReader">if set to <c>true</c> [dispose reader].</param>
        public DataClassReader(DbDataReader reader, DataMap map, bool disposeReader)
        {
            _dataMap = map;
            _reader = reader;

            DisposeBaseReader = disposeReader;
            TestInterfaces();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dispose base reader].
        /// </summary>
        /// <value><c>true</c> if [dispose base reader]; otherwise, <c>false</c>.</value>
        public bool DisposeBaseReader { get; set; }

        /// <summary>
        /// Gets the base reader.
        /// </summary>
        /// <value>The base reader.</value>
        public DbDataReader BaseReader
        {
            get { return _reader; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T item;
            while ((item = Read()) != null)
                yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (DisposeBaseReader) 
                _reader.Dispose();

            GC.SuppressFinalize(this);
            _disposed = true;
        }

        ~DataClassReader()
        {
            Dispose();
        }

        /// <summary>
        /// Reads the next record class form the reader.
        /// </summary>
        /// <returns></returns>
        public T Read()
        {
            if (!_reader.Read())
                return null;

            T item;
            if (_isCustomFill)
            {
                item = new T();
                ((ICustomFill)item).FillObject(_reader);                
            }
            else
            {				
				//if (_builder == null)
				//    _builder = CreateBuilder(_reader);
				
				item = BuildObject(_reader);

                if (_isSavable)
                    ((ISavable)item).CommitValues();                
            }

            return item;
        }

        private T BuildObject(IDataRecord record)
        {
            var obj = new T();
            for (int i = 0; i < _fillInfo.Length; i++)
            {
                QuickFillInfo info = _fillInfo[i];
                if (info == null) continue;

                object oValue = record.GetValue(i);
                if (Convert.IsDBNull(oValue)) continue;

            	try
            	{
					if (!info.IsAssignable)
					{
						if ((info.PropertyType == typeof(Guid)) && (oValue is string))
							info.PropertyInfo.SetValue(obj, new Guid(oValue.ToString()), null);
						else
							info.PropertyInfo.SetValue(obj, Convert.ChangeType(oValue, info.PropertyType), null);
					}
					else
						info.PropertyInfo.SetValue(obj, oValue, null);
            	}
            	catch (Exception ex)
            	{	
            		throw new PropertyReadException(info.PropertyInfo, oValue, ex);
            	}
            }

            return obj;
        }

        private void BuildQuickFillArray(DbDataReader reader)
        {
            // init quick fill array
            var outArray = new QuickFillInfo[reader.VisibleFieldCount];

            // put field name/ordinal pairs in dictionary for exception free lookup
            var keyComparer = StringComparer.CurrentCultureIgnoreCase;
            var readerFields = new Dictionary<string, int>(keyComparer);
            for (int i = 0; i < reader.VisibleFieldCount; i++)
                readerFields.Add(reader.GetName(i), i);

            foreach (IDataMapField field in _dataMap.ReadableFields)
            {
                int ordinal;
                if (!readerFields.TryGetValue(field.FieldName, out ordinal)) continue;

                Type propType = field.Property.PropertyType;
                if (propType.IsEnum) propType = Enum.GetUnderlyingType(propType);

                var qfi = new QuickFillInfo();
                qfi.PropertyInfo = field.Property;
                qfi.PropertyType = propType;
                qfi.FieldType = reader.GetFieldType(ordinal);
                qfi.IsAssignable = (propType.IsAssignableFrom(qfi.FieldType));

                outArray[ordinal] = qfi;
            }

            _fillInfo = outArray;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has rows.
        /// </summary>
        /// <value><c>true</c> if this instance has rows; otherwise, <c>false</c>.</value>
        public bool HasRows
        {
            get { return BaseReader.HasRows; }
        }

        private void TestInterfaces()
        {
            _isSavable = (typeof(T).GetInterface("ISavable", false) != null);
            _isCustomFill = (typeof(T).GetInterface("ICustomFill", false) != null);

			if (!_isCustomFill)
			{
				BuildQuickFillArray(_reader);
			}
        }

        #region Nested type: QuickFillInfo

        private class QuickFillInfo
        {
            public Type FieldType;
            public bool IsAssignable;
            public PropertyInfo PropertyInfo;
            public Type PropertyType;
        }

        #endregion

		private Func<IDataRecord, T> CreateBuilder(DbDataReader reader)
		{
			// put field name/ordinal pairs in dictionary for exception free lookup
			var keyComparer = StringComparer.CurrentCultureIgnoreCase;
			var readerFields = new Dictionary<string, int>(keyComparer);
			for (int i = 0; i < reader.VisibleFieldCount; i++)
				readerFields.Add(reader.GetName(i), i);

			// start generator
			var method = new DynamicMethod("DynamicCreate", typeof(T), new[] { typeof(IDataRecord) }, typeof(T), true);
			var generator = method.GetILGenerator();

			var result = generator.DeclareLocal(typeof(T));
			generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Stloc, result);

			foreach (IDataMapField field in _dataMap.ReadableFields)
            {
                int ordinal;
                if (!readerFields.TryGetValue(field.FieldName, out ordinal)) continue;
				if (field.Property.GetSetMethod(true) == null) continue;

                Type propType = field.Property.PropertyType;
                if (propType.IsEnum) propType = Enum.GetUnderlyingType(propType);

				var endIfLabel = generator.DefineLabel();			

				// gen code to check if field is null
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ldc_I4, ordinal);
				generator.Emit(OpCodes.Callvirt, isDBNullMethod);
				generator.Emit(OpCodes.Brtrue, endIfLabel);

				// get value from record onto stack
				generator.Emit(OpCodes.Ldloc, result);
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ldc_I4, ordinal);
				generator.Emit(OpCodes.Callvirt, getValueMethod);

            	Type dbFieldType = reader.GetFieldType(ordinal);
				if (propType.IsAssignableFrom(dbFieldType))
				{	
					// direct unbox/assign
					generator.Emit(OpCodes.Unbox_Any, dbFieldType);
				}
				else if ((propType == typeof(Guid)) && (dbFieldType == typeof(string)))
				{
					// deal with string->guid	
					generator.Emit(OpCodes.Castclass, typeof(string));
					generator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new[] { typeof(String) }) );
				}
				else
				{
					// deal with other converts
					generator.Emit(OpCodes.Ldtoken, dbFieldType);
					generator.Emit(OpCodes.Call, getTypeHandleMethod);
					generator.Emit(OpCodes.Call, convertChangeTypeMethod);
					generator.Emit(OpCodes.Unbox_Any, dbFieldType);
				}
				
				// load into property
				generator.Emit(OpCodes.Callvirt, field.Property.GetSetMethod(true));

				// end if
				generator.MarkLabel(endIfLabel);
			}

			generator.Emit(OpCodes.Ldloc, result);
			generator.Emit(OpCodes.Ret);

			return (Func<IDataRecord, T>)method.CreateDelegate(typeof(Func<IDataRecord, T>));
		}

		private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new[] { typeof(int) });
		private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new[] { typeof(int) });
		private static readonly MethodInfo getTypeHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle",new[] { typeof(RuntimeTypeHandle) });
		private static readonly MethodInfo convertChangeTypeMethod = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });
		
    }
}
