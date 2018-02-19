using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Project;
using TechTalk.SpecFlow.Tracing;
using TechTalk.SpecFlow.Utils;

namespace SpecFlow.Build.Tasks
{
    /// <summary>
    /// TODO: Make this a ToolTask that executes specflow.exe so that this project has no dependencies on the other projects
    /// </summary>
    public class GenerateFeatureFile : GeneratorTaskBase
    {
        private readonly List<ITaskItem> generatedFiles = new List<ITaskItem>();

        public bool VerboseOutput { get; set; }
        public bool ForceGeneration { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the directory that the generated feature files should be output.
        /// </summary>
        //[Required]
        //public String OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the collection of feature files being transpiled.
        /// </summary>
        [Required]
        public ITaskItem[] FeatureFiles { get; set; }

        /// <summary>
        /// Gets the generated feature files that have had been transpiled.
        /// </summary>
        [Output]
        public ITaskItem[] GeneratedFiles => this.generatedFiles.ToArray();

        protected override void DoExecute()
        {
            var traceListener = VerboseOutput ? (ITraceListener)new TextWriterTraceListener(GetMessageWriter(MessageImportance.High), "SpecFlow: ") : new NullListener();

            var specFlowProject = MsBuildProjectReader.LoadSpecFlowProjectFromMsBuild(this.ProjectPath);

            traceListener.WriteToolOutput("Processing project: " + specFlowProject.ProjectSettings.ProjectName);
            var generationSettings = new GenerationSettings()
            {
                CheckUpToDate = !this.ForceGeneration,
                WriteResultToFile = true
            };

            var testGeneratorFactory = new TestGeneratorFactory();

            using (var generator = testGeneratorFactory.CreateGenerator(specFlowProject.ProjectSettings))
            {
                traceListener.WriteToolOutput("Using Generator: {0}", generator.GetType().FullName);

                var featureFiles = this.FeatureFiles
                    .Select(x => new FeatureFileInput(FileSystemHelper.GetRelativePath(x.ItemSpec, specFlowProject.ProjectSettings.ProjectFolder)))
                    .ToList();

                //foreach (var featureFile in specFlowProject.FeatureFiles)
                foreach (var featureFile in featureFiles)
                {
                    var outputFilePath = generator.GetTestFullPath(featureFile);
                    var outputFile = this.PrepareOutputFile(outputFilePath);
                    featureFile.GeneratedTestProjectRelativePath = FileSystemHelper.GetRelativePath(outputFile.FilePathForWriting, specFlowProject.ProjectSettings.ProjectFolder);

                    var generationResult = generator.GenerateTestFile(featureFile, generationSettings);
                    if (!generationResult.Success)
                    {
                        traceListener.WriteToolOutput("{0} -> test generation failed", featureFile.ProjectRelativePath);

                        foreach (var testGenerationError in generationResult.Errors)
                        {
                            RecordError(testGenerationError.Message, featureFile.GetFullPath(specFlowProject.ProjectSettings), testGenerationError.Line, testGenerationError.LinePosition);
                        }
                    }
                    else if (generationResult.IsUpToDate)
                    {
                        traceListener.WriteToolOutput("{0} -> test up-to-date", featureFile.ProjectRelativePath);
                    }
                    else
                    {
                        traceListener.WriteToolOutput("{0} -> test updated", featureFile.ProjectRelativePath);
                    }

                    if (generationResult.Success)
                    {
                        this.generatedFiles.Add(new TaskItem(featureFile.GetGeneratedTestFullPath(specFlowProject.ProjectSettings)));
                    }
                }
            }
        }
    }
}
