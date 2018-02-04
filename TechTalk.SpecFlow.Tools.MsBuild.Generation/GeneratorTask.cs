using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Project;
using TechTalk.SpecFlow.Tracing;

namespace TechTalk.SpecFlow.Tools.MsBuild.Generation
{
    public class GenerateAll : GeneratorTaskBase
    {
        public bool VerboseOutput { get; set; }
        public bool ForceGeneration { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        private readonly List<ITaskItem> generatedFiles = new List<ITaskItem>();

        [Output]
        public ITaskItem[] GeneratedFiles => generatedFiles.ToArray();

        protected override void DoExecute()
        {
            var traceListener = VerboseOutput ? (ITraceListener)new TextWriterTraceListener(GetMessageWriter(MessageImportance.High), "SpecFlow: ") : new NullListener();

            var specFlowProject = MsBuildProjectReader.LoadSpecFlowProjectFromMsBuild(ProjectPath);

            var batchGenerator = new MsBuildBatchGenerator(traceListener, new TestGeneratorFactory(), this);
            batchGenerator.OnError += (featureFileInput, result) =>
            {
                foreach (var testGenerationError in result.Errors)
                {
                    RecordError(testGenerationError.Message, featureFileInput.GetFullPath(specFlowProject.ProjectSettings), testGenerationError.Line, testGenerationError.LinePosition);
                }
            };

            batchGenerator.OnSuccess += (featureFileInput, result) =>
            {
                generatedFiles.Add(new TaskItem(featureFileInput.GetGeneratedTestFullPath(specFlowProject.ProjectSettings)));
            };

            batchGenerator.ProcessProject(specFlowProject, ForceGeneration);
        }
    }
}