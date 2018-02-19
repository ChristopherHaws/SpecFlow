using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace TechTalk.SpecFlow.Specs.Drivers
{
    public class ConsoleTestOutputHelper : ITestOutputHelper
    {
        public void WriteLine(String message)
        {
            Console.WriteLine(message);
        }

        public void WriteLine(String format, params Object[] args)
        {
            Console.WriteLine(format, args);
        }
    }

    public class ProcessHelper
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ProcessHelper()
            : this(new ConsoleTestOutputHelper())
        {
        }

        public ProcessHelper(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public string ConsoleOutput { get; private set; }

        public string ConsoleError { get; private set; }

        public int RunProcess(string executablePath, string argumentsFormat, params object[] arguments)
        {
            string commandArguments = string.Format(argumentsFormat, arguments);
            ProcessStartInfo psi = new ProcessStartInfo(executablePath, commandArguments);

            this.testOutputHelper.WriteLine($"starting process {executablePath} {commandArguments}");
            this.testOutputHelper.WriteLine("\"{0}\" {1}", executablePath, commandArguments);

            using (Process process = new Process())
            {
                process.StartInfo.FileName = executablePath;
                process.StartInfo.Arguments = commandArguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                {
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                                this.testOutputHelper.WriteLine(e.Data);
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                                this.testOutputHelper.WriteLine(e.Data);
                            }
                        };

                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.WaitForExit();
                        outputWaitHandle.WaitOne();
                        errorWaitHandle.WaitOne();
                    }
                }

                this.ConsoleOutput = output.ToString();
                this.ConsoleError = error.ToString();

                return process.ExitCode;
            }
        }
    }
}