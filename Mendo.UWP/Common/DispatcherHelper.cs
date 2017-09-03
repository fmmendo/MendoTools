using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Mendo.UWP.Common
{
    public class DispatcherHelper
    {
        private static CoreDispatcher _dispatcher;
        /// <summary>
        /// Returns the UI thread Dispatcher for the Core Window of the application
        /// </summary>
        public static CoreDispatcher Dispatcher => _dispatcher ?? (_dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher);


        /// <summary>
        /// Ensures an Action is ALWAYS run on the UI thread. If there is no UI thread, things will die.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        public static Task<int> MarshallAsync(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var tcs = new TaskCompletionSource<int>();

            try
            {
                if (Dispatcher.HasThreadAccess)
                {
                    action.Invoke();
                    tcs.SetResult(0);
                }
                else
                {
                    var _action = Dispatcher.RunAsync(priority, () =>
                    {
                        action.Invoke();
                        tcs.SetResult(0);
                    });
                }

            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }


        /// <summary>
        /// Ensures a Function is ALWAYS run on the UI thread. If there is no UI thread, things will die.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        public static Task<int> MarshallFuncAsync(Func<Task> action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var tcs = new TaskCompletionSource<int>();

            try
            {
                if (Dispatcher.HasThreadAccess)
                {
                    Task tk = (action.Invoke()).ContinueWith((t) => tcs.SetResult(0), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    var _action = Dispatcher.RunAsync(priority, async () =>
                    {
                        await action.Invoke();
                        tcs.SetResult(0);
                    });
                }

            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }
}
