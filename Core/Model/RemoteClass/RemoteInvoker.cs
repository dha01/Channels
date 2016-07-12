using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Model.DataPacket;

namespace Core.Model.RemoteClass
{
	public class ResultItem
	{
		public Data Data { get; set; }
		public ManualResetEvent ManualResetEvent { get; set; }
	}
	
	/// <summary>
	/// Клас для обмена данными с вычислительным узлом.
	/// </summary>
	public class RemoteInvoker : RemoteClassBase
	{
		public ConcurrentDictionary<Guid, ResultItem> Results = new ConcurrentDictionary<Guid, ResultItem>();
		
		public ConcurrentDictionary<Guid, Data> _result = new ConcurrentDictionary<Guid, Data>();
	//	public ConcurrentDictionary<Guid, Task> _inProcess = new ConcurrentDictionary<Guid, Task>();
		public ConcurrentDictionary<Guid, ManualResetEvent> _waitResult = new ConcurrentDictionary<Guid, ManualResetEvent>();

		
		//private BlockingCollection<Dictionary<Guid, Data>> 
		public ConcurrentQueue<InvokePacket> InvokeQueue = new ConcurrentQueue<InvokePacket>();

		public Action RunQueueExecutor;

		public void EnqueuePacket(InvokePacket invoke_packet)
		{
			InvokeQueue.Enqueue(invoke_packet);
			if (RunQueueExecutor != null)
			{
				RunQueueExecutor.Invoke();
			}
		}

		public void Invoke(InvokePacket invoke_packet)
		{
			//Console.WriteLine("Получен пакет для вызова метода {0}", invoke_packet.InvokeMethod.MethodName);
			var ta = Results.TryAdd(invoke_packet.Guid, new ResultItem()
			{
				Data = new Data(invoke_packet.Guid),
				ManualResetEvent = new ManualResetEvent(false)
			});
			EnqueuePacket(invoke_packet);

		/*	var t = new Task(() =>
			{
				try
				{
					object[] input_params = invoke_packet.InputParams.ToList().Select(x =>
					{
						// Данные уже готовы к использованию.
						if (x.HasValue)
						{
							return x.Value;
						}
						
						try
						{
							// Данные находятся на этом сервере.
							return GetData(x.Guid).Value;
						}
						catch (Exception)
						{
							// Данные находятся на удаленном сервере.
							return x.Value;
						}
					}).ToArray();

					var value = Invoke(invoke_packet.InvokeMethod.Path, invoke_packet.InvokeMethod.TypeName, invoke_packet.InvokeMethod.MethodName, input_params);

					var data = new Data(invoke_packet.Guid, value);
					Console.WriteLine("Получен результат {0}", value);

					Task ta;
					_result.TryAdd(invoke_packet.Guid, data);
					_inProcess.TryRemove(invoke_packet.Guid, out ta);
					
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					throw;
				}
				
			});

			
			_inProcess.TryAdd(invoke_packet.Guid, t);
			t.Start();*/
		}

		public Data GetData(Guid guid)
		{
			//Console.WriteLine("Получен запрос на получение данных {0}", guid);
		/*	ManualResetEvent mr;
			if (_waitResult.ContainsKey(guid))
			{
				mr = _waitResult[guid];
			}
			else
			{
				mr = new ManualResetEvent(false);
				_waitResult.TryAdd(guid, mr);
			}*/

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
				var ta = Results.TryAdd(guid, result);
				//throw new Exception(string.Format("Данных с id {0} нет.", guid));
			}
			else
			{
				result = Results[guid];
			}
			result.ManualResetEvent.WaitOne();

			//Console.WriteLine("Отправлен результат {0}", result.Data.Value);
			return result.Data;

			
/*
			ManualResetEvent mr;
			Task t;
			
			if (_result.ContainsKey(guid))
			{
				return _result[guid];
			}
				
			if (_inProcess.ContainsKey(guid))
			{
				t = _inProcess[guid];
				mr = new ManualResetEvent(false);
				t.ContinueWith(task =>
				{
					mr.Set();
				});
			}
			else
			{
				throw new Exception(string.Format("Данные с идентификатором {0} отсутствуют.", guid));
			}

			mr.WaitOne();

			
			if (_result.ContainsKey(guid))
			{
				Console.WriteLine("Отправлен результат {0}", _result[guid]);
				return _result[guid];
			}

			throw new Exception("Непредвиденная ошибка!");*/
		}


		/*
		public object Invoke(InvokePacket invoke_packet)
		{
			var data = (DataPacket.Data)Invoke(invoke_packet.InvokeMethod.Path, invoke_packet.InvokeMethod.TypeName, invoke_packet.InvokeMethod.MethodName,
				invoke_packet.InputParams.ToList().Select(x => x.Value).ToArray());
			
			return Invoke(invoke_packet.InvokeMethod.Path, invoke_packet.InvokeMethod.TypeName, invoke_packet.InvokeMethod.MethodName,
				invoke_packet.InputParams.ToList().Select(x => x.Value).ToArray());
		}*/
	}
}
