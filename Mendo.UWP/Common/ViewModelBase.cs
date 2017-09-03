using Mendo.UWP.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Mendo.UWP.Common
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Private data store
        /// </summary>
        private Dictionary<string, object> data = new Dictionary<string, object>();

        /// <summary>
        /// When overridden, allows you to specify whether all standard SetProperty calls
        /// automatically attempt to fire their property change notifications on the dispatcher.
        /// Defaults to false. (Be aware of any possible threading issues this may cause - UI
        /// thread will be updated notable after the data layer)
        /// </summary>
        public virtual Boolean AutomaticallyMarshalToDispatcher => false;

        /// <summary>
        /// Used to specify the dispatcher priority used when automatically marshaling 
        /// to the Dispatcher thread or when using DispatcherSetProperty method
        /// </summary>
        public virtual CoreDispatcherPriority DefaultMarshalingPriority => CoreDispatcherPriority.Normal;

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
            OnPropertyChanged(propertyName, AutomaticallyMarshalToDispatcher);

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
            OnPropertyChanged(propertyName, AutomaticallyMarshalToDispatcher);

            return true;
        }

        /// <summary>
        /// Sets a property value. If the value has changed, property changed notifications are called
        /// for the specified property, and all dependent properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">New desired value of the property</param>
        /// <param name="propertyName">Name of the property being set. This cannot be inferred automatically because of the params overload.</param>
        /// <param name="dependentProperties">Names of additional properties to call PropertyChanged events for</param>
        /// <returns></returns>
        protected Boolean Set<T>(T value, String propertyName, params string[] dependentProperties)
        {
            if (Set(value, propertyName))
            {
                this.OnPropertiesChanged(dependentProperties);
                return true;
            }

            return false;
        }
        #endregion

        #region Get<T>
        /// <summary>
        /// Gets the value of a property. If the property does not exist, returns the default value.
        /// Best for value types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">Default value to set and return if property is null.</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected T GetV<T>(T defaultValue = default(T), [CallerMemberName] String propertyName = null)
        {
            object t;
            if (data.TryGetValue(propertyName, out t))
                return (T)t;

            data[propertyName] = defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Gets the value of a property. If the property does not exist, returns the default value.
        /// Best for object types.
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


            T value = (defaultValue == null) ? default(T) : defaultValue.Invoke();
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
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null, bool fireOverDispatcher = false)
        {
            try
            {
                bool forceFire = fireOverDispatcher || AutomaticallyMarshalToDispatcher;

                if (forceFire)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                else                    
                    DispatcherHelper.MarshallAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
            catch (Exception)
            {
            }
        }

        protected void OnPropertiesChanged(params String[] args)
        {
            // Marshall all of them together
            DispatcherHelper.MarshallAsync(() =>
            {
                args.DoImmediate(a => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(a)));
            });
        }

        /// <summary>
        /// Broadcasts an OnPropertyChanged notification signifying all properties should be re-evaluated
        /// </summary>
        /// <param name="forceFireOverDispatcher"></param>
        protected void OnAllPropertiesChanged(bool forceFireOverDispatcher = false)
        {
            try
            {
                var eventHandler = this.PropertyChanged;
                if (eventHandler != null)
                {
                    bool forceFire = forceFireOverDispatcher || AutomaticallyMarshalToDispatcher;

                    if (!forceFire)
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(String.Empty));
                    else
                        DispatcherHelper.MarshallAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(String.Empty)));
                }
            }
            catch (Exception e)
            {
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Frees up memory associated with the internal dictionary that may otherwise cause memory leakage
        /// </summary>
        public virtual void Dispose()
        {
            ((IDisposable)this).Dispose();
        }

        void IDisposable.Dispose()
        {
            data.Clear();
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
