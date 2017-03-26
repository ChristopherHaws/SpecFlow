﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Plugins;
using TechTalk.SpecFlow.Plugins;

namespace TechTalk.SpecFlow.GeneratorTests
{
    [TestFixture]
    public class GeneratorPluginLocatorTests
    {
        [TestCase(1, 9, 0, "SpecRun", 1, 5, 2, "net35")]
        [TestCase(2, 1, 0, "SpecRun", 1, 6, 0, "net45")]
        public void Should_locate_generator_when_using_local_nuget(int specFlowMajor, int specFlowMinor, int specFlowRevision, string pluginName, int pluginMajor, int pluginMinor, int pluginRevision, string frameworkMoniker)
        {
            // Arrange
            var projectSettings = new ProjectSettings()
            {
                ProjectFolder = @"C:\SolutionFolder\ProjectFolder\"
            };

            var fileSystem = new MockFileSystem(
                @"C:\SolutionFolder\packages\specrun.specflow.1.5.2\lib\net45\SpecRun.SpecFlowPlugin.dll",
                @"C:\SolutionFolder\packages\specrun.specflow.1-9-0.1.5.2\lib\net35\SpecRun.SpecFlowPlugin.dll",
                @"C:\SolutionFolder\packages\specrun.specflow.2-1-0.1.6.0\lib\net45\SpecRun.SpecFlowPlugin.dll");

            var executingAssemblyInfoMock = new Mock<IExecutingAssemblyInfo>();

            executingAssemblyInfoMock
                .Setup(x => x.GetCodeBase())
                .Returns(String.Format(@"file:///C:/SolutionFolder/packages/SpecFlow.{0}.{1}.{2}/tools/TechTalk.SpecFlow.Generator.dll", specFlowMajor, specFlowMinor, specFlowRevision));

            executingAssemblyInfoMock
                .Setup(x => x.GetVersion())
                .Returns(new Version(specFlowMajor, specFlowMinor, 0, specFlowRevision));

            var loader = new GeneratorPluginLocator(projectSettings, fileSystem, executingAssemblyInfoMock.Object);

            // Act
            var pluginDescriptor = new PluginDescriptor(pluginName, null, PluginType.GeneratorAndRuntime, null);
            var actual = loader.GetGeneratorPluginAssemblies(pluginDescriptor).Distinct().ToList();

            // Assert
            var expected = new List<string>
            {
                String.Format(@"C:\SolutionFolder\packages\specrun.specflow.{0}-{1}-{2}.{3}.{4}.{5}\lib\{6}\{7}.SpecFlowPlugin.dll",
                    specFlowMajor, specFlowMinor, specFlowRevision,
                    pluginMajor, pluginMinor, pluginRevision,
                    frameworkMoniker, pluginName)
            };

            CollectionAssert.AreEquivalent(expected, actual);
        }
        
        [TestCase(1, 9, 0, "SpecRun", 1, 5, 2, "net35")]
        [TestCase(2, 1, 0, "SpecRun", 1, 6, 0, "net45")]
        public void Should_locate_generator_when_using_global_nuget(int specFlowMajor, int specFlowMinor, int specFlowRevision, string pluginName, int pluginMajor, int pluginMinor, int pluginRevision, string frameworkMoniker)
        {
            // Arrange
            var projectSettings = new ProjectSettings()
            {
                ProjectFolder = @"C:\SolutionFolder\ProjectFolder\"
            };

            var fileSystem = new MockFileSystem(
                @"C:\Users\JDoe\.nuget\packages\specrun.specflow\1.5.2\lib\net45\SpecRun.SpecFlowPlugin.dll",
                @"C:\Users\JDoe\.nuget\packages\specrun.specflow.1-9-0\1.5.2\lib\net35\SpecRun.SpecFlowPlugin.dll",
                @"C:\Users\JDoe\.nuget\packages\specrun.specflow.2-1-0\1.6.0\lib\net45\SpecRun.SpecFlowPlugin.dll",
                @"C:\Users\JDoe\.nuget\packages\specrun.specflow.2-2-0\1.6.0\lib\net45\SpecRun.SpecFlowPlugin.dll");

            var executingAssemblyInfoMock = new Mock<IExecutingAssemblyInfo>();

            executingAssemblyInfoMock
                .Setup(x => x.GetCodeBase())
                .Returns(String.Format(@"file:///C:/Users/JDoe/.nuget/packages/SpecFlow/{0}.{1}.{2}/tools/TechTalk.SpecFlow.Generator.dll", specFlowMajor, specFlowMinor, specFlowRevision));

            executingAssemblyInfoMock
                .Setup(x => x.GetVersion())
                .Returns(new Version(specFlowMajor, specFlowMinor, 0, specFlowRevision));

            var loader = new GeneratorPluginLocator(projectSettings, fileSystem, executingAssemblyInfoMock.Object);

            // Act
            var pluginDescriptor = new PluginDescriptor(pluginName, null, PluginType.GeneratorAndRuntime, null);
            var actual = loader.GetGeneratorPluginAssemblies(pluginDescriptor).Distinct().ToList();

            // Assert
            var expected = new List<string>
            {
                String.Format(@"C:\Users\JDoe\.nuget\packages\specrun.specflow.{0}-{1}-{2}\{3}.{4}.{5}\lib\{6}\SpecRun.SpecFlowPlugin.dll",
                    specFlowMajor, specFlowMinor, specFlowRevision,
                    pluginMajor, pluginMinor, pluginRevision,
                    frameworkMoniker)
            };

            CollectionAssert.AreEquivalent(expected, actual);
        }
        
