using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Mendo.UAP.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
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
