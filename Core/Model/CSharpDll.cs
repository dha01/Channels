using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using Core.Model.RemoteClass;

namespace Core.Model
{

	public class CSharpDll : RemoteClassBase
	{
		public int Sum(int a, int b)
		{
			return a + b;
		}

		public object Invoke<TResult>(Func<int, int, TResult> func)
		{
			
			return 1;
		}

		
		
		public int GetAssambly(byte[] assembly)
		{
			var a = Assembly.Load(assembly);
			
			return 5;
		}
	}
}
