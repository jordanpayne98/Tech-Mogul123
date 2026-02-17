using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Core
{
    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();
        
        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            
            if (!_eventHandlers[eventType].Contains(handler))
            {
                _eventHandlers[eventType].Add(handler);
            }
        }
        
        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type eventType = typeof(T);
            
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Remove(handler);
                
                if (_eventHandlers[eventType].Count == 0)
                {
                    _eventHandlers.Remove(eventType);
                }
            }
        }
        
        public static void Publish<T>(T eventData) where T : class
        {
            Type eventType = typeof(T);
            
            if (_eventHandlers.ContainsKey(eventType))
            {
                var handlers = new List<Delegate>(_eventHandlers[eventType]);
                
                foreach (var handler in handlers)
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }
        
        public static void Clear()
        {
            _eventHandlers.Clear();
        }
        
        public static int GetSubscriberCount<T>() where T : class
        {
            Type eventType = typeof(T);
            return _eventHandlers.ContainsKey(eventType) ? _eventHandlers[eventType].Count : 0;
        }
    }
}
