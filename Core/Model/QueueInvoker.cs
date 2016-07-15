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
		/// Максимальное количество исполнителей.
		/// </summary>
		public int MaxQueueCount { get; set; }

		/// <summary>
		/// Текущее число запущенных исполнителей.
		/// </summary>
		private int _curQueueCount;

		public int CurQueueCount
		{
			get { return _curQueueCount; }
		}

		private bool IsRun { get; set; }

		public int QueueLength
		{
			get { return _invokeQueue.Count; }
		}

		/// <summary>
		/// Очередь на исполнение.
		/// </summary>
		private readonly ConcurrentQueue<T> _invokeQueue = new ConcurrentQueue<T>();

		/// <summary>
		/// Событие при извлечении из очереди.
		/// </summary>
		public Action<T> OnDequeue;

		private ManualResetEvent me = new ManualResetEvent(false);
		private object _lockObject = new object();

		private Semaphore _semaphore;

		private int QueueLengthCount;

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
			MaxQueueCount = queue_count;
			OnDequeue += action;
			_semaphore = new Semaphore(0, int.MaxValue);
			AddQueueInvoker();
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
			_semaphore.Release();
			//Interlocked.Increment(ref QueueLengthCount);
		}
		/*
		private void R()
		{
			Task.Run(() =>
			{
				while (AddQueueInvoker() && _curQueueCount >= MaxQueueCount)
				{
					
				}
			});
		}*/

		/// <summary>
		/// Запускает работу исполнителей.
		/// </summary>
		public void Run()
		{
			IsRun = true;
			/*if (_curQueueCount < QueueCount && AddQueueInvoker())
			{
				Run();
			}*/
		}

		/// <summary>
		/// Приостанавливает работу всех исполнителей.
		/// </summary>
		public void Stop()
		{
			IsRun = false;
			//me.Reset();
		}

		#endregion

		#region Methods / Private
		/*
		private void Dequeue()
		{
			Task.Run(() =>
			{
				_semaphore.WaitOne();
				T value;
				if (!_invokeQueue.TryDequeue(out value)) return;
				try
				{
					OnDequeue.Invoke(value);
				}
				catch (Exception e)
				{
					Console.WriteLine("QueueInvoker: ошибка при обрпботке элемента в очереди: {0}", e.Message);
				}
				_semaphore.Release();
			});
		}*/

		//private List<KeyValuePair<ManualResetEvent, bool>> _queueStopper = new List<KeyValuePair<ManualResetEvent, bool>>(); 
		private void AddQueueInvoker()
		{
			//var iv = Interlocked.Increment(ref _curQueueCount);
			if (_curQueueCount < MaxQueueCount)
			{
				_curQueueCount++;
				Task.Run(() =>
				{
					
					while (/*IsRun && _invokeQueue.TryDequeue(out value)*/true)
					{
						_semaphore.WaitOne();
						T value;
						if(!_invokeQueue.TryDequeue(out value))
						{
							_semaphore.Release();
							Console.WriteLine("QueueInvoker: ошибка при извлечении элемента из очереди.");
							continue;
						}
						try
						{
							OnDequeue.Invoke(value);
						}
						catch (Exception e)
						{
							Console.WriteLine("QueueInvoker: ошибка при обработке элемента в очереди: {0}", e.Message);
						}
					}
					/*Interlocked.Decrement(ref _curQueueCount);
					Thread.Sleep(10);
					if (_curQueueCount == 0 && _invokeQueue.Count > 0)
					{
						AddQueueInvoker();
					}*/
				});
				AddQueueInvoker();
			}
		}

		#endregion
	}
}