        [TestCase(1, 9, 0, "SpecRun", @".\bin\Debug\")]
        [TestCase(2, 1, 0, "SpecRun", @".\bin\Debug\")]
        public void Should_locate_generator_when_using_path(int specFlowMajor, int specFlowMinor, int specFlowRevision, string pluginName, string path)
        {
            // Arrange
            var projectSettings = new ProjectSettings()
            {
                ProjectFolder = @"C:\SolutionFolder\ProjectFolder\"
            };

            var fileSystem = new MockFileSystem(
                @"C:\SolutionFolder\packages\specrun.specflow.1.5.2\lib\net45\SpecRun.SpecFlowPlugin.dll",
                @"C:\SolutionFolder\packages\specrun.specflow.1-9-0.1.5.2\lib\net35\SpecRun.SpecFlowPlugin.dll",
                @"C:\SolutionFolder\packages\specrun.specflow.2-1-0.1.6.0\lib\net45\SpecRun.SpecFlowPlugin.dll",
                @"C:\SolutionFolder\ProjectFolder\bin\Debug\SpecRun.SpecFlowPlugin.dll");

            var executingAssemblyInfoMock = new Mock<IExecutingAssemblyInfo>();

            executingAssemblyInfoMock
                .Setup(x => x.GetCodeBase())
                .Returns(String.Format(@"file:///C:/SolutionFolder/packages/SpecFlow.{0}.{1}.{2}/tools/TechTalk.SpecFlow.Generator.dll", specFlowMajor, specFlowMinor, specFlowRevision));

            executingAssemblyInfoMock
                .Setup(x => x.GetVersion())
                .Returns(new Version(specFlowMajor, specFlowMinor, 0, specFlowRevision));

            var loader = new GeneratorPluginLocator(projectSettings, fileSystem, executingAssemblyInfoMock.Object);

            // Act
            var pluginDescriptor = new PluginDescriptor(pluginName, path, PluginType.GeneratorAndRuntime, null);
            var actual = loader.GetGeneratorPluginAssemblies(pluginDescriptor).Distinct().ToList();

            // Assert
            var expected = new List<string>
            {
                Path.GetFullPath(Path.Combine(@"C:\SolutionFolder\ProjectFolder\", path, pluginName + ".SpecFlowPlugin.dll"))
            };

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }

    public class MockFileSystem : IFileSystem
    {
        private readonly String[] filesAndDirectories;

        /// <summary>
        /// Directories should end in / or \ and files should not.
        /// </summary>
        public MockFileSystem(params string[] filesAndDirectories)
        {
            this.filesAndDirectories = ExplodePaths(filesAndDirectories).ToArray();
        }

        public bool FileExists(String path)
        {
            return this.filesAndDirectories.Contains(path, StringComparer.OrdinalIgnoreCase);
        }

        public string[] GetDirectories(string path, string searchPattern)
        {
            return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public string[] GetDirectories(string path, string searchPattern, SearchOption option)
        {
            path = Path.GetFullPath(path);

            var directories = this.filesAndDirectories
                .Select(Path.GetFullPath)
                .Select(Path.GetDirectoryName)
                .Where(x => x != null)
                .Distinct()
                .ToList();

            return directories.Where(directory =>
                directory.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                directory.StartsWith(path + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                directory.StartsWith(path + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .Select(directory => new
                {
                    FullPath = directory,
                    PathParts = GetPathParts(directory
                        .Substring(path.Length, directory.Length - path.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    )
                })
                .Where(x =>
                {
                    if (option == SearchOption.AllDirectories)
                    {
                        return x.PathParts.Any(part => MatchesWildcardPattern(part, searchPattern));
                    }
                    else if (x.PathParts.Length == 1)
                    {
                        return MatchesWildcardPattern(x.PathParts[0], searchPattern);
                    }

                    return false;
                })
                .Select(x => x.FullPath)
                .ToArray();
        }

        private static string[] GetPathParts(string path)
        {
            return path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///   Checks if name matches pattern with '?' and '*' wildcards.
        /// </summary>
        private static bool MatchesWildcardPattern(string input, string pattern)
        {
            pattern = pattern.Replace("?*", "*");
            pattern = pattern.Replace("*?", "*");
            pattern = pattern.Replace("**", "*");

            if (pattern.Equals("*"))
            {
                pattern = ".*";
            }
            else
            {
                pattern = pattern.Replace(".", @"\.");
                pattern = pattern.Replace("*", @".*");
                pattern = pattern.Replace("?", @".");

                pattern = String.Concat(@"\b", pattern, @"\b");
            }

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(input);
        }

        private static IEnumerable<string> ExplodePath(string path)
        {
            var parts = GetPathParts(path).ToList();
            parts.RemoveAt(parts.Count() - 1);

            var previous = String.Empty;

            foreach (var part in parts)
            {
                yield return previous = Path.Combine(previous, part) + Path.DirectorySeparatorChar;
            }

            yield return path;
        }

        private static IEnumerable<string> ExplodePaths(IEnumerable<string> paths)
        {
            return paths.SelectMany(x => ExplodePath(x)).Distinct();
        }
    }
}
