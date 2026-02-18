using System;
using System.Collections.Generic;

namespace TechMogul.Core
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();
        private readonly Action<Exception> _onError;
        
        public EventBus(Action<Exception> onError = null)
        {
            _onError = onError;
        }
        
        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            Type eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            
            if (!_eventHandlers[eventType].Contains(handler))
            {
                _eventHandlers[eventType].Add(handler);
            }
            
            return new Subscription<T>(this, handler);
        }
        
        private void Unsubscribe<T>(Action<T> handler)
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
        
        public void Publish<T>(T eventData)
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
                        _onError?.Invoke(ex);
                    }
                }
            }
        }
        
        public void Clear()
        {
            _eventHandlers.Clear();
        }
        
        public int GetSubscriberCount<T>()
        {
            Type eventType = typeof(T);
            return _eventHandlers.ContainsKey(eventType) ? _eventHandlers[eventType].Count : 0;
        }
        
        private class Subscription<T> : IDisposable
        {
            private EventBus _eventBus;
            private Action<T> _handler;
            private bool _disposed;
            
            public Subscription(EventBus eventBus, Action<T> handler)
            {
                _eventBus = eventBus;
                _handler = handler;
            }
            
            public void Dispose()
            {
                if (!_disposed)
                {
                    _eventBus?.Unsubscribe(_handler);
                    _eventBus = null;
                    _handler = null;
                    _disposed = true;
                }
            }
        }
    }
}
