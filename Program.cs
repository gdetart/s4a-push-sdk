using System;
using System.Collections.Generic;
using System.Threading;

namespace WatchServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            UdpServer server = new("192.168.0.27", 6005);
            Thread UdpServer = new(server.UdpServerRunning);
            UdpServer.Start();
            string key = Console.ReadLine();
            if (key == "ok")
            {
                server.ButtonsRepo.ForEach(Button =>
                {
                    Console.WriteLine(Button.Description);
                    Console.WriteLine(Button.SerialNumber);
                    Console.WriteLine(Button.Time);
                    Console.WriteLine(Button.DoorNumber);
                });
            }


        }


    }
}
