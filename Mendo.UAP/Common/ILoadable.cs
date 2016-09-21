using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mendo.UAP.Common
{
    public interface ILoadable
    {
        /// <summary>
        /// Returns true if the collection is loading
        /// </summary>
        Boolean IsLoading { get; }

        /// <summary>
        /// Returns true if the collection is loading, with no items inside it
        /// </summary>
        Boolean IsEmptyLoading { get; }

        /// <summary>
        /// Returns true if the collection is loading, but already has items
        /// inside it
        /// </summary>
        Boolean IsSubsequentLoading { get; }

        /// <summary>
        /// Returns true if the collection has been true through one load operation 
        /// previously.
        /// </summary>
        Boolean IsFirstLoaded { get; }

        /// <summary>
        /// Returns true if the collection is empty
        /// </summary>
        Boolean IsEmpty { get; }

        /// <summary>
        /// Returns true if the collection has been true through one load operation 
        /// but there are no items in the collection
        /// </summary>
        Boolean IsFirstLoadedEmpty { get; }

        /// <summary>
        /// Returns true is the collection is in the faulted state, and has
        /// not yet successfully been into the loaded state before
        /// </summary>
        Boolean IsFirstLoadFaulted { get; }

        Boolean IsFaulted { get; set; }

        LoadState LoadState { get; set; }

        String Title { get; set; }

        String ErrorMessage { get; set; }

        String EmptyMessage { get; set; }



        /// <summary>
        /// Event handler when all data has been loaded
        /// </summary>
        DataLoadedEventHandler OnDataLoaded { get; set; }

        /// <summary>
        /// Sets the relevant LoadState and IsFaulted properties of the collection.
        /// Optionally sets an error message you can bind too to display on the UI.
        /// </summary>
        /// <param name="errorMessage"></param>
        void SetFaulted(String errorMessage = null);

        void PreSerializing();

        void PostSerializing();
    }

    /// <summary>
    /// Event triggered when data is loaded
    /// </summary>
    public class DataLoadedEventArgs : EventArgs
    {
        public DataLoadedEventArgs()
        {

        }
    }

    /// <summary>
    /// Event triggered when data is loaded
    /// </summary>
    /// <param name="e"></param>
    public delegate void DataLoadedEventHandler(DataLoadedEventArgs e);
}
