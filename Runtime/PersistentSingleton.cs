using UnityEngine;

namespace Kickstarter.Singleton
{
    public class PersistentSingleton<T> : Singleton<T> where T : Component
    {
        protected override void Awake()
        {
            if (instance == null)
                DontDestroyOnLoad(gameObject);
            else
            {
                DestroyImmediate(gameObject);
                return;
            }
            base.Awake();
        }
    }
}
