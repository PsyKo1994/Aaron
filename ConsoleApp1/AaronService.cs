using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class AaronService
    {

        public void Aaron()
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }

        public void Start()
        {
            AaronService obj = new AaronService();
            Thread thr = new Thread(new ThreadStart(obj.Aaron));
            thr.Start();
        }

        public void Stop()
        {
            Console.WriteLine("end");
        }
    }
}
