using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Core.Model.Client;
using Core.Model.DataPacket;
using Core.Model.Server;

namespace Core.Model.RemoteClass
{
	public class RemoteCoordinator : RemoteClassBase
	{
		public List<Node> CalculativeServerList = new List<Node>();

		private Dictionary<Guid, Node> _resultInfo = new Dictionary<Guid, Node>();

		private static Random rand = new Random(DateTime.Now.Millisecond);

		private Node SelectNode()
		{
			if (!CalculativeServerList.Any())
			{
				throw new Exception("Список доступных вычислительных узлов пуст.");
			}
			int index = rand.Next(0, CalculativeServerList.Count);

			return CalculativeServerList[index];
		}

		public void SentToInvoke(InvokePacket invoke_packet)
		{
			var node = SelectNode();
			using (var ic = new InvokerClient(node))
			{
				foreach (var param in invoke_packet.InputParams)
				{
					lock (_resultInfo)
					{
						if (_resultInfo.ContainsKey(param.Guid))
						{
							param.OwnerNode = _resultInfo[param.Guid];
							param.IsEndOwner = true;
							continue;
						}
					}
					
					param.OwnerNode = new Node { IpAddress = ServerBase.GetLocalIpAddress(), Port = CoordinationServer.DefaultPort};
					param.IsEndOwner = false;
				}
				
				lock (_resultInfo)
				{
					_resultInfo.Add(invoke_packet.Guid, node);
				}
				ic.Invoke(invoke_packet);
			}
		}

		public DataBase GetData(Guid guid)
		{
			int count = 0;
			while (count < 10)
			{
				// TODO: Нужно добавить механизм ожидания результата.
				lock (_resultInfo)
				{
					if (_resultInfo.ContainsKey(guid))
					{
						return new DataInfo(guid, _resultInfo[guid]);
					}
				}
				Thread.Sleep(500);
				count++;
			}

			throw new Exception(string.Format("Данных с индификатором {0} не обнаружено.", guid));
		}
	}
}
