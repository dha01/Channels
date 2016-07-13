using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Model.Client;
using Core.Model.DataPacket;

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

		/// <summary>
		/// Клиенты для вычислительных узлов.
		/// </summary>
		private readonly ConcurrentDictionary<Node, InvokerClient> _invokerClients = new ConcurrentDictionary<Node, InvokerClient>();

		/// <summary>
		/// Список результатов вычислений.
		/// </summary>
		private readonly ConcurrentDictionary<Guid, Node> _resultInfo = new ConcurrentDictionary<Guid, Node>();

		/// <summary>
		/// Для выбра произвольного узла.
		/// </summary>
		private static readonly Random rand = new Random(DateTime.Now.Millisecond);

		#endregion

		#region Methods

		/// <summary>
		/// Выбирает узел для исполнения.
		/// TODO: нужен будет механиз для выбора подходящего сервера, а не произвольного.
		/// </summary>
		/// <returns>Узел.</returns>
		private Node SelectNode()
		{
			if (!CalculativeServerList.Any())
			{
				throw new Exception("Список доступных вычислительных узлов пуст.");
			}
			var index = rand.Next(0, CalculativeServerList.Count);

			return CalculativeServerList[index];
		}

		/// <summary>
		/// Подготавливает и отправляет пакет на исполнение на вычислительный узел.
		/// </summary>
		/// <param name="invoke_packet">Исполняемый пакет.</param>
		public void SentToInvoke(InvokePacket invoke_packet)
		{
			var node = SelectNode();
			
			if (!_invokerClients.ContainsKey(node))
			{
				_invokerClients.TryAdd(node, new InvokerClient(node));
			}

			var ic = _invokerClients[node];

			// Добавляет информацио о владельце данных, если он известен.
			foreach (var param in invoke_packet.InputParams)
			{
				if (_resultInfo.ContainsKey(param.Guid))
				{
					param.OwnerNode = _resultInfo[param.Guid];
					continue;
				}

				param.OwnerNode = node;
			}

			_resultInfo.TryAdd(invoke_packet.Guid, node);
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
				
				if (_resultInfo.ContainsKey(guid))
				{
					return new DataInfo(guid, _resultInfo[guid]);
				}
				Thread.Sleep(500);
				count++;
			}

			throw new Exception(string.Format("Данных с индификатором {0} не обнаружено.", guid));
		}

		#endregion
	}
}
