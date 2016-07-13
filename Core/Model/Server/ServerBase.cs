using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using Core.Model.RemoteClass;

namespace Core.Model.Server
{
	/// <summary>
	/// Базовый класс для сервера.
	/// </summary>
	public class ServerBase : IDisposable
	{
		#region Constants

		/// <summary>
		/// IP-адресс для отправки сообщений по протоколу UDP. 
		/// </summary>
		protected const string BROADCAST_ADDRESS = "224.0.0.0";

		/// <summary>
		/// Порт для приема сообщений по протоколу UDP.
		/// </summary>
		protected const int BROADCAST_PORT = 2500;

		/// <summary>
		/// Минимальное значение порта.
		/// </summary>
		protected const int PORT_MIN = 30000;

		/// <summary>
		/// Максимальное значение порта.
		/// </summary>
		protected const int PORT_MAX = 40000;

		#endregion

		#region Fields

		/// <summary>
		/// Случайное значения порта.
		/// </summary>
		protected static Random Random = new Random(DateTime.Now.Millisecond);
		
		/// <summary>
		/// Используемый порт.
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Все созданные службы.
		/// </summary>
		public static Dictionary<string, RemoteClassBase> Services = new Dictionary<string, RemoteClassBase>();

		/// <summary>
		/// Все созданные каналы связи.
		/// </summary>
		private static Dictionary<int, TcpServerChannel> _tcpServerChannels = new Dictionary<int, TcpServerChannel>();

		/// <summary>
		/// Порт для отправки сообщений по протоколу UDP.
		/// </summary>
		public static int UdpPingPort = GetRandomPort();

		/// <summary>
		/// Канал для удаленного взаимодействия.
		/// </summary>
		private TcpServerChannel _channel;

		/// <summary>
		/// Узел сервера.
		/// </summary>
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

		#endregion

		#region Constructor

		/// <summary>
		/// Создает сервер. Устанавливает порт.
		/// </summary>
		/// <param name="port">Порт.</param>
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

		#endregion

		#region Methods

		/// <summary>
		/// Добавляет службу для удаленного взаимодействия типа.
		/// </summary>
		/// <param name="remote_class_type">Тип для удаленого взаимодействия.</param>
		/// <returns>Экземпляр класса для удаленного взаимодействия.</returns>
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

		/// <summary>
		/// Добавляет службу для удаленного взаимодействия типа.
		/// </summary>
		/// <typeparam name="T">Тип для удаленого взаимодействия.</typeparam>
		/// <returns>Экземпляр класса для удаленного взаимодействия.</returns>
		public T AddRemoteClassService<T>() where T : RemoteClassBase
		{
			return (T)AddRemoteClassService(typeof (T));
		}

		/// <summary>
		/// Отправляет уведомление о собственном присутствии для других серверов.
		/// </summary>
		public void UdpNotification()
		{
			var message = Encoding.ASCII.GetBytes(string.Format("{0}:{1}:{2}", GetType().Name, Node.IpAddress, Node.Port));
			SendUdpMessage(message);
		}

		/// <summary>
		/// Закрывает открытые каналы связи.
		/// </summary>
		public void Dispose()
		{
			RemotingServices.Disconnect(Services[GetType().FullName]);
		}

		#endregion

		#region Methods / Static

		/// <summary>
		/// Отправляет сообщение по протоколу UDP.
		/// </summary>
		/// <param name="message"></param>
		public static void SendUdpMessage(byte[] message)
		{
			try
			{
				var sender = new UdpClient(GetRandomPort());
				sender.Connect(BROADCAST_ADDRESS, BROADCAST_PORT);

				sender.Send(message, message.Length);
				sender.Close();
			}
			catch (SocketException e)
			{
				if (e.ErrorCode == 10048)
				{
					Console.WriteLine("UdpPing: Порт {0} занят.", UdpPingPort);
					UdpPingPort = GetRandomPort();
				}
				else
				{
					Console.WriteLine("UdpPing: На порту {0} возникло исключение: {1}", UdpPingPort, e.Message);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("UdpPing: На порту {0} возникло исключение: {1}", UdpPingPort, e.Message);
			}
		}

		/// <summary>
		/// Получает локальный IP-адрес.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Получает произвольный порт, предварительно проверив его доступность.
		/// </summary>
		/// <returns>Порт.</returns>
		public static int GetRandomPort()
		{
			// Получаем произвольный порт из диапазона.
			for (var i = 0; i < (PORT_MAX - PORT_MIN) * 0.1; i++)
			{
				var port = Random.Next(PORT_MIN, PORT_MAX);
			
				try
				{
					Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					Socket.Bind(new IPEndPoint(IPAddress.Any, port));
					Socket.Close();
					return port;
				}
				catch (SocketException e)
				{
					if (e.ErrorCode == 10048)
					{
						Console.WriteLine("Порт {0} занят.", port);
					}
					else
					{
						Console.WriteLine("На порту {0} возникло исключение: {1}", port, e.Message);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("На порту {0} возникло исключение: {1}", port, e.Message);
				}
			}

			for (var i = PORT_MIN; i < PORT_MAX; i++)
			{
				var port = i;
			
				try
				{
					Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					Socket.Bind(new IPEndPoint(IPAddress.Any, port));
					Socket.Close();
					return port;
				}
				catch (SocketException e)
				{
					if (e.ErrorCode == 10048)
					{
						Console.WriteLine("Порт {0} занят.", port);
					}
					else
					{
						Console.WriteLine("На порту {0} возникло исключение: {1}", port, e.Message);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("На порту {0} возникло исключение: {1}", port, e.Message);
				}
			}
			throw new Exception(string.Format("Все порты в диапазоне от {0} до {1} заняты.", PORT_MIN, PORT_MAX));
		}

		#endregion
	}
}
