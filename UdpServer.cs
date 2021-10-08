using System;
using System.Collections.Generic;
using System.Threading;

namespace WatchServer
{
    internal class UdpServer : Helpers
    {

        public string WatchServerIP { get; init; }
        public int WatchServerPort { get; init; }
        public List<Controllers> allControllers = new();

        public UdpServer(string watchServerip, int WatchServerPort)
        {
            WatchServerIP = watchServerip;
            this.WatchServerPort = WatchServerPort;
        }
        public int SetupController(string ControllerIP, long controllerSN)
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
            string[] strIP = WatchServerIP.Split('.');
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
            pkt.data[4] = (byte)((WatchServerPort & 0xff));
            pkt.data[5] = (byte)((WatchServerPort >> 8) & 0xff);

            //Sent every 5 seconds: 05 (regular upload information cycle is 5 seconds (normal operation every 5 seconds you can costumise it however you want)
            pkt.data[6] = 5;

            int ret = pkt.Run();
            int success;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    Console.WriteLine("IP and port of the receiving server are set successfully");
                }
            }


            //Read the IP and port of the receiving server [Function ID: 0x92] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x92;

            ret = pkt.Run();
            success = 0;
            if (ret > 0)
            {
                Console.WriteLine("Read the IP and port of the receiving server successfully.");
                success = 1;
            }
            pkt.Close();
            return success;
        }
        public void CheckIfControllerExists(long sn, long recordIndexGet)
        {
            bool exists = true;
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

        }

        public void UdpServerRunning()
        {
            //Note: The firewall should allow all packets of this port to enter
            try
            {
                WG3000_COMM.Core.wgUdpServerCom udpserver = new(WatchServerIP, WatchServerPort);
                if (!udpserver.IsWatching())
                {
                    Console.WriteLine(" Failed to monitor state");

                }

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
                            sn = ByteToLong(buff, 4, 4); //Get serial number
                            recordIndexGet = ByteToLong(buff, 8, 4); //Get Record Index number
                            //if there is a controller check --if that controller exists in the array if not add new 
                            CheckIfControllerExists(sn, recordIndexGet);
                            foreach (Controllers s in System.Linq.Enumerable.ToList(allControllers))
                            {
                                if (s.SN == sn)
                                {
                                    if (s.Index < recordIndexGet)
                                    {
                                        s.Index = recordIndexGet;

                                        GetRecordInfo(buff);

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
