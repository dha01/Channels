using System;
using System.Collections.Generic;
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
	/// <summary>
	/// Координационный сервер.
	/// </summary>
	public class CoordinationServer : ServerBase
	{
		#region Fields

		/// <summary>
		/// Порт по умолчанию.
		/// </summary>
		public static int DefaultPort = 39999; 

		/// <summary>
		/// Объект для удаленного взаимодействия.
		/// </summary>
		private RemoteCoordinator _remoteCoordinator;

		/// <summary>
		/// Список вычислительных узлов.
		/// </summary>
		private List<CoordinationClient> _coordinationClientList = new List<CoordinationClient>();

		#endregion

		#region Constructor

		/// <summary>
		/// Инициализирует сервре с портом по умолчанию.
		/// </summary>
		public CoordinationServer()
			: this(DefaultPort)
		{

		}

		/// <summary>
		/// Инициализирует сервер с указанным портом.
		/// </summary>
		/// <param name="port">Порт.</param>
		public CoordinationServer(int port)
			: base(port)
		{
			_remoteCoordinator = AddRemoteClassService<RemoteCoordinator>();
			StartUdpNotification();
		}

		#endregion

		#region Methods

		public void AddInvokeServer(Node node)
		{
			_remoteCoordinator.CalculativeServerList.Add(node);
		}

		#endregion

		#region UdpNotification

		/// <summary>
		/// Принимает UDP сообщения.
		/// </summary>
		/// <param name="udp_client"></param>
		private void ReceiveUdpMessage(UdpClient udp_client)
		{
			IPEndPoint remote_ip = null;
			while (true)
			{
				var data = udp_client.Receive(ref remote_ip);

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

		/// <summary>
		/// Запускает UDP уведомитель.
		/// </summary>
		public void StartUdpNotification()
		{
			Task.Run(() =>
			{
				UdpClient udp_client = new UdpClient(BROADCAST_PORT);
				udp_client.JoinMulticastGroup(IPAddress.Parse(BROADCAST_ADDRESS), 50);
				while (true)
				{
					try
					{
						ReceiveUdpMessage(udp_client);
					}
					catch (Exception ex)
					{
						Console.WriteLine("UdpServer.Start.Task: {0}", ex.Message);
					}
				}
			});

			Task.Run(() =>
			{
				while (true)
				{
					UdpNotification();
					Thread.Sleep(10000);
				};
			});
		}

		#endregion
	}
}
