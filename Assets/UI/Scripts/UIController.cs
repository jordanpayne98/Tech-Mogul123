using System;
using System.Collections.Generic;
using UnityEngine;
using TechMogul.Core;

namespace TechMogul.UI
{
    public abstract class UIController : MonoBehaviour
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

            if (!enabled || EventBus == null)
                return;

            if (_isSubscribed)
                return;

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
                Debug.LogError($"[{GetType().Name}] Attempted to subscribe with null handler for {typeof(T).Name}", this);
                return;
            }

            if (EventBus == null)
            {
                EnsureServices();
                if (EventBus == null) return;
            }

            _subscriptions.Add(EventBus.Subscribe(handler));
        }

        private void EnsureServices()
        {
            if (EventBus != null) return;

            if (ServiceLocator.Instance.TryGet<IEventBus>(out var bus))
            {
                EventBus = bus;
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] IEventBus is not registered in ServiceLocator. UI cannot function without EventBus.", this);
                enabled = false; // prevents repeated exceptions / resubscribe loops
            }
        }

        private void UnsubscribeFromEvents()
        {
            foreach (var sub in _subscriptions)
                sub?.Dispose();

            _subscriptions.Clear();
        }
    }
}
