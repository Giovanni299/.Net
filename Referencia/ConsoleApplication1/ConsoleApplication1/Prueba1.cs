using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Prueba1
    {
        private bool cambio;

        public Prueba1(ref bool prueba)
        {
            this.cambio = prueba;
            var th = new Thread(() => esperar(ref cambio));
            th.Start();
        }
        
        public void esperar(ref bool prueba)
        {
            while(this.cambio)
            {
                Console.WriteLine("Esperando");
                Thread.Sleep(500);
            }
        }
    }
}
