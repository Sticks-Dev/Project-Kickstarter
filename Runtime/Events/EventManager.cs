using System;
using System.Collections.Generic;

namespace Kickstarter.Events
{
    /// <summary>
    /// Used for sending data between classes without the requirement of directly referencing it.
    /// Very useful for when many objects need to notified with some particular information.
    /// </summary>
    public static class EventManager
    {
        private static readonly Dictionary<Type,object> listeners = new Dictionary<Type, object>();

        /// <summary>
        /// Register a listener to an event according to the parameter type of the listener.
        /// </summary>
        /// <typeparam name="TArgument">The key for the event being used and the type of required parameter.</typeparam>
        /// <param name="listener">The method to be called when the matching event is triggered.</param>
        public static void Register<TArgument>(Action<TArgument> listener)
        {
            listeners.TryAdd(typeof(TArgument), new List<Action<TArgument>>());
            var typedListeners = listeners[typeof(TArgument)] as List<Action<TArgument>>;
            if (typedListeners.Contains(listener))
                return;
            typedListeners.Add(listener);
        }

        /// <summary>
        /// Deregister a listener from the event which shares a parameter type with the listener.
        /// </summary>
        /// <typeparam name="TArgument">The key for the event being used and the type of required parameter.</typeparam>
        /// <param name="listener">The method being removed from the event with a matching parameter type.</param>
        public static void Deregister<TArgument>(Action<TArgument> listener)
        {
            listeners.TryAdd(typeof(TArgument), new List<Action<object>>());
            var typedListeners = listeners[typeof(TArgument)] as List<Action<TArgument>>;
            if (!typedListeners.Contains(listener))
                return;
            typedListeners.Remove(listener);
        }

        /// <summary>
        /// Call an event to trigger all the listeners which are currently registered to it.
        /// </summary>
        /// <typeparam name="TArgument">The key for the event being used and the type of required parameter.</typeparam>
        /// <param name="argument">The information being passed into the event to be received by the listeners.</param>
        public static void Trigger<TArgument>(TArgument argument)
        {
            listeners.TryGetValue(typeof(TArgument), out var typedListeners);
            (typedListeners as List<Action<TArgument>>).ForEach(listener => listener(argument));
        }
    }
}