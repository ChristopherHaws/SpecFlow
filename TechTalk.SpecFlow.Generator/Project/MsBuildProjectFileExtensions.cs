using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace TechTalk.SpecFlow.Generator.Project
{
    static class MsBuildProjectFileExtensions
    {
        public static IEnumerable<ProjectItem> FeatureFiles(this Microsoft.Build.Evaluation.Project project)
        {
            return project.AllEvaluatedItems.Where(x => x.IsNonCompilingItem())
                                            .Where(x => x.IsFeatureFile() || x.IsExcelFeatureFile());
        }

        private static bool IsNonCompilingItem(this ProjectItem x)
        {
            return (x.ItemType == "Content" || x.ItemType == "None");
        }

        private static bool IsExcelFeatureFile(this ProjectItem x)
        {
            return x.EvaluatedInclude.EndsWith(".feature.xlsx", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsFeatureFile(this ProjectItem x)
        {
            return x.EvaluatedInclude.EndsWith(".feature", StringComparison.InvariantCultureIgnoreCase);
        }

        public static ProjectItem ApplicationConfigurationFile(this Microsoft.Build.Evaluation.Project project)
        {
            return project.AllEvaluatedItems.FirstOrDefault(x => IsNonCompilingItem(x) &&
                                                                 Path.GetFileName(x.EvaluatedInclude).Equals("app.config", StringComparison.InvariantCultureIgnoreCase));

        }
        
        public static ProjectNuGetInfo GetNuGetInfo(this Microsoft.Build.Evaluation.Project project)
        {
            var info = new ProjectNuGetInfo
            {
                Style = project.GetNuGetProjectStyle()
            };

            info.NuGetPath =
                project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "NuGetPackageRoot")?.EvaluatedValue ??
                project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "NuGetPackageFolders")?.EvaluatedValue;

            if (info.NuGetPath != null)
            {
                return info;
            }

            // If we couldn't get the package root from the project properties, fallback on getting the hint path
            // for the TechTalk.SpecFlow reference and traversing the path up to the packages directory.
            var projectName = project.AllEvaluatedProperties.First(x => x.Name == "ProjectName").EvaluatedValue;
            var specFlowReferenceHintPath = project
                .AllEvaluatedItems
                .Where(x => x.ItemType == "Reference")
                .Where(x => x.EvaluatedInclude.StartsWith("TechTalk.SpecFlow,", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()
                ?.DirectMetadata
                .FirstOrDefault(x => x.Name.Equals("HintPath", StringComparison.InvariantCultureIgnoreCase))
                ?.EvaluatedValue;

            if (specFlowReferenceHintPath != null)
            {
                var projectDirectory = project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "ProjectDir").EvaluatedValue;

                info.NuGetPath = GetNuGetPackagesFolderFromPath(Path.Combine(projectDirectory, specFlowReferenceHintPath));
            }

            return info;
        }

        private static ProjectNuGetStyle GetNuGetProjectStyle(this Microsoft.Build.Evaluation.Project project)
        {
            // If the NuGetProjectStyle property exists, use it. This will exist for PackageReference and project.json projects in VS2015 and VS2017
            var nuGetProjectStyle = project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "NuGetProjectStyle")?.EvaluatedValue;
            if (nuGetProjectStyle != null)
            {
                ProjectNuGetStyle style;
                if (Enum.TryParse<ProjectNuGetStyle>(nuGetProjectStyle, true, out style))
                {
                    return style;
                }
            }

            var projectName = project.AllEvaluatedProperties.First(x => x.Name == "ProjectName").EvaluatedValue;

            // Next, check for the existance of a project.json or projectName.project.json file
            var hasProjectJsonFile = project.AllEvaluatedItems.Any(x =>
                Path.GetFileName(x.EvaluatedInclude).Equals("project.json", StringComparison.InvariantCultureIgnoreCase) ||
                Path.GetFileName(x.EvaluatedInclude).Equals(projectName + ".project.json", StringComparison.InvariantCultureIgnoreCase));

            if (hasProjectJsonFile)
            {
                return ProjectNuGetStyle.ProjectJson;
            }

            // Otherwise assume the project is using a packages.config file.
            return ProjectNuGetStyle.PackagesConfig;
        }

        private static string GetNuGetPackagesFolderFromPath(string path)
        {
            var directory = new DirectoryInfo(path);

            while (!directory.Name.Equals("packages", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Path.GetPathRoot(path) == directory.FullName)
                {
                    return null;
                }

                directory = directory.Parent;
            }

            return directory.FullName;
        }
    }
}