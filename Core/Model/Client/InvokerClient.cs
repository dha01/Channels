using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Model.DataPacket;
using Core.Model.RemoteClass;

namespace Core.Model.Client
{
	public class InvokerClient : ClientBase
	{
		private RemoteInvoker _remoteInvoker;

		public InvokerClient(Node node)
		{
			_remoteInvoker = RemoteClassBase.StaticConnect<RemoteInvoker>(node);
		}

		public InvokerClient(string ip_address, int port)
			: base(ip_address, port)
		{
			_remoteInvoker = RemoteClassBase.Connect<RemoteInvoker>(ip_address, port);
		}
		/*
		public object Invoke(string path, string type_name, string method_name, object[] param)
		{
			return _remoteInvoker.Invoke(path, type_name, method_name, param);
		}*/
		/*
		public Data Invoke(InvokePacket invoke_packet)
		{
			return (Data)_remoteInvoker.Invoke(invoke_packet);
		}

		public Data Invoke(InvokeMethod invoke_method, Data[] input_params)
		{
			return new Data(_remoteInvoker.Invoke(new InvokePacket()
			{
				InvokeMethod = invoke_method,
				InputParams = input_params
			}));
		}*/

		public void Invoke(InvokePacket invoke_packet)
		{
			_remoteInvoker.Invoke(invoke_packet);
			/*var task = new Task<T>(() =>
			{
				Data.WaitAll(input_params);
				return (T)_remoteInvoker.Invoke(invoke_packet);
			});*/
			//return new DataValue<T>(invoke_packet.Guid);
		}

		public Data GetData(Guid guid)
		{
			try
			{
				//_remoteInvoker.Ping();
				return _remoteInvoker.GetData(guid);
			}
			catch (Exception e)
			{
				// TODO: неправильно ждать
				Thread.Sleep(100);
				return _remoteInvoker.GetData(guid);
			}
		}

		public override void Dispose()
		{
			if (_remoteInvoker != null)
			{
				_remoteInvoker.Dispose();
			}
		}
	}
}
