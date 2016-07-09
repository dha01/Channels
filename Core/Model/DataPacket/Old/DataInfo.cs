using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Model
{
	public class DataInfo
	{
		/// <summary>
		/// Идентификатор.
		/// </summary>
		public Guid Guid { get; set; }

		/// <summary>
		/// Узел владеющий результатом вычислений.
		/// </summary>
		public Node OwnerNode { get; set; }
	}
}
