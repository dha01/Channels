using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model.RemoteClass;

namespace Core.Model.Server
{
	public class CoordinationServer : ServerBase
	{
		public static int DefaultPort = 39999; 


		private List<InvokePacket> InvokePacketQueue = new List<InvokePacket>();
		private RemoteCoordinator _remoteCoordinator;

		public CoordinationServer()
			: this(DefaultPort)
		{

		}

		public CoordinationServer(int port)
			: base(port)
		{
			//AddRemoteClassService(typeof(RemoteCoordinator));
			_remoteCoordinator = AddRemoteClassService<RemoteCoordinator>();
		}

		public void AddInvokeServer(Node node)
		{
			_remoteCoordinator.CalculativeServerList.Add(node);
		}
	}
}
