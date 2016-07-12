using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Model.Client;
using Core.Model.RemoteClass;

namespace Core.Model.Server
{
	public class CoordinationServer : ServerBase
	{
		public static int DefaultPort = 39999; 

		private List<InvokePacket> InvokePacketQueue = new List<InvokePacket>();
		private RemoteCoordinator _remoteCoordinator;

		private List<CoordinationClient> _coordinationClientList = new List<CoordinationClient>(); 

		public CoordinationServer()
			: this(DefaultPort)
		{

		}

		public CoordinationServer(int port)
			: base(port)
		{
			//AddRemoteClassService(typeof(RemoteCoordinator));
			_remoteCoordinator = AddRemoteClassService<RemoteCoordinator>();
			UdpClient = new UdpClient(BROADCAST_PORT);
			Start();

			Task.Run(() =>
			{
				while (true)
				{
					UdpPing();
					Thread.Sleep(10000);
				};
			});
		}

		public void AddInvokeServer(Node node)
		{
			_remoteCoordinator.CalculativeServerList.Add(node);
		}

		public UdpClient UdpClient { get; set; }
		public void Start()
		{
			// UdpClient для получения данных
			UdpClient.JoinMulticastGroup(IPAddress.Parse(BROADCAST_ADDRESS), 50);

			IPEndPoint remoteIp = null;
			var t = new Task(() =>
			{
				while (true)
				{
					try
					{
						while (true)
						{
							var data = UdpClient.Receive(ref remoteIp);

							var message = Encoding.ASCII.GetString(data).Split(':');

							var node = new Node
							{
								IpAddress = message[1],
								Port = int.Parse(message[2])
							};

							List<Node> new_invoke_server_list = null;
							

							if (message[0].Equals(typeof(CoordinationServer).Name))
							{
								if (!_coordinationClientList.Exists(x => x.Node.IpAddress.Equals(node.IpAddress)))
								{
									var new_coordination_client = new CoordinationClient(node);
									_coordinationClientList.Add(new_coordination_client);
									var new_list = new_coordination_client.CalculativeServerList;
									var cur_list = _remoteCoordinator.CalculativeServerList;
									new_invoke_server_list = new_list.Where(x => !cur_list.Exists(y => y.Equals(x))).ToList();
								}
							}

							if (message[0].Equals(typeof(InvokerServer).Name))
							{
								var cur_list = _remoteCoordinator.CalculativeServerList;

								if (!cur_list.Exists(x => x.Equals(node)))
								{
									new_invoke_server_list = new List<Node> { node };
								}
							}

							if (new_invoke_server_list != null)
							{
								foreach (var new_invoke_server in new_invoke_server_list)
								{
									AddInvokeServer(new_invoke_server);
									Console.WriteLine("Установлен новый вычислительный узел: {0}:{1}", new_invoke_server.IpAddress, new_invoke_server.Port);
								}
							}

							Console.WriteLine(message[0] + ":" + message[1] + ":" + message[2]);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("UdpServer.Start.Task: {0}", ex.Message);
					}
					finally
					{
						//UdpClient.Close();
					}
				}
			});
			t.Start();
		}
	}
}
