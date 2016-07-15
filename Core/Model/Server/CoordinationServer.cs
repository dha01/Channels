using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Model.Client;
using Core.Model.DataPacket;
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

		/// <summary>
		/// Список доступных вычислитльных узлов.
		/// </summary>
		public List<Node> CalculativeServerList = new List<Node>();

		/// <summary>
		/// Клиенты для вычислительных узлов.
		/// </summary>
		private readonly ConcurrentDictionary<Node, InvokerClient> _invokerClients = new ConcurrentDictionary<Node, InvokerClient>();

		/// <summary>
		/// Список результатов вычислений.
		/// </summary>
		private readonly ConcurrentDictionary<Guid, Node> _resultInfo = new ConcurrentDictionary<Guid, Node>();

		private readonly ConcurrentDictionary<Guid, ManualResetEvent> _waitResult = new ConcurrentDictionary<Guid, ManualResetEvent>();

		/// <summary>
		/// Для выбра произвольного узла.
		/// </summary>
		private static readonly Random rand = new Random(DateTime.Now.Millisecond);

		private bool IsDisposed = false;

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
			_remoteCoordinator.OnEnqueuePacket += EnqueueInvokePacket;
			_remoteCoordinator.OnGetData += GetData;
			_remoteCoordinator.CalculativeServerList = CalculativeServerList;
			_remoteCoordinator.OnRemoveDataInfo += RemoveDataInfo;

			foreach (var node in CalculativeServerList)
			{
				AddInvokerClient(node);
			}

			StartUdpNotification();
		}

		#endregion

		private void AddInvokerClient(Node node)
		{
			InvokerClient ic;
			if (!_invokerClients.ContainsKey(node))
			{
				ic = new InvokerClient(node);
				ic._queueInvokePacket.OnDequeue += (p) =>
				{
					if (_waitResult.ContainsKey(p.Guid))
					{
						ManualResetEvent me;
						_waitResult.TryRemove(p.Guid, out me);
						me.Set();
					}
				};
				_invokerClients.TryAdd(node, ic);
			}
		}

		#region Methods

		public void RemoveDataInfo(Guid guid)
		{
			Node node;
			_resultInfo.TryRemove(guid, out node);
		}

		/// <summary>
		/// Возвращает результат вычисления с идентификатором<value>guid</value>
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public Data GetData(Guid guid)
		{
			Console.WriteLine("CoordinationServer: Запрошены данные: {0}", guid);
			var me = new ManualResetEvent(false);
			if (_waitResult.TryAdd(guid, me))
			{
				if (!_resultInfo.ContainsKey(guid))
				{
					Thread.Sleep(10);
				}
				
				if (!_resultInfo.ContainsKey(guid))
				{
					me.WaitOne();
				}
				_waitResult.TryRemove(guid, out me);
			}

			
			var ri = _resultInfo[guid];
			AddInvokerClient(ri);
			InvokerClient ic = _invokerClients[ri];
			/*
			if (!_invokerClients.ContainsKey(ri))
			{
				ic = new InvokerClient(ri);
				ic._queueInvokePacket.OnDequeue += (p) =>
				{
					if (_waitResult.ContainsKey(p.Guid))
					{
						ManualResetEvent mee;
						_waitResult.TryRemove(p.Guid, out mee);
						me.Set();
					}
				};
				_invokerClients.TryAdd(ri, ic);
			}
			else
			{
				ic = _invokerClients[ri];
			}*/
				//Console.WriteLine("По данным от координатора. Ожидаются данные с id {0} с сервера {1}:{2}", Guid, data_info.OwnerNode.IpAddress, data_info.OwnerNode.Port);
			
			return ic.GetData(guid);
			/*
			int count = 0;
			while (count < 10)
			{
				// TODO: Нужно добавить механизм ожидания результата.

				if (_resultInfo.ContainsKey(guid))
				{
					return new DataInfo(guid, _resultInfo[guid]);
				}
				Thread.Sleep(500);
				count++;
			}*/

			throw new Exception(string.Format("Данных с индификатором {0} не обнаружено.", guid));
		}

		/// <summary>
		/// Выбирает узел для исполнения.
		/// TODO: нужен будет механиз для выбора подходящего сервера, а не произвольного.
		/// </summary>
		/// <returns>Узел.</returns>
		private Node SelectNode()
		{
			if (!CalculativeServerList.Any())
			{
				throw new Exception("Список доступных вычислительных узлов пуст.");
			}
			var index = rand.Next(0, CalculativeServerList.Count);

			return CalculativeServerList[index];
		}

		public void EnqueueInvokePacket(InvokePacket invoke_packet)
		{
			var node = SelectNode();

			InvokerClient ic = _invokerClients[node];
			/*
			if (!_invokerClients.ContainsKey(node))
			{
				ic = new InvokerClient(node);
				ic._queueInvokePacket.OnDequeue += (p) =>
				{
					if (_waitResult.ContainsKey(p.Guid))
					{
						ManualResetEvent me;
						_waitResult.TryRemove(p.Guid, out me);
						me.Set();
					}
				};

				_invokerClients.TryAdd(node, ic);
			}
			else
			{
				ic = _invokerClients[node];
			}*/
			

			// Добавляет информацио о владельце данных, если он известен.
			foreach (var param in invoke_packet.InputParams)
			{
				if (_resultInfo.ContainsKey(param.Guid))
				{
					// Вычислительный узел владеющий данными.
					param.OwnerNode = _resultInfo[param.Guid];
				}

				// Координационный узел отправивший пакет.
				param.SenderNode = node;
			}

			_resultInfo.TryAdd(invoke_packet.Guid, node);
			ic.SentToInvoke(invoke_packet);
		}

		public void AddInvokeServer(Node node)
		{
			AddInvokerClient(node);
			CalculativeServerList.Add(node);
			
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
			//while (!IsDisposed)
			//{
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

				//Console.WriteLine(message[0] + ":" + message[1] + ":" + message[2]);
		//	}
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
				while (!IsDisposed)
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
				while (!IsDisposed)
				{
					UdpNotification();
					Thread.Sleep(10000);
				};
			});
		}

		#endregion

		public override void Dispose()
		{
			IsDisposed = true;
			/*var name = _remoteCoordinator.GetType().FullName;
			if (Services.ContainsKey(name))
			{
				RemotingServices.Disconnect(Services[name]);
				Services.Remove(name);
			}

			RemoteClassBase.Disconnect<RemoteCoordinator>(Node);
			base.Dispose();*/
		}
	}
}
