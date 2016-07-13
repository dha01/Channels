using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

		#endregion

		#region Constructor

		/// <summary>
		/// Задает случайный порт.
		/// </summary>
		public InvokerServer()
			: this(GetRandomPort())
		{
			// TODO : нужно корректно обработать случай если порт занят
			Task.Run(() =>
			{
				while (true)
				{
					UdpNotification();
					Thread.Sleep(5000);
				};
			});
		}

		/// <summary>
		/// Устанавливает указанный порт.
		/// </summary>
		/// <param name="port">Порт.</param>
		public InvokerServer(int port) : base(port)
		{
			_remoteInvoker = AddRemoteClassService<RemoteInvoker>();
			RunQueueInvoker();
			_remoteInvoker.RunQueueExecutor = () => { _runQueueExecutor.Set(); };
		}

		#endregion

		#region Methods / Public

		/// <summary>
		/// Запускает очередь исполнения процедур.
		/// </summary>
		public void RunQueueInvoker()
		{
			Task.Run(() => { InvokeQueue(); });
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
			return m.Invoke(obj, param);
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
							if (_remoteInvoker.Results.ContainsKey(x.Guid))
							{
								x.Value = _remoteInvoker.GetData(x.Guid).Value;
								return;
							}
							object re = x.Value;
						}
						catch (Exception e)
						{
							Console.WriteLine("InParallel: {0}", e.Message);
						}
					});
					_remoteInvoker.EnqueuePacket(invoke_packet);
				}
				catch (Exception e)
				{
					Console.WriteLine("InRun: {0}", e.Message);
				}
			});
		}

		/// <summary>
		/// Цикл исполнения процедур.
		/// </summary>
		private void InvokeQueue()
		{
			while (true)
			{
				try
				{
					InvokePacket invoke_packet;
					if (_remoteInvoker.InvokeQueue.TryDequeue(out invoke_packet))
					{
						if (invoke_packet.InputParams.Any(x => !x.HasValue))
						{
							PrepareInputParams(invoke_packet);
							continue;
						}

						object[] input_params = invoke_packet.InputParams.ToList().Select(x => x.Value).ToArray();
						var value = InvokeMethod(invoke_packet.InvokeMethod.Path, invoke_packet.InvokeMethod.TypeName, invoke_packet.InvokeMethod.MethodName, input_params);
						var result = _remoteInvoker.Results[invoke_packet.Guid];

						result.Data.Value = value;
						result.ManualResetEvent.Set();
					}
					else
					{
						if (_remoteInvoker.InvokeQueue.Count == 0)
						{
							_runQueueExecutor.Reset();
						}
					}
					_runQueueExecutor.WaitOne();
				}
				catch (Exception e)
				{
					Console.WriteLine("InWhile {0}", e.Message);
				}
			}
		}

		#endregion
	}
}
