namespace WatchServer
{
    public record AlarmEvent
    {
        public long SerialNumber { get; set; }
        public int DoorNumber { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
    }
}
