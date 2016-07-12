using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using Core.Model.RemoteClass;

namespace Core.Model.Server
{
	public class ServerBase : IDisposable
	{
		protected const string BROADCAST_ADDRESS = "224.0.0.0";
		protected const int BROADCAST_PORT = 2500;
		
		public int Port { get; set; }
		
		public static Dictionary<string, RemoteClassBase> Services = new Dictionary<string, RemoteClassBase>();
		
		
		private TcpServerChannel _channel;

		private static Dictionary<int, TcpServerChannel> _tcpServerChannels = new Dictionary<int, TcpServerChannel>();

		public ServerBase(int port)
		{
			try
			{
				if (!_tcpServerChannels.ContainsKey(port))
				{
					var channel = new TcpServerChannel("My", port);
					_tcpServerChannels.Add(port, channel);
					ChannelServices.RegisterChannel(channel);
					AddRemoteClassService(typeof(RemoteClassBase));
				}
				Port = port;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		public RemoteClassBase AddRemoteClassService(Type remote_class_type)
		{
			var class_name = remote_class_type.FullName;

			if (Services.ContainsKey(class_name))
			{
				throw new Exception(string.Format("Сервис для класса {0} уже добавлен.", class_name));
			}

			RemoteClassBase obj = (RemoteClassBase)Activator.CreateInstance(remote_class_type);
			obj.Guid = new Guid();
			Services.Add(class_name, obj);
			RemotingServices.Marshal(obj, class_name);

			return obj;
		}

		public T AddRemoteClassService<T>() where T : RemoteClassBase
		{
			return (T)AddRemoteClassService(typeof (T));
		}
		
		
		public void Dispose()
		{
			RemotingServices.Disconnect(Services[GetType().FullName]);
			//ChannelServices.UnregisterChannel(_channel);
		}

		public Node Node
		{
			get
			{
				return new Node
				{
					IpAddress = GetLocalIpAddress(),
					Port = Port
				};
			}
		}

		public void UdpPing()
		{
			try
			{
				var sender = new UdpClient(1234);
				sender.Connect(BROADCAST_ADDRESS, BROADCAST_PORT);

				var local_node = Node;
				var message = Encoding.ASCII.GetBytes(string.Format("{0}:{1}:{2}", GetType().Name, local_node.IpAddress, local_node.Port));
				sender.Send(message, message.Length);
				sender.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		public static string GetLocalIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("Local IP Address Not Found!");
		}
	}
}
