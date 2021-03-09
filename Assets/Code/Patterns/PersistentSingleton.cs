using UnityEngine;

namespace GUTGuide.Patterns
{
    /// <summary>
    /// Implementation of Singleton design pattern. With this implementation, the same instance will be available
    /// when moving to a different scene.
    /// </summary>
    public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// A singular instance of the class
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!_isSingletonDestroyed) return _instance;
                
                Debug.LogWarning($"Singleton of {nameof(T)} was destroyed! Null value was returned.");
                return null;
            }
        }
        
        /// <summary>
        /// A singular instance of the class
        /// </summary>
        private static T _instance;
        /// <summary>
        /// Flag used to mark singleton destruction
        /// </summary>
        private static bool _isSingletonDestroyed;

        protected virtual void Awake()
        {
            // If the  reference to instance does not exist and  the instance was not destroyed yet then this object
            // will take control
            if (_instance == null && !_isSingletonDestroyed)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            // Else this is another instance and it needs to be destroyed
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            // Skip if the instance is other than this object
            if (_instance != null) return;

            // Safely deactivate the singleton
            _isSingletonDestroyed = true;
            _instance = null;
        }
    }
}