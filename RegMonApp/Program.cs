using System;

namespace RegistryValueMonitor
{
    class Program
    {
        static void Main(string[] args)
        {

            Monitor s = new Monitor("RegistryValueMonitor", null);
            s.Start();
            while (Console.Read() != 'q') ;
        }

    }
}
