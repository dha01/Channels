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
		private const int MATRIX_SIZE = 1000;


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
		static Random rand = new Random();
		static int[][] a = new int[MATRIX_SIZE][];
		static int[][] b = new int[MATRIX_SIZE][];

		private static void ResetMatrix()
		{
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
		}

		static void Main(string[] args)
		{
			using (var cc = new CalculativeClient())
			//using (var iss = new InvokerServer())
			{
				while (true)
				{
					ResetMatrix();
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
				/*		var bb = new Bot();

					//var x = bb.MulMatrix(a, b);


					
					int[][][] matrix_array_ = new int[8][][];
					for (int i = 0; i < 8; i++)
					{
						matrix_array_[i] = bb.MulMatrixGood(a, b);
					}

					
					for (int i = 0; i < 4; i++)
					{
						matrix_array_[i] = bb.MulMatrixGood(matrix_array_[i], matrix_array_[i + 4]);
					}

					for (int i = 0; i < 2; i++)
					{
						matrix_array_[i] = bb.MulMatrixGood(matrix_array_[i], matrix_array_[i + 2]);
					}

					int[][] gr2 = bb.MulMatrixGood(matrix_array_[0], matrix_array_[1]);
					 
					*/

					var MulMatrix = new InvokeMethod<int[][]>()
					{
						Path = "../../../Model/bin/Debug/Model.dll",
						TypeName = "Model.Bot",
						MethodName = "MulMatrixGood",
					};
				//	gr = cc.Invoke(MulMatrix, a, b);
					
					DataValue[] matrix_array = new DataValue[8];
					for (int i = 0; i < 8; i++)
					{
						matrix_array[i] = cc.Invoke(MulMatrix, a, b);
					}
					
					for (int i = 0; i < 4; i++)
					{
						matrix_array[i] = cc.Invoke(MulMatrix, matrix_array[i], matrix_array[i + 4]);
					}

					/*
					for (int i = 0; i < 4; i++)
					{
						Console.WriteLine(matrix_array[i].Value);
					}*/

					for (int i = 0; i < 2; i++)
					{
						matrix_array[i] = cc.Invoke(MulMatrix, matrix_array[i], matrix_array[i + 2]);
					}

					gr = cc.Invoke(MulMatrix, matrix_array[0], matrix_array[1]);

					//var res = sc.Invoke("../../../Model/bin/Debug/Model.dll", "Model.Bot", "Sum", new object[] { 1, 2, 3 });
					Console.WriteLine("Ждем");
					
					try
					{
						//var bb = new Bot();
//						int[][] val2 = gr2;
						//int[][] val3 = cc.Invoke(MulMatrix, a, b);


						int[][] val = (int[][])gr.Value;
						
						Console.WriteLine("!!Результат!!");
						Console.WriteLine(val[0][0]);
						Console.WriteLine("!!Результат!!");
						//Console.WriteLine(matrix_array[0].Value);
						
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}

					Console.WriteLine("!!Конец!!");
					
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
