using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace KT_Interface
{
    public class ContainerRegistry
    {
        private static IUnityContainer _container;
        public static IUnityContainer Container
        {
            get
            {
                if (_container == null)
                    _container = new UnityContainer();

                return _container;
            }
        }
    }
}
