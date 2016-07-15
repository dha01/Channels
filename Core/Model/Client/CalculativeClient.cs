using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.Model.DataPacket;
using Core.Model.Server;

namespace Core.Model.Client
{
	/// <summary>
	/// Клиент для выполнения вычислений.
	/// </summary>
	public class CalculativeClient : ClientBase
	{
		#region Fields

		/// <summary>
		/// Клиент для координационного сервера.
		/// </summary>
		private readonly CoordinationClient _coordinationClient;

		/// <summary>
		/// Временный координационный сервер. 
		/// </summary>
		private static CoordinationServer _tmpCoordinationServer;

		/// <summary>
		/// Очередь отправки на исполнение.
		/// </summary>
		public readonly QueueInvoker<InvokePacket> _sentToInvokeQueue;

		public Node RootCoordinationNode { get; set; }

		#endregion

		#region Constructor

		public CalculativeClient(Node root_coordination_node) : this()
		{
			RootCoordinationNode = root_coordination_node;
		}

		/// <summary>
		/// Создает экземпляр класса для клиента выполнения вычислений.
		/// </summary>
		public CalculativeClient()
		{
			try
			{
				_coordinationClient = new CoordinationClient();
			}
			catch (Exception)
			{
				_tmpCoordinationServer = new CoordinationServer();
				_coordinationClient = new CoordinationClient();
			}

			RootCoordinationNode = _coordinationClient.Node;
			_sentToInvokeQueue = new QueueInvoker<InvokePacket>(_coordinationClient.SentToInvoke);

			_sentToInvokeQueue.OnDequeue += (p) => { Console.WriteLine("CalculativeClient: Пакет извлечен из очереди на выполение: \r\n {0}", p.Guid);};
		}

		#endregion

		#region Methods

		/// <summary>
		/// Вызов метода для исполнения.
		/// </summary>
		/// <typeparam name="T">Тип возвращаемых данных.</typeparam>
		/// <param name="invoke_method">Исполняемый метод.</param>
		/// <param name="input_params">Входные параметры.</param>
		/// <returns>Результат вычисления.</returns>
		public DataValue<T> Invoke<T>(InvokeMethod<T> invoke_method, params DataValue[] input_params)
		{
			var invoke_packet = new InvokePacket()
			{
				InvokeMethod = invoke_method,
				InputParams = input_params
			};
			_sentToInvokeQueue.Enqueue(invoke_packet);
			Console.WriteLine("CalculativeClient: Пакет помещен в очередь на выполение: \r\n {0}", invoke_packet.Guid);

			var result = new DataValue<T>(invoke_packet.Guid)
			{
				RootCoordinationNode = RootCoordinationNode,
				SenderNode = _coordinationClient.Node
			};

			return result;
		}

		public void Run()
		{
			_sentToInvokeQueue.Run();
		}

		public void Stop()
		{
			_sentToInvokeQueue.Stop();
		}

		/// <summary>
		/// Удаляет временный координационный сервер.
		/// </summary>
		public override void Dispose()
		{
			/*if (_tmpCoordinationServer != null)
			{
				_tmpCoordinationServer.Dispose();
			}

			if (_coordinationClient != null)
			{
				_coordinationClient.Dispose();
			}*/
		}

		#endregion
	}
}
