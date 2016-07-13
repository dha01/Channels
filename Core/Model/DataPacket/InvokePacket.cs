using System;

namespace Core.Model.DataPacket
{
	/// <summary>
	/// Пакет для исполнения.
	/// </summary>
	[Serializable]
	public class InvokePacket
	{
		#region Fields

		/// <summary>
		/// Уникаольный идентификатор.
		/// </summary>
		public Guid Guid { get; set; }

		/// <summary>
		/// Метод для исполнения.
		/// </summary>
		public InvokeMethod InvokeMethod { get; set; }

		/// <summary>
		/// Входные параметры.
		/// </summary>
		public DataValue[] InputParams { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Присваивает уникальный идентификатор.
		/// </summary>
		public InvokePacket()
		{
			Guid = Guid.NewGuid();
		}

		#endregion
	}
}
