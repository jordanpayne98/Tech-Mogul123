using UnityEngine;

namespace TechMogul.Core
{
    public static class ServiceBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            ServiceLocator.Instance.TryRegister<IEventBus>(
                new EventBus(ex => Debug.LogException(ex))
            );
        }
    }
}
