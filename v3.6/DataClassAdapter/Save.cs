using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Zonkey.ObjectModel;

namespace Zonkey
{
    public partial class DataClassAdapter<T>
    {
        private int _lastSaveSuccessType;

        /// <summary>
        /// Saves the specified object
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <returns>true/false</returns>
        public bool Save(T obj)
        {
            return HandleSaveResult( TrySave(obj, UpdateCriteria.Default, UpdateAffect.ChangedFields, SelectBack.Default) );
        }

        /// <summary>
        /// Saves the specified object
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <returns>true/false</returns>
        public bool Save(T obj, UpdateCriteria criteria)
        {
            return HandleSaveResult( TrySave(obj, criteria, UpdateAffect.ChangedFields, SelectBack.Default) );
        }

        /// <summary>
        /// Saves the specified object
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="affect">The <see cref="Zonkey.UpdateAffect"/> value that determines which rows to affect.</param>
        /// <param name="selectBack">The <see cref="Zonkey.SelectBack"/> value that determines whether to select back the changed rows.</param>
        /// <returns>true/false</returns>
        public bool Save(T obj, UpdateCriteria criteria, UpdateAffect affect, SelectBack selectBack)
        {
            return HandleSaveResult(TrySave(obj, criteria, affect, selectBack));
        }

        /// <summary>
        /// Tries to save the object
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <returns>A value of type <see cref="Zonkey.SaveResult"/></returns>
        public SaveResult TrySave(T obj)
        {
            return TrySave(obj, UpdateCriteria.Default, UpdateAffect.ChangedFields, SelectBack.Default);
        }

        /// <summary>
        /// Tries to save the object
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <returns>A value of type <see cref="Zonkey.SaveResult"/></returns>
        public SaveResult TrySave(T obj, UpdateCriteria criteria)
        {
            return TrySave(obj, criteria, UpdateAffect.ChangedFields, SelectBack.Default);
        }

        /// <summary>
        /// Tries to save the object
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="affect">The <see cref="Zonkey.UpdateAffect"/> value that determines which rows to affect.</param>
        /// <param name="selectBack">The <see cref="Zonkey.SelectBack"/> value that determines whether to select back the changed rows.</param>
        /// <returns>A value of type <see cref="Zonkey.SaveResult"/></returns>
        public SaveResult TrySave(T obj, UpdateCriteria criteria, UpdateAffect affect, SelectBack selectBack)
        {
            var objSV = obj as ISavable;
            if (objSV == null) throw new ArgumentException("Save() is only supported on classes that implement Zonkey.ObjectModel.ISavable", "obj");

            var objDCX = obj as DataClass;

            // raise pre-save event
            if (objDCX != null) objDCX.OnBeforeSave();

            SaveResult result;
            switch (objSV.DataRowState)
            {
                case DataRowState.Added:
                    result = TryInsert(obj, selectBack);

                    _lastSaveSuccessType = 1;
                    break;
                case DataRowState.Detached:
                    throw new InvalidOperationException("Cannot save objects in a detached state. Did you forget to use the new record constructor?");

                case DataRowState.Modified:
                    if (((selectBack == SelectBack.None) || (selectBack == SelectBack.AllFields))
                        && (criteria <= UpdateCriteria.ChangedFields) && (affect == UpdateAffect.ChangedFields))
                        result = TryUpdate2(obj, criteria, (selectBack == SelectBack.AllFields));
                    else
                        result = TryUpdate(obj, criteria, affect, selectBack);

                    _lastSaveSuccessType = 2;
                    break;

                default:
                    return SaveResult.Skipped;
            }

            // raise post-save event and commit values
            if (result == SaveResult.Success)
            {
                if (objDCX != null) objDCX.OnAfterSave(true);
                objSV.CommitValues();
            }

            return result;
        }

        /// <summary>
        /// Saves the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public int SaveCollection(ICollection<T> collection)
        {
            return SaveCollection(collection, UpdateCriteria.Default, UpdateAffect.ChangedFields, SelectBack.Default);            
        }

        /// <summary>
        /// Saves the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public int SaveCollection(ICollection<T> collection, UpdateCriteria criteria)
        {
            return SaveCollection(collection, criteria, UpdateAffect.ChangedFields, SelectBack.Default);            
        }

