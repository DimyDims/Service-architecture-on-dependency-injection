using System;

namespace Cerera.Services
{
    public sealed class ProjectServiceProvider : ServiceProvider
    {
        public static ProjectServiceProvider Instance => _instance;
        private static ProjectServiceProvider _instance;

        protected sealed override void OnAwake()
        {
            if (Instance != null)
            {
                throw new Exception("ProjectServiceProvider already exists");
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }

        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public sealed override object GetService(Type type)
        {
            return base.GetService(type);
        }
    }
}
