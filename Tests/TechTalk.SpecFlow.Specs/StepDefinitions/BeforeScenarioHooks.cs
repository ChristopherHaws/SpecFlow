using System;
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
            // Since this project runs tests in parallel, multiple threads end up executing this bit of code in parallel.
            // We should concider making the BeforeTestRun hook run once before splitting up the threads or make a new
            // hook for executing some bit of code before all of the tests.
            var timeout = TimeSpan.FromSeconds(10);

            using (var routine = new RunOnce("DeleteSpecFlowTempProjects"))
            {
                if (routine.AlreadyRunning)
                {
                    if (!routine.Wait(timeout))
                    {
                        throw new Exception("Could not delete the temp project directory.");
                    }

                    return;
                }

                // Wait for all of the test threads to be started.
                Thread.Sleep(2000);

                var directoryForTestProjects = InputProjectDriver.DetermineDirectoryForTestProjects();
                var stopwatch = Stopwatch.StartNew();

                foreach (var directory in Directory.EnumerateDirectories(directoryForTestProjects))
                {
                    while (Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.Delete(directory, true);
                            break;
                        }
                        catch (Exception ex)
                        {
                            // This throws because multiple threads are trying to do this at the same time
                            Thread.Sleep(10);

                            if (stopwatch.Elapsed < timeout)
                            {
                                throw new Exception($"Could not delete the temp project directory: {directory}", ex);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides a simple way to make sure that only one process is executing a block of code system wide.
    /// </summary>
    public class RunOnce : IDisposable
    {
        private readonly Mutex mutex;

        public RunOnce(string name)
        {
            this.mutex = new Mutex(true, name, out var created);
            this.AlreadyRunning = !created;
        }

        ~RunOnce()
        {
            this.Dispose(false);
        }

        public bool AlreadyRunning { get; private set; }

        public bool Wait(TimeSpan timeout)
        {
            return this.mutex.WaitOne(timeout);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            GC.SuppressFinalize(this);

            if (isDisposing)
            {
                if (!this.AlreadyRunning)
                {
                    this.mutex?.ReleaseMutex();
                }

                this.mutex?.Close();
            }
        }
    }
}
