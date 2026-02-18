using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Core
{
    public abstract class GameSystem : MonoBehaviour
    {
        protected IEventBus EventBus { get; private set; }
        
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _isSubscribed;
        
        protected virtual void Awake()
        {
            EnsureServices();
        }
        
        protected virtual void OnEnable()
        {
            EnsureServices();
            
            if (_isSubscribed)
            {
                return;
            }
            
            SubscribeToEvents();
            _isSubscribed = true;
        }
        
        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
            _isSubscribed = false;
        }
        
        protected abstract void SubscribeToEvents();
        
        protected void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                Debug.LogError($"[{GetType().Name}] Attempted to subscribe with null handler for event type {typeof(T).Name}");
                return;
            }
            
            if (EventBus == null)
            {
                EnsureServices();
                if (EventBus == null)
                {
                    return;
                }
            }
            
            IDisposable subscription = EventBus.Subscribe(handler);
            _subscriptions.Add(subscription);
        }
        
        private void EnsureServices()
        {
            if (EventBus != null)
            {
                return;
            }
            
            if (ServiceLocator.Instance.TryGet<IEventBus>(out IEventBus eventBus))
            {
                EventBus = eventBus;
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] IEventBus is not registered in ServiceLocator. System cannot function without EventBus.", this);
                enabled = false;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i]?.Dispose();
            }
            _subscriptions.Clear();
        }
    }
}
