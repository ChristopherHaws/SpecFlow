﻿using System;
using BoDi;
using FluentAssertions;
using Moq;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Plugins;
using TechTalk.SpecFlow.Plugins;
using Xunit;
using DefaultDependencyProvider = TechTalk.SpecFlow.Generator.DefaultDependencyProvider;

namespace TechTalk.SpecFlow.GeneratorTests
{
    public class GeneratorPluginTests
    {
        [Fact]
        public void Should_be_able_to_specify_a_plugin()
        {
            var configurationHolder = GetConfigWithPlugin();
            GeneratorContainerBuilder.DefaultDependencyProvider = new TestDefaultDependencyProvider(new Mock<IGeneratorPlugin>().Object);
            CreateDefaultContainer(configurationHolder);
        }

        [Fact]
        public void Should_be_able_to_register_dependencies_from_a_plugin()
        {
            var configurationHolder = GetConfigWithPlugin();

            GeneratorContainerBuilder.DefaultDependencyProvider = new TestDefaultDependencyProvider(new PluginWithCustomDependency());
            var container = CreateDefaultContainer(configurationHolder);
            var customDependency = container.Resolve<ICustomDependency>();
            customDependency.Should().BeOfType(typeof(CustomDependency));
        }

        [Fact]
        public void Should_be_able_to_change_default_configuration_from_a_plugin()
        {
            var configurationHolder = GetConfigWithPlugin();

            GeneratorContainerBuilder.DefaultDependencyProvider = new TestDefaultDependencyProvider(new PluginWithCustomConfiguration(conf => conf.SpecFlowConfiguration.StopAtFirstError = true));
            var container = CreateDefaultContainer(configurationHolder);
            var runtimeConfiguration = container.Resolve<SpecFlowProjectConfiguration>();
            runtimeConfiguration.SpecFlowConfiguration.StopAtFirstError.Should().BeTrue();
        }

        [Fact]
        public void Should_be_able_to_register_further_dependencies_based_on_the_configuration()
        {
            var configurationHolder = GetConfigWithPlugin();

            GeneratorContainerBuilder.DefaultDependencyProvider = new TestDefaultDependencyProvider(new PluginWithCustomization());

            // with default settings, the plugin should not change the header writer
            var container = CreateDefaultContainer(configurationHolder);
            var testHeaderWriter = container.Resolve<ITestHeaderWriter>();
            testHeaderWriter.Should().BeOfType<TestHeaderWriter>();

            // with StopAtFirstError == true, we should get a custom factory
            var specialConfiguratuion = new SpecFlowConfigurationHolder(ConfigSource.AppConfig, string.Format(@"<specFlow>
                  <plugins>
                    <add name=""MyCompany.MyPlugin"" />
                  </plugins>
                  <runtime stopAtFirstError=""true"" />
                </specFlow>"));
            container = CreateDefaultContainer(specialConfiguratuion);
            var customHeaderWriter = container.Resolve<ITestHeaderWriter>();
            customHeaderWriter.Should().BeOfType<CustomHeaderWriter>();
        }

        [Fact]
        public void Should_be_able_to_specify_a_plugin_with_parameters()
        {
            var configurationHolder = new SpecFlowConfigurationHolder(ConfigSource.AppConfig, string.Format(@"<specFlow>
                  <plugins>
                    <add name=""MyCompany.MyPlugin"" parameters=""foo, bar"" />
                  </plugins>
                </specFlow>"));
            var pluginMock = new Mock<IGeneratorPlugin>();
            GeneratorContainerBuilder.DefaultDependencyProvider = new TestDefaultDependencyProvider(pluginMock.Object);
            CreateDefaultContainer(configurationHolder);

            pluginMock.Verify(p => p.Initialize(It.IsAny<GeneratorPluginEvents>(), It.Is<GeneratorPluginParameters>(pp => pp.Parameters == "foo, bar")));
        }

        private SpecFlowConfigurationHolder GetConfigWithPlugin()
        {
            return new SpecFlowConfigurationHolder(ConfigSource.AppConfig, string.Format(@"<specFlow>
                  <plugins>
                    <add name=""MyCompany.MyPlugin"" />
                  </plugins>
                </specFlow>"));
        }

        private IObjectContainer CreateDefaultContainer(SpecFlowConfigurationHolder configurationHolder)
        {
            return GeneratorContainerBuilder.CreateContainer(configurationHolder, new ProjectSettings());
        }

        class TestDefaultDependencyProvider : DefaultDependencyProvider
        {
            private readonly IGeneratorPlugin pluginToReturn;

            public TestDefaultDependencyProvider(IGeneratorPlugin pluginToReturn)
            {
                this.pluginToReturn = pluginToReturn;
            }

            public override void RegisterDefaults(ObjectContainer container)
            {
                base.RegisterDefaults(container);

                var pluginLoaderStub = new Mock<IGeneratorPluginLoader>();
                pluginLoaderStub.Setup(pl => pl.LoadPlugin(It.IsAny<PluginDescriptor>())).Returns(pluginToReturn);
                container.RegisterInstanceAs<IGeneratorPluginLoader>(pluginLoaderStub.Object);
            }
        }

        public interface ICustomDependency
        {

        }

        public class CustomDependency : ICustomDependency
        {

        }

        public class CustomHeaderWriter : ITestHeaderWriter
        {
            public Version DetectGeneratedTestVersion(string generatedTestContent)
            {
                throw new NotImplementedException();
            }
        }

        public class PluginWithCustomDependency : IGeneratorPlugin
        {
            public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters)
            {
                generatorPluginEvents.RegisterDependencies += (sender, args) => args.ObjectContainer.RegisterTypeAs<CustomDependency, ICustomDependency>();
            }
        }

        public class PluginWithCustomization : IGeneratorPlugin
        {
            public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters)
            {
                generatorPluginEvents.CustomizeDependencies += (sender, args) =>
                {
                    if (args.SpecFlowProjectConfiguration.SpecFlowConfiguration.StopAtFirstError)
                        args.ObjectContainer.RegisterTypeAs<CustomHeaderWriter, ITestHeaderWriter>();
                };
            }
        }

        public class PluginWithCustomConfiguration : IGeneratorPlugin
        {
            private readonly Action<SpecFlowProjectConfiguration> specifyDefaults;

            public PluginWithCustomConfiguration(Action<SpecFlowProjectConfiguration> specifyDefaults)
            {
                this.specifyDefaults = specifyDefaults;
            }

            public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters)
            {
                generatorPluginEvents.ConfigurationDefaults += (sender, args) => { specifyDefaults(args.SpecFlowProjectConfiguration); };
            }
        }
    }
}
