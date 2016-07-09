using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using Core.Model;
using Core.Model.Client;
using Core.Model.DataPacket;
using Core.Model.RemoteClass;
using Core.Model.Server;
using Model;

namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			//using (var cs = new CoordinationServer(39998))
			/*using (var iss = new InvokerServer())
			{
				var node = new Node() { IpAddress = "192.168.1.64", Port = 39999 };


				var str = String.Format("tcp://{0}:{1}/{2}", node.IpAddress, node.Port, typeof(RemoteCoordinator).FullName);
				var obj = RemotingServices.Connect(typeof(RemoteCoordinator), str);
			}*/
			//var channel = new TcpServerChannel("My",39999);
			//_tcpServerChannels.Add(port, channel);
			//ChannelServices.RegisterChannel(channel);
			
			using (var coordination_server = new CoordinationServer())
			//using (var invoke_server = new InvokerServer())
			using (var cc = new CalculativeClient())
			{
				coordination_server.AddInvokeServer(new Node()
				{
					IpAddress = "192.168.1.64",
					Port = InvokerServer.DefaultPort
				});
			/*
				coordination_server.AddInvokeServer(new Node()
				{
					IpAddress = "192.168.1.4",
					Port = InvokerServer.DefaultPort
				});*/
				
				while (true)
				{
				
				
					System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
					myStopwatch.Start(); //запуск
					
					
					var sum = new InvokeMethod<int>()
					{
						Path = "../../../Model/bin/Debug/Model.dll",
						TypeName = "Model.Bot",
						MethodName = "Sum",
					};

					var res1 = cc.Invoke(sum, 2, 3, 4);
					var res2 = cc.Invoke(sum, 4, 5, 6);
					var res3 = cc.Invoke(sum, 7, 8, 9);
					var res4 = cc.Invoke(sum, res1, res2, res3);



					//var res = sc.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });

					Console.WriteLine(res4.Value);
					myStopwatch.Stop();

					Console.WriteLine(myStopwatch.ElapsedMilliseconds);
					Console.ReadKey();
				}
			}



			/*
			Core.Model.DataPacket.Data xnt = new Core.Model.DataPacket.Data(1);
			Core.Model.DataPacket.Data<int> xt = new Core.Model.DataPacket.Data<int>(2);


			Core.Model.DataPacket.Data<double> xts = xnt;
			Core.Model.DataPacket.Data xnts = xt;
			*/
			//M2();

			

			//Bot b = new Bot();

			//CSharpDll d = RemoteClass.Connect<CSharpDll>("192.168.1.64", 39997);
		/*	var d = (RemoteClassBase)RemotingServices.Connect(typeof(RemoteClassBase), String.Format("tcp://{0}:{1}/{2}", "192.168.1.64", 39999, typeof(RemoteClassBase).FullName));
			var g = d.Guid;

			RemoteInvoker b = RemoteClassBase.Connect<RemoteInvoker>("192.168.1.64", 39999);

			object z = null;
			try
			{
				z = b.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });
				//z2 = b2.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}*/

			/*


			using (var c = new ClientBase("192.168.1.64", 39999))
			{
				//Bot d2 = RemoteClass.Connect<Bot>("192.168.1.64", 39997);
				int i = 0;
				while (true)
				{
					var x = c.Ping();
					Console.WriteLine("Ping success");

					
					
					//var z2 = d.Invoke(d.Sum);
					M4();

					RemoteInvoker b2 = RemoteClassBase.Connect<RemoteInvoker>("192.168.1.64", 39997);
					//var cc = d.Sum(2, 2);
					//var ccc = b.Sum(2, 2, 2);
					object z = null;
					object z2 = null;
					
					b2.Dispose();
					Console.WriteLine(z);
					Console.WriteLine("Ок");
				
					Console.ReadKey();
					//M3();
					//b.count = 25;
					//b.Val = 45;




					//var x = b.Sum(i++, 2, Assembly.LoadFrom("../../bin/Debug/Model.dll"));
					//	Console.WriteLine(b.GetPath());
					//Console.WriteLine(b.count);
					//b.SetAssembly();
				}
			}*/
		}

		public static void M4()
		{
			using (var b2 = RemoteClassBase.Connect<RemoteInvoker>("192.168.1.64", 39997))
			{
				b2.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });
			}
		}

		public static void M3()
		{
			var b = new CSharpDll();
			var x = b.GetAssambly(File.ReadAllBytes(@"ExternalLib/Model.dll"));
			Console.WriteLine(x);
		}

		public static void M2()
		{
			
			
			//создать и зарегистрировать клиентский канал
			TcpClientChannel channel = new TcpClientChannel();
			ChannelServices.RegisterChannel(channel);
			//зарегистрировать удаленный класс в локальном\n домене
			/*RemotingConfiguration.RegisterWellKnownClientType(
				typeof(CSharpDll),//удаленный класс
				//URI удаленного класса
				"tcp://localhost:39997/Core.Model.CSharpDll");*/
			//ChannelServices.UnregisterChannel(channel);
			var b = RemoteClassBase.Connect<CSharpDll>("192.168.1.64", 39996);
			
			// (CSharpDll)RemotingServices.Connect(typeof(CSharpDll), "tcp://localhost:39996/Core.Model.CSharpDll");
			//var c = RemotingServices.GetServerTypeForUri("tcp://localhost:39996/Core.Model.CSharpDll");
			//var b = new CSharpDll();
			var x = b.GetAssambly(File.ReadAllBytes(@"ExternalLib/Model.dll"));
			Console.WriteLine(x);

			var b2 = (CSharpDll)RemotingServices.Connect(typeof(CSharpDll), "tcp://localhost:39996/Core.Model.CSharpDll");
			var x2 = b.GetAssambly(File.ReadAllBytes(@"ExternalLib/Model.dll"));
			Console.WriteLine(x);
			//var x = RemotingConfiguration.GetRegisteredActivatedClientTypes();
		}
	}
}
