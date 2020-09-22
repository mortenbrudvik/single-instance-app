using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using shared;

namespace app2
{ 
    class Program
    {
        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync("Starting app2");
            try
            {
                var instanceService = new SingleInstanceService();
                if (!await instanceService.Start())
                {
                    await Console.Out.WriteLineAsync("There is all ready another instance of this application running");
                    await instanceService.SignalFirstInstance(new List<string>() { "Hello from app2" });
                }
                else
                {
                    await Console.Out.WriteLineAsync("This is the first instance of this application running");
                }

                Console.ReadKey();
                instanceService.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
