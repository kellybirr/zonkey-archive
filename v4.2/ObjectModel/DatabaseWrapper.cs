using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace Zonkey.ObjectModel
{
	/// <summary>
	/// KB: I'm trying something new
	/// </summary>
	public class DatabaseWrapper : IDisposable
	{
		/// <summary>
		/// Private cache of adapters
		/// </summary>
		private readonly Dictionary<Type, DataClassAdapter> _adapters 
			= new Dictionary<Type, DataClassAdapter>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseWrapper"/> class and connects to the Database
		/// </summary>
		public DatabaseWrapper(string connectionName)
		{
			Connection = DbConnectionFactory.OpenConnection(connectionName);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseWrapper"/> class and connects to the Database
		/// </summary>
		public DatabaseWrapper(DbConnection connection)
		{
			Connection = connection;
		}

		/// <summary>
		/// Gets or sets the database connection.
		/// </summary>
		/// <value>The connection.</value>
		public virtual DbConnection Connection { get; private set; }


		/// <summary>
		/// Gets a DataClassAdapter of type Tdc.
		/// </summary>
		/// <typeparam name="Tdc">DC type</typeparam>
		/// <returns></returns>
		public virtual DataClassAdapter<Tdc> Adapter<Tdc>() 
			where Tdc : class, new()
		{
			if (_adapters.ContainsKey(typeof(Tdc)))
				return (DataClassAdapter<Tdc>)_adapters[typeof (Tdc)];

			lock (this)
			{	// double-check after lock (be thread safe, sort of)
				if (_adapters.ContainsKey(typeof(Tdc)))
					return (DataClassAdapter<Tdc>)_adapters[typeof(Tdc)];

				var adapter = new DataClassAdapter<Tdc>(Connection);
				_adapters.Add(typeof(Tdc), adapter);

				return adapter;				
			}
		}

		/// <summary>
		/// Equivalent to calling DataClassAdapter.GetSingelItem(linq)
		/// </summary>
		/// <typeparam name="Tdc">The type of the dc.</typeparam>
		/// <param name="expression">The filter expression.</param>
		/// <returns></returns>
		public virtual Tdc GetOne<Tdc>(Expression<Func<Tdc, bool>> expression)
			where Tdc : class, new()
		{
			return Adapter<Tdc>().GetSingleItem(expression);
		}

		/// <summary>
		/// Equivalent to calling DataClassAdapter.Save
		/// </summary>
		/// <typeparam name="Tdc">The type of the dc.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public virtual bool Save<Tdc>(Tdc obj)
			where Tdc : class, ISavable, new()
		{
			return Adapter<Tdc>().Save(obj);
		}

		/// <summary>
		/// Equivalent to calling DataClassAdapter.Save
		/// </summary>
		/// <typeparam name="Tdc">The type of the dc.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="updateCriteria">The update criteria</param>
		/// <returns></returns>
		public virtual bool Save<Tdc>(Tdc obj, UpdateCriteria updateCriteria)
			where Tdc : class, ISavable, new()
		{
			return Adapter<Tdc>().Save(obj, updateCriteria);
		}

		/// <summary>
		/// Equivalent to calling DataClassAdapter.Save
		/// </summary>
		/// <typeparam name="Tdc">The type of the dc.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="updateCriteria">The update criteria</param>
		/// <param name="updateAffect">Affect which fields</param>
		/// <param name="selectBack">Select what back</param>
		/// <returns></returns>
		public virtual bool Save<Tdc>(Tdc obj, UpdateCriteria updateCriteria, UpdateAffect updateAffect, SelectBack selectBack)
			where Tdc : class, ISavable, new()
		{
			return Adapter<Tdc>().Save(obj, updateCriteria, updateAffect, selectBack);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{			
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (! disposing) return;

			_adapters.Clear();
			Connection.Dispose();
		}
	}
}
