using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Xml.Serialization;

namespace Zonkey.ObjectModel
{
    /// <summary>
    /// Provides an abstraction of a Database table to a class.
    /// </summary>
    public abstract class DataClass : IDisposable, Zonkey.ObjectModel.ISavable
    {
        /// <summary>
        /// Occurs when the <see cref="Zonkey.ObjectModel.DataClass"/>'s Dispose() method is called.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler Disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataClass"/> class.
        /// </summary>
        /// <param name="addingNew">if set to <c>true</c> then initializes object for insertion to database.</param>
        protected DataClass(bool addingNew)
        {
            DataRowState = (addingNew) ? DataRowState.Added : DataRowState.Detached;

            _originalValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the original values.
        /// </summary>
        /// <value>The original values.</value>
        [XmlIgnore]
        [Browsable(false), Bindable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IDictionary<string, object> OriginalValues
        {
            get { return _originalValues; }
        }
        private IDictionary<string, object> _originalValues;

        /// <summary>
        /// Gets or sets the state of the data row.
        /// </summary>
        /// <value>The state of the data row.</value>
        [XmlAttribute("dataState")]
        [Browsable(false), Bindable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataRowState DataRowState
        {
            get { return _dataRowState; }
            set { _dataRowState = value; }
        }
        private DataRowState _dataRowState;

        /// <summary>
        /// Updates the supplied field to the specified value and tracks changes.
        /// </summary>
        /// <typeparam name="T">The Type of the field</typeparam>
        /// <param name="fieldName">Exact Property name of the field</param>
        /// <param name="fieldRef">A Refrence to the private field to hold the data</param>
        /// <param name="value">The value to set the field to</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        protected virtual void SetFieldValue<T>(string fieldName, ref T fieldRef, T value)
        {
            if ((_dataRowState == DataRowState.Unchanged) || (_dataRowState == DataRowState.Modified))
            {
                if (! _originalValues.ContainsKey(fieldName))
                {
                    _originalValues.Add(fieldName, fieldRef);
                    _dataRowState = DataRowState.Modified;
                }
            }

            fieldRef = value;
        }

        /// <summary>
        /// Commits the values.
        /// </summary>
        public virtual void CommitValues()
        {
            if (_originalValues.Count > 0)
                _originalValues.Clear();

            _dataRowState = DataRowState.Unchanged;
        }

        /// <summary>
        /// Called just before the record is saved to the database.
        /// </summary>
        protected internal virtual void OnBeforeSave()
        {
        }

        /// <summary>
        /// Called after the record is saved tot he database.
        /// </summary>
        /// <param name="isNewRow">if set to <c>true</c> [is new row].</param>
        protected internal virtual void OnAfterSave(bool isNewRow)
        {
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _originalValues.Clear();
                _originalValues = null;

                if (Disposed != null)
                    Disposed(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the key field names.
        /// </summary>
        /// <param name="obj">The obj to scan.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
        public static string[] GetKeyFields(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<string> keys = new List<string>();
            PropertyInfo[] propList = obj.GetType().GetProperties();

            foreach (PropertyInfo pi in propList)
            {
                DataFieldAttribute attr = DataFieldAttribute.GetFromProperty(pi);
                if ((attr != null) && (attr.IsKeyField)) keys.Add(pi.Name);
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Converts DBNull to a null refrence, or returns the supplied value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static object DBToNull(object value)
        {
            return (Convert.IsDBNull(value)) ? null : value;
        }

        #endregion
    }
}