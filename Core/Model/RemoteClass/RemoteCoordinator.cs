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
	/// <summary>
	/// Класс для обмена данными с удаленным координационным узлом.
	/// </summary>
	public class RemoteCoordinator : RemoteClassBase
	{
		#region Fields

		/// <summary>
		/// Список доступных вычислитльных узлов.
		/// </summary>
		public List<Node> CalculativeServerList = new List<Node>();

		private Dictionary<Node, InvokerClient> _invokerClients = new Dictionary<Node, InvokerClient>();

		/// <summary>
		/// Список результатов вычислений.
		/// </summary>
		private Dictionary<Guid, Node> _resultInfo = new Dictionary<Guid, Node>();

		private static Random rand = new Random(DateTime.Now.Millisecond);

		#endregion

		private Node SelectNode()
		{
			if (!CalculativeServerList.Any())
			{
				throw new Exception("Список доступных вычислительных узлов пуст.");
			}
			int index = rand.Next(0, CalculativeServerList.Count);

			return CalculativeServerList[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoke_packet"></param>
		public void SentToInvoke(InvokePacket invoke_packet)
		{
			InvokerClient ic;
			var node = SelectNode();
			lock (_invokerClients)
			{
				if (!_invokerClients.ContainsKey(node))
				{
					_invokerClients.Add(node, new InvokerClient(node));
				}

				ic = _invokerClients[node];
			}

			foreach (var param in invoke_packet.InputParams)
			{
				lock (_resultInfo)
				{
					if (_resultInfo.ContainsKey(param.Guid))
					{
						param.OwnerNode = _resultInfo[param.Guid];
						continue;
					}
				}

				param.OwnerNode = node;
			}
				
			lock (_resultInfo)
			{
				_resultInfo.Add(invoke_packet.Guid, node);
			}

			//Console.WriteLine("Данные с id {0} будут на сервере {1}:{2}", invoke_packet.Guid, node.IpAddress, node.Port);
			ic.Invoke(invoke_packet);
		}

		/// <summary>
		/// Возвращает результат вычисления с идентификатором<value>guid</value>
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
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
