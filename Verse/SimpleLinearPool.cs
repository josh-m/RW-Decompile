using System;
using System.Collections.Generic;

namespace Verse
{
	public class SimpleLinearPool<T> where T : new()
	{
		private List<T> items = new List<T>();

		private int readIndex;

		public T Get()
		{
			if (this.readIndex >= this.items.Count)
			{
				this.items.Add(Activator.CreateInstance<T>());
			}
			return this.items[this.readIndex++];
		}

		public void Clear()
		{
			this.readIndex = 0;
		}
	}
}
