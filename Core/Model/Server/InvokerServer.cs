using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core.Model.Client;
using Core.Model.DataPacket;
using Core.Model.RemoteClass;

namespace Core.Model.Server
{
	/// <summary>
	/// Сервер вычислительного узла.
	/// </summary>
	public class InvokerServer : ServerBase
	{
		#region Fields

		/// <summary>
		/// Порт по умолчанию.
		/// </summary>
		public static int DefaultPort = 39998;

		/// <summary>
		/// Клас для обмена данными с вычислительным узлом.
		/// </summary>
		private RemoteInvoker _remoteInvoker;

		/// <summary>
		/// Згруженные библиотеки.
		/// </summary>
		private ConcurrentDictionary<string, Assembly> _assamblies = new ConcurrentDictionary<string, Assembly>();

		/// <summary>
		/// Отвечат приостановку и запуск очередей.
		/// </summary>
		public ManualResetEvent _runQueueExecutor = new ManualResetEvent(false);

		private CoordinationServer _tmpCoordinationServer;
		private CoordinationClient _coordinationClient;

		/// <summary>
		/// Очередь отправки на исполнение.
		/// </summary>
		public readonly QueueInvoker<InvokePacket> _sentToInvokeQueue;

		#endregion

		#region Constructor

		/// <summary>
		/// Задает случайный порт.
		/// </summary>
		public InvokerServer()
			: this(GetRandomPort())
		{
			
		}

		/// <summary>
		/// Устанавливает указанный порт.
		/// </summary>
		/// <param name="port">Порт.</param>
		public InvokerServer(int port) : base(port)
		{
			// Тут это и должно быть.
			Task.Run(() =>
			{
				while (true)
				{
					UdpNotification();
					Thread.Sleep(5000);
				}
			});
			/*
			try
			{
				_coordinationClient = new CoordinationClient();
			}
			catch (Exception)
			{
				_tmpCoordinationServer = new CoordinationServer();
				_coordinationClient = new CoordinationClient();
			}*/
			
			_remoteInvoker = AddRemoteClassService<RemoteInvoker>();

			_sentToInvokeQueue = new QueueInvoker<InvokePacket>(Invoke, 5);
			_sentToInvokeQueue.OnDequeue += (invoke_packet) => { Console.WriteLine("InvokerServer: Пакет извлечен из очереди: \r\n {0}", invoke_packet.Guid); };

			_remoteInvoker.OnInvoke += (invoke_packet) =>
			{
				_sentToInvokeQueue.Enqueue(invoke_packet);
			};
			_remoteInvoker.OnInvoke += (invoke_packet) => { Console.WriteLine("InvokerServer: Получен пакет: \r\n {0}", invoke_packet.Guid); };
		}

		#endregion

		#region Methods / Public

		public void GetStatistic()
		{
			Console.WriteLine("QueueLength: {0}", _sentToInvokeQueue.QueueLength);
			Console.WriteLine("CurQueueCount: {0}", _sentToInvokeQueue.CurQueueCount);
			Console.WriteLine("Results.Count: {0}", _remoteInvoker.Results.Count);
			

			if (_remoteInvoker.Results.Count > 0)
			{
				var item = _remoteInvoker.Results.First();
				Console.WriteLine("item.Value.Data.Value: {0}", item.Value.Data.GetValue());
				//_remoteInvoker.Results.First().Value.ManualResetEvent.Set();
			}
		}

		/// <summary>
		/// Исполняет метод и возвращает полученное значение.
		/// </summary>
		/// <param name="path">Путь к библиотеке.</param>
		/// <param name="type_name">Название типа.</param>
		/// <param name="method_name">Название метода.</param>
		/// <param name="param">Входные параметры.</param>
		/// <returns>Результат выполнения процедуры.</returns>
		public object InvokeMethod(string path, string type_name, string method_name, object[] param)
		{
			if (!_assamblies.ContainsKey(path))
			{
				_assamblies.TryAdd(path, Assembly.LoadFrom(path));
			}
			
			Assembly a = _assamblies[path];

			var t = a.GetType(type_name);
			var m = t.GetMethod(method_name);
			var obj = Activator.CreateInstance(t);

			try
			{
				return m.Invoke(obj, param);
			}
			catch (Exception e)
			{
				return e.InnerException;
			}
		}

		#endregion

		#region Methods / Private

		/// <summary>
		/// Подготавливает входные параметры для процедуры и возвращает на исполнение в очередь.
		/// </summary>
		/// <param name="invoke_packet"></param>
		private void PrepareInputParams(InvokePacket invoke_packet)
		{
			Task.Run(() =>
			{
				try
				{
					Parallel.ForEach(invoke_packet.InputParams, x =>
					{
						try
						{
							if (x.HasValue)
							{
								return;
							}
							//TODO: Нужно будет обрабатывать отваоившиеся узлы.

							Console.WriteLine("InParallel: {0}:{1}", x.OwnerNode.IpAddress, x.OwnerNode.Port);
							if (_remoteInvoker.Results.ContainsKey(x.Guid))
							{
								x.Value = _remoteInvoker.GetData(x.Guid).GetValue();
								x.HasValue = true;
								return;
							}
							Console.WriteLine("InParallel Value");
							x.GetValue();
							x.HasValue = true;
						}
						catch (Exception e)
						{
							Console.WriteLine("InParallel: {0}", e.Message);
							throw;
						}
					});
					//_remoteInvoker.EnqueuePacket(invoke_packet);
					_sentToInvokeQueue.Enqueue(invoke_packet);
				}
				catch (Exception e)
				{
					Console.WriteLine("InRun: {0}", e.Message);
				}
			});
		}

		private void Invoke(InvokePacket invoke_packet)
		{
			if (invoke_packet.InputParams.Any(x => !x.HasValue))
			{
				PrepareInputParams(invoke_packet);
				return;
			}
			
			object[] input_params = invoke_packet.InputParams.ToList().Select(x => x.GetValue()).ToArray();
			var value = InvokeMethod(invoke_packet.InvokeMethod.Path, invoke_packet.InvokeMethod.TypeName, invoke_packet.InvokeMethod.MethodName, input_params);
			var result = _remoteInvoker.Results[invoke_packet.Guid];

			result.Data.Value = value;
			Console.WriteLine("InvokerServer: Получен результат для пакета: {0}", invoke_packet.Guid);
			result.ManualResetEvent.Set();
		}

		public void Run()
		{
			_sentToInvokeQueue.Run();
		}

		public void Stop()
		{
			_sentToInvokeQueue.Stop();
		}

		#endregion
	}
}
