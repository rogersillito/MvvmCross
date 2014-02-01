﻿// MvxIoCTest.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System.Collections.Generic;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;
using NUnit.Framework;

namespace Cirrious.CrossCore.Test
{
    [TestFixture]
    public class MvxIoCTest
    {
        public interface IA { IB B { get; } }
        public interface IB { }
        public interface IC { }
        public class A : IA
        {
            public A(IB b) { B = b; } 
            public IB B { get; set; }
        }
        public class B : IB
        { public B(IC c) { } }
        public class C : IC
        { public C(IA a) { } }
        public class C2 : IC
        { public C2() { } }

        [Test]
        public void TryResolve_CircularLazyRegistration_ReturnsFalse()
        {
            MvxSingleton.ClearAllSingletons();
            var instance = MvxSimpleIoCContainer.Initialize();

            Mvx.LazyConstructAndRegisterSingleton<IA, A>();
            Mvx.LazyConstructAndRegisterSingleton<IB, B>();
            Mvx.LazyConstructAndRegisterSingleton<IC, C>();

            IA a;
            var result = Mvx.TryResolve(out a);
            Assert.IsFalse(result);
            Assert.IsNull(a);
        }

        [Test]
        public void TryResolve_NonCircularRegistration_ReturnsTrue()
        {
            MvxSingleton.ClearAllSingletons();
            var instance = MvxSimpleIoCContainer.Initialize();

            Mvx.LazyConstructAndRegisterSingleton<IA, A>();
            Mvx.LazyConstructAndRegisterSingleton<IB, B>();
            Mvx.LazyConstructAndRegisterSingleton<IC, C2>();

            IA a;
            var result = Mvx.TryResolve(out a);
            Assert.IsTrue(result);
            Assert.IsNotNull(a);
        }

        [Test]
        public void TryResolve_LazySingleton_ReturnsSameSingletonEachTime()
        {
            MvxSingleton.ClearAllSingletons();
            var instance = MvxSimpleIoCContainer.Initialize();

            Mvx.LazyConstructAndRegisterSingleton<IA, A>();
            Mvx.LazyConstructAndRegisterSingleton<IB, B>();
            Mvx.LazyConstructAndRegisterSingleton<IC, C2>();

            IA a0;
            var result = Mvx.TryResolve(out a0);
            Assert.IsTrue(result);
            Assert.IsNotNull(a0);

            for (int i = 0; i < 100; i++)
            {
                IA a1;
                result = Mvx.TryResolve(out a1);
                Assert.IsTrue(result);
                Assert.AreSame(a0, a1);
            }
        }

        [Test]
        public void TryResolve_NonLazySingleton_ReturnsSameSingletonEachTime()
        {
            MvxSingleton.ClearAllSingletons();
            var instance = MvxSimpleIoCContainer.Initialize();

            Mvx.LazyConstructAndRegisterSingleton<IB, B>();
            Mvx.LazyConstructAndRegisterSingleton<IC, C2>();
            Mvx.ConstructAndRegisterSingleton<IA, A>();

            IA a0;
            var result = Mvx.TryResolve(out a0);
            Assert.IsTrue(result);
            Assert.IsNotNull(a0);

            for (int i = 0; i < 100; i++)
            {
                IA a1;
                result = Mvx.TryResolve(out a1);
                Assert.IsTrue(result);
                Assert.AreSame(a0, a1);
            }
        }

        [Test]
        public void TryResolve_Dynamic_ReturnsDifferentInstanceEachTime()
        {
            MvxSingleton.ClearAllSingletons();
            var instance = MvxSimpleIoCContainer.Initialize();

            Mvx.LazyConstructAndRegisterSingleton<IB, B>();
            Mvx.LazyConstructAndRegisterSingleton<IC, C2>();
            Mvx.RegisterType<IA, A>();

            var previous = new Dictionary<IA,bool>();

            for (int i = 0; i < 100; i++)
            {
                IA a1;
                var result = Mvx.TryResolve(out a1);
                Assert.IsTrue(result);
                Assert.IsFalse(previous.ContainsKey(a1));
                Assert.AreEqual(i, previous.Count);
                previous.Add(a1, true);
            }
        }

        [Test]
        public void TryResolve_ParameterConstructors_CreatesParametersUsingIocResolution()
        {
            MvxSingleton.ClearAllSingletons();
            var instance = MvxSimpleIoCContainer.Initialize();

            Mvx.RegisterType<IB, B>();
            Mvx.LazyConstructAndRegisterSingleton<IC, C2>();
            Mvx.RegisterType<IA, A>();

            IA a1;
            var result = Mvx.TryResolve(out a1);
            Assert.IsTrue(result);
            Assert.IsNotNull(a1);
            Assert.IsNotNull(a1.B);
            Assert.IsInstanceOf<B>(a1.B);
        }

        // TODO - there are so many tests we could and should do here!
    }
}