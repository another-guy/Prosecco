using System.Linq;
using System.Reflection;

namespace Prosecco.Tests
{
    // TODO Move to other project Tests.Common
    public static class Create<T>
    {
        public static T UsingPrivateConstructor(object[] parameters)
        {
            var defaultConstructor =
                typeof(T)
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .First();
            return (T)defaultConstructor.Invoke(parameters);
        }
    }
}
