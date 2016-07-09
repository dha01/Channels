using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Model.DataPacket;
using Core.Model.RemoteClass;
using Core.Model.Server;

namespace Core.Model.Client
{
	public class CalculativeClient : ClientBase
	{

		public List<Node> Services = new List<Node>();

		private CoordinationClient _coordinationClient;

		private CoordinationServer _tmpCoordinationServer;

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
		}

		public DataValue<T> Invoke<T>(InvokeMethod<T> invoke_method, params DataValue[] input_params)
		{
			var invoke_packet = new InvokePacket()
			{
				InvokeMethod = invoke_method,
				InputParams = input_params
			};
			
			var result = new DataValue<T>(invoke_packet.Guid);

			Task.Run(() =>
			{
				_coordinationClient.SentToInvoke(invoke_packet);
			});

			return result;
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_tmpCoordinationServer != null)
			{
				_tmpCoordinationServer.Dispose();
			}
		}
	}
}
