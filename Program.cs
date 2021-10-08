using System;
using System.Collections.Generic;
using System.Threading;

namespace WatchServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            WatchingServer server = new("192.168.0.122", "192.168.0.27", 153162793, 6005);
            Thread UdpServer = new(server.WatchingServerRuning);
            UdpServer.Start();
           var key= Console.ReadLine();
            if (key == "ok")
            {
                server.ButtonsRepo.ForEach(Button=> {
                    Console.WriteLine(Button.Description);
                    Console.WriteLine(Button.SerialNumber);
                    Console.WriteLine(Button.Time);
                    Console.WriteLine(Button.DoorNumber);
                });
            }
            

        }


    }
    public class Controllers
    {
        public long SN;
        public long Index;
        public Controllers(long serialNumber, long latestIndex)
        {
            SN = serialNumber;
            Index = latestIndex;
        }
    }

    internal class WatchingServer : Helpers
    {
        public string ControllerIP;
        public string watchServerIP;
        public long controllerSN;
        public int watchServerPort;

        public WatchingServer(string Controllerip, string watchServerip, long controllerSn, int WatchServerPort)
        {
            ControllerIP = Controllerip;
            watchServerIP = watchServerip;
            controllerSN = controllerSn;
            watchServerPort = WatchServerPort;

        }
        public int TestWatchingServer()  //Receive server test - setup
        {
            AccessPacket pkt = new();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //Set the IP and port of the receiving server [Function ID: 0x90]
            //(If you do not want the controller to send data, the receiving server IP is set to 0.0.0.0)
            //The port of the receiving server: 61005
            //Send once every 5 seconds: 05
            pkt.Reset();
            pkt.functionID = 0x90;
            string[] strIP = watchServerIP.Split('.');
            if (strIP.Length == 4)
            {
                pkt.data[0] = byte.Parse(strIP[0]);
                pkt.data[1] = byte.Parse(strIP[1]);
                pkt.data[2] = byte.Parse(strIP[2]);
                pkt.data[3] = byte.Parse(strIP[3]);
            }
            else
            {
                return 0;
            }

            //The port of the receiving server: 61005
            pkt.data[4] = (byte)((watchServerPort & 0xff));
            pkt.data[5] = (byte)((watchServerPort >> 8) & 0xff);

            //Sent every 5 seconds: 05 (regular upload information cycle is 5 seconds [normal operation every 5 seconds you can costumise it however you want)
            pkt.data[6] = 5;

            int ret = pkt.Run();
            int success;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    Console.WriteLine("Set the IP and port of the receiving server");
                }
            }


            //Read the IP and port of the receiving server [Function ID: 0x92] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x92;

            ret = pkt.Run();
            success = 0;
            if (ret > 0)
            {
                Console.WriteLine("Read the IP and port of the receiving server successfully...");
                success = 1;
            }
            pkt.Close();
            return success;
        }

        public void WatchingServerRuning()
        {
            //Note: The firewall should allow all packets of this port to enter
            try
            {
                WG3000_COMM.Core.wgUdpServerCom udpserver = new WG3000_COMM.Core.wgUdpServerCom(watchServerIP, watchServerPort);
                if (!udpserver.IsWatching())
                {
                    Console.WriteLine(" Failed to monitor state");
                  
                }
               
                List<Controllers> allControllers = new List<Controllers> { };
                int recv_cnt;
                while (true)
                {

                    recv_cnt = udpserver.receivedCount();



                    if (recv_cnt > 0)
                    {
                        byte[] buff = udpserver.getRecords();
                        if (buff[1] == 0x20) //
                        {
                            long sn;
                            long recordIndexGet;
                            bool exists = true;
                            sn = byteToLong(buff, 4, 4); //Get serial number
                            recordIndexGet = byteToLong(buff, 8, 4); //Get Record Index number

                            //if there is a controller check --if that controller exists in the array if not add new 
                            if (allControllers.Count > 0)
                            {
                                foreach (Controllers controller in System.Linq.Enumerable.ToList(allControllers))
                                {
                                    if (controller.SN == sn)
                                    {
                                        exists = true;
                                        break;
                                    }
                                    else
                                    {
                                        exists = false;
                                    }
                                }
                                if (!exists)
                                {
                                    Controllers addController = new(sn, recordIndexGet);
                                    allControllers.Add(addController);
                                }
                            }
                            //if controllers list is empty add the controller
                            else
                            {
                                Controllers addController = new(sn, recordIndexGet);
                                allControllers.Add(addController);
                            }
                            foreach (Controllers s in System.Linq.Enumerable.ToList(allControllers))
                            {
                                //Console.WriteLine(controller.SN);
                                if (s.SN == sn)
                                {
                                    if (s.Index < recordIndexGet)
                                    {
                                        s.Index = recordIndexGet;

                                        ReturnRecordInfo(buff);

                                    }
                                }

                            }
                        }

                    }
                    else
                    {
                        Thread.Sleep(3000);  //'Delay 

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // throw;
            }
        }




    }
}
