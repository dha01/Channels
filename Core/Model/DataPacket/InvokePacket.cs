using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model.DataPacket;

namespace Core.Model
{
	[Serializable]
	public class InvokeMethod<TResult> : InvokeMethod
	{

	}
	
	[Serializable]
	public class InvokeMethod
	{
		public string Path { get; set; }
		public string TypeName { get; set; }
		public string MethodName { get; set; }
	}

	[Serializable]
	public class InvokePacket
	{
		public Guid Guid { get; set; }
		public InvokeMethod InvokeMethod { get; set; }
		public DataValue[] InputParams { get; set; }

		public InvokePacket()
		{
			Guid = Guid.NewGuid();
		}
	}
}
