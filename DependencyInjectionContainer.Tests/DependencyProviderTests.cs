using DependencyInjectionContainer.Exception;
using DependencyInjectionContainer.Tests.TestableClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionContainer.Tests
{
    [TestClass]
    public class DependencyProviderTests
    {
        [TestMethod]
        public void GenericResolveSimpleTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IService<IRepository>, ServiceImpl1<MyRepository>>();
            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>();
            var service = container.Resolve<IService<IRepository>>();
            Assert.IsInstanceOfType(repository, typeof(MyRepository));
            Assert.IsInstanceOfType(service, typeof(ServiceImpl1<MyRepository>));
        }

        [TestMethod]
        public void SingletonTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeType.Singleton, "1");
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeType.InstancePerDependency, "2");
            var container = new DependencyProvider(configuration);
            var serviceImpl1 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl2 = container.Resolve<IService<IRepository>>("2");

            var serviceImpl11 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl22 = container.Resolve<IService<IRepository>>("2");

            Assert.IsTrue(serviceImpl1 == serviceImpl11);
            Assert.IsFalse(serviceImpl2 == serviceImpl22);
        }

        [TestMethod]
        public void BaseInterfaceTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register<IBaseService, ServiceImpl1<IRepository>>();
            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IBaseService>();

            Assert.IsInstanceOfType(service, typeof(ServiceImpl1<IRepository>));
        }

        [TestMethod]
        public void ListOfDependenciesWithOpenGenericsTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IRepository), typeof(SomeRepository));
            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();
            Assert.IsTrue(services.Count == 2);

            Assert.IsInstanceOfType(services[0], typeof(MyRepository));
            Assert.IsInstanceOfType(services[1], typeof(SomeRepository));
        }

        [TestMethod]
        public void ListOfDependenciesTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, SomeRepository>();
            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            Assert.IsTrue(services.Count == 2);

            Assert.IsInstanceOfType(services[0], typeof(MyRepository));
            Assert.IsInstanceOfType(services[1], typeof(SomeRepository));
        }

        [TestMethod]
        public void RecursionTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<SelfDependent, SelfDependent>();
            var container = new DependencyProvider(configuration);

            Assert.ThrowsException<DependencyException>(() => container.Resolve<SelfDependent>());
        }

        [TestMethod]
        public void CyclicDependencyTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<Class1, Class1>();
            configuration.Register<Class2, Class2>();
            var container = new DependencyProvider(configuration);

            Assert.ThrowsException<DependencyException>(() => container.Resolve<Class1>());
            Assert.ThrowsException<DependencyException>(() => container.Resolve<Class2>());
        }

        [TestMethod]
        public void WrongIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("kek");
            var container = new DependencyProvider(configuration);

            Assert.ThrowsException<DependencyException>(() => container.Resolve<IRepository>("wrong"));
        }

        [TestMethod]
        public void ProvideByIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("my");
            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>("my");

            Assert.IsInstanceOfType(repository, typeof(MyRepository));
        }

        [TestMethod]
        public void ProvidedAttributeTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, MyAnotherRepository>("yes");
            configuration.Register<IService<IRepository>, ServiceImpl3<IRepository>>();
            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IService<IRepository>>();

            Assert.IsInstanceOfType(service, typeof(ServiceImpl3<IRepository>));
            Assert.IsInstanceOfType(((ServiceImpl3<IRepository>)service).Repository, typeof(MyAnotherRepository));
        }
    }
}
