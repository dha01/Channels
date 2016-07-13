using System;

namespace Core.Model.DataPacket
{
	/// <summary>
	/// Базовй клас для хранения данных.
	/// Содержит только непосредственно значение.
	/// </summary>
	[Serializable]
	public class Data : DataBase
	{
		#region Fields

		/// <summary>
		/// Значение.
		/// </summary>
		public new object Value 
		{
			get { return base.Value; }
			set { base.Value = value; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Конструктор по умолчанию.
		/// Устанавливается новый уникальный идентификатор без значения.
		/// </summary>
		public Data()
		{
			
		}

		/// <summary>
		/// Устанавливается только уникальный идентификатор.
		/// </summary>
		/// <param name="guid">Уникальный идентификатор.</param>
		public Data(Guid guid)
			: base(guid)
		{

		}

		/// <summary>
		/// Устанавливается новый уникальный идентификатор и значение.
		/// </summary>
		/// <param name="value">Значение.</param>
		public Data(object value)
			: base(value)
		{
			HasValue = true;
		}

		/// <summary>
		/// Устанавливается уникальный идентификатор и значение.
		/// </summary>
		/// <param name="guid">Уникальный идентиифкатор.</param>
		/// <param name="value">Значение.</param>
		public Data(Guid guid, object value)
			: base(guid, value)
		{
			HasValue = true;
		}

		#endregion

		#region Implicit

		public static implicit operator Data(int obj)
		{
			return new DataValue(obj);
		}

		public static implicit operator int(Data obj)
		{
			return (int)obj.Value;
		}

		#endregion
	}

	/// <summary>
	/// Базовй клас для хранения данных.
	/// Содержит только непосредственно значение.
	/// Типизированный.
	/// </summary>
	[Serializable]
	public class Data<T> : Data
	{
		#region Fields

		/// <summary>
		/// Значение.
		/// </summary>
		public new T Value 
		{
			get { return (T)base.Value; }
			set { base.Value = value; }
		}

		#endregion

		#region Constructor

		public Data(Guid guid, object value)
			: base(guid, value)
		{

		}

		public Data()
		{
		}

		public Data(Guid guid)
			: base(guid)
		{
			
		}

		public Data(T value)
			: base(value)
		{

		}

		#endregion

		#region Implicit

		public static implicit operator Data<T>(T obj)
		{
			return new Data<T>(obj);
		}

		#endregion
	}
}
