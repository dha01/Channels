using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Model
{
	/// <summary>
	/// Очередь исполнения.
	/// </summary>
	/// <typeparam name="T">Тип объекта исполнения.</typeparam>
	public class QueueInvoker<T>
	{
		#region Constants

		/// <summary>
		/// Количество исполнителей по умолчанию.
		/// </summary>
		private const int DEFAULT_QUEUE_COUNT = 1;

		#endregion

		#region Fields

		/// <summary>
		/// Количество исполнителей.
		/// </summary>
		private int QueueCount { get; set; }

		private int CurQueueCount = 0;

		private bool IsRun = true;

		public int QueueLength
		{
			get { return _invokeQueue.Count; }
		}

		/// <summary>
		/// Очередь на исполнение.
		/// </summary>
		private readonly ConcurrentQueue<T> _invokeQueue = new ConcurrentQueue<T>();

		/// <summary>
		/// Отвечает за приостановку исполнения при отсутствии записей в очереди.
		/// </summary>
		private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

		/// <summary>
		/// Отвечает за приостановку исполнения по требованию.
		/// </summary>
		private readonly ManualResetEvent _runManualResetEvent = new ManualResetEvent(false);

		private readonly ManualResetEvent _enManualResetEvent = new ManualResetEvent(true);
		private readonly ManualResetEvent _deManualResetEvent = new ManualResetEvent(true);

		private T[] _circle = new T[100];

		private int _enqueueIndex = 0;

		private int _dequeueIndex = 0;

		private Object _enqueueLock = new Object();
		private Object _dequeueLock = new Object();
		/// <summary>
		/// Событие при извлечении из очереди.
		/// </summary>
		public Action<T> OnDequeue;

		#endregion

		#region Constructor

		/// <summary>
		/// Инициализирует очередь исполнения исполняемым действием.
		/// </summary>
		/// <param name="action">Исполняемое действие.</param>
		public QueueInvoker(Action<T> action)
			: this(action, DEFAULT_QUEUE_COUNT)
		{

		}

		/// <summary>
		/// Инициализирует очередь исполнения с определенным числом исполнителей.
		/// </summary>
		/// <param name="action">Исполняемое действие.</param>
		/// <param name="queue_count">Число исполнителей.</param>
		public QueueInvoker(Action<T> action, int queue_count)
		{
			QueueCount = queue_count;
			OnDequeue += action;
			Run();
		}

		#endregion

		#region Methods / Public
		
		/// <summary>
		/// Добавляет новый объект в очередь на исполнение.
		/// </summary>
		/// <param name="value"></param>
		public void Enqueue(T value)
		{
			_invokeQueue.Enqueue(value);
			AddQueue();

			//_manualResetEvent.Set();

/*			_enManualResetEvent.WaitOne();
			int cur_index;
			lock (_enqueueLock)
			{
				cur_index = _enqueueIndex++;
				_deManualResetEvent.Set();
				if (_dequeueIndex % _circle.Length > _enqueueIndex % _circle.Length)
				{
					
					_dequeueIndex--;
				}
			}
			_circle[cur_index % _circle.Length] = value;

			_manualResetEvent.Set();*/
		}
		/*
		public T Dequeue()
		{
			_deManualResetEvent.WaitOne();

			//_invokeQueue.Enqueue(result);
			int cur_index;
			lock (_dequeueLock)
			{
				cur_index = _dequeueIndex++;
				if (_dequeueIndex >= _enqueueIndex)
				{
					_dequeueIndex--;
					_deManualResetEvent.Reset();
				}
			}
			T result = _circle[cur_index % _circle.Length];

			_enManualResetEvent.Set();
			return result;
		}*/

		/// <summary>
		/// Запускает работу всех исполнителей.
		/// </summary>
		public void Run()
		{
			IsRun = true;
		}

		/// <summary>
		/// Приостанавливает работу всех исполнителей.
		/// </summary>
		public void Stop()
		{
			IsRun = false;
		}

		#endregion

		#region Methods / Private

		/// <summary>
		/// Добавляет нового исполнителя.
		/// </summary>
		private void AddQueue()
		{
			if (CurQueueCount < QueueCount && _invokeQueue.Count > CurQueueCount)
			{
				Interlocked.Increment(ref CurQueueCount);
				AddQueue();
				Task.Run(() =>
				{
					try
					{
						while (IsRun)
						{
							T value;
							if (_invokeQueue.TryDequeue(out value))
							{
								if (OnDequeue != null)
								{
									OnDequeue.Invoke(value);
								}
							}
							else
							{
								Interlocked.Decrement(ref CurQueueCount);
								break;
							}
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
				});
			}
		}

		#endregion
	}
}
