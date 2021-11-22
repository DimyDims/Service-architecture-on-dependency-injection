using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Cerera.Services
{
    public class ServiceCollection : IServiceCollection
    {
        public abstract class Service
        {
            private bool _initialized;

            public abstract object GetInstance();

            internal void Initialize(IServiceProvider serviceProvider)
            {
                if (_initialized)
                {
                    return;
                }

                _initialized = true;
                OnInitialize(serviceProvider);
            }

            protected virtual void OnInitialize(IServiceProvider serviceProvider) { }
        }

        private class InstanceService : Service
        {
            public object Instance { get; }

            public InstanceService(object instance)
            {
                Instance = instance;
            }

            public override object GetInstance()
            {
                return Instance;
            }
        }

        private abstract class ReflectionService : Service
        {
            private Type _implementation;

            public ReflectionService(Type implementation)
            {
                _implementation = implementation;
            }

            protected object CreateInstance(IServiceProvider serviceProvider)
            {
                ConstructorInfo constructor = GetConstructor(_implementation);
                object result = constructor.Invoke(new object[constructor.GetParameters().Length]);
                serviceProvider.InjectDependencies(result);
                return result;
            }

            private static ConstructorInfo GetConstructor(Type type)
            {
                ConstructorInfo result = null;
                int parametersCount = int.MaxValue;
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                foreach (var constructor in type.GetConstructors(flags))
                {
                    int count = constructor.GetParameters().Length;
                    if (count < parametersCount)
                    {
                        result = constructor;
                        parametersCount = count;
                    }
                }

                return result;
            }
        }

        private class SingletonReflectionService : ReflectionService
        {
            private object _instance;

            public SingletonReflectionService(Type implementation)
                : base(implementation) { }

            public override object GetInstance()
            {
                return _instance;
            }

            protected override void OnInitialize(IServiceProvider serviceProvider)
            {
                _instance = CreateInstance(serviceProvider);
            }
        }

        private class InstanceReflectionService : ReflectionService
        {
            private IServiceProvider _serviceProvider;

            public InstanceReflectionService(Type implementation)
                : base(implementation) { }

            protected override void OnInitialize(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override object GetInstance()
            {
                return CreateInstance(_serviceProvider);
            }
        }

        private abstract class ScriptableObjectService : Service
        {
            private readonly string _path;

            private readonly Type _type;

            public ScriptableObjectService(string path, Type type)
            {
                _path = path;
                _type = type;
            }

            protected object Load(IServiceProvider serviceProvider)
            {
                object resource = Resources.Load(_path, _type);
                serviceProvider.InjectDependencies(resource);
                return resource;
            }
        }

        private class SingletonScriptableObjectService: ScriptableObjectService
        {
            private object _instance;

            public SingletonScriptableObjectService(string path, Type type)
                : base(path, type) { }

            protected override void OnInitialize(IServiceProvider serviceProvider)
            {
                _instance = Load(serviceProvider);
            }

            public override object GetInstance()
            {
                return _instance;
            }
        }

        private class InstanceScriptableObjectService : ScriptableObjectService
        {
            private IServiceProvider _serviceProvider;

            public InstanceScriptableObjectService(string path, Type type)
                : base(path, type) { }

            protected override void OnInitialize(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override object GetInstance()
            {
                return Load(_serviceProvider);
            }
        }

        public struct ServiceInfo
        {
            public Type Type { get; }

            public Service Service { get; }

            public ServiceInfo(Type type, Service service)
            {
                Type = type;
                Service = service;
            }
        }

        private readonly List<ServiceInfo> _services = new List<ServiceInfo>();

        public IReadOnlyCollection<ServiceInfo> GetServices()
        {
            return _services.AsReadOnly();
        }

        public IServiceCollection AddSingleton<T>()
        {
            return AddSingleton<T, T>();
        }

        public IServiceCollection AddSingleton<TService, TImplementation>()
        {
            ServiceInfo serviceInfo = new ServiceInfo(
                typeof(TService),
                new SingletonReflectionService(typeof(TImplementation))
            );

            _services.Add(serviceInfo);
            return this;
        }

        public IServiceCollection AddSingleton<T>(string resourcePath)
            where T : ScriptableObject
        {
            return AddSingleton<T, T>(resourcePath);
        }

        public IServiceCollection AddSingleton<TService, TImplementation>(string resourcePath)
            where TImplementation : ScriptableObject
        {
            ServiceInfo serviceInfo = new ServiceInfo(
                typeof(TService),
                new SingletonScriptableObjectService(resourcePath, typeof(TImplementation))
            );

            _services.Add(serviceInfo);
            return this;
        }

        public IServiceCollection AddTransient<T>()
        {
            return AddTransient<T, T>();
        }

        public IServiceCollection AddTransient<TService, TImplementation>()
        {
            ServiceInfo serviceInfo = new ServiceInfo(
                typeof(TService),
                new InstanceReflectionService(typeof(TImplementation))
            );

            _services.Add(serviceInfo);
            return this;
        }

        public IServiceCollection AddTransient<T>(string resourcePath)
            where T : ScriptableObject
        {
            return AddTransient<T, T>(resourcePath);
        }

        public IServiceCollection AddTransient<TService, TImplementation>(string resourcePath)
            where TImplementation : ScriptableObject
        {
            ServiceInfo serviceInfo = new ServiceInfo(
                typeof(TService),
                new InstanceScriptableObjectService(resourcePath, typeof(TImplementation))
            );

            _services.Add(serviceInfo);
            return this;
        }

        public IServiceCollection AddInstance<T>(T instance)
        {
            ServiceInfo serviceInfo = new ServiceInfo(typeof(T), new InstanceService(instance));
            _services.Add(serviceInfo);
            return this;
        }
    }
}
