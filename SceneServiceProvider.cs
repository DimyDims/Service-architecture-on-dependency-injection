using System;
using UnityEngine;

namespace Cerera.Services
{
    public sealed class SceneServiceProvider : ServiceProvider
    {
        public override object GetService(Type type)
        {
            object service = base.GetService(type);
            if (service != null)
            {
                return service;
            }

            if (ProjectServiceProvider.Instance == null)
            {
                return null;
            }

            return ProjectServiceProvider.Instance.GetService(type);
        }
    }
}
