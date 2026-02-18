using UnityEngine;

namespace TechMogul.Core
{
    public interface IDefinitionResolver
    {
        T Resolve<T>(string id) where T : Object;
        string GetId(Object obj);
    }
}
