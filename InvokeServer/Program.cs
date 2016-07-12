using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using Core.Model;
using Core.Model.RemoteClass;
using Core.Model.Server;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var ff = AppDomain.CurrentDomain.GetAssemblies();
			ff = AppDomain.CurrentDomain.GetAssemblies();
			using (var invoker_server = new InvokerServer())
			{
				Console.WriteLine("InvokerServer. {0}:{1}", invoker_server.Node.IpAddress, invoker_server.Node.Port);

				while (true)
				{
					Console.WriteLine("Ок.");
					Console.ReadKey();
				}
			}
		}
	}
}
