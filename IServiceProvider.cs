using System;

namespace Cerera.Services
{
    public interface IServiceProvider
    {
        public void InjectDependencies(object dependentObject);

        public T GetService<T>()
            where T : class;

        public object GetService(Type type);
    }
}
