using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public bool IsEndOwner { get; set; }
		public Node OwnerNode { get; set; }
		
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
				if (IsEndOwner)
				{
					if (OwnerNode == null)
					{
						OwnerNode = new Node
						{
							IpAddress = ServerBase.GetLocalIpAddress(),
							Port = InvokerServer.DefaultPort
						};
					}

					using (var ic = new InvokerClient(OwnerNode))
					{
						return (T)ic.GetData(Guid).Value;
					}
				}

				if (OwnerNode == null)
				{
					OwnerNode = new Node
					{
						IpAddress = ServerBase.GetLocalIpAddress(),
						Port = CoordinationServer.DefaultPort
					};
				}

				// Необходимо узнать конечного владельца данных .
				using (var cc = new CoordinationClient(OwnerNode))
				{
					var data = cc.GetData(Guid);

					if (data is DataInfo)
					{
						var data_info = (DataInfo)data;

						using (var ic = new InvokerClient(data_info.OwnerNode))
						{
							return (T)ic.GetData(data_info.Guid).Value;
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

		public static implicit operator DataValue<T>(T obj)
		{
			return new DataValue<T>(obj);
		}
	}

	[Serializable]
	public class DataValue : Data
	{
		public bool IsEndOwner { get; set; }
		public Node OwnerNode { get; set; }
		
		public DataValue(Guid guid)
			: base(guid)
		{

		}

		public DataValue(Guid guid, object value)
			: base(guid, value)
		{

		}

		public DataValue(object value)
			: base(value)
		{

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
				if (IsEndOwner)
				{
					if (OwnerNode == null)
					{
						OwnerNode = new Node
						{
							IpAddress = ServerBase.GetLocalIpAddress(),
							Port = InvokerServer.DefaultPort
						};
					}

					using (var ic = new InvokerClient(OwnerNode))
					{
						return ic.GetData(Guid).Value;
					}
				}

				if (OwnerNode == null)
				{
					OwnerNode = new Node
					{
						IpAddress = ServerBase.GetLocalIpAddress(),
						Port = CoordinationServer.DefaultPort
					};
				}

				// Необходимо узнать конечного владельца данных .
				using (var cc = new CoordinationClient(OwnerNode))
				{
					var data = cc.GetData(Guid);

					if (data is DataInfo)
					{
						var data_info = (DataInfo)data;

						using (var ic = new InvokerClient(data_info.OwnerNode))
						{
							return ic.GetData(data_info.Guid).Value;
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
	}
}
