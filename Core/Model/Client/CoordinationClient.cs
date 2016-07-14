using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model.DataPacket;
using Core.Model.RemoteClass;
using Core.Model.Server;

namespace Core.Model.Client
{
	public class CoordinationClient : ClientBase
	{
		public List<Node> CalculativeServerList
		{
			get
			{
				return _remoteCoordinator.CalculativeServerList;
			}
		}
		private RemoteCoordinator _remoteCoordinator;

		public CoordinationClient(Node node)
		{
			Node = node;
			_remoteCoordinator = RemoteClassBase.StaticConnect<RemoteCoordinator>(node);
		}

		
		public CoordinationClient(string ip_address, int port)
			: this(new Node{IpAddress = ip_address, Port = port})
		{

		}

		public CoordinationClient()
			: this(GetLocalHost())
		{

		}

		public void SentToInvoke(InvokePacket invoke_packet)
		{
			_remoteCoordinator.SentToInvoke(invoke_packet);
		}

		public DataBase GetData(Guid guid)
		{
			return _remoteCoordinator.GetData(guid);
		}

		public void RemoveDataInfo(Guid guid)
		{
			_remoteCoordinator.RemoveDataInfoata(guid);
		}

		public static Node GetLocalHost()
		{
			return new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}
	}
}
