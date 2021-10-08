using System;
using System.Collections.Generic;

namespace WatchServer
{
    public class Helpers
    {
        public long byteToLong(byte[] buff, int start, int len)
        {
            long val = 0;
            for (int i = 0; i < len && i < 4; i++)
            {
                long lng = buff[i + start];
                val += (lng << (8 * i));
            }
            return val;
        }
        public List<CardEvent> CardsRepo = new();
        public List<ButtonEvent> ButtonsRepo = new();
        public List<AlarmEvent> AlarmsRepo = new();


        public void ReturnRecordInfo(byte[] recv)
        {
            int recordType = recv[12];

            int recordValid = recv[13];

            int recordDoorNO = recv[14];
            _ = recv[15];

            long recordCardNO = byteToLong(recv, 16, 4);

            string recordTime = $"{recv[20]}{recv[21]}-{recv[22]}-{recv[23]} {recv[24]}:{recv[25]}:{recv[26]}";
            int reason = recv[27];

            if (recordType == 0)
            {
                //No records
            }
            else if (recordType == 0xff)
            {
                //record overwriten
            }
            else if (recordType == 1)
            {
                CardEvent Card = new();
                Card.CardNumber = recordCardNO;
                Card.DoorNumber = recordDoorNO;
                Card.Valid = recordValid == 1 ? true : false;
                Card.Time = recordTime;
                Card.Description = GetReasonDetailEnglish(reason);
                CardsRepo.Add(Card);
            }
            else if (recordType == 2)
            {
                ButtonEvent Button = new();
                Button.SerialNumber = recordCardNO;
                Button.DoorNumber = recordDoorNO;
                Button.Time = recordTime;
                Button.Description = GetReasonDetailEnglish(reason);
                ButtonsRepo.Add(Button);
            }
            else if (recordType == 3)
            {
                AlarmEvent Alarm = new();
                Alarm.SerialNumber = recordCardNO;
                Alarm.SerialNumber = recordDoorNO;
                Alarm.Time = recordTime;
                Alarm.Description = GetReasonDetailEnglish(reason);
                AlarmsRepo.Add(Alarm);
            }
            else
            {
            }
        }
        public string GetReasonDetailEnglish(int Reason) //English description
        {   
            if (Reason > 45)
            {
                return "";
            }
            if (Reason <= 0)
            {
                return "";
            }
            return RecordDetails[(Reason - 1) * 4 + 2]; //English information
        }

        public readonly string[] RecordDetails =
      {
"1","SwipePass","Swipe","notNeeded",
"2","SwipePass","Swipe Close","notNeeded",
"3","SwipePass","Swipe Open","notNeeded",
"4","SwipePass","Swipe Limited Times","notNeeded",
"5","SwipeNOPass","Denied Access: PC Control","notNeeded",
"6","SwipeNOPass","Denied Access: No PRIVILEGE","notNeeded",
"7","SwipeNOPass","Denied Access: Wrong PASSWORD","notNeeded",
"8","SwipeNOPass","Denied Access: AntiBack","notNeeded",
"9","SwipeNOPass","Denied Access: More Cards","notNeeded",
"10","SwipeNOPass","Denied Access: First Card Open","notNeeded",
"11","SwipeNOPass","Denied Access: Door Set NC","notNeeded",
"12","SwipeNOPass","Denied Access: InterLock","notNeeded",
"13","SwipeNOPass","Denied Access: Limited Times","notNeeded",
"14","SwipeNOPass","Denied Access: Limited Person Indoor","notNeeded",
"15","SwipeNOPass","Denied Access: Invalid Timezone","notNeeded",
"16","SwipeNOPass","Denied Access: In Order","notNeeded",
"17","SwipeNOPass","Denied Access: SWIPE GAP LIMIT","notNeeded",
"18","SwipeNOPass","Denied Access","notNeeded",
"19","SwipeNOPass","Denied Access: Limited Times","notNeeded",
"20","ValidEvent","Push Button","notNeeded",
"21","ValidEvent","Push Button Open","notNeeded",
"22","ValidEvent","Push Button Close","notNeeded",
"23","ValidEvent","Door Open","notNeeded",
"24","ValidEvent","Door Closed","notNeeded",
"25","ValidEvent","Super Password Open Door","notNeeded",
"26","ValidEvent","Super Password Open","notNeeded",
"27","ValidEvent","Super Password Close","notNeeded",
"28","Warn","Controller Power On","notNeeded",
"29","Warn","Controller Reset","notNeeded",
"30","Warn","Push Button Invalid: Disable","notNeeded",
"31","Warn","Push Button Invalid: Forced Lock","notNeeded",
"32","Warn","Push Button Invalid: Not On Line","notNeeded",
"33","Warn","Push Button Invalid: InterLock","notNeeded",
"34","Warn","Threat","notNeeded",
"35","Warn","Threat Open","notNeeded",
"36","Warn","Threat Close","notNeeded",
"37","Warn","Open too long","notNeeded",
"38","Warn","Forced Open","notNeeded",
"39","Warn","Fire","notNeeded",
"40","Warn","Forced Close","notNeeded",
"41","Warn","Guard Against Theft","notNeeded",
"42","Warn","7*24Hour Zone","notNeeded",
"43","Warn","Emergency Call","notNeeded",
"44","RemoteOpen","Remote Open Door","notNeeded",
"45","RemoteOpen","Remote Open Door By USB Reader","notNeeded"
        };
    }

    }
