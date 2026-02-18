using System;
using System.Collections.Generic;

namespace TechMogul.Core
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ?? (_instance = new ServiceLocator());
        
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        public void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Service of type {type.Name} is already registered.");
            }
            
            _services[type] = service;
        }
    
        public bool TryRegister<T>(T service) where T : class
        {
            Type type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                return false;
            }
            
            _services[type] = service;
            return true;
        }
        
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            if (_services.TryGetValue(type, out object service))
            {
                return service as T;
            }
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
        }
        
        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            
            if (_services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return service != null;
            }
            
            service = null;
            return false;
        }
        
        public void Clear()
        {
            _services.Clear();
        }
        
        public static void ResetInstance()
        {
            _instance = null;
        }
    }
}
