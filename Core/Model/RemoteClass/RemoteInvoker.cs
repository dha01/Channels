using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Model.DataPacket;

namespace Core.Model.RemoteClass
{
	public class RemoteInvoker : RemoteClassBase
	{
		private Dictionary<Guid, Data> _result = new Dictionary<Guid, Data>();
		private Dictionary<Guid, Task> _inProcess = new Dictionary<Guid, Task>();

		public object Invoke(string path, string type_name, string method_name, object[] param)
		{
			var a = Assembly.LoadFrom(path);
			var t = a.GetType(type_name);
			var m = t.GetMethod(method_name);
			var obj = Activator.CreateInstance(t);
			return m.Invoke(obj, param);
		}

		public void Invoke(InvokePacket invoke_packet)
		{
			Console.WriteLine("Получен пакет для вызова метода {0}", invoke_packet.InvokeMethod.MethodName);
			
			var t = new Task(() =>
			{
				try
				{
					var value = Invoke(invoke_packet.InvokeMethod.Path, invoke_packet.InvokeMethod.TypeName, invoke_packet.InvokeMethod.MethodName,
					invoke_packet.InputParams.ToList().Select(x => x.Value).ToArray());

					var data = new Data(invoke_packet.Guid, value);
					Console.WriteLine("Получен результат {0}", value);
					lock (_result)
					lock (_inProcess)
					{
						_inProcess.Remove(invoke_packet.Guid);
						_result.Add(invoke_packet.Guid, data);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					throw;
				}
				
			});

			lock (_inProcess)
			{
				_inProcess.Add(invoke_packet.Guid, t);
			}
			t.Start();
		}

		public Data GetData(Guid guid)
		{
			Console.WriteLine("Получен запрос на получение данных {0}", guid);
			
			ManualResetEvent mr;
			Task t;
			lock (_result)
			{
				if (_result.ContainsKey(guid))
				{
					return _result[guid];
				}


				lock (_inProcess)
				{
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
				}
			}

			mr.WaitOne();
			t.Wait();
			lock (_result)
			{
				if (_result.ContainsKey(guid))
				{
					Console.WriteLine("Отправлен результат {0}", _result[guid]);
					return _result[guid];
				}
			}

			throw new Exception("Непредвиденная ошибка!");
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
