using System.Collections.Generic;
using UnityEngine;
using TechMogul.Data;
using TechMogul.Products;
using TechMogul.Systems;

namespace TechMogul.Core
{
    public class DefinitionResolver : IDefinitionResolver
    {
        private readonly Dictionary<string, Object> _idToAsset = new Dictionary<string, Object>();
        private readonly Dictionary<Object, string> _assetToId = new Dictionary<Object, string>();
        
        public DefinitionResolver(DefinitionRegistrySO registry)
        {
            if (registry == null)
            {
                Debug.LogError("DefinitionRegistry is null. Cannot initialize DefinitionResolver.");
                return;
            }
            
            RegisterDefinitions(registry.roles);
            RegisterDefinitions(registry.productCategories);
            RegisterDefinitions(registry.contractTemplates);
            RegisterDefinitions(registry.features);
            RegisterDefinitions(registry.qaTiers);
            RegisterDefinitions(registry.eras);
            RegisterDefinitions(registry.technologies);
            RegisterDefinitions(registry.marketCategories);
        }
        
        public T Resolve<T>(string id) where T : Object
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            
            if (_idToAsset.TryGetValue(id, out Object obj))
            {
                return obj as T;
            }
            
            Debug.LogWarning($"Failed to resolve definition with ID: {id}");
            return null;
        }
        
        public string GetId(Object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            
            if (_assetToId.TryGetValue(obj, out string id))
            {
                return id;
            }
            
            Debug.LogWarning($"No ID registered for asset: {obj.name}");
            return string.Empty;
        }
        
        private void RegisterDefinitions<T>(List<T> definitions) where T : Object, IIdentifiable
        {
            if (definitions == null)
            {
                return;
            }
            
            foreach (T definition in definitions)
            {
                if (definition == null)
                {
                    continue;
                }
                
                string id = definition.Id;
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogWarning($"Definition {definition.name} has no ID. Skipping registration.");
                    continue;
                }
                
                if (_idToAsset.ContainsKey(id))
                {
                    Debug.LogError($"Duplicate ID '{id}' found for {definition.name}. IDs must be unique.");
                    continue;
                }
                
                _idToAsset[id] = definition;
                _assetToId[definition] = id;
            }
        }
    }
}
