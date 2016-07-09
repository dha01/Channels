using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Core.Model
{
	[Serializable]
	public class Node
	{
		public string IpAddress { get; set; }
		public int Port { get; set; }
	}
}
