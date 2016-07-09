using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model.RemoteClass;

namespace Core.Model.Server
{
	public class InvokerServer : ServerBase
	{
		public static int DefaultPort = 39998;
		public InvokerServer()
			: this(DefaultPort)
		{

		}

		public InvokerServer(int port) : base(port)
		{
			AddRemoteClassService(typeof(RemoteInvoker));
		}
	}
}
