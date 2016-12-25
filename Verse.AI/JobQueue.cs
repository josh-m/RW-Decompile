using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class JobQueue : IExposable
	{
		private List<Job> jobs = new List<Job>();

		public int Count
		{
			get
			{
				return this.jobs.Count;
			}
		}

		public Job this[int index]
		{
			get
			{
				return this.jobs[index];
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Job>(ref this.jobs, "jobs", LookMode.Deep, new object[0]);
		}

		public void EnqueueFirst(Job j)
		{
			this.jobs.Insert(0, j);
		}

		public void EnqueueLast(Job j)
		{
			this.jobs.Add(j);
		}

		public Job Dequeue()
		{
			if (this.jobs.NullOrEmpty<Job>())
			{
				return null;
			}
			Job result = this.jobs[0];
			this.jobs.RemoveAt(0);
			return result;
		}

		public Job Peek()
		{
			return this.jobs[0];
		}

		public void Clear()
		{
			this.jobs.Clear();
		}
	}
}
