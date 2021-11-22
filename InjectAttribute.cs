using System;

namespace Cerera.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InjectAttribute : Attribute { }
}
