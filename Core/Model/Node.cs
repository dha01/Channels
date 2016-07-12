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
		protected bool Equals(Node other)
		{
			return string.Equals(IpAddress, other.IpAddress) && Port == other.Port;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Node) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((IpAddress != null ? IpAddress.GetHashCode() : 0)*397) ^ Port;
			}
		}

		public string IpAddress { get; set; }
		public int Port { get; set; }
	}
}
