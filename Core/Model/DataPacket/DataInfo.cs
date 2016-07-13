using System;

namespace Core.Model.DataPacket
{
	/// <summary>
	/// Содержит информацио об узле с требуемыми данными.
	/// </summary>
	[Serializable]
	public class DataInfo : DataBase
	{
		#region Fields

		/// <summary>
		/// Узел владеющий данными.
		/// </summary>
		public Node OwnerNode { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Инициализирует по уникальному идентификатору и узлу, владеющему данными.
		/// </summary>
		/// <param name="guid">Уникальный идентификатор.</param>
		/// <param name="owner_node">Узел владеющий данными.</param>
		public DataInfo(Guid guid, Node owner_node)
			: base(guid)
		{
			OwnerNode = owner_node;
		}

		#endregion
	}
}
