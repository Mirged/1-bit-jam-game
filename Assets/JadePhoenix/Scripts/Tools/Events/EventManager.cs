using System;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    /// <summary>
    /// Represents a game event with an associated name.
    /// </summary>
    public struct GameEvent
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string EventName;

        /// <summary>
        /// Constructs a new game event with the given name.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public GameEvent(string eventName)
        {
            EventName = eventName;
        }

        // A static instance of GameEvent used in the Trigger method.
        static GameEvent e;

        /// <summary>
        /// Triggers the event by updating the name and calling the associated event manager.
        /// </summary>
        /// <param name="newName">The new name of the event to trigger.</param>
        public static void Trigger(string newName)
        {
            e.EventName = newName;
            EventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// Manages event listeners and provides methods to add, remove and trigger events.
    /// </summary>
    /// <remarks>
    /// This class is executed always, even if the script component is not enabled.
    /// </remarks>
    [ExecuteAlways]
    public static class EventManager
    {
        // A dictionary that contains all subscribers categorized by their type.
        private static Dictionary<Type, List<IEventListenerBase>> _subscribersList;

        // Static constructor that initializes the subscribers list.
        static EventManager()
        {
            _subscribersList = new Dictionary<Type, List<IEventListenerBase>>();
        }

        /// <summary>
        /// Adds a listener to the specified event type.
        /// </summary>
        /// <typeparam name="EventType">The event type.</typeparam>
        /// <param name="listener">The listener to add.</param>
        public static void AddListener<EventType>(IEventListener<EventType> listener) where EventType : struct
        {
            Type eventType = typeof(EventType);

            if (!_subscribersList.ContainsKey(eventType))
            {
                _subscribersList[eventType] = new List<IEventListenerBase>();
            }

            if (!ListenersExist(eventType, listener))
            {
                _subscribersList[eventType].Add(listener);
            }
        }

        /// <summary>
        /// Removes a listener from a given event type.
        /// </summary>
        /// <typeparam name="EventType">The event type.</typeparam>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveListener<EventType>(IEventListener<EventType> listener) where EventType : struct
        {
            Type eventType = typeof(EventType);

            if (!_subscribersList.ContainsKey(eventType)) { return; }

            List<IEventListenerBase> subscriberList = _subscribersList[eventType];

            for (int i = 0; i < subscriberList.Count; i++)
            {
                if (subscriberList[i] == listener)
                {
                    subscriberList.Remove(subscriberList[i]);

                    if (subscriberList.Count == 0)
                    {
                        _subscribersList.Remove(eventType);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Triggers an event, notifying all associated listeners.
        /// </summary>
        /// <typeparam name="EventType">The event type to trigger.</typeparam>
        /// <param name="newEvent">The instance of the event to trigger.</param>
        public static void TriggerEvent<EventType>(EventType newEvent) where EventType : struct
        {
            List<IEventListenerBase> listeners;
            if (!_subscribersList.TryGetValue(typeof(EventType), out listeners)) { return; }

            for (int i = 0; i < listeners.Count; i++)
            {
                (listeners[i] as IEventListener<EventType>).OnEvent(newEvent);
            }
        }

        /// <summary>
        /// Checks if there are subscribers for a certain type of events
        /// </summary>
        /// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="receiver">Receiver.</param>
        private static bool ListenersExist(Type type, IEventListenerBase listener)
        {
            List<IEventListenerBase> listeners;

            if (!_subscribersList.TryGetValue(type, out listeners)) return false;

            bool listenersExist = false;

            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] == listener)
                {
                    listenersExist = true;
                    break;
                }
            }
            return listenersExist;
        }
    }

    /// <summary>
    /// Provides extension methods to register and unregister event listeners.
    /// </summary>
    public static class EventRegister
    {
        // Delegate definition for event handlers.
        public delegate void Delegate<T>(T eventType);

        /// <summary>
        /// Starts listening to an event of the specified type.
        /// </summary>
        /// <typeparam name="EventType">The event type.</typeparam>
        /// <param name="caller">The caller of this extension method.</param>
        public static void EventListeningStart<EventType>(this IEventListener<EventType> caller) where EventType : struct
        {
            EventManager.AddListener<EventType>(caller);
        }

        /// <summary>
        /// Stops listening to an event of the specified type.
        /// </summary>
        /// <typeparam name="EventType">The event type.</typeparam>
        /// <param name="caller">The caller of this extension method.</param>
        public static void EventListeningStop<EventType>(this IEventListener<EventType> caller) where EventType : struct
        {
            EventManager.RemoveListener<EventType>(caller);
        }
    }

    /// <summary>
    /// Represents a base interface for all event listeners.
    /// </summary>
    public interface IEventListenerBase { };

    /// <summary>
    /// Defines an interface for classes that want to listen for specific events.
    /// </summary>
    /// <typeparam name="T">The event type to listen to.</typeparam>
    public interface IEventListener<T> : IEventListenerBase
    {
        /// <summary>
        /// Method called when the associated event is triggered.
        /// </summary>
        /// <param name="eventType">The instance of the event being triggered.</param>
        void OnEvent(T eventType);
    }
}
