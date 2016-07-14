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

		public QueueInvoker<InvokePacket> _queueInvokePacket;

		public InvokerClient(Node node)
		{
			Node = node;
			
			_remoteInvoker = RemoteClassBase.StaticConnect<RemoteInvoker>(node);
			_queueInvokePacket = new QueueInvoker<InvokePacket>(Invoke);

			_queueInvokePacket.OnDequeue += (p) => { Console.WriteLine("InvokerClient {0}:{1}: Пакет извлечен: \r\n {2}", Node.IpAddress, Node.Port, p.Guid); };
		}
		/*
		public InvokerClient(string ip_address, int port)
			: base(ip_address, port)
		{
			_remoteInvoker = RemoteClassBase.Connect<RemoteInvoker>(ip_address, port);
		}*/

		private void Invoke(InvokePacket invoke_packet)
		{
			_remoteInvoker.Invoke(invoke_packet);
		}

		public void SentToInvoke(InvokePacket invoke_packet)
		{
			_queueInvokePacket.Enqueue(invoke_packet);
			Console.WriteLine("InvokerClient {0}:{1}: Пакет помещен: \r\n {2}", Node.IpAddress, Node.Port, invoke_packet.Guid);
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
			/*if (_remoteInvoker != null)
			{
				_remoteInvoker.Dispose();
			}*/
		}

		public void Run()
		{
			_queueInvokePacket.Run();
		}

		public void Stop()
		{
			_queueInvokePacket.Stop();
		}
	}
}
