using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
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
		private const int MATRIX_SIZE = 100;


		static int[][] Mul(int[][] a, int[][] b)
		{
			int[][] bt = new int[MATRIX_SIZE][];

			for (int i = 0; i < MATRIX_SIZE; ++i)
			{
				bt[i] = new int[MATRIX_SIZE];
				for (int j = 0; j < MATRIX_SIZE; ++j)
				{
					bt[i][j] = b[j][i];
				}
			}

			var mul = new InvokeMethod<int>()
			{
				Path = "../../../Model/bin/Debug/Model.dll",
				TypeName = "Model.Bot",
				MethodName = "Mul",
			};

			int[][] result = new int[MATRIX_SIZE][];

			using (var cc = new CalculativeClient())
			{
				Parallel.For(0, MATRIX_SIZE, (i) =>
				{
					result[i] = new int[MATRIX_SIZE];

					Parallel.For(0, MATRIX_SIZE, (j) =>
					{
						result[i][j] = cc.Invoke(mul, a[i], bt[j]).Value;
						Console.WriteLine("Получен результат для [{0}][{1}]", i, j);
					});
				});
			}

			return result;
		}

		static void Main(string[] args)
		{
			var rand = new Random();
			
			int[][] a = new int[MATRIX_SIZE][];
			int[][] b = new int[MATRIX_SIZE][];

			for (var i = 0; i < MATRIX_SIZE; i++)
			{
				a[i] = new int[MATRIX_SIZE];
				b[i] = new int[MATRIX_SIZE];
				for (var i2 = 0; i2 < MATRIX_SIZE; i2++)
				{
					a[i][i2] = rand.Next(0, 100);
					b[i][i2] = rand.Next(0, 100);
				}
			}

			using (var cc = new CalculativeClient())
			using (var iss = new InvokerServer())
			{
				while (true)
				{
					
					Console.ReadKey();
					System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
					myStopwatch.Start(); //запуск
					DataValue gr = 0;

					/*var sum = new InvokeMethod<int>()
					{
						Path = "../../../Model/bin/Debug/Model.dll",
						TypeName = "Model.Bot",
						MethodName = "Sum",
					};
					for (int i = 0; i < 1000; i++)
					{
						
						gr = cc.Invoke(sum, 1, 2, 3);
						Console.WriteLine(gr.Value);
					}*/
						/*var bb = new Bot();

					var x = bb.MulMatrix(a, b);*/
						
					
					var MulMatrix = new InvokeMethod<int[][]>()
					{
						Path = "../../../Model/bin/Debug/Model.dll",
						TypeName = "Model.Bot",
						MethodName = "MulMatrix",
					};
					gr = cc.Invoke(MulMatrix, a, b);



					//var res = sc.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });
					Console.WriteLine("Ждем");

						try
						{
							Console.WriteLine(gr.Value);
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
					
					myStopwatch.Stop();

					Console.WriteLine(myStopwatch.ElapsedMilliseconds);
				//	var dd = RemoteClassBase._objects;
					}
				/*	GC.WaitForPendingFinalizers();
					GC.Collect(1, GCCollectionMode.Forced);*/
			}
		}
	}
}
