using UnityEngine;

namespace Cerera.Services
{
    public interface IServiceCollection
    {
        public IServiceCollection AddSingleton<T>();

        public IServiceCollection AddSingleton<TService, TImplementation>();

        public IServiceCollection AddSingleton<T>(string resourcePath)
            where T : ScriptableObject;

        public IServiceCollection AddSingleton<TService, TImplementation>(string resourcePath)
            where TImplementation : ScriptableObject;

        public IServiceCollection AddTransient<T>();

        public IServiceCollection AddTransient<TService, TImplementation>();

        public IServiceCollection AddTransient<T>(string resourcePath)
            where T : ScriptableObject;

        public IServiceCollection AddTransient<TService, TImplementation>(string resourcePath)
            where TImplementation : ScriptableObject;

        public IServiceCollection AddInstance<T>(T instance);
    }
}
