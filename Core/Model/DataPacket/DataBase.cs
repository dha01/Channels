using System;

namespace Core.Model.DataPacket
{
	/// <summary>
	/// Базовый класс для объекта с данными.
	/// </summary>
	[Serializable]
	public class DataBase
	{
		#region Fields

		/// <summary>
		/// Уникальный идентификатор.
		/// </summary>
		public Guid Guid { get; set; }

		/// <summary>
		/// Содержит значение.
		/// </summary>
		public bool HasValue { get; set; }

		/// <summary>
		/// Значение.
		/// </summary>
		protected object _value;

		/// <summary>
		/// Значение.
		/// </summary>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
				HasValue = true;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Устанавливается новый уникальный идентификатор без значения.
		/// </summary>
		public DataBase()
			: this(Guid.NewGuid())
		{
		}

		/// <summary>
		/// Устанавливается только уникальный идентификатор.
		/// </summary>
		/// <param name="guid">Уникальный идентификатор.</param>
		public DataBase(Guid guid)
		{
			Guid = guid;
		}

		/// <summary>
		/// Устанавливается новый уникальный идентификатор и значение.
		/// </summary>
		/// <param name="value">Значение.</param>
		public DataBase(object value)
			: this(Guid.NewGuid(), value)
		{

		}

		/// <summary>
		/// Устанавливается уникальный идентификатор и значение.
		/// </summary>
		/// <param name="guid">Уникальный идентиифкатор.</param>
		/// <param name="value">Значение.</param>
		public DataBase(Guid guid, object value)
		{
			Guid = guid;
			Value = value;
		}

		#endregion
	}
}
