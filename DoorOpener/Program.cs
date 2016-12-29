using Microsoft.Owin.Hosting;
using System;
using System.Net.Http;

namespace DoorOpener
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseUrl = "http://*:5000";
            using (WebApp.Start<Startup>(baseUrl))
            {
                while (true)
                {
                    Console.WriteLine("Waiting...");
                    System.Threading.Thread.Sleep(20000);
                }
                //Console.WriteLine("Press Enter to quit.");
                //Console.ReadKey();
                // http://localhost:5000/api/doors/
            }
        }
    }
}
