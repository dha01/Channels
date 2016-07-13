using System;
using System.Collections.Concurrent;
using System.Threading;
using Core.Model.DataPacket;

namespace Core.Model.RemoteClass
{
	/// <summary>
	/// Клас для обмена данными с вычислительным узлом.
	/// </summary>
	public class RemoteInvoker : RemoteClassBase
	{
		#region ResultItem

		/// <summary>
		/// Результат исполнения метода.
		/// </summary>
		public class ResultItem
		{
			/// <summary>
			/// Данные.
			/// </summary>
			public Data Data { get; set; }

			/// <summary>
			/// Событе означающие, что данные были получены.
			/// </summary>
			public ManualResetEvent ManualResetEvent { get; set; }
		}

		#endregion

		#region Fields

		/// <summary>
		/// Список результатов вычисления.
		/// </summary>
		public ConcurrentDictionary<Guid, ResultItem> Results = new ConcurrentDictionary<Guid, ResultItem>();
		
		/// <summary>
		/// TODO: пределать на использование класса QueueInvoker
		/// </summary>
		public ConcurrentQueue<InvokePacket> InvokeQueue = new ConcurrentQueue<InvokePacket>();
		public Action RunQueueExecutor;

		#endregion

		#region Methods

		/// <summary>
		/// Добавляет пакет в очередь исполнения.
		/// </summary>
		/// <param name="invoke_packet">Пакет для исполнения.</param>
		public void EnqueuePacket(InvokePacket invoke_packet)
		{
			InvokeQueue.Enqueue(invoke_packet);
			if (RunQueueExecutor != null)
			{
				RunQueueExecutor.Invoke();
			}
		}

		/// <summary>
		/// Исполняет пакет.
		/// </summary>
		/// <param name="invoke_packet"></param>
		public void Invoke(InvokePacket invoke_packet)
		{
			Results.TryAdd(invoke_packet.Guid, new ResultItem()
			{
				Data = new Data(invoke_packet.Guid),
				ManualResetEvent = new ManualResetEvent(false)
			});
			EnqueuePacket(invoke_packet);
		}

		/// <summary>
		/// Возвращает данные с указанным идентификатором.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public Data GetData(Guid guid)
		{
			ManualResetEvent me;
			ResultItem result;
			if (!Results.ContainsKey(guid))
			{
				me = new ManualResetEvent(false);
				
				result = new ResultItem()
				{
					Data = new Data(guid),
					ManualResetEvent = me
				};
				Results.TryAdd(guid, result);
			}
			else
			{
				result = Results[guid];
			}
			result.ManualResetEvent.WaitOne();
			return result.Data;
		}

		#endregion
	}
}
