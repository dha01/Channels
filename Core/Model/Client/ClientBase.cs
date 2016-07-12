using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Core.Model.RemoteClass;
using Core.Model.Server;

namespace Core.Model.Client
{
	public class ClientBase : IDisposable
	{
		private RemoteClassBase _remoteClassBase;

		public Node Node { get; set; }

		public ClientBase()
		{

		}

		public ClientBase(Node node)
		{
			Node = node;
			_remoteClassBase = RemoteClassBase.Connect<RemoteClassBase>(node);
		}

		public ClientBase(string ip_address, int port)
			: this(new Node()
			{
				IpAddress = ip_address,
				Port = port
			})
		{

		}

		public bool Ping()
		{
			try
			{
				return _remoteClassBase.Ping();
			}
			catch (Exception)
			{
				return false;
			}			
		}

		public virtual void Dispose()
		{
			if (_remoteClassBase != null)
			{
				_remoteClassBase.Dispose();
			}
		}
	}
}
