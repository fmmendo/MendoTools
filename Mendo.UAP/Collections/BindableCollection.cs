using Mendo.UAP.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace Mendo.UAP
{
    /// <summary>
    /// reference: http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged, ISupportIncrementalLoading//, ILoadableResult
    {
        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BindableCollection() : base() { }

        /// <summary>
        /// Initializes a new instance of the BindableCollection class that contains elements
        /// copied from the specified list
        /// </summary>
        /// <param name="list">The list whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the BindableCollection in the same order they are read by
        /// the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public BindableCollection(List<T> list) : base((list != null) ? new List<T>(list.Count) : list)
        {
            // Workaround for VSWhidbey bug 562681 (tracked by Windows bug 1369339).
            // We should be able to simply call the base(list) ctor.  But Collection<T> doesn't copy the list 
            // (contrary to the documentation) - it uses the list directly as its storage.  So we do the copying here.
            CopyFrom(list);
        }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains elements
        /// copied from the specified collection and has sufficient capacity to accommodate the
        /// number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the same order they are read by
        /// the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public BindableCollection(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CopyFrom(collection);
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = Items;
            if (collection != null && items != null)
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Adds a collection of items to a BindableCollection. 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="notifyType"></param>
        public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notifyType = NotifyCollectionChangedAction.Add)
        {
            var items = Items;
            if (collection != null && items != null)
            {
                using (var enumerator = collection.GetEnumerator())
                {
                    if (notifyType == NotifyCollectionChangedAction.Add)
                    {
                        // only notify add
                        while (enumerator.MoveNext())
                        {
                            items.Add(enumerator.Current);
                            OnCollectionChanged(NotifyCollectionChangedAction.Add, enumerator.Current, items.Count - 1);
                        }
                    }
                    else
                    {
                        while (enumerator.MoveNext())
                        {
                            items.Add(enumerator.Current);
                        }

                        if (notifyType == NotifyCollectionChangedAction.Reset)
                            OnCollectionReset();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the relevant LoadState and IsFaulted properties of the collection.
        /// </summary>
        /// <param name="errorMessage">Optional error message</param>
        public void SetFaulted(String errorMessage = null)
        {
            ErrorMessage = errorMessage;
            IsFaulted = true;

            if (!IsFirstLoaded)
                LoadState = LoadState.FirstLoadFailed;
            else
                LoadState = LoadState.SuccessiveLoadFailed;
        }
        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Called by base class Collection<T> when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            CheckReentrancy();
            base.ClearItems();
            OnPropertyChanged(nameof(IsFirstLoadedEmpty));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        /// <summary>
        /// Called by base class Collection<T> when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            T removedItem = this[index];

            base.RemoveItem(index);

            OnPropertyChanged(nameof(IsFirstLoadedEmpty));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        /// <summary>
        /// Called by base class Collection<T> when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);
            OnPropertyChanged(nameof(IsFirstLoadedEmpty));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        /// <summary>
        /// Called by base class Collection<T> when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            T originalItem = this[index];
            base.SetItem(index, item);
            OnPropertyChanged(nameof(IsFirstLoadedEmpty));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        /// <summary>
        /// Called by base class BindableCollection<T> when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            T removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);
            OnPropertyChanged(nameof(IsFirstLoadedEmpty));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }

        /// <summary>
        /// Disallow reentrant attempts to change this collection. E.g. a event handler
        /// of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        /// <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        /// </code>
        /// </remarks>
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException("Reentrancy no allowed");
            }
        }

        #endregion Protected Methods

        #region Private Types

        /// <summary>
        /// The SimpleMonitor class helps prevent reentrant calls
        /// </summary>
        private class SimpleMonitor : IDisposable
        {
            int _busyCount;

            public bool Busy => _busyCount > 0;

            public void Enter() => ++_busyCount;
            public void Dispose() => --_busyCount;
        }

        #endregion Private Types

        #region Private Fields

        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";

        private SimpleMonitor _monitor = new SimpleMonitor();

        #endregion Private Fields

        #region Properties

        /// <summary>
        /// When set to false, prevents IncrementalLoading from continuing.
        /// </summary>
        public Boolean CanLoadMoreItems
        {
            get { return canLoadMoreItems; }
            set
            {
                canLoadMoreItems = value;
                OnPropertyChanged(nameof(CanLoadMoreItems));
                OnPropertyChanged(nameof(HasMoreItems));
            }
        }

        /// <summary>
        /// Returns whether the list is currently loading items.
        /// </summary>
        public Boolean IsLoading
        {
            get { return isLoading; }
            private set
            {
                isLoading = value;
                if (value)
                {
                    IsFaulted = false;
                    ErrorMessage = null;
                }

                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(IsEmptyLoading));
                OnPropertyChanged(nameof(IsSubsequentLoading));
                OnPropertyChanged(nameof(CanLoadMoreItems));
            }
        }

        public Boolean IsNotLoaded => LoadState == LoadState.NotLoaded;

        /// <summary>
        /// Returns true if the collection is empty but loading items.
        /// </summary>
        public bool IsEmptyLoading => IsLoading && Count == 0;

        /// <summary>
        /// Returns true if the collection has items and is loading more.
        /// </summary>
        public bool IsSubsequentLoading => IsLoading && Count > 0;

        /// <summary>
        /// Returns true if the collection has been through one load operation 
        /// previously.
        /// </summary>
        public bool IsFirstLoaded
        {
            get { return isFirstLoaded; }
            set
            {
                isFirstLoaded = value;

                OnPropertyChanged(nameof(IsFirstLoaded));
                OnPropertyChanged(nameof(IsFirstLoadFaulted));
                OnPropertyChanged(nameof(IsFirstLoadedEmpty));
            }
        }

        public bool IsFirstLoadedWithContent => IsFirstLoaded && this.Items.Count > 0;

        public uint PageSize
        {
            get { return pageSize; }
            set { pageSize = value; OnPropertyChanged(nameof(PageSize)); }
        }

        public uint PageOffset
        {
            get { return pageOffset; }
            set { pageOffset = value; OnPropertyChanged(nameof(PageOffset)); }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public string EmptyMessage
        {
            get { return emptyMessage; }
            set { emptyMessage = value; OnPropertyChanged(nameof(EmptyMessage)); }
        }

        /// <summary>
        /// Returns true if there are no items in the collection
        /// </summary>
        public bool IsEmpty => !Items.Any();

        /// <summary>
        /// Returns true if the collection has loaded but still has no items
        /// </summary>
        public bool IsFirstLoadedEmpty => IsFirstLoaded && !Items.Any();

        /// <summary>
        /// Returns true is the collection failed first loading
        /// </summary>
        public bool IsFirstLoadFaulted => !IsFirstLoaded && IsFaulted;

        public bool IsFaulted
        {

            get { return isFaulted; }
            set
            {
                isFaulted = value;

                OnPropertyChanged(nameof(IsFaulted));
                OnPropertyChanged(nameof(IsFirstLoadFaulted));
            }
        }

        public LoadState LoadState
        {
            get { return loadState; }
            set
            {
                loadState = value;

                switch (value)
                {
                    case LoadState.Loaded:
                        IsFirstLoaded = true;
                        IsLoading = false;
                        break;
                    case LoadState.Loading:
                        IsLoading = true;
                        break;
                    case LoadState.NotLoaded:
                        IsFirstLoaded = false;
                        IsLoading = false;
                        break;
                    case LoadState.FirstLoadFailed:
                    case LoadState.SuccessiveLoadFailed:
                        IsFaulted = true;
                        IsLoading = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the UI thread Dispatcher
        /// </summary>
        protected CoreDispatcher Dispatcher => CoreApplication.MainView.CoreWindow.Dispatcher;

        #endregion Properties

        #region Interface Implementations

        #region INotifyPropertyChanged implementation

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
#if !FEATURE_NETCORE
#endif
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged implementation

        #region INotifyCollectionChanged implementation

        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        /// see <seealso cref="INotifyCollectionChanged"/>
        /// </remarks>
#if !FEATURE_NETCORE
        //[field: NonSerializedAttribute()]
#endif
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this BindableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }/// <summary>
         /// Helper to raise CollectionChanged event to any listeners
         /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion INotifyCollectionChanged implementation

        #region ISupportIncrementalLoading implementation

        public WeakFunc<Task> AsyncLoadAction { get; set; }

        public bool HasMoreItems => AsyncLoadAction != null && CanLoadMoreItems && !IsLoading;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var tcs = new TaskCompletionSource<LoadMoreItemsResult>();

            Task.Run(
                async () =>
                {
                    try
                    {
                        if (AsyncLoadAction != null && AsyncLoadAction.IsAlive)
                        {
                            if (Dispatcher.HasThreadAccess)
                            {
                                await AsyncLoadAction.Execute();
                            }
                            else
                            {
                                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                {
                                    await AsyncLoadAction.Execute();
                                    tcs.TrySetResult(new LoadMoreItemsResult() { Count = (uint)Items.Count });
                                });
                            }
                        }
                        else if (AsyncLoadAction == null)
                            throw new ArgumentNullException();
                        else if (!AsyncLoadAction.IsAlive)
                            throw new ArgumentOutOfRangeException("Item Load Action is no longer alive");
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                    }
                }
            );

            return tcs.Task.AsAsyncOperation();
        }

        #endregion ISupportIncrementalLoading implementation

        #endregion Interface Implementations

        private bool canLoadMoreItems = true;
        private bool isFirstLoaded = false;
        private bool isLoading = false;
        private bool isFaulted = false;

        private uint pageSize = 10;
        private uint pageOffset = 0;

        private string errorMessage = string.Empty;
        private string emptyMessage = string.Empty;

        public LoadState loadState = LoadState.NotLoaded;
    }
}
