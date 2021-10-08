using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatchServer
{
    internal class AccessPacket
    {
        public static int WGPacketSize = 64;                //Short Packet Length

        //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Type
        public static int Type = 0x17;      //2015-04-29 22:22:50			//Type

        public static int ControllerPort = 60000;        //Access Controller' Port
        public static long SpecialFlag = 0x55AAAA55;     //Special logo to prevent misuse

        public int functionID;                           //Function ID
        public long iDevSn;                              //Deceive Serial Number(Controller) four bytes, nine dec number
        public string IP;                                //Access Controller' IP Address

        public byte[] data = new byte[56];               //56 bytes of data [including sequenceId]
        public byte[] recv = new byte[WGPacketSize];     //Receive Data buffer

        public AccessPacket()
        {
            Reset();
        }

        public void Reset()  //Data reset
        {
            for (int i = 0; i < 56; i++)
            {
                data[i] = 0;
            }
        }

        private static long sequenceId;     //

        public byte[] ToByte() //Generates a 64-byte short package
        {
            byte[] buff = new byte[WGPacketSize];
            sequenceId++;

            buff[0] = (byte)Type;
            buff[1] = (byte)functionID;
            Array.Copy(System.BitConverter.GetBytes(iDevSn), 0, buff, 4, 4);
            Array.Copy(data, 0, buff, 8, data.Length);
            Array.Copy(System.BitConverter.GetBytes(sequenceId), 0, buff, 40, 4);
            return buff;
        }

        private readonly WG3000_COMM.Core.wgMjController controller = new WG3000_COMM.Core.wgMjController();

        public int Run()  //send command ,receive return command
        {
            byte[] buff = ToByte();

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    return -1;
                }
                else
                {
                    //sequenceId
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //Type consistent
                        && (recv[1] == functionID)              //Function ID is consistent
                        && (sequenceIdReceived == sequenceId))  //Controller'Serial number  correspondence
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //Retry three times

            return -1;
        }
        public static long SequenceIdSent()//
        {
            return sequenceId; // The last issue of the serial number(xid)
        }


        public void Close()
        {
            Console.WriteLine("close");
        }
    }

}
