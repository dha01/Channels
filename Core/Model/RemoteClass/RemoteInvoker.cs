using System;
using System.Collections.Concurrent;
using System.Threading;
using Core.Model.Client;
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
		
		public Action<InvokePacket> OnInvoke;

		#endregion

		#region Methods

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
			
			OnInvoke.Invoke(invoke_packet);
		}

		/// <summary>
		/// Возвращает данные с указанным идентификатором.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public Data GetData(Guid guid)
		{
			Console.WriteLine("RemoteInvoker: Запрошены данные: {0}", guid);
			
			ResultItem result = new ResultItem()
			{
				Data = new Data(guid),
				ManualResetEvent = new ManualResetEvent(false)
			};
			Results.TryAdd(guid, result);

			result = Results[guid];
			result.ManualResetEvent.WaitOne(60000);
			Results.TryRemove(result.Data.Guid, out result);
			using (var cc = new CoordinationClient())
			{
				cc.RemoveDataInfo(result.Data.Guid);
			}

			Console.WriteLine("RemoteInvoker: Возвращены данные: {0}", result.Data.Guid);
			return result.Data;
		}

		#endregion
	}
}
