using System;

namespace Core.Model.DataPacket
{
	/// <summary>
	/// Метод для исполнения.
	/// </summary>
	[Serializable]
	public class InvokeMethod
	{
		#region Fields

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

		#endregion
	}

	/// <summary>
	/// Метод для исполнения с типом возвразаемого значения.
	/// </summary>
	/// <typeparam name="TResult">Тип данных возвращаемый методом.</typeparam>
	[Serializable]
	public class InvokeMethod<TResult> : InvokeMethod
	{

	}
}
