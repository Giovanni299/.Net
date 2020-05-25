using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            bool prueba = true;

            Prueba1 qwe = new Prueba1(ref prueba);


            Thread.Sleep(5000);
            prueba = false;
            while(true)
            {
                Thread.Sleep(500);
            }
        }
    }
}
