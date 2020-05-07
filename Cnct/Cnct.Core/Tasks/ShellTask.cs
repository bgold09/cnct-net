using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Cnct.Core.Tasks
{
    internal class ShellTask : CnctTaskBase
    {
        public ShellTask(ILogger logger)
            : base(logger)
        {
        }

        public override Task ExecuteAsync()
        {
            var startInfo = new ProcessStartInfo()
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = true,
                //FileName ="" ,
                //Arguments = ""
            };

            using var process = new Process()
            {
                StartInfo = startInfo,
            };
;
            var outputs = new List<string>();
            var errors = new List<string>();

            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                outputs.Add(e.Data);
                Console.WriteLine(e.Data);
            });

            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                errors.Add(e.Data);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Data);
                Console.ResetColor();
            });

            //PowerShell

            //process.StandardError

            return Task.CompletedTask;
        }
    }
}
