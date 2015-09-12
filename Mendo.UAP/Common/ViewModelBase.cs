using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Mendo.UAP.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        #region Set<T>
        /// <summary>
        /// Sets the backing field of a referenced property and fires off a
        /// PropertyChangedEvent if the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Sets the value of a property to our internal dictionary, and fires off a
        /// PropertyChangedEvent if the value has changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool Set<T>(T value, [CallerMemberName] String propertyName = null)
        {
            if (data == null)
                return false;

            object t;
            if (data.TryGetValue(propertyName, out t) && Equals(t, value))
                return false;

            data[propertyName] = value;
            OnPropertyChanged(propertyName);

            return true;
        }
        #endregion

        #region Get<T>
        /// <summary>
        /// Gets the value of a property. If the property does not exist, returns the default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">Default value to set and return if property is null.</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected T Get<T>(T defaultValue = default(T), [CallerMemberName] String propertyName = null)
        {
            object t;
            if (data.TryGetValue(propertyName, out t))
                return (T)t;

            data[propertyName] = defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Gets the value of a property. If the property does not exist, returns the default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">Default value to set and return if property is null.</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected T Get<T>(Func<T> defaultValue = null, [CallerMemberName] String propertyName = null) where T : class
        {
            object t;
            if (data.TryGetValue(propertyName, out t))
                return (T)t;

            T value = defaultValue?.Invoke();
            data[propertyName] = value;
            return value;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify Property Changed
        /// </summary>
        /// <param name="propertyName">Property Name (optional)</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                var eventHandler = PropertyChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception e)
            {
            }
        }
        #endregion

        private static CoreDispatcher _dispatcher;
        /// <summary>
        /// Returns the UI thread Dispatcher for the Core Window of the application
        /// </summary>
        public static CoreDispatcher Dispatcher
        {
            get
            {
                if (_dispatcher == null)
                    _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                return _dispatcher;
            }
        }
    }
}
