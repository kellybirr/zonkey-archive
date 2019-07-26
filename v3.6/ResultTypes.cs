using System.Collections.Generic;

namespace Zonkey
{
    /// <summary>
    /// Provides methods and properties for storing and retrieving <see cref="Zonkey.SaveResult"/> status when performing database operations on a collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionSaveResult<T>
    {
        /// <summary>
        /// Gets the items in the collection that were skipped.
        /// </summary>
        /// <value>The list of skipped items.</value>
        public IList<T> Skipped
        {
            get { return _skipped; }
        }

        private readonly List<T> _skipped = new List<T>();

        /// <summary>
        /// Gets the items in the collection that were inserted.
        /// </summary>
        /// <value>The list of inserted items.</value>
        public IList<T> Inserted
        {
            get { return _inserted; }
        }

        private readonly List<T> _inserted = new List<T>();

        /// <summary>
        /// Gets the items in the collection that were updated.
        /// </summary>
        /// <value>The list of updated items.</value>
        public IList<T> Updated
        {
            get { return _updated; }
        }

        private readonly List<T> _updated = new List<T>();

        /// <summary>
        /// Gets the items in the collection that were deleted.
        /// </summary>
        /// <value>The list of deleted items.</value>
        public IList<T> Deleted
        {
            get { return _deleted; }
        }

        private readonly List<T> _deleted = new List<T>();

        /// <summary>
        /// Gets the items in the collection that failed.
        /// </summary>
        /// <value>The list of failed items.</value>
        public IList<T> Failed
        {
            get { return _failed; }
        }

        private readonly List<T> _failed = new List<T>();

        /// <summary>
        /// Gets the items in the collection that conflicted.
        /// </summary>
        /// <value>The list of conflicted items.</value>
        public IList<T> Conflicted
        {
            get { return _conflicted; }
        }

        private readonly List<T> _conflicted = new List<T>();

        /// <summary>
        /// Gets the number of errors that occurred when performing the database operation on the collection.
        /// </summary>
        /// <value>The error count.</value>
        public int ErrorCount
        {
            get { return _failed.Count + _conflicted.Count; }
        }
    }
}