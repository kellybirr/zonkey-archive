using System;
using System.Collections.Generic;
using System.Threading;

namespace Zonkey
{
    /// <summary>
    /// Provides an adapter to facilitate the reading/writing of database values and a <see cref="Zonkey.ObjectModel.DataClass"/> instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DataClassAdapter<T>
    {
        private ThreadMode _asyncThreadMode = ThreadMode.Pooled;

        /// <summary>
        /// Gets or sets the async thread mode.
        /// </summary>
        /// <value>The async thread mode.</value>
        public ThreadMode AsyncThreadMode
        {
            get { return _asyncThreadMode; }
            set { _asyncThreadMode = value; }
        }

        /// <summary>
        /// Aborts a Fill or Fill*Async Operation, causing it to 
        /// return the number of records previously read.
        /// </summary>
        public void AbortFill()
        {
            _cancelFill = true;
        }

        /// <summary>
        /// Fills the collection async.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="filters">The filters.</param>
        /// <returns><c>true</c> if async fill thread is successfully started, otherwise <c>false</c>.</returns>
        public bool FillAsync(ICollection<T> collection, params SqlFilter[] filters)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if ((filters == null) || (filters.Length == 0)) throw new ArgumentNullException("filters");

            ThreadStart ts = delegate
            {
                int nRecords = FillInternal(collection, string.Empty, FillMethod.FilterArray, filters);

                if (FillAsyncComplete != null)
                    FillAsyncComplete(this, new FillStatusEventArgs(nRecords, true));
            };

            return StartAsyncFill(ts);
        }

        /// <summary>
        /// Fills the collection async.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns><c>true</c> if async fill thread is successfully started, otherwise <c>false</c>.</returns>
        public bool FillAsync(ICollection<T> collection, string filter, params object[] parameters)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (filter == null) throw new ArgumentNullException("filter");

            ThreadStart ts = delegate
            {
                int nRecords = FillInternal(collection, filter, FillMethod.FilterText, parameters);

                if (FillAsyncComplete != null)
                    FillAsyncComplete(this, new FillStatusEventArgs(nRecords, true));
            };

            return StartAsyncFill(ts);
        }

        /// <summary>
        /// Fills all async.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if async fill thread is successfully started, otherwise <c>false</c>.</returns>
        public bool FillAllAsync(ICollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            ThreadStart ts = delegate
            {
                int nRecords = FillInternal(collection, null, FillMethod.Unfiltered, null);

                if (FillAsyncComplete != null)
                    FillAsyncComplete(this, new FillStatusEventArgs(nRecords, true));
            };

            return StartAsyncFill(ts);
        }

        /// <summary>
        /// Fills the with SP async.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="storedProcName">Name of the stored proc.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns><c>true</c> if async fill thread is successfully started, otherwise <c>false</c>.</returns>
        public bool FillWithSPAsync(ICollection<T> collection, string storedProcName, params object[] parameters)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (storedProcName == null) throw new ArgumentNullException("storedProcName");

            ThreadStart ts = delegate
            {
                int nRecords = FillInternal(collection, storedProcName, FillMethod.StoredProcedure, parameters);

                if (FillAsyncComplete != null)
                    FillAsyncComplete(this, new FillStatusEventArgs(nRecords, true));
            };

            return StartAsyncFill(ts);
        }

        private bool StartAsyncFill(ThreadStart ts)
        {
            if (_asyncThreadMode == ThreadMode.Pooled)
            {
                return ThreadPool.QueueUserWorkItem(delegate { ts(); });
            }
            else
            {
                Thread myThread = new Thread(ts);
                myThread.IsBackground = true;

                if (_asyncThreadMode == ThreadMode.LowPriority)
                    myThread.Priority = ThreadPriority.BelowNormal;

                myThread.Start();

                return true;
            }
        }
    }
}