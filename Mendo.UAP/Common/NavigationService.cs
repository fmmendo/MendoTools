using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Mendo.UAP.Common
{
    //public interface INavigationService
    //{

    //    event NavigatingCancelEventHandler Navigating;
    //    void Navigate(Type type);
    //    void Navigate(Type type, object parameter);
    //    void Navigate(string type);
    //    void Navigate(string type, object parameter);
    //    void GoBack();
    //    void GoHome();

    //    //void NavigateTo(object pageType, object data, NavigationTransitionInfo transition = null);
    //    //void NavigateTo(object pageType);
    //    //object GetData(object pageType);
    //    //void RemoveData(object pageType);
    //    //void RegisterData(object pageType, object data);
    //    //void RemoveLastEntry();
    //    //void Clear();

    //}

    public static class NavigationService// : INavigationService
    {
        private static Frame _frame;


        public static void SetFrame(Frame frame)
        {
            _frame = frame;
        }
        
        public static Boolean Navigate(Type pageType, object parameter = null, NavigationTransitionInfo transitionOverride = null)
        {
            EnsureReady();

            var key = (_frame.Content == null) ? "Page-0" : "Page-" + (_frame.BackStackDepth + 1);

            if (SuspensionManager.ParameterStates.ContainsKey(key))
                SuspensionManager.ParameterStates[key] = parameter;
            else
                SuspensionManager.ParameterStates.Add(key, parameter);

            return _frame.Navigate(pageType, key, transitionOverride);
        }

        public static void GoHome()
        {
            EnsureReady();

            while (_frame.CanGoBack)
                _frame.GoBack();
        }

        public static bool CanGoBack()
        {
            if (_frame != null)
                return _frame.CanGoBack;
            else
                return false;
        }

        public static void GoBack()
        {
            EnsureReady();

            if (_frame.CanGoBack)
                _frame.GoBack();
        }

        static void EnsureReady()
        {
            if (_frame == null)
                throw new NullReferenceException("You must call SetFrame(...) and register a Frame before using Navigation Service");
        }
    }
}
