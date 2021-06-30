using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dapper Dino
            //https://www.youtube.com/watch?v=7-tyLCAO4mY&t=369s
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}