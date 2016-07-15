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

		public Action<InvokePacket> OnEnqueuePacket;
		public Func<Guid, DataBase> OnGetData;
		public Action<Guid> OnRemoveDataInfo;
		public List<Node> CalculativeServerList;

		#endregion

		#region Methods

		/// <summary>
		/// Подготавливает и отправляет пакет на исполнение на вычислительный узел.
		/// </summary>
		/// <param name="invoke_packet">Исполняемый пакет.</param>
		public void SentToInvoke(InvokePacket invoke_packet)
		{
			OnEnqueuePacket.Invoke(invoke_packet);
		}

		/// <summary>
		/// Возвращает результат вычисления с идентификатором<value>guid</value>
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public DataBase GetData(Guid guid)
		{
			return OnGetData.Invoke(guid);
		}

		public void RemoveDataInfo(Guid guid)
		{
			OnRemoveDataInfo.Invoke(guid);
		}

		#endregion
	}
}
