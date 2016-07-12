using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Model.Client;
using Core.Model.Server;

namespace Core.Model.DataPacket
{
	[Serializable]
	public class DataBase
	{
		public Guid Guid { get; set; }

		protected object _value;
		public bool HasValue { get; set; }

		public DataBase(Guid guid, object value)
		{
			Guid = guid;
			_value = value;
			HasValue = true;
		}

		public DataBase() : this(Guid.NewGuid())
		{
		}

		public DataBase(Guid guid)
		{
			Guid = guid;
		}
	}

	/// <summary>
	/// Содержит информацио об узле с требуемыми данными.
	/// </summary>
	[Serializable]
	public class DataInfo : DataBase
	{
		public Node OwnerNode { get; set; }

		public DataInfo(Guid guid, Node owner_node) 
			: base(guid)
		{
			OwnerNode = owner_node;
		}
	}
	
	/// <summary>
	/// Базовй клас для хранения данных.
	/// Содержит только непосредственно значение.
	/// </summary>
	[Serializable]
	public class Data : DataBase
	{
		public virtual object Value 
		{
			get { return _value; }
			set
			{
				HasValue = true;
				_value = value;
			}
		}

		public Data(Guid guid, object value)
			: base(guid, value)
		{

		}

		public Data(Guid guid)
			: base(guid)
		{

		}

		public Data(object value)
		{
			HasValue = true;
			_value = value;
		}

		public static implicit operator Data(int obj)
		{
			return new DataValue(obj);
		}

		public static implicit operator int(Data obj)
		{
			return (int)obj.Value;
		}
	}

	/// <summary>
	/// Базовй клас для хранения данных.
	/// Содержит только непосредственно значение.
	/// Типизированный.
	/// </summary>
	[Serializable]
	public class Data<T> : DataBase
	{
		public virtual T Value 
		{
			get { return (T)_value; }
			set
			{
				HasValue = true;
				_value = value;
			}
		}

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
		{
			HasValue = true;
			_value = value;
		}

		public static implicit operator Data<T>(T obj)
		{
			return new Data<T>(obj);
		}

		public static implicit operator Data(Data<T> obj)
		{
			return new Data(obj.Value);
		}

		public static implicit operator Data<T>(Data obj)
		{
			return new Data<T>((T)obj.Value);
		}
	}

	/// <summary>
	/// Клас осуществляющий получение результата из удаленного источника.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class DataValue<T> : Data<T>
	{
		public Node OwnerNode { get; set; }

		public Node SenderNode { get; set; }
		
		public DataValue(Guid guid)
			: base(guid)
		{

		}

		public DataValue(Guid guid, T value)
			: base(guid, value)
		{

		}

		public DataValue(T value)
			: base(value)
		{

		}

		public override T Value
		{
			get
			{
				if (HasValue)
				{
					return (T)_value;
				}

				// Известен конечный владелец данных.
				if (OwnerNode != null)
				{
					using (var ic = new InvokerClient(OwnerNode))
					{
						Console.WriteLine("Ожидаются данные с id {0} с сервера {1}:{2}", Guid, OwnerNode.IpAddress, OwnerNode.Port);
						_value = (T)ic.GetData(Guid).Value;
						HasValue = true;
						return (T)_value;
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
							_value = (T)ic.GetData(data_info.Guid).Value;
							HasValue = true;
							return (T)_value;
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

		public static implicit operator Data(DataValue<T> obj)
		{
			if (obj.HasValue)
			{
				return new Data(obj.Guid, obj.Value);
			}

			return new Data(obj.Guid);
		}

		public static implicit operator DataValue(DataValue<T> obj)
		{
			if (obj.HasValue)
			{
				return new DataValue(obj.Guid, obj.Value);
			}

			return new DataValue(obj.Guid);
		}
		/*
		public static implicit operator DataValue<T>(DataValue<T> obj)
		{
			return new DataValue<T>(obj);
		}

		public static implicit operator DataValue<T>(T obj)
		{
			return new DataValue<T>(obj);
		}*/

		public static implicit operator T(DataValue<T> obj)
		{
			return obj.Value;
		}
	}

	[Serializable]
	public class DataValue : Data
	{
		/// <summary>
		/// Вычислительный узел с результатами вычисления.
		/// </summary>
		public Node OwnerNode { get; set; }

		/// <summary>
		/// Узел отправителя.
		/// </summary>
		public Node SenderNode { get; set; }
		
		public DataValue(Guid guid)
			: base(guid)
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		public DataValue(Guid guid, object value)
			: base(guid, value)
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		public DataValue(object value)
			: base(value)
		{
			SenderNode = new Node
			{
				IpAddress = ServerBase.GetLocalIpAddress(),
				Port = CoordinationServer.DefaultPort
			};
		}

		public override object Value
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

		public static implicit operator DataValue(int obj)
		{
			return new DataValue(obj);
		}

		public static implicit operator int(DataValue obj)
		{
			return (int)obj.Value;
		}
	}
}
