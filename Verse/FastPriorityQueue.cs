using System;
using System.Collections.Generic;

namespace Verse
{
	public class FastPriorityQueue<T>
	{
		protected List<T> innerList = new List<T>();

		protected IComparer<T> comparer;

		public int Count
		{
			get
			{
				return this.innerList.Count;
			}
		}

		public FastPriorityQueue()
		{
			this.comparer = Comparer<T>.Default;
		}

		public FastPriorityQueue(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public void Push(T item)
		{
			int num = this.innerList.Count;
			this.innerList.Add(item);
			while (num != 0)
			{
				int num2 = (num - 1) / 2;
				if (this.CompareElements(num, num2) >= 0)
				{
					return;
				}
				this.SwapElements(num, num2);
				num = num2;
			}
		}

		public T Pop()
		{
			T result = this.innerList[0];
			int num = 0;
			this.innerList[0] = this.innerList[this.innerList.Count - 1];
			this.innerList.RemoveAt(this.innerList.Count - 1);
			while (true)
			{
				int num2 = num;
				int num3 = 2 * num + 1;
				int num4 = 2 * num + 2;
				if (this.innerList.Count > num3 && this.CompareElements(num, num3) > 0)
				{
					num = num3;
				}
				if (this.innerList.Count > num4 && this.CompareElements(num, num4) > 0)
				{
					num = num4;
				}
				if (num == num2)
				{
					break;
				}
				this.SwapElements(num, num2);
			}
			return result;
		}

		public void Clear()
		{
			this.innerList.Clear();
		}

		protected void SwapElements(int i, int j)
		{
			T value = this.innerList[i];
			this.innerList[i] = this.innerList[j];
			this.innerList[j] = value;
		}

		protected int CompareElements(int i, int j)
		{
			return this.comparer.Compare(this.innerList[i], this.innerList[j]);
		}
	}
}
