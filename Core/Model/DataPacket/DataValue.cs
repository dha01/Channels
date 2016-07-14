using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Model.Client;
using Core.Model.Server;

namespace Core.Model.DataPacket
{
	[Flags]
	public enum DataState
	{
		Null			= 0x000000,
		NotReduce		= 0x000001,
		Debug			= 0x000010,
		Save			= 0x000100
	}
	
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
		public Node RootCoordinationNode { get; set; }
		
		/// <summary>
		/// Вычислительный узел с результатами вычисления.
		/// </summary>
		public Node OwnerNode { get; set; }

		/// <summary>
		/// Узел отправителя.
		/// </summary>
		public Node SenderNode { get; set; }

		public object GetValue()
		{
			return GetValue(DataState.Null);
		}

		public object GetValue(DataState state)
		{
			if (HasValue)
			{
				return _value;
			}
			/*
			// Известен конечный владелец данных.
			if (OwnerNode != null)
			{
				using (var ic = new InvokerClient(RootCoordinationNode))
				{
					Console.WriteLine("Ожидаются данные с id {0} с сервера {1}:{2}", Guid, RootCoordinationNode.IpAddress, RootCoordinationNode.Port);
					_value = ic.GetData(Guid).Value;
					HasValue = true;
					return _value;
				}
			}*/

			// Необходимо узнать конечного владельца данных .
			using (var cc = new CoordinationClient(SenderNode))
			{
				_value = cc.GetData(Guid).Value;
				return _value;
				/*if (data is DataInfo)
				{
					var data_info = (DataInfo)data;

					using (var ic = new InvokerClient(data_info.OwnerNode))
					{
						//Console.WriteLine("По данным от координатора. Ожидаются данные с id {0} с сервера {1}:{2}", Guid, data_info.OwnerNode.IpAddress, data_info.OwnerNode.Port);
						_value = ic.GetData(data_info.Guid).Value;
						HasValue = true;
						return _value;
					}
				}*/
			}

			throw new Exception("Непредвиденная ошибка при получении данных.");
		}

		/// <summary>
		/// Значение.
		/// </summary>
		public new object Value
		{
			get { return GetValue(); }
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

		public static implicit operator DataValue(int[] obj)
		{
			return new DataValue(obj);
		}

		public static implicit operator int[](DataValue obj)
		{
			return (int[])obj.Value;
		}

		public static implicit operator DataValue(int[][] obj)
		{
			return new DataValue(obj);
		}

		public static implicit operator DataValue(DataValue[][] obj)
		{
			return new DataValue(ToArray<int>(obj));
		}

		public static implicit operator int[][](DataValue obj)
		{
			return (int[][])obj.Value;
		}

		/*public static implicit operator DataValue(DataValue[][] obj)
		{
			int size_x = obj.Length;
			int size_y = 0;

			if (size_x > 0)
			{
				size_y = obj[0].Length;
			}

			List<List<object>> result = new List<List<object>>(size_x);
			
			Parallel.For(0, size_x, (i) =>
			{
				result[i] = new List<object>(size_y);
				Parallel.For(0, size_y, (j) =>
				{
					result[i][j] = obj[i][j].Value;
				});
			});

			object[][] resul_o = new object[size_x][];

			for (var i = 0; i < size_x; i++)
			{
				resul_o[i] = new object[size_y];
				for (var j = 0; j < size_y; j++)
				{
					resul_o[i][j] = result[i][j];
				}
			}

			return new DataValue(resul_o);
		}*/
		#endregion

		public static T[][] ToArray<T>(DataValue[][] obj)
		{
			int size_x = obj.Length;
			int size_y = 0;

			if (size_x > 0)
			{
				size_y = obj[0].Length;
			}

			T[][] result = new T[size_x][];

			Parallel.For(0, size_x, (int i) =>
			{
				result[i] = new T[size_y];
				Parallel.For(0, size_y, (j) =>
				{
					Data o;
					lock (obj)
					{
						o = obj[i][j];
					}
					var val = (T)((DataValue)o).Value;
					Console.WriteLine("{0} {1} {2}", i, j, size_x * i + j);
					lock (result)
					{
						result[i][j] = val;
					}
				});
			});

			return result;
		}
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
			get
			{
				return (T)base.Value;

				/*if (HasValue)
				{
					return (T)_value;
				}

				// Известен конечный владелец данных.
				if (RootCoordinationNode != null)
				{
					using (var ic = new InvokerClient(RootCoordinationNode))
					{
						Console.WriteLine("Ожидаются данные с id {0} с сервера {1}:{2}", Guid, RootCoordinationNode.IpAddress, RootCoordinationNode.Port);
						_value = ic.GetData(Guid).Value;
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
							//Console.WriteLine("По данным от координатора. Ожидаются данные с id {0} с сервера {1}:{2}", Guid, data_info.OwnerNode.IpAddress, data_info.OwnerNode.Port);
							_value = ic.GetData(data_info.Guid).Value;
							HasValue = true;
							return (T)_value;
						}
					}
				}

				throw new Exception("Непредвиденная ошибка при получении данных.");*/
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

		public static implicit operator DataValue<T>(T obj)
		{
			return new DataValue<T>(obj);
		}

		public static implicit operator T(DataValue<T> obj)
		{
			return obj.Value;
		}

		#endregion
	}
}
