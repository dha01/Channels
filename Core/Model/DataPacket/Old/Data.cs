using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
	[Serializable]
	public class Data : IDisposable
	{
		public static void WaitAll(Data[] data_array)
		{
			foreach (var data in data_array)
			{
				data.Wait();
			}
		}
		
		public Guid Guid { get; set; }

		
		protected bool _hasValue = false;

		public Data(object data)
		{
			_data = data;
			_hasValue = true;
		}

		public object Value
		{
			get { return _Data; }
		}


		protected object _data;
		protected virtual object _Data
		{
			get
			{
				return _data;
			}

			set
			{
				_data = value;
				_hasValue = true;
			}
		}

		public virtual void Wait()
		{
			var x = _Data;
		}

		public void Dispose()
		{
			var g = 5;
		}

		~Data()
		{
			var g = 5;
		}

		public static implicit operator Data(int obj)
		{
			return new Data(obj);
		}
	}

	[Serializable]
	public class Data<T> : Data
	{
		
		[NonSerialized]
		private Task<T> _task;
		
		public Data(T data)
			: base(data)
		{

		}

		public Data(Task<T> data) : base(null)
		{
			_task = data;
			_task.Start();
		}

		protected override object _Data
		{
			get
			{
				if (!_hasValue)
				{
					if (_task == null)
					{
						throw new Exception("Фигня в Data");
					}
					_data = _task.Result;
					_hasValue = true;
				}

				return _data;
			}

			set
			{
				_data = value;
				_hasValue = true;
			}
		}
		/*
		public override void Wait()
		{
			_task.Wait();
		}*/

		public new T Value
		{
			get { return (T)_Data; }
		}
		
		public static implicit operator T(Data<T> obj)
		{
			return (T)obj._Data;
		}
		
		public static implicit operator Data<T>(Task<T> obj)
		{
			return new Data<T>(obj);
		}

		public static implicit operator Data<T>(T obj)
		{
			return new Data<T>(obj);
		}
	}
}
