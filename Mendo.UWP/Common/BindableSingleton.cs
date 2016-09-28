namespace Mendo.UWP.Common
{
    public class BindableSingleton<T> : ViewModelBase where T : new()
    {
        private static T _instance = new T();
        public static T Instance => _instance;
    }
}
