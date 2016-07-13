using System;
using Core.Model.Client;
using Core.Model.Server;

namespace Core.Model.DataPacket
{
	/// <summary>
	/// Клас осуществляющий получение результата из удаленного источника.
	/// </summary>
	[Serializable]
	public class DataValue : Data
	{
		#region Fields

		/// <summary>
		/// Вычислительный узел с результатами вычисления.
		/// </summary>
		public Node OwnerNode { get; set; }

		/// <summary>
		/// Узел отправителя.
		/// </summary>
		public Node SenderNode { get; set; }

		/// <summary>
		/// Значение.
		/// </summary>
		public new object Value
		{
			get
			{
				if (HasValue)
				{
					return _value;
				}

				// Известен конечный владелец данных.
				if (OwnerNode != null)
				{
					using (var ic = new InvokerClient(OwnerNode))
					{
						Console.WriteLine("Ожидаются данные с id {0} с сервера {1}:{2}", Guid, OwnerNode.IpAddress, OwnerNode.Port);
						_value = ic.GetData(Guid).Value;
						HasValue = true;
						return _value;
					}
				}

				// Необходимо узнать конечного владельца данных .
				using (var cc = new CoordinationClient(SenderNode))
				{
					var data = cc.GetData(Guid);

					if (data is DataInfo)
					{
						var data_info = (DataInfo)data;

						using (var ic = new InvokerClient(data_info.OwnerNode))
						{
							Console.WriteLine("По данным от координатора. Ожидаются данные с id {0} с сервера {1}:{2}", Guid, data_info.OwnerNode.IpAddress, data_info.OwnerNode.Port);
							_value = ic.GetData(data_info.Guid).Value;
							HasValue = true;
							return _value;
						}
					}
				}

				throw new Exception("Непредвиденная ошибка при получении данных.");
			}
			set
			{
				HasValue = true;
				_value = value;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Конструктор по умолчанию.
		/// Устанавливается новый уникальный идентификатор без значения.
		/// </summary>
		public DataValue()
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		/// <summary>
		/// Устанавливается только уникальный идентификатор.
		/// </summary>
		/// <param name="guid">Уникальный идентификатор.</param>
		public DataValue(Guid guid)
			: base(guid)
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		/// <summary>
		/// Устанавливается новый уникальный идентификатор и значение.
		/// </summary>
		/// <param name="value">Значение.</param>
		public DataValue(object value)
			: base(value)
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		/// <summary>
		/// Устанавливается уникальный идентификатор и значение.
		/// </summary>
		/// <param name="guid">Уникальный идентиифкатор.</param>
		/// <param name="value">Значение.</param>
		public DataValue(Guid guid, object value)
			: base(guid, value)
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		#endregion

		#region Implicit

		public static implicit operator DataValue(int obj)
		{
			return new DataValue(obj);
		}

		public static implicit operator int(DataValue obj)
		{
			return (int)obj.Value;
		}

		#endregion
	}
	
	/// <summary>
	/// Клас осуществляющий получение результата из удаленного источника.
	/// Типизированный.
	/// </summary>
	/// <typeparam name="T">Тип.</typeparam>
	[Serializable]
	public class DataValue<T> : DataValue
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

		/// <summary>
		/// Конструктор по умолчанию.
		/// Устанавливается новый уникальный идентификатор без значения.
		/// </summary>
		public DataValue()
		{

		}

		/// <summary>
		/// Устанавливается только уникальный идентификатор.
		/// </summary>
		/// <param name="guid">Уникальный идентификатор.</param>
		public DataValue(Guid guid)
			: base(guid)
		{

		}

		/// <summary>
		/// Устанавливается новый уникальный идентификатор и значение.
		/// </summary>
		/// <param name="value">Значение.</param>
		public DataValue(T value)
			: base(value)
		{

		}

		/// <summary>
		/// Устанавливается уникальный идентификатор и значение.
		/// </summary>
		/// <param name="guid">Уникальный идентиифкатор.</param>
		/// <param name="value">Значение.</param>
		public DataValue(Guid guid, T value)
			: base(guid, value)
		{

		}

		#endregion

		#region Implicit

		public static implicit operator T(DataValue<T> obj)
		{
			return obj.Value;
		}

		#endregion
	}
}
