using System;

namespace Verse.AI
{
	public struct ThinkResult : IEquatable<ThinkResult>
	{
		private Job jobInt;

		private ThinkNode sourceNodeInt;

		public Job Job
		{
			get
			{
				return this.jobInt;
			}
		}

		public ThinkNode SourceNode
		{
			get
			{
				return this.sourceNodeInt;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.Job != null;
			}
		}

		public static ThinkResult NoJob
		{
			get
			{
				return new ThinkResult(null, null);
			}
		}

		public ThinkResult(Job job, ThinkNode sourceNode)
		{
			this.jobInt = job;
			this.sourceNodeInt = sourceNode;
		}

		public override string ToString()
		{
			string text = (this.Job == null) ? "null" : this.Job.ToString();
			string text2 = (this.SourceNode == null) ? "null" : this.SourceNode.ToString();
			return string.Concat(new string[]
			{
				"(job=",
				text,
				" sourceNode=",
				text2,
				")"
			});
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine<Job>(seed, this.jobInt);
			return Gen.HashCombine<ThinkNode>(seed, this.sourceNodeInt);
		}

		public override bool Equals(object obj)
		{
			return obj is ThinkResult && this.Equals((ThinkResult)obj);
		}

		public bool Equals(ThinkResult other)
		{
			return this.jobInt == other.jobInt && this.sourceNodeInt == other.sourceNodeInt;
		}

		public static bool operator ==(ThinkResult lhs, ThinkResult rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ThinkResult lhs, ThinkResult rhs)
		{
			return !(lhs == rhs);
		}
	}
}
