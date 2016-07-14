using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Model.Client;

namespace Core.Model
{
	public class Distributed : CalculativeClient
	{
		public Distributed()
		{
		}

		public Distributed(Node coordination_node)
			: base(coordination_node)
		{

		}
	}
}
