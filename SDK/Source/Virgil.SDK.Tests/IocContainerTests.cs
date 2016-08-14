﻿namespace Virgil.SDK.Tests
{
    using FluentAssertions;
    using NUnit.Framework;

    public class IocContainerTests
    {
        [Test]
        public void RegisterSingleton_ResolvingTypeAndConcreteType_ShouldResolveSingleInstance()
        {
            var ioc = new IocContainer();

            ioc.RegisterSingleton<IIocTest1, IocTest1>();
            var resolvedType = ioc.Resolve<IIocTest1>();

            resolvedType.Should().NotBeNull();
            resolvedType.Should().BeOfType<IocTest1>();

            resolvedType.Should().Be(ioc.Resolve<IIocTest1>());
        }

        [Test]
        public void RegisterTransient_ResolvingTypeAndConcreteType_ShouldResolveTransientInstance()
        {
            var ioc = new IocContainer();

            ioc.RegisterTransient<IIocTest1, IocTest1>();
            var resolvedType = ioc.Resolve<IIocTest1>();

            resolvedType.Should().NotBeNull();
            resolvedType.Should().BeOfType<IocTest1>();

            resolvedType.Should().NotBe(ioc.Resolve<IIocTest1>());
        }

        [Test]
        public void RegisterInstance_ResolvingTypeAndInstanceAsParameter_ShouldResolvePassedInstance()
        {
            var ioc = new IocContainer();

            var instance = new IocTest1();

            ioc.RegisterInstance<IIocTest1, IocTest1>(instance);
            var resolvedType = ioc.Resolve<IIocTest1>();

            resolvedType.Should().NotBeNull();
            resolvedType.Should().BeOfType<IocTest1>();

            resolvedType.Should().Be(instance);
        }

        [Test]
        public void Resolve_ResolvingType_ShouldResolveTypeWithMultipleDependencies()
        {
            var ioc = new IocContainer();

            var instance = new IocTest1();

            ioc.RegisterInstance<IIocTest1, IocTest1>(instance);
            ioc.RegisterSingleton<IIocTest3, IocTest3>();
            ioc.RegisterTransient<IIocTest2, IocTest2>();

            var resolvedType = ioc.Resolve<IIocTest2>();

            resolvedType.Should().NotBeNull();
            resolvedType.Should().BeOfType<IocTest2>();

            resolvedType.Test1.Should().Be(ioc.Resolve<IIocTest1>());
            resolvedType.Test3.Should().Be(ioc.Resolve<IIocTest3>());
        }
    }

    public interface IIocTest1
    {
    }

    public interface IIocTest2
    {
        IIocTest1 Test1 { get; }
        IIocTest3 Test3 { get; }
    }

    public interface IIocTest3
    {
    }

    public class IocTest1 : IIocTest1
    {
    }

    public class IocTest2 : IIocTest2
    {
        public IIocTest1 Test1 { get; set; }
        public IIocTest3 Test3 { get; set; }

        public IocTest2(IIocTest1 test1, IIocTest3 test3)
        {
            this.Test1 = test1;
            this.Test3 = test3;
        }
    }

    public class IocTest3 : IIocTest3
    {
    }
}