        /// <summary>
        /// Saves the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="affect">The affect.</param>
        /// <param name="selectBack">The select back.</param>
        /// <returns></returns>
        public int SaveCollection(ICollection<T> collection, UpdateCriteria criteria, UpdateAffect affect, SelectBack selectBack)
        {
            var result = TrySaveCollection(collection, criteria, affect, selectBack);
            
            if ((result.Conflicted.Count > 0) || (result.Failed.Count > 0))
                throw new CollectionSaveException<T>(result);

            return (result.Inserted.Count + result.Updated.Count);
        }

        /// <summary>
        /// Saves the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>A value of type <see cref="T:Zonkey.CollectionSaveResult"/></returns>
        public CollectionSaveResult<T> TrySaveCollection(ICollection<T> collection)
        {
            return TrySaveCollection(collection, UpdateCriteria.Default, UpdateAffect.ChangedFields, SelectBack.Default);
        }

        /// <summary>
        /// Saves the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="criteria">The criteria.</param>
        /// <returns>A value of type <see cref="T:Zonkey.CollectionSaveResult"/></returns>
        public CollectionSaveResult<T> TrySaveCollection(ICollection<T> collection, UpdateCriteria criteria)
        {
            return TrySaveCollection(collection, criteria, UpdateAffect.ChangedFields, SelectBack.Default);
        }

        /// <summary>
        /// Saves the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="affect">The <see cref="Zonkey.UpdateAffect"/> value that determines which rows to affect.</param>
        /// <param name="selectBack">The <see cref="Zonkey.SelectBack"/> value that determines whether to select back the changed rows.</param>
        /// <returns>A value of type <see cref="T:Zonkey.CollectionSaveResult"/></returns>
        public CollectionSaveResult<T> TrySaveCollection(ICollection<T> collection, UpdateCriteria criteria, UpdateAffect affect, SelectBack selectBack)
        {
            var colResult = new CollectionSaveResult<T>();

            var bindCol = collection as ITrackDeletedItems<T>;
            if (bindCol != null)
            {
                foreach (T obj in bindCol.DeletedItems)
                {
                    var objSV = obj as ISavable;
                    if (objSV != null)
                    {
                        if (objSV.DataRowState == DataRowState.Deleted)
                        {
                            DeleteItem(obj);
                            colResult.Deleted.Add(obj);
                        }
                        else
                            colResult.Skipped.Add(obj);
                    }
                    else
                        colResult.Failed.Add(obj);
                }
            }

            foreach (T obj in collection)
            {
                var objSV = obj as ISavable;
                if (objSV != null)
                {
                    if (objSV.DataRowState != DataRowState.Unchanged)
                    {
                        switch (TrySave(obj, criteria, affect, selectBack))
                        {
                            case SaveResult.Skipped:
                                colResult.Skipped.Add(obj);
                                break;
                            case SaveResult.Conflict:
                                colResult.Conflicted.Add(obj);
                                break;
                            case SaveResult.Fail:
                                colResult.Failed.Add(obj);
                                break;
                            case SaveResult.Success:
                                if (_lastSaveSuccessType == 1)
                                    colResult.Inserted.Add(obj);
                                else
                                    colResult.Updated.Add(obj);

                                break;
                        }
                    }
                    else
                        colResult.Skipped.Add(obj);
                }
                else
                    colResult.Failed.Add(obj);
            }

            return colResult;
        }

