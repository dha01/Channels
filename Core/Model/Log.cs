using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
	enum LogType
	{
		Debug,
		Warning,
		Exception
	}

	public class LogMessage
	{
		public string Path { get; set; }
		public string Message { get; set; }
	}
	
	public class Log
	{
		public const string REMOVE = "Remove";
		
		private static string _path = "F:\\Main Folder\\Creation\\Аспирантура\\Channels\\Log";
		
		private static QueueInvoker<LogMessage> _writeToFileQueue = new QueueInvoker<LogMessage>(WtiteToFile);
		public static void WtiteToFile(LogMessage log_message)
		{
			while(true)
			{
				try
				{
					using (StreamWriter sw = File.AppendText(log_message.Path))
					{
						sw.WriteLine(log_message.Message);
					}
					break;
				}
				catch (Exception)
				{

				}
			}	
	/*
			if (log_message.Message.Contains(REMOVE))
			{
				File.Delete(log_message.Path);
			}*/
		}

		public static Dictionary<Guid, int> _indexs = new Dictionary<Guid, int>(); 

		public static void Trace(Guid guid, string message)
		{
			/*if (!_indexs.ContainsKey(guid))
			{
				_indexs.Add(guid, 0);
			}

			if (_indexs[guid] < 0)
			{
				return;
			}

			if (message.Contains(REMOVE))
			{
				_indexs[guid] = -100;
			}
			

			_indexs[guid]++;

			_writeToFileQueue.Enqueue(new LogMessage()
			{
				Message = string.Format("{0} {1}", _indexs[guid], message),
				Path = string.Format("F:\\Main Folder\\Creation\\Аспирантура\\Channels\\Log\\{0}.txt",guid)
			});*/
		}
		
		public void Write(string message, params object[] param)
		{
			Console.WriteLine(message, param);
		}
	}
}
