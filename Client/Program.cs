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
			
			using (var cc = new CalculativeClient())
			{
				while (true)
				{

					Console.ReadKey();
					System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
					myStopwatch.Start(); //запуск
					
					
					var sum = new InvokeMethod<int>()
					{
						Path = "../../../Model/bin/Debug/Model.dll",
						TypeName = "Model.Bot",
						MethodName = "Sum",
					};

					DataValue gr = 0;
					for (int i = 0; i < 1000; i++)
					{
						//var res1 = cc.Invoke(sum, 1, 2, 3);
						var res2 = cc.Invoke(sum, 1, 1, 1);
						var res3 = cc.Invoke(sum, 1, 1, 5);

						gr = cc.Invoke(sum, gr, res2, res3);
					}
/*
					var res1 = cc.Invoke(sum, 2, 3, 4);
					var res2 = cc.Invoke(sum, 4, 5, 6);
					var res3 = cc.Invoke(sum, 7, 8, 9);
					var res4 = cc.Invoke(sum, res1, res2, res3);*/



					//var res = sc.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });
					Console.WriteLine("Ждем");
					Console.WriteLine(gr.Value);
					myStopwatch.Stop();

					Console.WriteLine(myStopwatch.ElapsedMilliseconds);
				}
			}
		}
	}
}
