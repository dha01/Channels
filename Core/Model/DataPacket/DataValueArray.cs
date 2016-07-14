using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DataPacket
{


	public class DataValueArray<T> : DataValue<T[]>
	{
		private readonly DataValue<T>[] _arary;

		public DataValue<T> this[int index]
		{
			get
			{
				return _arary[index];
			}

			set
			{
				_arary[index] = value;
			}
		}

		public int Length
		{
			get { return _arary.Length; }
		}

		public new T[] Value
		{
			get
			{
				var length = _arary.Length;
				var result = new T[length];
				Parallel.For(0, length, (i) =>
				{
					result[i] = (T)_arary[i].Value;
				});
				return result;
			}
		}

		public DataValueArray(int count)
		{
			_arary = new DataValue<T>[count];
		}

		public DataValueArray(DataValue<T>[] arary)
		{
			_arary = arary;
		}

		public DataValueArray(T[] arary)
		{
			var length = arary.Length;
			_arary = new DataValue<T>[length];

			for(int i = 0; i < length; i++)
			{
				_arary[i] = arary[i];
			}
		}

		public static implicit operator T[](DataValueArray<T> obj)
		{
			return obj.Value;
		}

		public static implicit operator T[][](DataValueArray<T> obj)
		{
			return null; // obj.Value;
		}

		public static implicit operator DataValueArray<T>(T[] obj)
		{
			return new DataValueArray<T>(obj);
		}
	}
}