        /// <summary>
        /// Tries to update the specified object
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="affect">The <see cref="Zonkey.UpdateAffect"/> value that determines which rows to affect.</param>
        /// <param name="selectBack">The <see cref="Zonkey.SelectBack"/> value that determines whether to select back the changed rows.</param>
        /// <returns>A <see cref="Zonkey.SaveResult"/> value based on the outcome of the update operation.</returns>
        public SaveResult TryUpdate(T obj, UpdateCriteria criteria, UpdateAffect affect, SelectBack selectBack)
        {
            try
            {
                DbCommand[] commands = CommandBuilder.GetUpdateCommands(obj, criteria, affect, selectBack);
                return UpdateInternal(obj, commands, (selectBack != SelectBack.None));
            }
            catch (DataException) { throw; }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="affect">The <see cref="Zonkey.UpdateAffect"/> value that determines which rows to affect.</param>
        /// <param name="selectBack">The <see cref="Zonkey.SelectBack"/> value that determines whether to select back the changed rows.</param>
        /// <returns>true/false based on the outcome of the update operation.</returns>
        public bool Update(T obj, UpdateCriteria criteria, UpdateAffect affect, SelectBack selectBack)
        {
            return HandleSaveResult( TryUpdate(obj, criteria, affect, selectBack) );
        }

        /// <summary>
        /// Updates the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="doSelectBack">if set to <c>true</c> [do select back].</param>
        /// <returns>A <see cref="Zonkey.SaveResult"/> value based on the outcome of the update operation.</returns>
        public SaveResult TryUpdate2(T obj, UpdateCriteria criteria, bool doSelectBack)
        {
            if (! (obj is ISavable))
                throw new ArgumentException("Update2() is only supported on classes that implement Zonkey.ObjectModel.ISavable", "obj");

            try
            {
                DbCommand[] commands = CommandBuilder.GetUpdate2Commands((ISavable)obj, criteria, doSelectBack);
                return UpdateInternal(obj, commands, doSelectBack);
            }
            catch (DataException) { throw; }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="criteria">The criteria of type <see cref="Zonkey.UpdateCriteria"/>.</param>
        /// <param name="doSelectBack">if set to <c>true</c> [do select back].</param>
        /// <returns></returns>
        public bool Update2(T obj, UpdateCriteria criteria, bool doSelectBack)
        {
            return HandleSaveResult( TryUpdate2(obj, criteria, doSelectBack) );
        }

        private SaveResult UpdateInternal(T obj, DbCommand[] commands, bool doSelectBack)
        {
            if (commands == null)
            {
                var objSV = obj as ISavable;
                if (objSV != null)
                    objSV.DataRowState = DataRowState.Unchanged;

                return SaveResult.Skipped;
            }

            DbCommand readerCommand = commands[commands.Length - 1];
            int nRecordsAffected = 0;

            if ( (! doSelectBack) || (commands.Length == 2) )
            {
                nRecordsAffected = ExecuteNonQueryInternal(commands[0]);
                if (nRecordsAffected != 1) return SaveResult.Fail;

                if ( (! doSelectBack) || (commands[1] == null) )
                    return SaveResult.Success;
            }

            using (DbDataReader reader = ExecuteReaderInternal(readerCommand, CommandBehavior.SingleRow))
            {
                if (nRecordsAffected == 1)
                {
                    if (reader.Read())
                    {
                        PopulateSingleObject(obj, reader, false);
                        return SaveResult.Success;
                    }

                    return SaveResult.Fail;
                }
                
                // detect conflict or failure
                return (reader.Read()) ? SaveResult.Conflict : SaveResult.Fail;
            }
        }

        /// <summary>
        /// Inserts the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="selectBack">The <see cref="Zonkey.SelectBack"/> value that determines which rows, if any, to return.</param>
        /// <returns>A <see cref="Zonkey.SaveResult"/> value based on the outcome of the insert operation.</returns>
        public SaveResult TryInsert(T obj, SelectBack selectBack)
        {
            try
            {
                DbCommand[] commands = CommandBuilder.GetInsertCommands(obj, selectBack);
                DbCommand readerCommand = commands[commands.Length - 1];

                if ((selectBack == SelectBack.None) || (commands.Length == 2))
                {
                    int nRecordsAffected = ExecuteNonQueryInternal(commands[0]);
                    if (nRecordsAffected != 1) return SaveResult.Fail;

                    if (selectBack == SelectBack.None)
                        return SaveResult.Success;
                }

                using (DbDataReader reader = ExecuteReaderInternal(readerCommand, CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        PopulateSingleObject(obj, reader, false);
                        return SaveResult.Success;
                    }
                    
                    return SaveResult.Fail;
                }
            }
            catch (DataException) { throw; }
            catch (Exception ex)
            {
                throw new DataException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserts the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="selectBack">The select back.</param>
        /// <returns></returns>
        public bool Insert(T obj, SelectBack selectBack)
        {
            return HandleSaveResult( TryInsert(obj, selectBack) );
        }

        /// <summary>
        /// Handles the save result.
        /// </summary>
        /// <param name="result">The result</param>
        /// <returns></returns>
        private static bool HandleSaveResult(SaveResult result)
        {
            switch (result)
            {
                case SaveResult.Skipped:
                    return false;
                case SaveResult.Success:
                    return true;
                case SaveResult.Conflict:
                    throw new UpdateConflictException("A synchronization conflict was detected during the update");
                case SaveResult.Fail:
                    throw new SaveFailedException("The save appears to have failed for an unknown reason, or did not affect the expected number of records");
                default:
                    throw new ArgumentException("Unrecognized or invalid SaveResult");
            }
        }
    }
}