using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cerera.Services
{
    public abstract class ServiceProvider : MonoBehaviour, IServiceProvider, IDependentObjectInstantiator
    {
        [SerializeField] private ServicesConfigurator[] _configurators;

        private readonly Dictionary<Type, ServiceCollection.Service> _services
            = new Dictionary<Type, ServiceCollection.Service>();

        private void Awake()
        {
            OnAwake();
            ServiceCollection collection = new ServiceCollection();
            collection.AddInstance<IDependentObjectInstantiator>(this);
            ConfigureServices(collection);
            InitializeServices(collection);
            InitializeDependentObjects();
            Initialized();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            foreach (ServicesConfigurator configurator in _configurators)
            {
                configurator.ConfigureServices(services);
            }
        }

        private void InitializeServices(ServiceCollection collection)
        {
            IReadOnlyCollection<ServiceCollection.ServiceInfo> servicesInfo = collection.GetServices();
            foreach (var serviceInfo in servicesInfo)
            {
                _services[serviceInfo.Type] = serviceInfo.Service;
                serviceInfo.Service.Initialize(this);
            }
        }

        protected virtual void OnAwake() { }

        private void InitializeDependentObjects()
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                IDependentObject[] dependentObjects = root.GetComponentsInChildren<IDependentObject>(includeInactive: true);
                foreach (var dependentObject in dependentObjects)
                {
                    InjectDependencies(dependentObject);
                }
            }
        }

        private void Initialized()
        {
            foreach (ServicesConfigurator configurator in _configurators)
            {
                configurator.OnServicesInitialized();
            }
        }

        public void InjectDependencies(object dependentObject)
        {
            Type type = dependentObject.GetType();
            while (typeof(IDependentObject).IsAssignableFrom(type))
            {
                InjectDependencies(dependentObject, type);
                type = type.BaseType;
            }
        }

        private void InjectDependencies(object dependentObject, Type type)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            IEnumerable<MethodInfo> methodsInfo = type.GetMethods(flags)
                .Where(field => field.GetCustomAttribute<InjectAttribute>() != null);
            foreach (var methodInfo in methodsInfo)
            {
                ParameterInfo[] parametersInfo = methodInfo.GetParameters();
                object[] parameters = new object[parametersInfo.Length];
                for (int i = 0; i < parametersInfo.Length; i++)
                {
                    ParameterInfo parameterInfo = parametersInfo[i];
                    Type parameterType = parameterInfo.ParameterType;
                    object service = GetService(parameterType);
                    if (service == null && !parameterInfo.IsOptional)
                    {
                        throw new NullReferenceException($"No service with type {parameterType}");
                    }

                    parameters[i] = service;
                }

                methodInfo.Invoke(dependentObject, parameters);
            }
        }

        public T GetService<T>()
            where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public virtual object GetService(Type type)
        {
            if (_services.ContainsKey(type))
            {
                return _services[type].GetInstance();
            }

            return null;
        }

        public T InstantiateDependentObject<T>(T original)
            where T : UnityEngine.Object
        {
            return (T)InstantiateDependentObject((UnityEngine.Object)original);
        }

        public UnityEngine.Object InstantiateDependentObject(UnityEngine.Object original)
        {
            bool wasActivated = DeactivateObject(original);
            UnityEngine.Object copy = Instantiate(original);
            InjectObjectDependencies(copy);
            RestoreObjectActivation(original, wasActivated);
            RestoreObjectActivation(copy, wasActivated);
            return copy;
        }

        public T InstantiateDependentObject<T>(T original, Transform parent)
            where T : UnityEngine.Object
        {
            return (T)InstantiateDependentObject((UnityEngine.Object)original, parent);
        }

        public UnityEngine.Object InstantiateDependentObject(UnityEngine.Object original, Transform parent)
        {
            bool wasActivated = DeactivateObject(original);
            UnityEngine.Object copy = Instantiate(original, parent);
            InjectObjectDependencies(copy);
            RestoreObjectActivation(original, wasActivated);
            RestoreObjectActivation(copy, wasActivated);
            return copy;
        }

        public T InstantiateDependentObject<T>(T original, Transform parent, bool worldPositionStays)
            where T : UnityEngine.Object
        {
            return (T)InstantiateDependentObject((UnityEngine.Object)original, parent, worldPositionStays);
        }

        public UnityEngine.Object InstantiateDependentObject(UnityEngine.Object original, Transform parent, bool instantiateInWorldSpace)
        {
            bool wasActivated = DeactivateObject(original);
            UnityEngine.Object copy = Instantiate(original, parent, instantiateInWorldSpace);
            InjectObjectDependencies(copy);
            RestoreObjectActivation(original, wasActivated);
            RestoreObjectActivation(copy, wasActivated);
            return copy;
        }

        public T InstantiateDependentObject<T>(T original, Vector3 position, Quaternion rotation)
            where T : UnityEngine.Object
        {
            return (T)InstantiateDependentObject((UnityEngine.Object)original, position, rotation);
        }

        public UnityEngine.Object InstantiateDependentObject(UnityEngine.Object original, Vector3 position, Quaternion rotation)
        {
            bool wasActivated = DeactivateObject(original);
            UnityEngine.Object copy = Instantiate(original, position, rotation);
            InjectObjectDependencies(copy);
            RestoreObjectActivation(original, wasActivated);
            RestoreObjectActivation(copy, wasActivated);
            return copy;
        }

        public T InstantiateDependentObject<T>(T original, Vector3 position, Quaternion rotation, Transform parent)
            where T : UnityEngine.Object
        {
            return (T)InstantiateDependentObject((UnityEngine.Object)original, position, rotation, parent);
        }

        public UnityEngine.Object InstantiateDependentObject(UnityEngine.Object original, Vector3 position, Quaternion rotation, Transform parent)
        {
            bool wasActivated = DeactivateObject(original);
            UnityEngine.Object copy = Instantiate(original, position, rotation, parent);
            InjectObjectDependencies(copy);
            RestoreObjectActivation(original, wasActivated);
            RestoreObjectActivation(copy, wasActivated);
            return copy;
        }

        private static bool DeactivateObject(UnityEngine.Object unityObject)
        {
            bool wasActivated = false;
            if (unityObject is GameObject gameObject)
            {
                wasActivated = gameObject.activeSelf;
                gameObject.SetActive(false);
            }
            else if (unityObject is Component component)
            {
                return DeactivateObject(component.gameObject);
            }

            return wasActivated;
        }

        private static void RestoreObjectActivation(UnityEngine.Object unityObject, bool wasActivated)
        {
            if (!wasActivated)
            {
                return;
            }

            if (unityObject is Component component)
            {
                RestoreObjectActivation(component.gameObject, wasActivated);
            }
            else
            {
                ((GameObject)unityObject).SetActive(true);
            }
        }

        private void InjectObjectDependencies(UnityEngine.Object unityObject)
        {
            if (unityObject is GameObject gameObject)
            {
                foreach (var dependentObject in gameObject.GetComponentsInChildren<IDependentObject>())
                {
                    InjectDependencies(dependentObject);
                }
            }
            else if (unityObject is Component component)
            {
                InjectObjectDependencies(component.gameObject);
            }
            else if (unityObject is IDependentObject dependentObject)
            {
                InjectDependencies(dependentObject);
            }
        }

        public object AddComponent(GameObject gameObject, Type componentType)
        {
            return gameObject.AddComponent(componentType);
        }

        public T AddComponent<T>(GameObject gameObject)
            where T : Component
        {
            return gameObject.AddComponent<T>();
        }
    }
}
