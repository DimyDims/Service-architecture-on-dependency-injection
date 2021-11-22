using UnityEngine;

namespace Cerera.Services
{
    public interface IDependentObjectInstantiator
    {
        public T InstantiateDependentObject<T>(T original)
            where T : Object;

        public Object InstantiateDependentObject(Object original);

        public T InstantiateDependentObject<T>(T original, Transform parent)
            where T : Object;

        public Object InstantiateDependentObject(Object original, Transform parent);

        public T InstantiateDependentObject<T>(T original, Transform parent, bool worldPositionStays)
            where T : Object;

        public Object InstantiateDependentObject(Object original, Transform parent, bool instantiateInWorldSpace);

        public T InstantiateDependentObject<T>(T original, Vector3 position, Quaternion rotation)
            where T : Object;

        public Object InstantiateDependentObject(Object original, Vector3 position, Quaternion rotation);

        public T InstantiateDependentObject<T>(T original, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object;

        public Object InstantiateDependentObject(Object original, Vector3 position, Quaternion rotation, Transform parent);

        public object AddComponent(GameObject gameObject, System.Type componentType);

        public T AddComponent<T>(GameObject gameObject)
            where T : Component;
    }
}
