using System;

namespace Verse
{
	public class FloodFillRangeQueue
	{
		private FloodFillRange[] array;

		private int count;

		private int head;

		private int debugNumTimesExpanded;

		private int debugMaxUsedSpace;

		public int Count
		{
			get
			{
				return this.count;
			}
		}

		public FloodFillRange First
		{
			get
			{
				return this.array[this.head];
			}
		}

		public string PerfDebugString
		{
			get
			{
				return string.Concat(new object[]
				{
					"NumTimesExpanded: ",
					this.debugNumTimesExpanded,
					", MaxUsedSize= ",
					this.debugMaxUsedSpace,
					", ClaimedSize=",
					this.array.Length,
					", UnusedSpace=",
					this.array.Length - this.debugMaxUsedSpace
				});
			}
		}

		public FloodFillRangeQueue(int initialSize)
		{
			this.array = new FloodFillRange[initialSize];
			this.head = 0;
			this.count = 0;
		}

		public void Enqueue(FloodFillRange r)
		{
			if (this.count + this.head == this.array.Length)
			{
				FloodFillRange[] destinationArray = new FloodFillRange[2 * this.array.Length];
				Array.Copy(this.array, this.head, destinationArray, 0, this.count);
				this.array = destinationArray;
				this.head = 0;
				this.debugNumTimesExpanded++;
			}
			this.array[this.head + this.count++] = r;
			this.debugMaxUsedSpace = this.count + this.head;
		}

		public FloodFillRange Dequeue()
		{
			FloodFillRange result = default(FloodFillRange);
			if (this.count > 0)
			{
				result = this.array[this.head];
				this.array[this.head] = default(FloodFillRange);
				this.head++;
				this.count--;
			}
			return result;
		}
	}
}
