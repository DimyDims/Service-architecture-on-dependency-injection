using UnityEngine;

namespace Cerera.Services
{
    public abstract class ServicesConfigurator : MonoBehaviour
    {
        public abstract void ConfigureServices(IServiceCollection serviceCollection);

        public virtual void OnServicesInitialized() { }
    }
}
