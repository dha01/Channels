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
		private static Assembly a;
		
		public static void M1()
		{
			//var p = Path.GetFullPath("../../../Model/bin/Debug/Model.dll");
			//var mp = "F:\\Main Folder\\Creation\\Аспирантура\\ChannelsTests\\Model\\bin\\Debug\\Model.dll";


			//var a = Assembly.LoadFrom("../../../Model/bin/Debug/Model.dll");
			/*var*/
			/*
			a = Assembly.LoadFrom("ExternalLib/Model.dll");
			var type_name = "Model.Bot";
			//a = Assembly.GetAssembly(typeof (Bot));

			AppDomainSetup domainSetup = new AppDomainSetup { PrivateBinPath = "ExternalLib/Model.dll" };
			var newDomain = AppDomain.CreateDomain("Model", null, domainSetup);
			var c = (newDomain.CreateInstanceFromAndUnwrap("ExternalLib/Model.dll", type_name));

			var types = a.GetTypes();

			

			var ft = types.First(x => x.FullName.Equals(type_name));
			var pr = ft.GetMethods();
			*/

			//создать и зарегистрировать канал на порту 39993
			TcpServerChannel channel = new TcpServerChannel(39997);
			ChannelServices.RegisterChannel(channel);

			//var cc = new WellKnownServiceTypeEntry()

			//зарегистрировать класс для удаленной активизации
		/*	RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(CSharpDll),//регистрируемый класс
				"Core.Model.CSharpDll",//URI регистрируемого класса
				//режим активизации для каждого клиентского вызова
				WellKnownObjectMode.Singleton);
			*/
			RemoteClassBase obj = new RemoteClassBase();
			RemotingServices.Marshal(obj, "Core.Model.RemoteClassBase");

			/*

			var a = Assembly.LoadFrom("../../../Model/bin/Debug/Model.dll");

			var type_name = "Model.Bot";
			var t = a.GetType(type_name);

			var obj2 = Activator.CreateInstance(t);
			RemotingServices.Marshal(obj, type_name);*/

			//CSharpDll.IsRegistered = true;
			//ContextBoundObjectProxyAttribute._registeredTypes.Add(typeof(CSharpDll));
			//var t = RemotingConfiguration.RegisterWellKnownServiceType(typeof(Bot));
			/*

			var a = Assembly.LoadFrom("../../../Model/bin/Debug/Model.dll");
			
			var type_name = "Model.Bot";
			var t = a.GetType(type_name);

			RemotingConfiguration.RegisterWellKnownServiceType(
				t,//регистрируемый класс
				type_name,//URI регистрируемого класса
				//режим активизации для каждого клиентского вызова
				WellKnownObjectMode.SingleCall);
			ContextBoundObjectProxyAttribute._registeredTypes.Add(t);*/
		}

		public class ProxyClass : MarshalByRefObject { }

		public static void LoadAssembly()
		{
		/*	string pathToDll = Assembly.GetExecutingAssembly().CodeBase;
			AppDomainSetup domainSetup = new AppDomainSetup { PrivateBinPath = pathToDll };
			var newDomain = AppDomain.CreateDomain("FooBar", null, domainSetup);
			ProxyClass c = (ProxyClass)(newDomain.CreateInstanceFromAndUnwrap(pathToDll, typeof(ProxyClass).FullName));
			Console.WriteLine(c == null);

			Console.ReadKey(true);*/
		}
		
		static void Main(string[] args)
		{
			var ff = AppDomain.CurrentDomain.GetAssemblies();
			
			
			//M1();
			ff = AppDomain.CurrentDomain.GetAssemblies();
			//AppDomain.

			//using (var n = new InvokerServer(39999))
			//using (var coordination_server = new CoordinationServer())
			using (var invoker_server = new InvokerServer())
			{
				//coordination_server.AddInvokeServer(invoker_server.Node);
				/*coordination_server.AddInvokeServer(new Node()
				{
					IpAddress = "192.168.1.64",
					Port = InvokerServer.DefaultPort
				});
				coordination_server.AddInvokeServer(new Node()
				{
					IpAddress = "192.168.1.4",
					Port = InvokerServer.DefaultPort
				});*/
				Console.WriteLine("InvokerServer.");
			/*
			TcpServerChannel channel = new TcpServerChannel(39999);
			ChannelServices.RegisterChannel(channel);

			RemoteClassBase obj = (RemoteClassBase)Activator.CreateInstance(typeof(RemoteClassBase));
			//obj.Guid = new Guid();
			RemotingServices.Marshal(obj, typeof(RemoteClassBase).FullName);

			RemoteInvoker obj2 = (RemoteInvoker)Activator.CreateInstance(typeof(RemoteInvoker));
			RemotingServices.Marshal(obj2, typeof(RemoteInvoker).FullName);
			*/

				while (true)
				{
					Console.WriteLine("Ок.");
					Console.ReadKey();
				
					//var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					//ConfigurationManager.RefreshSection("runtime");
					/*
					for (int i = 0; i < config.Sections.Count; i++)
					{
						var connectionStringsSection = config.Sections["runtime"].;
					}
					*/
					// .GetSection("runtime");
					//connectionStringsSection.ConnectionStrings["Blah"].ConnectionString = "Data Source=blah;Initial Catalog=blah;UID=blah;password=blah";
					//config.Save();
					//ConfigurationManager.RefreshSection("connectionStrings");

					//ConfigurationManager.RefreshSection("runtime");
					//Console.WriteLine("Конфигурация обновлена.");
				}
			}
		}
	}
}
