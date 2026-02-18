using System;

namespace TechMogul.Core
{
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(T evt);
        int GetSubscriberCount<T>();
        void Clear();
    }
}
