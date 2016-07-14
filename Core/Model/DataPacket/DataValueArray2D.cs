using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DataPacket
{
	public class DataValueArray2D<T> : DataValue<T[][]>
	{
		private readonly DataValueArray<DataValueArray<T>> _arary;

		public DataValueArray<T> this[int index]
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

		public new T[][] Value
		{
			get
			{
				var length = _arary.Length;
				var result = new T[length][];
				Parallel.For(0, length, (i) =>
				{
					result[i] = _arary[i].Value;
				});
				return result;
			}
		}

		public DataValueArray2D(int count)
		{
			_arary = new DataValueArray<DataValueArray<T>>(count);
		}

		public DataValueArray2D(DataValueArray<DataValueArray<T>> arary)
		{
			_arary = arary;
		}

		public static implicit operator T[][](DataValueArray2D<T> obj)
		{
			return null; // obj.Value;
		}
	}
}
