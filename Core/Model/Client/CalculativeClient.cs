using System;
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
		private readonly CoordinationServer _tmpCoordinationServer;

		/// <summary>
		/// Очередь отправки на исполнение.
		/// </summary>
		public readonly QueueInvoker<InvokePacket> _sentToInvokeQueue;

		#endregion

		#region Constructor

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

			_sentToInvokeQueue = new QueueInvoker<InvokePacket>(_coordinationClient.SentToInvoke);
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

			return new DataValue<T>(invoke_packet.Guid);
		}

		/// <summary>
		/// Удаляет временный координационный сервер.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			if (_tmpCoordinationServer != null)
			{
				_tmpCoordinationServer.Dispose();
			}
		}

		#endregion
	}
}
