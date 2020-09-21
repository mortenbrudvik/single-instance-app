using System;
using shared;

namespace app2
{ 
    class Program
    {
        static void Main(string[] args)
        {
            var instanceService = new SingleInstanceService();
            if (!instanceService.Start())
            {
                Console.Out.WriteLine("There is all ready another instance of this application running");
            }
            else
            {
                Console.Out.WriteLine("This is the first instance of this application running");
            }

            Console.ReadKey();
            instanceService.Stop();
        }
    }
}
