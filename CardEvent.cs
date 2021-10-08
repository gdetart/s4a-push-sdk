using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchServer
{
	public record CardEvent
	{
		public long CardNumber { get ; set; }
		public int DoorNumber { get; set; }
		public bool Valid { get; set; }
        public string Time { get; set;}
		public string Description { get; set; }
	}
}
