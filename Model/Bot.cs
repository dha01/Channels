using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Model;
using Core.Model.Client;
using Core.Model.DataPacket;
using Core.Model.RemoteClass;

namespace Model
{

	public class Bot : RemoteClassBase
	{
		public int count = 0;

		public Assembly _assembly;

		public Assembly Assembly
		{
			get { return _assembly; }

			set
			{
				_assembly = value;
				Console.WriteLine(_assembly.FullName);
			}
		}

		public int _val;

		public int Val
		{
			get { return _val; }

			set
			{
				_val = value;
				Console.WriteLine(_val);
			}
		}
		
		public Bot()
		{
			//Console.WriteLine("Created!");
		}

		public int SetAssembly(int a)
		{
			return a;
			//Console.WriteLine(a);
			//_assembly = assembly;
		}

		public int Mul(int[] a, int[] b)
		{
			var size = a.Length;
			int result = 0;
			for (int i = 0; i < size; i++)
			{
				result += a[i]*b[i];
			}
			return result;
		}

		public int[][] MulMatrix(int[][] a, int[][] b)
		{
			var size = a.Length;

			var result = new DataValueArray2D<int>(size);

			int[][] bt = new int[size][];

			for (int i = 0; i < size; ++i)
			{
				bt[i] = new int[size];
				for (int j = 0; j < size; ++j)
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
			
			using (var cc = new CalculativeClient())
			{
				//cc._sentToInvokeQueue.Stop();
				for (var i = 0; i < size; i++)
				{
					result[i] = new DataValueArray<int>(size);
					for (var j = 0; j < size; j++)
					{
						result[i][j] = cc.Invoke(mul, a[i], bt[j]);
					}
				}

				//cc._sentToInvokeQueue.Run();
			}
			Console.WriteLine("Ждем!");
			
			int[][] x = result.Value;
			return x;
		}

		public int Sum(int a, int b, int c)
		{
			count++;
			/*
			Count++;
			SCount++;*/
			//Console.WriteLine(a + b + c);
			return a + b + c;
		}

		public string GetPath()
		{
			return Environment.CurrentDirectory;
		}
    }
}
