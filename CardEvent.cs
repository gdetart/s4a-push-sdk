namespace WatchServer
{
    public record CardEvent
    {
        public long CardNumber { get; set; }
        public int DoorNumber { get; set; }
        public bool Valid { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
    }
}
