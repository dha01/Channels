using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Model.DataPacket;

namespace Core.Model
{
	/// <summary>
	/// Метод для исполнения с типом возвразаемого значения.
	/// </summary>
	/// <typeparam name="TResult">Тип данных возвращаемый методом.</typeparam>
	[Serializable]
	public class InvokeMethod<TResult> : InvokeMethod
	{

	}
	
	/// <summary>
	/// Метод для исполнения.
	/// </summary>
	[Serializable]
	public class InvokeMethod
	{
		/// <summary>
		/// Путь к файлу.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Названия типа.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Название метода.
		/// </summary>
		public string MethodName { get; set; }
	}

	/// <summary>
	/// Пакет для исполнения.
	/// </summary>
	[Serializable]
	public class InvokePacket
	{
		private static long Counter = 0;

		public long Priority = Counter++;
		
		/// <summary>
		/// Уникаольный идентификатор.
		/// </summary>
		public Guid Guid { get; set; }

		/// <summary>
		/// Метод для исполнения.
		/// </summary>
		public InvokeMethod InvokeMethod { get; set; }

		/// <summary>
		/// Входные параметры.
		/// </summary>
		public DataValue[] InputParams { get; set; }

		/// <summary>
		/// Присваивает уникальный идентификатор.
		/// </summary>
		public InvokePacket()
		{
			Guid = Guid.NewGuid();
		}
	}
}
