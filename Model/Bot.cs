using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Model;
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
			Console.WriteLine("Created!");
		}

		public int SetAssembly(int a)
		{
			return a;
			//Console.WriteLine(a);
			//_assembly = assembly;
		}

		public int Sum(int a, int b, int c)
		{
			count++;

			Count++;
			SCount++;
			Console.WriteLine(a + b + c);
			return a + b + c;
		}

		public string GetPath()
		{
			return Environment.CurrentDirectory;
		}
    }
}
