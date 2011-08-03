//-------------------------------------------------------------------------------
// <copyright file="ExtensionConfigurationSectionBehaviorTest.cs" company="bbv Software Services AG">
//   Copyright (c) 2008-2011 bbv Software Services AG
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace bbv.Common.Bootstrapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class ExtensionConfigurationSectionBehaviorTest
    {
        private const string SomeExtensionPropertyName = "SomeProperty";

        private const string SomeExtensionPropertyValue = "AnyValue";

        private readonly Mock<IHaveConversionCallbacks> conversionCallbacks;

        private readonly Mock<ILoadConfigurationSection> sectionProvider;

        private readonly Mock<IConsumeConfiguration> consumer;

        private readonly Mock<IHaveConfigurationSectionName> sectionNameProvider;

        private readonly List<IExtension> extensions;

        private readonly Mock<IExtensionPropertyReflector> extensionPropertyReflector;

        private readonly ExtensionConfigurationSectionBehavior testee;

        public ExtensionConfigurationSectionBehaviorTest()
        {
            this.extensionPropertyReflector = new Mock<IExtensionPropertyReflector>();
            this.consumer = new Mock<IConsumeConfiguration>();
            this.sectionNameProvider = new Mock<IHaveConfigurationSectionName>();
            this.sectionProvider = new Mock<ILoadConfigurationSection>();
            this.conversionCallbacks = new Mock<IHaveConversionCallbacks>();

            this.extensions = new List<IExtension> { Mock.Of<IExtension>(), Mock.Of<IExtension>(), };

            this.testee = new TestableExtensionConfigurationSectionBehavior(this.extensionPropertyReflector.Object, this.consumer.Object, this.sectionNameProvider.Object, this.sectionProvider.Object, this.conversionCallbacks.Object);
        }

        [Fact]
        public void Behave_ShouldConsumeSectionFromProvider()
        {
            var expectedConfiguration = new Dictionary<string, string> { { "AnyKey", "AnyValue" } };
            var configuration = new Dictionary<string, string>();

            var configurationSection = ExtensionConfigurationSectionHelper.CreateSection(expectedConfiguration);

            this.sectionProvider.Setup(p => p.GetSection(It.IsAny<string>())).Returns(configurationSection);
            this.consumer.Setup(c => c.Configuration).Returns(configuration);

            this.testee.Behave(new List<IExtension> { this.extensions.First() });

            configuration.Should().BeEquivalentTo(expectedConfiguration);
        }

        [Fact]
        public void Behave_ShouldAcquireSectionName()
        {
            this.SetupEmptyConsumerConfiguration();

            this.testee.Behave(this.extensions);

            this.sectionNameProvider.Verify(p => p.SectionName);
        }

        [Fact]
        public void Behave_ShouldAcquireSectionByName()
        {
            this.SetupEmptyConsumerConfiguration();

            const string AnySectionName = "SectionName";

            this.sectionNameProvider.Setup(p => p.SectionName).Returns(AnySectionName);

            this.testee.Behave(this.extensions);

            this.sectionProvider.Verify(p => p.GetSection(AnySectionName));
        }

        [Fact]
        public void Behave_ShouldReflectPropertiesOfExtensions()
        {
            this.SetupEmptyConsumerConfiguration();

            this.testee.Behave(this.extensions);

            this.extensionPropertyReflector.Verify(r => r.Reflect(It.IsAny<IExtension>()), Times.Exactly(2));
        }

        [Fact]
        public void Behave_ShouldAcquireConversionCallbacks()
        {
            this.SetupEmptyConsumerConfiguration();

            this.testee.Behave(this.extensions);

            this.conversionCallbacks.VerifyGet(x => x.ConversionCallbacks);
            this.conversionCallbacks.VerifyGet(x => x.DefaultConversionCallback);
        }

        [Fact]
        public void Behave_WhenReflectedPropertyMatchesConfiguration_ShouldAcquireCallback()
        {
            bool wasCalled = false;

            var dictionary = new Dictionary<string, Func<string, PropertyInfo, object>>
                {
                    {
                        SomeExtensionPropertyName, (value, info) =>
                            {
                                wasCalled = true;
                                return value;
                            }
                        }
                };

            this.conversionCallbacks.Setup(x => x.ConversionCallbacks).Returns(dictionary);

            this.extensionPropertyReflector.Setup(x => x.Reflect(It.IsAny<IExtension>())).Returns(new List<PropertyInfo> { GetSomePropertyPropertyInfo() });
            this.consumer.Setup(x => x.Configuration).Returns(new Dictionary<string, string> { { SomeExtensionPropertyName, SomeExtensionPropertyValue } });

            var someExtension = new SomeExtension();
            this.testee.Behave(new List<IExtension> { someExtension });

            wasCalled.Should().BeTrue();
            someExtension.SomeProperty.Should().Be(SomeExtensionPropertyValue);
        }

        [Fact]
        public void Behave_WhenNoConversionCallbackFound_ShouldUseDefaultCallback()
        {
            this.conversionCallbacks.Setup(x => x.ConversionCallbacks).Returns(
                new Dictionary<string, Func<string, PropertyInfo, object>>());

            bool wasCalled = false;
            Func<string, PropertyInfo, object> defaultCallback = (value, info) =>
                {
                    wasCalled = true;
                    return value;
                };
            this.conversionCallbacks.Setup(x => x.DefaultConversionCallback).Returns(defaultCallback);

            this.extensionPropertyReflector.Setup(x => x.Reflect(It.IsAny<IExtension>())).Returns(new List<PropertyInfo> { GetSomePropertyPropertyInfo() });
            this.consumer.Setup(x => x.Configuration).Returns(new Dictionary<string, string> { { SomeExtensionPropertyName, SomeExtensionPropertyValue } });

            var someExtension = new SomeExtension();
            this.testee.Behave(new List<IExtension> { someExtension });

            wasCalled.Should().BeTrue();
            someExtension.SomeProperty.Should().Be(SomeExtensionPropertyValue);
        }

        private static PropertyInfo GetSomePropertyPropertyInfo()
        {
            return typeof(SomeExtension).GetProperty(SomeExtensionPropertyName);
        }

        private void SetupEmptyConsumerConfiguration()
        {
            this.consumer.Setup(x => x.Configuration).Returns(new Dictionary<string, string>());
        }

        private class SomeExtension : IExtension
        {
            public string SomeProperty { get; private set; }
        }

        private class TestableExtensionConfigurationSectionBehavior : ExtensionConfigurationSectionBehavior
        {
            private readonly IConsumeConfiguration consumer;

            private readonly IHaveConfigurationSectionName sectionNameProvider;

            private readonly ILoadConfigurationSection sectionProvider;

            private readonly IHaveConversionCallbacks conversionCallbacks;

            public TestableExtensionConfigurationSectionBehavior(
                IExtensionPropertyReflector extensionPropertyReflector,
                IConsumeConfiguration consumer,
                IHaveConfigurationSectionName sectionNameProvider,
                ILoadConfigurationSection sectionProvider,
                IHaveConversionCallbacks conversionCallbacks)
                : base(extensionPropertyReflector)
            {
                this.conversionCallbacks = conversionCallbacks;
                this.sectionProvider = sectionProvider;
                this.sectionNameProvider = sectionNameProvider;
                this.consumer = consumer;
            }

            protected override IHaveConfigurationSectionName CreateHaveConfigurationSectionName(IExtension extension)
            {
                return this.sectionNameProvider;
            }

            protected override ILoadConfigurationSection CreateLoadConfigurationSection(IExtension extension)
            {
                return this.sectionProvider;
            }

            protected override IConsumeConfiguration CreateConsumeConfiguration(IExtension extension)
            {
                return this.consumer;
            }

            protected override IHaveConversionCallbacks CreateHaveConversionCallbacks(IExtension extension)
            {
                return this.conversionCallbacks;
            }
        }
    }
}