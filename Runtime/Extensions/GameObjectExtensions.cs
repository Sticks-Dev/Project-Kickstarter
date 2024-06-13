using UnityEngine;

namespace Kickstarter.Extensions
{
    public static class GameObjectExtensions
    {
        public static TComponent GetOrAdd<TComponent>(this GameObject go) where TComponent : Component
        {
            var component = go.GetComponent<TComponent>();
            if (!component)
                component = go.AddComponent<TComponent>();
            return component;
        }

        public static T OrNull<T>(this T obj) where T : Object
        {
            return obj ? obj : null;
        }
    }
}
