namespace Mendo.UAP.Common
{
    public class SingletonViewModelBase<T> : ViewModelBase where T : new()
    {
        private static T _instance = new T();

        /// <summary>
        /// Returns the singleton instance of this class
        /// </summary>
        public static T Instance => _instance;
    }
}
