using System.Diagnostics;
using System.IO;
using System.Threading;
using TechTalk.SpecFlow.Specs.Drivers;

namespace TechTalk.SpecFlow.Specs.StepDefinitions
{
    [Binding]
    public sealed class BeforeScenarioHooks
    {
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            var directoryForTestProjects = InputProjectDriver.DetermineDirectoryForTestProjects();

            var stopwatch = Stopwatch.StartNew();
            while (Directory.Exists(directoryForTestProjects))
            {
                try
                {
                    Directory.Delete(directoryForTestProjects, true);
                    break;
                }
                catch
                {
                    // This throws because multiple threads are trying to do this at the same time
                    Thread.Sleep(10);

                    if (stopwatch.Elapsed.Seconds < 5)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
