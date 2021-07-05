using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Run as a service
            // https://www.youtube.com/watch?v=y64L-3HKuP0

            //Dapper Dino
            //https://www.youtube.com/watch?v=7-tyLCAO4mY&t=369s

            //Not service Code
            //var bot = new Bot();
            //bot.RunAsync().GetAwaiter().GetResult();

            //Service code
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<AaronService>(s =>
                {
                    s.ConstructUsing(AaronService => new AaronService());
                    s.WhenStarted(AaronService => AaronService.Aaron());
                    s.WhenStopped(AaronService => AaronService.Aaron());
                });

                x.RunAsLocalSystem();
                x.SetServiceName("Aaron");
                x.SetDisplayName("Aaron");
                x.SetDescription("Aaron bot for Discord");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}