using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using Core.Model.Server;

namespace Core.Model.RemoteClass
{
	public class RemoteClassBase : MarshalByRefObject, IDisposable
	{
		public static int SCount = 0;
		public int Count = 0;
		protected static Dictionary<Guid, object> _objects = new Dictionary<Guid, object>();
		private static Dictionary<string, RemoteClassBase> StaticConnects = new Dictionary<string, RemoteClassBase>();

		public bool IsStatic = false;

		public Guid Guid { get; set; }
		
		public Guid RegisterNewObject<T>() where T : RemoteClassBase
		{
			var guid = Guid.NewGuid();
			var obj = (T)Activator.CreateInstance(typeof(T));

			obj.Guid = guid;
			_objects.Add(guid, obj);

			RemotingServices.Marshal(obj, guid.ToString());
			return guid;
		}

		public static T StaticConnect<T>(Node node) where T : RemoteClassBase
		{
			var class_name = typeof(T).FullName;
			T obj;
			/*if (node.IpAddress.Equals(ServerBase.GetLocalIpAddress()) && ServerBase.Services.ContainsKey(class_name))
			{
				obj = (T) ServerBase.Services[class_name];
				obj.IsStatic = true;
			}
			else
			{*/
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
			//}
			//obj.Ping();
			return obj;
		}

		public static T Connect<T>(Node node) where T : RemoteClassBase
		{
			var class_name = typeof(T).FullName;

			/*if (node.IpAddress.Equals(ServerBase.GetLocalIpAddress()) && ServerBase.Services.ContainsKey(class_name) && ServerBase.Services[class_name].Port == node.Port)
			{
				return (T)ServerBase.Services[class_name];
			}*/
			
			var o = StaticConnect<RemoteClassBase>(node);//(RemoteClassBase)RemotingServices.Connect(typeof(RemoteClassBase), String.Format("tcp://{0}:{1}/{2}", node.IpAddress, node.Port, typeof(RemoteClassBase).FullName));
			var guid = o.RegisterNewObject<T>();

			var obj = (T)RemotingServices.Connect(typeof(T), String.Format("tcp://{0}:{1}/{2}", node.IpAddress, node.Port, guid));
			return obj;
		}

		public static T Connect<T>(string ip_address, int port) where T : RemoteClassBase
		{
			return Connect<T>(new Node()
			{
				IpAddress = ip_address,
				Port = port
			});
		}

		public bool Ping()
		{
			Console.WriteLine("Пинг");
			return true;
		}

		public void Dispose()
		{
			if (!IsStatic)
			{
				_objects.Remove(Guid);
				RemotingServices.Disconnect(this);
			}
		}

		~RemoteClassBase()
		{
			Dispose();
		}
	}

}
