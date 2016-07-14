using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace Core.Model.RemoteClass
{
	/// <summary>
	/// Базовый класс для работы с удаленными объектами.
	/// </summary>
	public class RemoteClassBase : MarshalByRefObject, IDisposable
	{
		#region Fields

		/// <summary>
		/// Список динамических объектов на сервере.
		/// </summary>
		public static Dictionary<Guid, object> _objects = new Dictionary<Guid, object>();

		/// <summary>
		/// Список статических объектов на сервере.
		/// </summary>
		public static readonly Dictionary<string, RemoteClassBase> StaticConnects = new Dictionary<string, RemoteClassBase>();

		/// <summary>
		/// Является ли объект статическим.
		/// </summary>
		public bool IsStatic = false;

		/// <summary>
		/// Уникальный идентификатор.
		/// </summary>
		public Guid Guid { get; set; }

		#endregion

		#region NewObjects

		/// <summary>
		/// Регистрирует новый удаленный объект со стороны сервера, а клиенту возвращает его уникальный идентификатор для подключения.
		/// </summary>
		/// <typeparam name="T">Тип удаленного объекта.</typeparam>
		/// <returns>Уникальный идентификатор для подключения.</returns>
		public Guid RegisterNewObject<T>() where T : RemoteClassBase
		{
			var guid = Guid.NewGuid();
			var obj = (T)Activator.CreateInstance(typeof(T));

			obj.Guid = guid;
			_objects.Add(guid, obj);

			RemotingServices.Marshal(obj, guid.ToString());
			return guid;
		}

		/// <summary>
		/// Регистрирует статический объект со стороны сервера, а клиенту возвращает уже готовый к использованию объект,
		/// для которого уже установлено соединение с сервером. 
		/// </summary>
		/// <typeparam name="T">Тип.</typeparam>
		/// <param name="node">Узел сервера.</param>
		/// <returns>Подключенный удаленный объект.</returns>
		public static T StaticConnect<T>(Node node) where T : RemoteClassBase
		{
			T obj;
			var str = String.Format("tcp://{0}:{1}/{2}", node.IpAddress, node.Port, typeof (T).FullName);
			lock (StaticConnects)
			{
				if (StaticConnects.ContainsKey(str))
				{
					obj = (T)StaticConnects[str];
				}
				else
				{
					obj = (T)RemotingServices.Connect(typeof(T), str);
					obj.IsStatic = true;
					StaticConnects.Add(str, obj);
				}
			}
			return obj;
		}

		public static void Disconnect<T>(Node node)
		{
			var str = String.Format("tcp://{0}:{1}/{2}", node.IpAddress, node.Port, typeof(T).FullName);
			lock (StaticConnects)
			{
				if (StaticConnects.ContainsKey(str))
				{
					StaticConnects.Remove(str);
				}
			}
		}

		/// <summary>
		/// Регистрирует динамический объект со стороны сервера, а клиенту возвращает уже готовый к использованию объект,
		/// для которого уже установлено соединение с сервером. 
		/// </summary>
		/// <typeparam name="T">Тип.</typeparam>
		/// <param name="node">Узел сервера.</param>
		/// <returns>Подключенный удаленный объект.</returns>
		public static T Connect<T>(Node node) where T : RemoteClassBase
		{
			var o = StaticConnect<RemoteClassBase>(node);
			var guid = o.RegisterNewObject<T>();

			var obj = (T)RemotingServices.Connect(typeof(T), string.Format("tcp://{0}:{1}/{2}", node.IpAddress, node.Port, guid));
			return obj;
		}

		/// <summary>
		/// Регистрирует динамический объект со стороны сервера, а клиенту возвращает уже готовый к использованию объект,
		/// для которого уже установлено соединение с сервером. 
		/// </summary>
		/// <typeparam name="T">Тип.</typeparam>
		/// <param name="ip_address">IP-адрес.</param>
		/// <param name="port">Порт.</param>
		/// <returns>Подключенный удаленный объект.</returns>
		public static T Connect<T>(string ip_address, int port) where T : RemoteClassBase
		{
			return Connect<T>(new Node()
			{
				IpAddress = ip_address,
				Port = port
			});
		}

		#endregion

		#region Methods

		/// <summary>
		/// Пинг.
		/// </summary>
		/// <returns>Успех.</returns>
		public bool Ping()
		{
			Console.WriteLine("Пинг");
			return true;
		}

		/// <summary>
		/// Закрывает соединение с объектом на сервере.
		/// </summary>
		public void Dispose()
		{
		/*	if (!IsStatic)
			{
				_objects.Remove(Guid);
				RemotingServices.Disconnect(this);
			}*/
		}

		/// <summary>
		/// Закрывает соединение с объектом на сервере.
		/// </summary>
		~RemoteClassBase()
		{
			Dispose();
		}

		#endregion
	}

}
